#region Copyright
// ****************************************************************************
// <copyright file="WindowsPhoneBootstrapperBase.cs">
// Copyright © Vyacheslav Volkov 2012-2014
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************
#endregion
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Infrastructure.Navigation;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Utils;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Infrastructure
{
    /// <summary>
    ///     Represents the base class that is used to start MVVM application.
    /// </summary>
    public abstract class WindowsPhoneBootstrapperBase : BootstrapperBase
    {
        #region Fields

        /// <summary>
        /// Gets the name of binding assembly.
        /// </summary>
        protected const string BindingAssemblyName = "MugenMvvmToolkit.Binding.WinRT";
        private readonly PlatformInfo _platform;
        private readonly PhoneApplicationFrame _rootFrame;

        #endregion

        #region Constructors

        static WindowsPhoneBootstrapperBase()
        {
#if V71
            ServiceProvider.WeakReferenceFactory = PlatformExtensions.CreateWeakReference;
#endif
            DynamicMultiViewModelPresenter.CanShowViewModelDefault = CanShowViewModelTabPresenter;
            DynamicViewModelNavigationPresenter.CanShowViewModelDefault = CanShowViewModelNavigationPresenter;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="WindowsPhoneBootstrapperBase" /> class.
        /// </summary>
        protected WindowsPhoneBootstrapperBase([NotNull] PhoneApplicationFrame rootFrame)
        {
            Should.NotBeNull(rootFrame, "rootFrame");
            _rootFrame = rootFrame;
            _platform = PlatformExtensions.GetPlatformInfo();
        }

        #endregion

        #region Overrides of BootstrapperBase

        /// <summary>
        ///     Gets the current platform.
        /// </summary>
        public override PlatformInfo Platform
        {
            get { return _platform; }
        }

        /// <summary>
        ///     Starts the current bootstraper.
        /// </summary>
        protected override void OnInitialize()
        {
            base.OnInitialize();
            IocContainer.BindToConstant<INavigationService>(new FrameNavigationService(_rootFrame, IocContainer.Get<ISerializer>()));
            FrameStateManager.RegisterFrame(_rootFrame);
            Should.PropertyBeNotNull(PhoneApplicationService.Current, "PhoneApplicationService.Current");
            PhoneApplicationService.Current.Launching += OnLaunching;
            var provider = IocContainer.Get<INavigationProvider>();
        }

        /// <summary>
        ///     Gets the application assemblies.
        /// </summary>
        protected override ICollection<Assembly> GetAssemblies()
        {
            var listAssembly = new List<Assembly>();
            foreach (AssemblyPart part in Deployment.Current.Parts)
            {
                string assemblyName = part.Source.Replace(".dll", string.Empty);
                if (assemblyName.Contains("/"))
                    continue;
                try
                {
                    Assembly assembly = Assembly.Load(assemblyName);
                    if (MvvmUtils.NonFrameworkAssemblyFilter(assembly))
                        listAssembly.Add(assembly);
                }
                catch (Exception e)
                {
                    Tracer.Error(e.Flatten(true));
                }
            }
            return listAssembly;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Starts the current bootstrapper.
        /// </summary>
        public virtual void Start(IDataContext context = null)
        {
            if (context == null)
                context = DataContext.Empty;
            Initialize();
            CreateMainViewModel(GetMainViewModelType(), context).ShowAsync((model, result) => model.Dispose(), context: context);
        }

        /// <summary>
        ///     Creates the main view model.
        /// </summary>
        [NotNull]
        protected virtual IViewModel CreateMainViewModel([NotNull] Type viewModelType, [NotNull] IDataContext context)
        {
            return IocContainer
                .Get<IViewModelProvider>()
                .GetViewModel(viewModelType, context);
        }

        /// <summary>
        ///     Gets the type of main view model.
        /// </summary>
        [NotNull]
        protected abstract Type GetMainViewModelType();

        private void OnLaunching(object sender, LaunchingEventArgs args)
        {
            PhoneApplicationService.Current.Launching -= OnLaunching;
            Start();
        }

        private static bool CanShowViewModelTabPresenter(IViewModel viewModel, IDataContext dataContext, IViewModelPresenter arg3)
        {
            var viewName = dataContext.GetData(ActivationConstants.ViewName) ??
                    viewModel.Settings.Metadata.GetData(ActivationConstants.ViewName);
            var container = MvvmUtils.GetIocContainer(viewModel, true);
            var mappingProvider = container.Get<IViewMappingProvider>();
            var mappingItem = mappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, false);
            return mappingItem == null || !typeof(Page).IsAssignableFrom(mappingItem.ViewType);
        }

        private static bool CanShowViewModelNavigationPresenter(IViewModel viewModel, IDataContext dataContext, IViewModelPresenter arg3)
        {
            var viewName = dataContext.GetData(ActivationConstants.ViewName) ??
                    viewModel.Settings.Metadata.GetData(ActivationConstants.ViewName);
            var container = MvvmUtils.GetIocContainer(viewModel, true);
            var mappingProvider = container.Get<IViewMappingProvider>();
            var mappingItem = mappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, false);
            return mappingItem != null && typeof(Page).IsAssignableFrom(mappingItem.ViewType);
        }

        #endregion
    }
}