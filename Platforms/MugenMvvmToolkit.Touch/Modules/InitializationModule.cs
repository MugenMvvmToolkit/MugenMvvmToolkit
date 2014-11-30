#region Copyright
// ****************************************************************************
// <copyright file="InitializationModule.cs">
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

using System.Threading;
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
        #region Cosntructors

        static InitializationModule()
        {
            if (ServiceProvider.DesignTimeManager.IsDesignMode)
                ServiceProvider.AttachedValueProvider = new AttachedValueProvider();
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
        protected InitializationModule(LoadMode mode = LoadMode.All, int priority = InitializationModulePriority)
            : base(mode, priority)
        {
        }

        #endregion

        #region Overrides of InitializationModuleBase

        /// <summary>
        ///     Loads the current module.
        /// </summary>
        protected override bool LoadInternal()
        {
            var load = base.LoadInternal();
            if (load)
            {
                IocContainer.BindToBindingInfo(GetApplicationStateManager());
                IApplicationStateManager stateManager;
                if (IocContainer.TryGet(out stateManager))
                    PlatformExtensions.ApplicationStateManager = stateManager;
            }
            return load;
        }

        /// <summary>
        ///     Gets the <see cref="IApplicationStateManager" /> that will be used in all view models by default.
        /// </summary>
        /// <returns>An instance of <see cref="IApplicationStateManager" />.</returns>
        protected virtual BindingInfo<IApplicationStateManager> GetApplicationStateManager()
        {
            return BindingInfo<IApplicationStateManager>.FromType<ApplicationStateManager>(DependencyLifecycle.SingleInstance);
        }

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
                presenter.DynamicPresenters.Add(
                    new DynamicViewModelWindowPresenter(container.Get<IViewMappingProvider>(),
                        container.Get<IWrapperManager>(), container.Get<IThreadManager>(),
                        container.Get<IOperationCallbackManager>()));
                return presenter;
            }, DependencyLifecycle.SingleInstance);
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
        ///     Gets the <see cref="IThreadManager" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IThreadManager" />.</returns>
        protected override BindingInfo<IThreadManager> GetThreadManager()
        {
            return BindingInfo<IThreadManager>.FromMethod((container, list) => new ThreadManager(SynchronizationContext.Current), DependencyLifecycle.SingleInstance);
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
        ///     Gets the <see cref="INavigationCachePolicy" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="INavigationCachePolicy" />.</returns>
        protected override BindingInfo<INavigationCachePolicy> GetNavigationCachePolicy()
        {
            return BindingInfo<INavigationCachePolicy>.FromType<EmptyNavigationCachePolicy>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IToastPresenter" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IToastPresenter" />.</returns>
        protected override BindingInfo<IToastPresenter> GetToastPresenter()
        {
            return BindingInfo<IToastPresenter>.FromType<ToastPresenter>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IOperationCallbackFactory" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IOperationCallbackFactory" />.</returns>
        protected override BindingInfo<IOperationCallbackFactory> GetOperationCallbackFactory()
        {
            return BindingInfo<IOperationCallbackFactory>.FromType<SerializableOperationCallbackFactory>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IAttachedValueProvider" /> that will be used by default.
        /// </summary>
        /// <returns>An instance of <see cref="IAttachedValueProvider" />.</returns>
        protected override BindingInfo<IAttachedValueProvider> GetAttachedValueProvider()
        {
            return BindingInfo<IAttachedValueProvider>.FromType<AttachedValueProvider>(DependencyLifecycle.SingleInstance);
        }

        #endregion
    }
}