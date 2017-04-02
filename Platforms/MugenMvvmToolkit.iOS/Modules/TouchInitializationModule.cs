#region Copyright

// ****************************************************************************
// <copyright file="TouchInitializationModule.cs">
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

using MugenMvvmToolkit.iOS.Infrastructure;
using MugenMvvmToolkit.iOS.Infrastructure.Callbacks;
using MugenMvvmToolkit.iOS.Infrastructure.Mediators;
using MugenMvvmToolkit.iOS.Infrastructure.Navigation;
using MugenMvvmToolkit.iOS.Infrastructure.Presenters;
using MugenMvvmToolkit.iOS.Interfaces;
using MugenMvvmToolkit.iOS.Interfaces.Mediators;
using MugenMvvmToolkit.iOS.Interfaces.Views;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models.IoC;
using MugenMvvmToolkit.Modules;
using UIKit;

namespace MugenMvvmToolkit.iOS.Modules
{
    public class TouchInitializationModule : InitializationModuleBase
    {
        #region Methods

        public override bool Load(IModuleContext context)
        {
            var mediatorFactory = TouchToolkitExtensions.MediatorFactory;
            TouchToolkitExtensions.MediatorFactory = (controller, dataContext, type) =>
            {
                if (controller is UIViewController && typeof(IMvvmViewControllerMediator).IsAssignableFrom(type))
                    return new MvvmViewControllerMediator((UIViewController)controller);
                return mediatorFactory?.Invoke(controller, dataContext, type);
            };
            TouchToolkitExtensions.NativeObjectManager = new DefaultNativeObjectManager();

            BindApplicationStateManager(context, context.IocContainer);
            return base.Load(context);
        }

        protected virtual void BindApplicationStateManager(IModuleContext context, IIocContainer container)
        {
            container.Bind<IApplicationStateManager, ApplicationStateManager>(DependencyLifecycle.SingleInstance);
        }

        protected override void BindViewModelPresenter(IModuleContext context, IIocContainer container)
        {
            container.BindToMethod((iocContainer, list) =>
            {
                IViewModelPresenter presenter = iocContainer.Get<ViewModelPresenter>();
                presenter.DynamicPresenters.Add(iocContainer.Get<DynamicViewModelNavigationPresenter>());
                var windowPresenter = iocContainer.Get<DynamicViewModelWindowPresenter>();
                windowPresenter.RegisterMediatorFactory<ModalViewMediator, IModalView>();
                presenter.DynamicPresenters.Add(windowPresenter);
                return presenter;
            }, DependencyLifecycle.SingleInstance);
        }

        protected override void BindMessagePresenter(IModuleContext context, IIocContainer container)
        {
            container.Bind<IMessagePresenter, MessagePresenter>(DependencyLifecycle.SingleInstance);
        }

        protected override void BindToastPresenter(IModuleContext context, IIocContainer container)
        {
            container.Bind<IToastPresenter, ToastPresenter>(DependencyLifecycle.SingleInstance);
        }

        protected override void BindThreadManager(IModuleContext context, IIocContainer container)
        {
            container.BindToMethod<IThreadManager>((iocContainer, list) => new ThreadManager(ServiceProvider.UiSynchronizationContext), DependencyLifecycle.SingleInstance);
        }

        protected override void BindNavigationProvider(IModuleContext context, IIocContainer container)
        {
            container.Bind<INavigationProvider, NavigationProvider>(DependencyLifecycle.SingleInstance);
        }

        protected override void BindOperationCallbackFactory(IModuleContext context, IIocContainer container)
        {
            container.Bind<IOperationCallbackFactory, SerializableOperationCallbackFactory>(DependencyLifecycle.SingleInstance);
        }

        protected override void BindAttachedValueProvider(IModuleContext context, IIocContainer container)
        {
            IAttachedValueProvider attachedValueProvider = new AttachedValueProvider();
            ServiceProvider.AttachedValueProvider = attachedValueProvider;
            container.BindToConstant(attachedValueProvider);
        }

        #endregion
    }
}