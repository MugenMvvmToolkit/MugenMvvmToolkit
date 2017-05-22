#region Copyright

// ****************************************************************************
// <copyright file="UwpBootstrapperBase.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using Windows.ApplicationModel;
using Windows.Foundation.Metadata;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.UWP.Infrastructure.Navigation;
using MugenMvvmToolkit.ViewModels;
using MugenMvvmToolkit.UWP.Interfaces.Navigation;

namespace MugenMvvmToolkit.UWP.Infrastructure
{
    public abstract class UwpBootstrapperBase : BootstrapperBase
    {
        #region Fields

        private readonly PlatformInfo _platform;

        #endregion

        #region Constructors

        static UwpBootstrapperBase()
        {
            ApplicationSettings.MultiViewModelPresenterCanShowViewModel = CanShowViewModelTabPresenter;
            ApplicationSettings.NavigationPresenterCanShowViewModel = CanShowViewModelNavigationPresenter;
        }

        protected UwpBootstrapperBase(bool isDesignMode, PlatformInfo platform = null) : base(isDesignMode)
        {
            _platform = platform ?? UwpToolkitExtensions.GetPlatformInfo();
        }

        protected UwpBootstrapperBase(Frame frame = null, PlatformInfo platform = null)
            : this(false, platform)
        {
            Frame = frame;
            _platform = platform ?? UwpToolkitExtensions.GetPlatformInfo();
        }

        #endregion

        #region Properties

        public Func<IIocContainer, IDynamicViewModelPresenter> RootPresenterFactory { get; set; }

        public Frame Frame { get; set; }

        #endregion

        #region Overrides of BootstrapperBase

        protected override PlatformInfo Platform => _platform;

        protected override void InitializeInternal()
        {
            base.InitializeInternal();

            if (Frame != null)
            {
                var service = CreateNavigationService(Frame);
                if (service != null)
                    IocContainer.BindToConstant(service);
            }

            var rootPresenter = GetRootPresenter();
            if (rootPresenter != null)
                IocContainer.Get<IViewModelPresenter>().DynamicPresenters.Add(rootPresenter);

            var application = Application.Current;
            if (application != null)
            {
                if (ApiInformation.IsEventPresent("Windows.UI.Xaml.Application", nameof(Application.EnteredBackground)))
                    application.EnteredBackground += OnEnteredBackground;
                if (ApiInformation.IsEventPresent("Windows.UI.Xaml.Application", nameof(Application.LeavingBackground)))
                    application.LeavingBackground += OnLeavingBackground;
            }
        }

        protected override void UpdateAssemblies(HashSet<Assembly> assemblies)
        {
            base.UpdateAssemblies(assemblies);
            assemblies.Add(typeof(UwpBootstrapperBase).GetAssembly());
            var application = Application.Current;
            if (application != null)
                assemblies.Add(application.GetType().GetAssembly());
            TryLoadAssemblyByType("AttachedMembers", "MugenMvvmToolkit.UWP.Binding", assemblies);
        }

        #endregion

        #region Methods

        public virtual void Start()
        {
            Initialize();
            MvvmApplication.Start();
        }

        protected virtual IDynamicViewModelPresenter GetRootPresenter()
        {
            return RootPresenterFactory?.Invoke(IocContainer);
        }

        protected virtual INavigationService CreateNavigationService(Frame frame)
        {
            return new FrameNavigationService(frame, ServiceProvider.Get<IViewModelProvider>());
        }

        private static void OnLeavingBackground(object sender, LeavingBackgroundEventArgs leavingBackgroundEventArgs)
        {
            ServiceProvider.Application?.SetApplicationState(ApplicationState.Active, null);
        }

        private static void OnEnteredBackground(object sender, EnteredBackgroundEventArgs enteredBackgroundEventArgs)
        {
            ServiceProvider.Application?.SetApplicationState(ApplicationState.Background, null);
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
