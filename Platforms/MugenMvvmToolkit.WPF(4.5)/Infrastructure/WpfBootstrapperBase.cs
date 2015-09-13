#region Copyright

// ****************************************************************************
// <copyright file="WpfBootstrapperBase.cs">
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
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Mediators;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;
using MugenMvvmToolkit.WPF.Infrastructure.Mediators;
using MugenMvvmToolkit.WPF.Infrastructure.Navigation;
using MugenMvvmToolkit.WPF.Interfaces.Navigation;
using MugenMvvmToolkit.WPF.Modules;

namespace MugenMvvmToolkit.WPF.Infrastructure
{
    /// <summary>
    ///     Represents the base class that is used to start MVVM application.
    /// </summary>
    public abstract class WpfBootstrapperBase : BootstrapperBase
    {
        #region Fields

        /// <summary>
        /// Gets the name of binding assembly.
        /// </summary>
        protected const string BindingAssemblyName = "MugenMvvmToolkit.WPF.Binding";
        private readonly PlatformInfo _platform;

        #endregion

        #region Constructors

        static WpfBootstrapperBase()
        {
            ReflectionExtensions.GetTypesDefault = assembly => assembly.GetTypes();
            DynamicMultiViewModelPresenter.CanShowViewModelDefault = CanShowViewModelTabPresenter;
            DynamicViewModelNavigationPresenter.CanShowViewModelDefault = CanShowViewModelNavigationPresenter;
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="WpfBootstrapperBase" /> class.
        /// </summary>
        protected WpfBootstrapperBase([NotNull] Application application, bool autoStart = true, PlatformInfo platform = null)
        {
            Should.NotBeNull(application, "application");
            _platform = platform ?? PlatformExtensions.GetPlatformInfo();
            application.Startup += ApplicationOnStartup;
            AutoStart = autoStart;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Indicates that bootstrapper should call the Start method when Application.Startup is raised.
        /// </summary>
        public bool AutoStart { get; set; }

        /// <summary>
        ///     Indicates that the MainWindow should use only Uri navigation.
        /// </summary>
        public bool UseUriNavigation { get; set; }

        /// <summary>
        ///     An application shuts down when either the main view model closes, or Application.Shutdown() is called.
        /// </summary>
        public bool ShutdownOnMainViewModelClose { get; set; }

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
            var iocContainer = app.IocContainer;
            var ctx = new DataContext(app.Context);
            if (!ctx.Contains(NavigationConstants.IsDialog))
                ctx.Add(NavigationConstants.IsDialog, false);
            var viewModelType = app.GetStartViewModelType();

            NavigationWindow rootWindow = null;
            var mappingProvider = iocContainer.Get<IViewMappingProvider>();
            IViewMappingItem mapping = mappingProvider.FindMappingForViewModel(viewModelType, ctx.GetData(NavigationConstants.ViewName), true);
            if (typeof(Page).IsAssignableFrom(mapping.ViewType))
            {
                rootWindow = CreateNavigationWindow();
                var service = CreateNavigationService(rootWindow);
                iocContainer.BindToConstant(service);
            }
            var vm = iocContainer
               .Get<IViewModelProvider>()
               .GetViewModel(viewModelType, ctx);
            vm.ShowAsync((model, result) =>
            {
                model.Dispose();
                if (ShutdownOnMainViewModelClose)
                {
                    Application application = Application.Current;
                    if (application != null)
                    {
                        Action action = application.Shutdown;
                        application.Dispatcher.BeginInvoke(action);
                    }
                }
            }, context: ctx);
            if (rootWindow != null)
            {
                IWindowViewMediator mediator = new WindowViewMediator(rootWindow, vm, iocContainer.Get<IThreadManager>(),
                    iocContainer.Get<IViewManager>(), iocContainer.Get<IWrapperManager>(),
                    iocContainer.Get<IOperationCallbackManager>());
                mediator.UpdateView(new PlatformWrapperRegistrationModule.WindowViewWrapper(rootWindow), true, ctx);
                rootWindow.Show();
            }
        }

        /// <summary>
        ///     Gets the application assemblies.
        /// </summary>
        protected virtual ICollection<Assembly> GetAssemblies()
        {
            var assemblies = new HashSet<Assembly>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies().SkipFrameworkAssemblies())
            {
                if (assemblies.Add(assembly))
                    assemblies.AddRange(assembly.GetReferencedAssemblies().Select(Assembly.Load).SkipFrameworkAssemblies());
            }
            TryLoadAssembly(BindingAssemblyName, assemblies);
            return assemblies;
        }

        /// <summary>
        ///     Creates an instance of <see cref="INavigationService" />.
        /// </summary>
        [NotNull]
        protected virtual INavigationService CreateNavigationService(NavigationWindow window)
        {
            return UseUriNavigation
                ? new WindowNavigationService(window)
                : new WindowNavigationService(window, type => MvvmApplication.Current.IocContainer.Get(type));
        }

        /// <summary>
        ///     Creates an instance of <see cref="NavigationWindow" />, if need.
        /// </summary>
        [NotNull]
        protected virtual NavigationWindow CreateNavigationWindow()
        {
            return new NavigationWindow();
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