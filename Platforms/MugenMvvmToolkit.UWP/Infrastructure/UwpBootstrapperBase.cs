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

using System.Collections.Generic;
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using JetBrains.Annotations;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;
using MugenMvvmToolkit.UWP.Infrastructure.Navigation;
using MugenMvvmToolkit.UWP.Interfaces.Navigation;

namespace MugenMvvmToolkit.UWP.Infrastructure
{
    public abstract class UwpBootstrapperBase : BootstrapperBase
    {
        #region Fields

        private readonly Frame _rootFrame;
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

        protected UwpBootstrapperBase([CanBeNull] Frame rootFrame, PlatformInfo platform = null)
            : this(false, platform)
        {
            _rootFrame = rootFrame;
            _platform = platform ?? UwpToolkitExtensions.GetPlatformInfo();
        }

        #endregion

        #region Properties

        protected Frame RootFrame => _rootFrame;

        #endregion

        #region Overrides of BootstrapperBase

        protected override PlatformInfo Platform => _platform;

        protected override void InitializeInternal()
        {
            base.InitializeInternal();
            var service = CreateNavigationService(_rootFrame);
            if (service != null)
                IocContainer.BindToConstant(service);
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

        [CanBeNull]
        protected virtual INavigationService CreateNavigationService(Frame frame)
        {
            if (frame == null)
                return null;
            return new FrameNavigationService(frame, IocContainer.Get<IViewModelProvider>());
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
