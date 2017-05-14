#region Copyright

// ****************************************************************************
// <copyright file="WpfBootstrapperBase.cs">
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
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;
using MugenMvvmToolkit.WPF.Infrastructure.Presenters;

namespace MugenMvvmToolkit.WPF.Infrastructure
{
    public abstract class WpfBootstrapperBase : BootstrapperBase
    {
        #region Constructors

        static WpfBootstrapperBase()
        {
            ReflectionExtensions.GetTypesDefault = assembly => assembly.GetTypes();
            ApplicationSettings.MultiViewModelPresenterCanShowViewModel = CanShowViewModelTabPresenter;
        }

        internal WpfBootstrapperBase(bool isDesignMode, PlatformInfo platform = null)
            : base(isDesignMode)
        {
            Platform = platform ?? WpfToolkitExtensions.GetPlatformInfo();
        }

        protected WpfBootstrapperBase([NotNull] Application application, bool autoStart = true, PlatformInfo platform = null)
            : this(false, platform)
        {
            Should.NotBeNull(application, nameof(application));
            application.Startup += ApplicationOnStartup;
            AutoStart = autoStart;
        }

        #endregion

        #region Properties

        public bool AutoStart { get; set; }

        public bool ShutdownOnMainViewModelClose { get; set; }

        public Func<IIocContainer, IDynamicViewModelPresenter> RootPresenterFactory { get; set; }

        protected override PlatformInfo Platform { get; }

        #endregion

        #region Methods

        public virtual void Start()
        {
            Initialize();
            if (!MvvmApplication.Context.Contains(NavigationConstants.IsDialog))
                MvvmApplication.Context.Add(NavigationConstants.IsDialog, false);
            var rootPresenter = GetRootPresenter();
            if (rootPresenter != null)
                IocContainer.Get<IViewModelPresenter>().DynamicPresenters.Add(rootPresenter);
            MvvmApplication.Start();
        }

        protected virtual IDynamicViewModelPresenter GetRootPresenter()
        {
            if (RootPresenterFactory != null)
                return RootPresenterFactory(IocContainer);
            return new WpfRootDynamicViewModelPresenter {ShutdownOnMainViewModelClose = ShutdownOnMainViewModelClose};
        }

        private void ApplicationOnStartup(object sender, StartupEventArgs args)
        {
            var application = sender as Application;
            if (application != null)
                application.Startup -= ApplicationOnStartup;
            if (AutoStart)
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

        protected override void UpdateAssemblies(HashSet<Assembly> assemblies)
        {
            base.UpdateAssemblies(assemblies);
            try
            {
                assemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic));
            }
            catch
            {
                if (!IsDesignMode)
                    throw;
            }
            TryLoadAssemblyByType("AttachedMembers", "MugenMvvmToolkit.WPF.Binding", assemblies);
        }

        #endregion
    }
}