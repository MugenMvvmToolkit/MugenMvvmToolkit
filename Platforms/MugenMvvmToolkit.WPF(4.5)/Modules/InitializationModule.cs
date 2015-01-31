#region Copyright

// ****************************************************************************
// <copyright file="InitializationModule.cs">
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
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Infrastructure.Navigation;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.IoC;

namespace MugenMvvmToolkit.Modules
{
    /// <summary>
    ///     Represents the class that is used to initialize the IOC adapter.
    /// </summary>
    public class InitializationModule : InitializationModuleBase
    {
        #region Constructors

        static InitializationModule()
        {
            if (ServiceProvider.DesignTimeManager.IsDesignMode)
#if WINDOWS_PHONE && V71
                ServiceProvider.AttachedValueProvider = new WeakReferenceAttachedValueProvider();            
#else
                ServiceProvider.AttachedValueProvider = new AttachedValueProvider();
#endif
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InitializationModule" /> class.
        /// </summary>
        public InitializationModule()
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="InitializationModule" /> class.
        /// </summary>
        protected InitializationModule(LoadMode loadMode = LoadMode.All, int priority = InitializationModulePriority)
            : base(loadMode, priority)
        {
        }

        #endregion

        #region Properties

#if WPF
        /// <summary>
        ///     Enabling subscribe to CommandManager.RequerySuggested event.
        /// </summary>
        public bool UseNativeCommandManager { get; set; }
#endif

        #endregion

        #region Overrides of InitializationModuleBase

        /// <summary>
        ///     Loads the current module.
        /// </summary>
        protected override bool LoadInternal()
        {
            if (base.LoadInternal())
            {
#if WPF
                if (UseNativeCommandManager)
                {
                    ApplicationSettings.AddCanExecuteChangedEvent = (@base, handler) => ServiceProvider
                       .ThreadManager
                       .Invoke(ExecutionMode.AsynchronousOnUiThread, handler, handler, (h1, h2) => System.Windows.Input.CommandManager.RequerySuggested += h1);
                    ApplicationSettings.RemoveCanExecuteChangedEvent = (@base, handler) => ServiceProvider
                        .ThreadManager
                        .Invoke(ExecutionMode.AsynchronousOnUiThread, handler, handler, (h1, h2) => System.Windows.Input.CommandManager.RequerySuggested -= h1);
                }
#elif NETFX_CORE || WINDOWSCOMMON || WINDOWS_PHONE
                IocContainer.BindToBindingInfo(GetApplicationStateManager());
                IApplicationStateManager stateManager;
                if (IocContainer.TryGet(out stateManager))
                    PlatformExtensions.ApplicationStateManager = stateManager;
#endif
                return true;
            }
            return false;
        }

        /// <summary>
        ///     Gets the <see cref="IVisualStateManager" /> which will be used by default.
        /// </summary>
        /// <returns>An instance of <see cref="IVisualStateManager" />.</returns>
        protected override BindingInfo<IVisualStateManager> GetVisualStateManager()
        {
            return BindingInfo<IVisualStateManager>.FromType<VisualStateManager>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IMessagePresenter" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IMessagePresenter" />.</returns>
        protected override BindingInfo<IMessagePresenter> GetMessagePresenter()
        {
            return BindingInfo<IMessagePresenter>.FromType<MessagePresenter>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IAttachedValueProvider" /> that will be used by default.
        /// </summary>
        /// <returns>An instance of <see cref="IAttachedValueProvider" />.</returns>
        protected override BindingInfo<IAttachedValueProvider> GetAttachedValueProvider()
        {
#if WINDOWS_PHONE && V71
            return BindingInfo<IAttachedValueProvider>.FromType<WeakReferenceAttachedValueProvider>(DependencyLifecycle.SingleInstance);
#else
            return BindingInfo<IAttachedValueProvider>.FromType<AttachedValueProvider>(DependencyLifecycle.SingleInstance);
#endif

        }

#if !NETFX_CORE && !WINDOWSCOMMON
        /// <summary>
        ///     Gets the <see cref="IReflectionManager" /> that will be used by default.
        /// </summary>
        /// <returns>An instance of <see cref="IReflectionManager" />.</returns>
        protected override BindingInfo<IReflectionManager> GetReflectionManager()
        {
            return BindingInfo<IReflectionManager>.FromType<ExpressionReflectionManagerEx>(DependencyLifecycle.SingleInstance);
        }
#endif

#if WINDOWS_PHONE || NETFX_CORE || WINDOWSCOMMON
        /// <summary>
        ///     Gets the <see cref="IOperationCallbackFactory" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IOperationCallbackFactory" />.</returns>
        protected override BindingInfo<IOperationCallbackFactory> GetOperationCallbackFactory()
        {
            return BindingInfo<IOperationCallbackFactory>.FromType<SerializableOperationCallbackFactory>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IApplicationStateManager" /> that will be used in all view models by default.
        /// </summary>
        /// <returns>An instance of <see cref="IApplicationStateManager" />.</returns>
        protected virtual BindingInfo<IApplicationStateManager> GetApplicationStateManager()
        {
            return BindingInfo<IApplicationStateManager>.FromType<ApplicationStateManager>(DependencyLifecycle.SingleInstance);
        }
#endif

        /// <summary>
        ///     Gets the <see cref="IViewModelPresenter" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IViewModelPresenter" />.</returns>
        protected override BindingInfo<IViewModelPresenter> GetViewModelPresenter()
        {
            return BindingInfo<IViewModelPresenter>.FromMethod((container, list) =>
            {
                var presenter = new ViewModelPresenter();
                presenter.DynamicPresenters.Add(new DynamicViewModelNavigationPresenter());
#if !WINDOWS_PHONE && !NETFX_CORE
                presenter.DynamicPresenters.Add(
                                    new DynamicViewModelWindowPresenter(container.Get<IViewMappingProvider>(), container.Get<IViewManager>(),
                                        container.Get<IWrapperManager>(), container.Get<IThreadManager>(),
                                        container.Get<IOperationCallbackManager>()));
#endif

                return presenter;
            }, DependencyLifecycle.SingleInstance);
        }

#if WPF
        /// <summary>
        ///     Gets the <see cref="ITracer" /> that will be used by default.
        /// </summary>
        /// <returns>An instance of <see cref="ITracer" />.</returns>
        protected override BindingInfo<ITracer> GetTracer()
        {
            return BindingInfo<ITracer>.FromType<TracerEx>(DependencyLifecycle.SingleInstance);
        }
#endif
        /// <summary>
        ///     Gets the <see cref="IThreadManager" /> which will be used in all view models by default.
        /// </summary>
        /// <returns>An instance of <see cref="IThreadManager" />.</returns>
        protected override BindingInfo<IThreadManager> GetThreadManager()
        {
#if WPF
            return BindingInfo<IThreadManager>.FromMethod((container, list) => new ThreadManager(System.Windows.Threading.Dispatcher.CurrentDispatcher), DependencyLifecycle.SingleInstance);
#elif SILVERLIGHT
            return BindingInfo<IThreadManager>.FromMethod((container, list) => new ThreadManager(System.Windows.Deployment.Current.Dispatcher), DependencyLifecycle.SingleInstance);
#elif NETFX_CORE || WINDOWSCOMMON
            return BindingInfo<IThreadManager>.FromMethod((container, list) => new ThreadManager(Windows.UI.Xaml.Window.Current.Dispatcher), DependencyLifecycle.SingleInstance);
#endif
        }

        /// <summary>
        ///     Gets the <see cref="INavigationProvider" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="INavigationProvider" />.</returns>
        protected override BindingInfo<INavigationProvider> GetNavigationProvider()
        {
            return BindingInfo<INavigationProvider>.FromType<NavigationProvider>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IToastPresenter" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IToastPresenter" />.</returns>
        protected override BindingInfo<IToastPresenter> GetToastPresenter()
        {
            return BindingInfo<IToastPresenter>.FromType<ToastPresenter>(DependencyLifecycle.SingleInstance);
        }

        #endregion
    }
}