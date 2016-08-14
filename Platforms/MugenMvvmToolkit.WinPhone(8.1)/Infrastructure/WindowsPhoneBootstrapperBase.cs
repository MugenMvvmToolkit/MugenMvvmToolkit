#region Copyright

// ****************************************************************************
// <copyright file="WindowsPhoneBootstrapperBase.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
    public abstract class WindowsPhoneBootstrapperBase : BootstrapperBase
    {
        #region Fields

        protected internal const string BindingAssemblyName = "MugenMvvmToolkit.WinPhone.Binding";
        private readonly PhoneApplicationFrame _rootFrame;
        private readonly PlatformInfo _platform;

        #endregion

        #region Constructors

        static WindowsPhoneBootstrapperBase()
        {
            SetDefaultPlatformValues();
        }

        protected WindowsPhoneBootstrapperBase([NotNull] PhoneApplicationFrame rootFrame, PlatformInfo platform = null)
        {
            Should.NotBeNull(rootFrame, nameof(rootFrame));
            _rootFrame = rootFrame;
            _platform = platform ?? PlatformExtensions.GetPlatformInfo();
            PhoneApplicationService.Current.Launching += OnLaunching;
        }

        #endregion

        #region Properties

        protected PhoneApplicationFrame RootFrame => _rootFrame;

        #endregion

        #region Overrides of BootstrapperBase

        protected override void InitializeInternal()
        {
            var application = CreateApplication();
            var iocContainer = CreateIocContainer();
            var service = CreateNavigationService(_rootFrame);
            if (service != null)
                iocContainer.BindToConstant(service);
            application.Initialize(_platform, iocContainer, GetAssemblies().ToArrayEx(), InitializationContext ?? DataContext.Empty);
            FrameStateManager.RegisterFrame(_rootFrame);
            Should.PropertyNotBeNull(PhoneApplicationService.Current, nameof(PhoneApplicationService) + nameof(PhoneApplicationService.Current));
        }

        #endregion

        #region Methods

        public virtual void Start(IDataContext context = null)
        {
            Initialize();
            MvvmApplication.Current.Start(context);
        }

        protected virtual ICollection<Assembly> GetAssemblies()
        {
            var assemblies = new HashSet<Assembly>();
            foreach (AssemblyPart part in Deployment.Current.Parts)
            {
                string assemblyName = part.Source.Replace(".dll", string.Empty);
                if (assemblyName.Contains("/"))
                    continue;
                try
                {
                    assemblies.Add(Assembly.Load(assemblyName));
                }
                catch (Exception e)
                {
                    Tracer.Error(e.Flatten(true));
                }
            }
            return assemblies;
        }

        [CanBeNull]
        protected virtual INavigationService CreateNavigationService(PhoneApplicationFrame frame)
        {
            return new FrameNavigationService(frame, true);
        }

        private void OnLaunching(object sender, LaunchingEventArgs args)
        {
            PhoneApplicationService.Current.Launching -= OnLaunching;
            Start();
        }

        internal static void SetDefaultPlatformValues()
        {
            DynamicMultiViewModelPresenter.CanShowViewModelDefault = CanShowViewModelTabPresenter;
            DynamicViewModelNavigationPresenter.CanShowViewModelDefault = CanShowViewModelNavigationPresenter;
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
