#region Copyright

// ****************************************************************************
// <copyright file="WpfBootstrapperBase.cs">
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
    public abstract class WpfBootstrapperBase : BootstrapperBase, IDynamicViewModelPresenter
    {
        #region Fields

        protected const string BindingAssemblyName = "MugenMvvmToolkit.WPF.Binding";
        private readonly PlatformInfo _platform;
        private NavigationWindow _rootWindow;

        #endregion

        #region Constructors

        static WpfBootstrapperBase()
        {
            ReflectionExtensions.GetTypesDefault = assembly => assembly.GetTypes();
            DynamicMultiViewModelPresenter.CanShowViewModelDefault = CanShowViewModelTabPresenter;
            DynamicViewModelNavigationPresenter.CanShowViewModelDefault = CanShowViewModelNavigationPresenter;
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

        protected override void InitializeInternal()
        {
            var application = CreateApplication();
            var iocContainer = CreateIocContainer();
            application.Initialize(_platform, iocContainer, GetAssemblies().ToArrayEx(), InitializationContext ?? DataContext.Empty);
        }

        #endregion

        #region Implementation of IDynamicViewModelPresenter

        int IDynamicViewModelPresenter.Priority => int.MaxValue;

        INavigationOperation IDynamicViewModelPresenter.TryShowAsync(IViewModel viewModel, IDataContext context, IViewModelPresenter parentPresenter)
        {
            parentPresenter.DynamicPresenters.Remove(this);
            var operation = parentPresenter.ShowAsync(viewModel, context);
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
                var iocContainer = MvvmApplication.Current.IocContainer;
                IWindowViewMediator mediator = new WindowViewMediator(_rootWindow, viewModel, iocContainer.Get<IThreadManager>(),
                    iocContainer.Get<IViewManager>(), iocContainer.Get<IWrapperManager>(),
                    iocContainer.Get<IOperationCallbackManager>());
                mediator.UpdateView(new PlatformWrapperRegistrationModule.WindowViewWrapper(_rootWindow), true, context);
                _rootWindow.Show();
            }
            return operation;
        }

        #endregion

        #region Methods

        public virtual void Start(IDataContext context = null)
        {
            Initialize();
            context = context.ToNonReadOnly();
            if (!context.Contains(NavigationConstants.IsDialog))
                context.Add(NavigationConstants.IsDialog, false);
            var app = MvvmApplication.Current;
            var viewModelType = app.GetStartViewModelType();

            var mappingProvider = app.IocContainer.Get<IViewMappingProvider>();
            IViewMappingItem mapping = mappingProvider.FindMappingForViewModel(viewModelType, context.GetData(NavigationConstants.ViewName), true);
            if (typeof(Page).IsAssignableFrom(mapping.ViewType))
            {
                _rootWindow = CreateNavigationWindow();
                var service = CreateNavigationService(_rootWindow);
                app.IocContainer.BindToConstant(service);
            }
            app.IocContainer.Get<IViewModelPresenter>().DynamicPresenters.Add(this);
            app.Start(context);
        }

        protected virtual ICollection<Assembly> GetAssemblies()
        {
            var assemblies = new HashSet<Assembly>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies().Where(x=>!x.IsDynamic))
            {
                if (assemblies.Add(assembly))
                    assemblies.AddRange(assembly.GetReferencedAssemblies().Select(Assembly.Load));
            }
            TryLoadAssembly(BindingAssemblyName, assemblies);
            return assemblies;
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
