#region Copyright

// ****************************************************************************
// <copyright file="InitializationModule.cs">
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
using MugenMvvmToolkit.WPF.Infrastructure.Callbacks;
using MugenMvvmToolkit.WPF.Infrastructure.Navigation;
using MugenMvvmToolkit.WPF.Infrastructure.Presenters;

namespace MugenMvvmToolkit.WPF.Modules
#elif WINDOWS_UWP
using MugenMvvmToolkit.UWP.Infrastructure;
using MugenMvvmToolkit.UWP.Infrastructure.Navigation;
using MugenMvvmToolkit.UWP.Infrastructure.Presenters;
using MugenMvvmToolkit.UWP.Infrastructure.Callbacks;
using MugenMvvmToolkit.UWP.Interfaces;

namespace MugenMvvmToolkit.UWP.Modules
#endif
{
    public class InitializationModule : InitializationModuleBase
    {
        #region Constructors

        public InitializationModule()
        {
        }

        protected InitializationModule(LoadMode loadMode, int priority)
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
#elif WINDOWS_UWP
                IocContainer.BindToBindingInfo(GetApplicationStateManager());
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


#if !WINDOWS_UWP
        protected override BindingInfo<IReflectionManager> GetReflectionManager()
        {
            return BindingInfo<IReflectionManager>.FromType<ExpressionReflectionManagerEx>(DependencyLifecycle.SingleInstance);
        }
#endif
        protected override BindingInfo<IOperationCallbackFactory> GetOperationCallbackFactory()
        {
            return BindingInfo<IOperationCallbackFactory>.FromType<SerializableOperationCallbackFactory>(DependencyLifecycle.SingleInstance);
        }

#if WINDOWS_UWP
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
                presenter.DynamicPresenters.Add(
                                    new DynamicViewModelWindowPresenter(container.Get<IViewMappingProvider>(), container.Get<IViewManager>(),
                                        container.Get<IWrapperManager>(), container.Get<IThreadManager>(),
                                        container.Get<IOperationCallbackManager>()));
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
#elif WINDOWS_UWP
            if (Context.Mode.HasFlagEx(LoadMode.Design) && Windows.UI.Xaml.Window.Current.Dispatcher == null)
                return base.GetThreadManager();
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
