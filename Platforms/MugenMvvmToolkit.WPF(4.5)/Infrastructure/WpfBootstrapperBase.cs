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
        protected WpfBootstrapperBase([NotNull] Application application, bool autoStart = true)
            : base(PlatformExtensions.GetPlatformInfo())
        {
            Should.NotBeNull(application, "application");
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
        ///     Gets the application assemblies.
        /// </summary>
        protected override ICollection<Assembly> GetAssemblies()
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

        #endregion

        #region Methods

        /// <summary>
        ///     Starts the current bootstrapper.
        /// </summary>
        public virtual void Start()
        {
            InitializationContext = new DataContext(InitializationContext);
            if (!InitializationContext.Contains(NavigationConstants.IsDialog))
                InitializationContext.Add(NavigationConstants.IsDialog, false);
            Initialize();
            Type viewModelType = GetMainViewModelType();
            NavigationWindow rootWindow = null;
            var mappingProvider = IocContainer.Get<IViewMappingProvider>();
            IViewMappingItem mapping = mappingProvider.FindMappingForViewModel(viewModelType, InitializationContext.GetData(NavigationConstants.ViewName), true);
            if (typeof(Page).IsAssignableFrom(mapping.ViewType))
            {
                rootWindow = CreateNavigationWindow();
                var service = CreateNavigationService(rootWindow);
                IocContainer.BindToConstant(service);
            }
            var vm = CreateMainViewModel(viewModelType);
            vm.ShowAsync((model, result) =>
            {
                model.Dispose();
                if (ShutdownOnMainViewModelClose)
                {
                    Application app = Application.Current;
                    if (app != null)
                    {
                        Action action = app.Shutdown;
                        app.Dispatcher.BeginInvoke(action);
                    }
                }
            }, context: new DataContext(InitializationContext));
            if (rootWindow != null)
            {
                IWindowViewMediator mediator = new WindowViewMediator(rootWindow, vm, IocContainer.Get<IThreadManager>(),
                    IocContainer.Get<IViewManager>(), IocContainer.Get<IWrapperManager>(),
                    IocContainer.Get<IOperationCallbackManager>());
                mediator.UpdateView(new PlatformWrapperRegistrationModule.WindowViewWrapper(rootWindow), true, new DataContext(InitializationContext));
                rootWindow.Show();
            }
        }

        /// <summary>
        ///     Creates the main view model.
        /// </summary>
        [NotNull]
        protected virtual IViewModel CreateMainViewModel([NotNull] Type viewModelType)
        {
            return IocContainer
                .Get<IViewModelProvider>()
                .GetViewModel(viewModelType, new DataContext(InitializationContext));
        }

        /// <summary>
        ///     Gets the type of main view model.
        /// </summary>
        [NotNull]
        protected abstract Type GetMainViewModelType();

        /// <summary>
        ///     Creates an instance of <see cref="INavigationService" />.
        /// </summary>
        [NotNull]
        protected virtual INavigationService CreateNavigationService(NavigationWindow window)
        {
            return UseUriNavigation
                ? new WindowNavigationService(window)
                : new WindowNavigationService(window, type => IocContainer.Get(type));
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