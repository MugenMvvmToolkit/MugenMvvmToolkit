#region Copyright
// ****************************************************************************
// <copyright file="WpfBootstrapperBase.cs">
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
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure.Mediators;
using MugenMvvmToolkit.Infrastructure.Navigation;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Mediators;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Infrastructure
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
        protected const string BindingAssemblyName = "MugenMvvmToolkit.Binding.WPF";
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
        protected WpfBootstrapperBase([NotNull] Application application, bool autoStart = true)
        {
            Should.NotBeNull(application, "application");
            if (autoStart)
                application.Startup += ApplicationOnStartup;
            _platform = PlatformExtensions.GetPlatformInfo();
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Indicates that the MainWindow should use only Uri navigation.
        /// </summary>
        protected bool UseUriNavigation { get; set; }

        /// <summary>
        /// An application shuts down when either the main view model closes, or Application.Shutdown() is called.
        /// </summary>
        public bool ShutdownOnMainViewModelClose { get; set; }

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
        public virtual void Start(IDataContext context = null)
        {
            context = context.ToNonReadOnly();
            context.AddOrUpdate(NavigationConstants.IsDialog, false);
            Initialize();
            Type viewModelType = GetMainViewModelType();
            NavigationWindow rootWindow = null;
            var mappingProvider = IocContainer.Get<IViewMappingProvider>();
            IViewMappingItem mapping = mappingProvider.FindMappingForViewModel(viewModelType, context.GetData(NavigationConstants.ViewName), true);
            if (typeof(Page).IsAssignableFrom(mapping.ViewType))
            {
                rootWindow = CreateNavigationWindow();
                var service = CreateNavigationService(rootWindow);
                IocContainer.BindToConstant(service);
            }
            var vm = CreateMainViewModel(viewModelType, context);
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
            }, context: context);
            if (rootWindow != null)
            {
                IWindowViewMediator mediator = new WindowViewMediator(rootWindow, vm, IocContainer.Get<IThreadManager>(),
                    IocContainer.Get<IViewManager>(), IocContainer.Get<IOperationCallbackManager>());
                mediator.UpdateView(new ViewManagerEx.WindowView(rootWindow), true, context);
                rootWindow.Show();
            }
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