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
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.IoC;
using MugenMvvmToolkit.Modules;
#if WPF
using MugenMvvmToolkit.WPF.Infrastructure;
using MugenMvvmToolkit.WPF.Infrastructure.Navigation;
using MugenMvvmToolkit.WPF.Infrastructure.Presenters;

namespace MugenMvvmToolkit.WPF.Modules
#elif SILVERLIGHT
using MugenMvvmToolkit.Silverlight.Infrastructure;
using MugenMvvmToolkit.Silverlight.Infrastructure.Navigation;
using MugenMvvmToolkit.Silverlight.Infrastructure.Presenters;

namespace MugenMvvmToolkit.Silverlight.Modules
#elif WINDOWSCOMMON
using MugenMvvmToolkit.WinRT.Infrastructure;
using MugenMvvmToolkit.WinRT.Infrastructure.Navigation;
using MugenMvvmToolkit.WinRT.Infrastructure.Presenters;
using MugenMvvmToolkit.WinRT.Infrastructure.Callbacks;
using MugenMvvmToolkit.WinRT.Interfaces;

namespace MugenMvvmToolkit.WinRT.Modules
#elif WINDOWS_PHONE
using MugenMvvmToolkit.WinPhone.Infrastructure;
using MugenMvvmToolkit.WinPhone.Infrastructure.Navigation;
using MugenMvvmToolkit.WinPhone.Infrastructure.Presenters;
using MugenMvvmToolkit.WinPhone.Infrastructure.Callbacks;
using MugenMvvmToolkit.WinPhone.Interfaces;

namespace MugenMvvmToolkit.WinPhone.Modules
#endif
{
    public class InitializationModule : InitializationModuleBase
    {
        #region Constructors

        public InitializationModule()
        {
        }

        protected InitializationModule(LoadMode loadMode = LoadMode.All, int priority = InitializationModulePriority)
            : base(loadMode, priority)
        {
        }

        #endregion

        #region Properties

#if WPF
        public bool UseNativeCommandManager { get; set; }
#endif

        #endregion

        #region Overrides of InitializationModuleBase

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
#elif WINDOWSCOMMON || WINDOWS_PHONE
                IocContainer.BindToBindingInfo(GetApplicationStateManager());
                IApplicationStateManager stateManager;
                if (IocContainer.TryGet(out stateManager))
                    PlatformExtensions.ApplicationStateManager = stateManager;
#endif
                return true;
            }
            return false;
        }

        protected override BindingInfo<IMessagePresenter> GetMessagePresenter()
        {
#if WPF
            if (Context.Platform.Platform != PlatformType.WPF)
                return BindingInfo<IMessagePresenter>.Empty;
#endif
            return BindingInfo<IMessagePresenter>.FromType<MessagePresenter>(DependencyLifecycle.SingleInstance);
        }

#if !NET4
        protected override BindingInfo<IAttachedValueProvider> GetAttachedValueProvider()
        {
            return BindingInfo<IAttachedValueProvider>.FromType<AttachedValueProvider>(DependencyLifecycle.SingleInstance);
        }
#endif


#if !WINDOWSCOMMON
        protected override BindingInfo<IReflectionManager> GetReflectionManager()
        {
            return BindingInfo<IReflectionManager>.FromType<ExpressionReflectionManagerEx>(DependencyLifecycle.SingleInstance);
        }
#endif

#if WINDOWS_PHONE || WINDOWSCOMMON
        protected override BindingInfo<IOperationCallbackFactory> GetOperationCallbackFactory()
        {
            return BindingInfo<IOperationCallbackFactory>.FromType<SerializableOperationCallbackFactory>(DependencyLifecycle.SingleInstance);
        }

        protected virtual BindingInfo<IApplicationStateManager> GetApplicationStateManager()
        {
            return BindingInfo<IApplicationStateManager>.FromType<ApplicationStateManager>(DependencyLifecycle.SingleInstance);
        }
#endif

        protected override BindingInfo<IViewModelPresenter> GetViewModelPresenter()
        {
#if WPF
            if (Context.Platform.Platform != PlatformType.WPF)
            {
                MvvmApplication.Initialized += MvvmApplicationOnInitialized;
                return BindingInfo<IViewModelPresenter>.Empty;
            }
#endif
            return BindingInfo<IViewModelPresenter>.FromMethod((container, list) =>
            {
                var presenter = new ViewModelPresenter();
                presenter.DynamicPresenters.Add(new DynamicViewModelNavigationPresenter());
#if !WINDOWS_PHONE
                presenter.DynamicPresenters.Add(
                                    new DynamicViewModelWindowPresenter(container.Get<IViewMappingProvider>(), container.Get<IViewManager>(),
                                        container.Get<IWrapperManager>(), container.Get<IThreadManager>(),
                                        container.Get<IOperationCallbackManager>()));
#endif

                return presenter;
            }, DependencyLifecycle.SingleInstance);
        }

#if WPF
        protected override BindingInfo<ITracer> GetTracer()
        {
            return BindingInfo<ITracer>.FromType<TracerEx>(DependencyLifecycle.SingleInstance);
        }
#endif
        protected override BindingInfo<IThreadManager> GetThreadManager()
        {
#if WPF
            if (Context.Platform.Platform != PlatformType.WPF)
                return BindingInfo<IThreadManager>.Empty;
            return BindingInfo<IThreadManager>.FromMethod((container, list) => new ThreadManager(System.Windows.Threading.Dispatcher.CurrentDispatcher), DependencyLifecycle.SingleInstance);
#elif SILVERLIGHT || WINDOWS_PHONE
            return BindingInfo<IThreadManager>.FromMethod((container, list) => new ThreadManager(System.Windows.Deployment.Current.Dispatcher), DependencyLifecycle.SingleInstance);
#elif WINDOWSCOMMON
            return BindingInfo<IThreadManager>.FromMethod((container, list) => new ThreadManager(Windows.UI.Xaml.Window.Current.Dispatcher), DependencyLifecycle.SingleInstance);
#endif
        }

        protected override BindingInfo<INavigationProvider> GetNavigationProvider()
        {
            return BindingInfo<INavigationProvider>.FromType<NavigationProvider>(DependencyLifecycle.SingleInstance);
        }

        protected override BindingInfo<IToastPresenter> GetToastPresenter()
        {
#if WPF
            if (Context.Platform.Platform != PlatformType.WPF)
                return BindingInfo<IToastPresenter>.Empty;
#endif
            return BindingInfo<IToastPresenter>.FromType<ToastPresenter>(DependencyLifecycle.SingleInstance);
        }

        #endregion

        #region Methods
#if WPF
        private static void MvvmApplicationOnInitialized(object sender, EventArgs eventArgs)
        {
            MvvmApplication.Initialized -= MvvmApplicationOnInitialized;
            IViewModelPresenter presenter;
            if (ServiceProvider.TryGet(out presenter))
            {
                presenter.DynamicPresenters.Add(new DynamicViewModelNavigationPresenter());
                presenter.DynamicPresenters.Add(ServiceProvider.Get<DynamicViewModelWindowPresenter>());
            }
        }
#endif
        #endregion

    }
}
