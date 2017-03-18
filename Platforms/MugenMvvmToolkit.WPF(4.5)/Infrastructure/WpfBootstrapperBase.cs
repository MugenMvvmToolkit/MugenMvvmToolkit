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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Mediators;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
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
    public abstract class WpfBootstrapperBase : BootstrapperBase, IDynamicViewModelPresenter
    {
        #region Fields

        protected internal const string BindingAssemblyName = "MugenMvvmToolkit.WPF.Binding";
        private readonly PlatformInfo _platform;
        private NavigationWindow _rootWindow;

        #endregion

        #region Constructors

        static WpfBootstrapperBase()
        {
            SetDefaultPlatformValues();
        }

        protected WpfBootstrapperBase([NotNull] Application application, bool autoStart = true, PlatformInfo platform = null)
        {
            Should.NotBeNull(application, nameof(application));
            _platform = platform ?? PlatformExtensions.GetPlatformInfo();
            application.Startup += ApplicationOnStartup;
            AutoStart = autoStart;
        }

        #endregion

        #region Properties

        public bool AutoStart { get; set; }

        public bool UseUriNavigation { get; set; }

        public bool ShutdownOnMainViewModelClose { get; set; }

        #endregion

        #region Overrides of BootstrapperBase

        protected override PlatformInfo Platform => _platform;

        protected override IList<Assembly> GetAssemblies()
        {
            var assemblies = new HashSet<Assembly>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic))
            {
                if (assemblies.Add(assembly))
                    assemblies.AddRange(assembly.GetReferencedAssemblies().Select(Assembly.Load));
            }
            TryLoadAssembly(BindingAssemblyName, assemblies);
            return assemblies.ToArrayEx();
        }

        #endregion

        #region Implementation of IDynamicViewModelPresenter

        int IDynamicViewModelPresenter.Priority => int.MaxValue;

        IAsyncOperation IDynamicViewModelPresenter.TryShowAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            parentPresenter.DynamicPresenters.Remove(this);
            var operation = parentPresenter.ShowAsync(context);
            if (ShutdownOnMainViewModelClose)
            {
                operation.ContinueWith(result =>
                {
                    Application application = Application.Current;
                    if (application != null)
                    {
                        Action action = application.Shutdown;
                        application.Dispatcher.BeginInvoke(action);
                    }
                });
            }
            if (_rootWindow != null)
            {
                var viewModel = context.GetData(NavigationConstants.ViewModel);
                if (viewModel == null)
                    return null;
                var iocContainer = ServiceProvider.IocContainer;
                IWindowViewMediator mediator = new WindowViewMediator(_rootWindow, iocContainer.Get<IThreadManager>(),
                    iocContainer.Get<IViewManager>(), iocContainer.Get<IWrapperManager>(), iocContainer.Get<INavigationDispatcher>(), iocContainer.Get<IEventAggregator>());
                mediator.Initialize(viewModel, context);
                mediator.UpdateView(new WpfWrapperRegistrationModule.WindowViewWrapper(_rootWindow), true, context);
                _rootWindow.Show();
            }
            return operation;
        }

        Task<bool> IDynamicViewModelPresenter.TryCloseAsync(IDataContext context, IViewModelPresenter parentPresenter)
        {
            return null;
        }

        #endregion

        #region Methods

        public virtual void Start()
        {
            Initialize();
            var app = ServiceProvider.Application;
            if (!app.Context.Contains(NavigationConstants.IsDialog))
                app.Context.Add(NavigationConstants.IsDialog, false);
            var viewModelType = app.GetStartViewModelType();

            var mappingProvider = app.IocContainer.Get<IViewMappingProvider>();
            IViewMappingItem mapping = mappingProvider.FindMappingForViewModel(viewModelType, app.Context.GetData(NavigationConstants.ViewName), true);
            if (typeof(Page).IsAssignableFrom(mapping.ViewType))
            {
                _rootWindow = CreateNavigationWindow();
                var service = CreateNavigationService(_rootWindow);
                app.IocContainer.BindToConstant(service);
            }
            app.IocContainer.Get<IViewModelPresenter>().DynamicPresenters.Add(this);
            app.Start();
        }

        [NotNull]
        protected virtual INavigationService CreateNavigationService(NavigationWindow window)
        {
            return new WindowNavigationService(window, UseUriNavigation);
        }

        [NotNull]
        protected virtual NavigationWindow CreateNavigationWindow()
        {
            return new NavigationWindow();
        }

        internal static void SetDefaultPlatformValues()
        {
            ReflectionExtensions.GetTypesDefault = assembly => assembly.GetTypes();
            ApplicationSettings.MultiViewModelPresenterCanShowViewModel = CanShowViewModelTabPresenter;
            ApplicationSettings.NavigationPresenterCanShowViewModel = CanShowViewModelNavigationPresenter;
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
