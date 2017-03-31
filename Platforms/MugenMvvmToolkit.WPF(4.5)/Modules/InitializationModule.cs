#region Copyright

// ****************************************************************************
// <copyright file="InitializationModule.cs">
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

using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.IoC;
using MugenMvvmToolkit.Modules;
#if WPF
using System;
using System.Linq;
using MugenMvvmToolkit.WPF.Infrastructure;
using MugenMvvmToolkit.WPF.Infrastructure.Callbacks;
using MugenMvvmToolkit.WPF.Infrastructure.Navigation;
using MugenMvvmToolkit.WPF.Infrastructure.Presenters;
using MugenMvvmToolkit.WPF.Infrastructure.Mediators;
using MugenMvvmToolkit.WPF.Interfaces.Navigation;
using MugenMvvmToolkit.WPF.Interfaces.Views;

namespace MugenMvvmToolkit.WPF.Modules
{
    public class WpfInitializationModule : InitializationModuleBase
    {
#elif WINDOWS_UWP
using MugenMvvmToolkit.UWP.Infrastructure;
using MugenMvvmToolkit.UWP.Infrastructure.Navigation;
using MugenMvvmToolkit.UWP.Infrastructure.Presenters;
using MugenMvvmToolkit.UWP.Infrastructure.Callbacks;
using MugenMvvmToolkit.UWP.Infrastructure.Mediators;
using MugenMvvmToolkit.UWP.Interfaces;
using MugenMvvmToolkit.UWP.Interfaces.Navigation;
using MugenMvvmToolkit.UWP.Interfaces.Views;

namespace MugenMvvmToolkit.UWP.Modules
{
    public class UwpInitializationModule : InitializationModuleBase
    {
#endif

        #region Properties

#if WPF
        public bool UseNativeCommandManager { get; set; }
#endif

        #endregion

        public override bool Load(IModuleContext context)
        {
            if (base.Load(context))
            {
#if WPF
                if (UseNativeCommandManager)
                {
                    ApplicationSettings.CommandAddCanExecuteChangedEvent = (@base, handler) => ServiceProvider
                       .ThreadManager
                       .Invoke(ExecutionMode.AsynchronousOnUiThread, handler, handler, (h1, h2) => System.Windows.Input.CommandManager.RequerySuggested += h1);
                    ApplicationSettings.CommandRemoveCanExecuteChangedEvent = (@base, handler) => ServiceProvider
                        .ThreadManager
                        .Invoke(ExecutionMode.AsynchronousOnUiThread, handler, handler, (h1, h2) => System.Windows.Input.CommandManager.RequerySuggested -= h1);
                }
#elif WINDOWS_UWP
                BindApplicationStateManager(context, context.IocContainer);
#endif           
                BindNavigationCachePolicy(context, context.IocContainer);
                return true;
            }
            return false;
        }

        protected override void BindMessagePresenter(IModuleContext context, IIocContainer container)
        {
#if WPF
            if (context.PlatformInfo.Platform != PlatformType.WPF)
                return;
#endif
            container.Bind<IMessagePresenter, MessagePresenter>(DependencyLifecycle.SingleInstance);
        }

#if !NET4
        protected override void BindAttachedValueProvider(IModuleContext context, IIocContainer container)
        {
            IAttachedValueProvider attachedValueProvider = new AttachedValueProvider();
            ServiceProvider.AttachedValueProvider = attachedValueProvider;
            container.BindToConstant(attachedValueProvider);
        }
#endif

#if WINDOWS_UWP
        protected virtual void BindApplicationStateManager(IModuleContext context, IIocContainer container)
        {
            container.Bind<IApplicationStateManager, ApplicationStateManager>(DependencyLifecycle.SingleInstance);
        }
#else
        protected override void BindReflectionManager(IModuleContext context, IIocContainer container)
        {
            IReflectionManager reflectionManager = new ExpressionReflectionManagerEx();
            ServiceProvider.ReflectionManager = reflectionManager;
            container.BindToConstant(reflectionManager);
        }
#endif
        protected override void BindOperationCallbackFactory(IModuleContext context, IIocContainer container)
        {
            container.Bind<IOperationCallbackFactory, SerializableOperationCallbackFactory>(DependencyLifecycle.SingleInstance);
        }

        protected override void BindViewModelPresenter(IModuleContext context, IIocContainer container)
        {
#if WPF
            if (context.PlatformInfo.Platform != PlatformType.WPF)
            {
                ServiceProvider.Initialized += MvvmApplicationOnInitialized;
                return;
            }
#endif
            container.BindToMethod((iocContainer, list) =>
            {
                IViewModelPresenter presenter = iocContainer.Get<ViewModelPresenter>();
                var windowPresenter = iocContainer.Get<DynamicViewModelWindowPresenter>();
                windowPresenter.RegisterMediatorFactory<WindowViewMediator, IWindowView>();
                presenter.DynamicPresenters.Add(windowPresenter);
                presenter.DynamicPresenters.Add(iocContainer.Get<DynamicViewModelNavigationPresenter>());
                return presenter;
            }, DependencyLifecycle.SingleInstance);
        }

#if WPF
        protected override void BindTracer(IModuleContext context, IIocContainer container)
        {
            ITracer tracer = new TracerEx();
            ServiceProvider.Tracer = tracer;
            container.BindToConstant(tracer);
        }

        private static void MvvmApplicationOnInitialized(object sender, EventArgs eventArgs)
        {
            ServiceProvider.Initialized -= MvvmApplicationOnInitialized;
            IViewModelPresenter presenter;
            if (ServiceProvider.TryGet(out presenter))
            {
                presenter.DynamicPresenters.Add(ServiceProvider.Get<DynamicViewModelNavigationPresenter>());

                var windowPresenter = presenter.DynamicPresenters.OfType<DynamicViewModelWindowPresenter>().FirstOrDefault();
                if (windowPresenter == null)
                {
                    windowPresenter = ServiceProvider.Get<DynamicViewModelWindowPresenter>();
                    presenter.DynamicPresenters.Add(windowPresenter);
                }
                windowPresenter.RegisterMediatorFactory<WindowViewMediator, IWindowView>();
            }
        }
#endif

        protected override void BindThreadManager(IModuleContext context, IIocContainer container)
        {
#if WPF
            if (context.PlatformInfo.Platform == PlatformType.WPF)
                container.BindToMethod<IThreadManager>((c, list) => new ThreadManager(System.Windows.Threading.Dispatcher.CurrentDispatcher), DependencyLifecycle.SingleInstance);
#elif WINDOWS_UWP
            if (context.Mode.IsDesignMode() && Windows.UI.Xaml.Window.Current.Dispatcher == null)
                base.BindThreadManager(context, container);
            else
                container.BindToMethod<IThreadManager>((c, list) => new ThreadManager(Windows.UI.Xaml.Window.Current.Dispatcher), DependencyLifecycle.SingleInstance);
#endif
        }

        protected override void BindNavigationProvider(IModuleContext context, IIocContainer container)
        {
            container.Bind<INavigationProvider, NavigationProvider>(DependencyLifecycle.SingleInstance);
        }

        protected virtual void BindNavigationCachePolicy(IModuleContext context, IIocContainer container)
        {
            container.Bind<INavigationCachePolicy, DefaultNavigationCachePolicy>(DependencyLifecycle.SingleInstance);
        }

        protected override void BindToastPresenter(IModuleContext context, IIocContainer container)
        {
#if WPF
            if (context.PlatformInfo.Platform != PlatformType.WPF)
                return;
#endif
            container.Bind<IToastPresenter, ToastPresenter>(DependencyLifecycle.SingleInstance);
        }
    }
}
