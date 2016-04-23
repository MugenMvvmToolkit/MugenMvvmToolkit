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

using MugenMvvmToolkit.iOS.Infrastructure;
using MugenMvvmToolkit.iOS.Infrastructure.Callbacks;
using MugenMvvmToolkit.iOS.Infrastructure.Navigation;
using MugenMvvmToolkit.iOS.Infrastructure.Presenters;
using MugenMvvmToolkit.iOS.Interfaces;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.IoC;
using MugenMvvmToolkit.Modules;

namespace MugenMvvmToolkit.iOS.Modules
{
    public class InitializationModule : InitializationModuleBase
    {
        #region Cosntructors

        public InitializationModule()
        {
        }

        protected InitializationModule(LoadMode mode, int priority)
            : base(mode, priority)
        {
        }

        #endregion

        #region Overrides of InitializationModuleBase

        protected override bool LoadInternal()
        {
            var load = base.LoadInternal();
            if (load)
                IocContainer.BindToBindingInfo(GetApplicationStateManager());
            return load;
        }

        protected virtual BindingInfo<IApplicationStateManager> GetApplicationStateManager()
        {
            return BindingInfo<IApplicationStateManager>.FromType<ApplicationStateManager>(DependencyLifecycle.SingleInstance);
        }

        protected override BindingInfo<IViewModelPresenter> GetViewModelPresenter()
        {
            return BindingInfo<IViewModelPresenter>.FromMethod((container, list) =>
            {
                var presenter = new ViewModelPresenter();
                presenter.DynamicPresenters.Add(new DynamicViewModelNavigationPresenter());
                presenter.DynamicPresenters.Add(
                    new DynamicViewModelWindowPresenter(container.Get<IViewMappingProvider>(),
                        container.Get<IViewManager>(), container.Get<IWrapperManager>(), container.Get<IThreadManager>(),
                        container.Get<IOperationCallbackManager>()));
                return presenter;
            }, DependencyLifecycle.SingleInstance);
        }

        protected override BindingInfo<IMessagePresenter> GetMessagePresenter()
        {
            return BindingInfo<IMessagePresenter>.FromType<MessagePresenter>(DependencyLifecycle.SingleInstance);
        }

        protected override BindingInfo<IThreadManager> GetThreadManager()
        {
            return BindingInfo<IThreadManager>.FromMethod((container, list) => new ThreadManager(ServiceProvider.UiSynchronizationContext), DependencyLifecycle.SingleInstance);
        }

        protected override BindingInfo<INavigationProvider> GetNavigationProvider()
        {
            return BindingInfo<INavigationProvider>.FromType<NavigationProvider>(DependencyLifecycle.SingleInstance);
        }

        protected override BindingInfo<IToastPresenter> GetToastPresenter()
        {
            return BindingInfo<IToastPresenter>.FromType<ToastPresenter>(DependencyLifecycle.SingleInstance);
        }

        protected override BindingInfo<IOperationCallbackFactory> GetOperationCallbackFactory()
        {
            return BindingInfo<IOperationCallbackFactory>.FromType<SerializableOperationCallbackFactory>(DependencyLifecycle.SingleInstance);
        }

        protected override BindingInfo<IAttachedValueProvider> GetAttachedValueProvider()
        {
            return BindingInfo<IAttachedValueProvider>.FromType<AttachedValueProvider>(DependencyLifecycle.SingleInstance);
        }

        #endregion
    }
}