#region Copyright

// ****************************************************************************
// <copyright file="WindowsPhoneBootstrapperBase.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;
using MugenMvvmToolkit.WinPhone.Infrastructure.Navigation;
using MugenMvvmToolkit.WinPhone.Interfaces.Navigation;

namespace MugenMvvmToolkit.WinPhone.Infrastructure
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
        protected const string BindingAssemblyName = "MugenMvvmToolkit.WinPhone.Binding";
        private readonly PhoneApplicationFrame _rootFrame;
        private readonly PlatformInfo _platform;

        #endregion

        #region Constructors

        static WindowsPhoneBootstrapperBase()
        {
            DynamicMultiViewModelPresenter.CanShowViewModelDefault = CanShowViewModelTabPresenter;
            DynamicViewModelNavigationPresenter.CanShowViewModelDefault = CanShowViewModelNavigationPresenter;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="WindowsPhoneBootstrapperBase" /> class.
        /// </summary>
        protected WindowsPhoneBootstrapperBase([NotNull] PhoneApplicationFrame rootFrame, PlatformInfo platform = null)
        {
            Should.NotBeNull(rootFrame, "rootFrame");
            _rootFrame = rootFrame;
            _platform = platform ?? PlatformExtensions.GetPlatformInfo();
            PhoneApplicationService.Current.Launching += OnLaunching;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the root frame.
        /// </summary>
        protected PhoneApplicationFrame RootFrame
        {
            get { return _rootFrame; }
        }

        #endregion

        #region Overrides of BootstrapperBase

        /// <summary>
        ///     Initializes the current bootstrapper.
        /// </summary>
        protected override void InitializeInternal()
        {
            var application = CreateApplication();
            var iocContainer = CreateIocContainer();
            application.Initialize(_platform, iocContainer, GetAssemblies().ToArrayEx(), InitializationContext ?? DataContext.Empty);
            FrameStateManager.RegisterFrame(_rootFrame);
            var service = CreateNavigationService(_rootFrame);
            if (service != null)
                iocContainer.BindToConstant(service);
            Should.PropertyNotBeNull(PhoneApplicationService.Current, "PhoneApplicationService.Current");
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Starts the current bootstrapper.
        /// </summary>
        public virtual void Start()
        {
            Initialize();
            var app = MvvmApplication.Current;
            var ctx = new DataContext(app.Context);
            var viewModelType = app.GetStartViewModelType();
            var viewModel = app.IocContainer
               .Get<IViewModelProvider>()
               .GetViewModel(viewModelType, ctx);
            viewModel.ShowAsync((model, result) => model.Dispose(), context: ctx);
        }

        /// <summary>
        ///     Gets the application assemblies.
        /// </summary>
        protected virtual ICollection<Assembly> GetAssemblies()
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
                    if (assembly.IsToolkitAssembly())
                        listAssembly.Add(assembly);
                }
                catch (Exception e)
                {
                    Tracer.Error(e.Flatten(true));
                }
            }
            return listAssembly;
        }

        /// <summary>
        ///     Creates an instance of <see cref="INavigationService" />.
        /// </summary>
        [CanBeNull]
        protected virtual INavigationService CreateNavigationService(PhoneApplicationFrame frame)
        {
            return new FrameNavigationService(frame);
        }

        private void OnLaunching(object sender, LaunchingEventArgs args)
        {
            PhoneApplicationService.Current.Launching -= OnLaunching;
            Start();
        }

        private static bool CanShowViewModelTabPresenter(IViewModel viewModel, IDataContext dataContext, IViewModelPresenter arg3)
        {
            var viewName = viewModel.GetViewName(dataContext);
            var container = viewModel.GetIocContainer(true);
            var mappingProvider = container.Get<IViewMappingProvider>();
            var mappingItem = mappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, false);
            return mappingItem == null || !typeof(Page).IsAssignableFrom(mappingItem.ViewType);
        }

        private static bool CanShowViewModelNavigationPresenter(IViewModel viewModel, IDataContext dataContext, IViewModelPresenter arg3)
        {
            var viewName = viewModel.GetViewName(dataContext);
            var container = viewModel.GetIocContainer(true);
            var mappingProvider = container.Get<IViewMappingProvider>();
            var mappingItem = mappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, false);
            return mappingItem != null && typeof(Page).IsAssignableFrom(mappingItem.ViewType);
        }

        #endregion
    }
}