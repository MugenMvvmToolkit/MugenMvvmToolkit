#region Copyright
// ****************************************************************************
// <copyright file="InitializationModuleBase.cs">
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
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Infrastructure.Navigation;
using MugenMvvmToolkit.Infrastructure.Presenters;
using MugenMvvmToolkit.Infrastructure.Validation;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.Presenters;
using MugenMvvmToolkit.Interfaces.Validation;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.IoC;

namespace MugenMvvmToolkit.Modules
{
    /// <summary>
    ///     Represents the class that is used to initialize the IOC adapter.
    /// </summary>
    public abstract class InitializationModuleBase : ModuleBase
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="InitializationModuleBase" /> class.
        /// </summary>
        protected InitializationModuleBase(LoadMode mode = LoadMode.All, int priority = InitializationModulePriority)
            : base(false, mode, priority)
        {
        }

        #endregion

        #region Overrides of IocModule

        /// <summary>
        ///     Loads the current module.
        /// </summary>
        protected override bool LoadInternal()
        {
            IocContainer.BindToBindingInfo(GetAttachedValueProvider());
            IAttachedValueProvider attachedValueProvider;
            if (IocContainer.TryGet(out attachedValueProvider))
                ServiceProvider.AttachedValueProvider = attachedValueProvider;
            IocContainer.BindToBindingInfo(Mode == LoadMode.Runtime ? GetThreadManager() : GetThreadManagerInternal());
            IocContainer.BindToBindingInfo(GetSerializer());
            IocContainer.BindToBindingInfo(GetOperationCallbackManager());
            IocContainer.BindToBindingInfo(GetTaskExceptionHandler());
            IocContainer.BindToBindingInfo(GetOperationCallbackFactory());
            IOperationCallbackFactory callbackFactory;
            if (IocContainer.TryGet(out callbackFactory))
                ServiceProvider.OperationCallbackFactory = callbackFactory;
            IocContainer.BindToBindingInfo(GetViewMappingProvider());
            IocContainer.BindToBindingInfo(GetViewManager());
            IocContainer.BindToBindingInfo(GetDisplayNameProvider());
            IocContainer.BindToBindingInfo(GetVisualStateManager());
            IocContainer.BindToBindingInfo(GetViewModelProvider());
            IocContainer.BindToBindingInfo(GetMessagePresenter());
            IocContainer.BindToBindingInfo(GetToastPresenter());
            IocContainer.BindToBindingInfo(GetViewModelPresenter());
            IocContainer.BindToBindingInfo(GetWrapperManager());
            IocContainer.BindToBindingInfo(GetEventAggregator());
            IocContainer.BindToBindingInfo(GetValidationElementProvider());
            IocContainer.BindToBindingInfo(GetEntityStateProvider());
            IocContainer.BindToBindingInfo(GetValidatorProvider());
            IocContainer.BindToBindingInfo(GetTracer());
            IocContainer.BindToBindingInfo(GetReflectionManager());
            IocContainer.BindToBindingInfo(GetNavigationCachePolicy());
            IocContainer.BindToBindingInfo(GetNavigationProvider());
            return true;
        }

        /// <summary>
        ///     Unloads the current module.
        /// </summary>
        protected override void UnloadInternal()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Gets the <see cref="IAttachedValueProvider" /> that will be used by default.
        /// </summary>
        /// <returns>An instance of <see cref="IAttachedValueProvider" />.</returns>
        protected virtual BindingInfo<IAttachedValueProvider> GetAttachedValueProvider()
        {
            return BindingInfo<IAttachedValueProvider>.Empty;
        }

        /// <summary>
        ///     Gets the <see cref="IReflectionManager" /> that will be used by default.
        /// </summary>
        /// <returns>An instance of <see cref="IReflectionManager" />.</returns>
        protected virtual BindingInfo<IReflectionManager> GetReflectionManager()
        {
            return BindingInfo<IReflectionManager>.FromType<ExpressionReflectionManager>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IVisualStateManager" /> that will be used by default.
        /// </summary>
        /// <returns>An instance of <see cref="IVisualStateManager" />.</returns>
        protected virtual BindingInfo<IVisualStateManager> GetVisualStateManager()
        {
            return BindingInfo<IVisualStateManager>.Empty;
        }

        /// <summary>
        ///     Gets the <see cref="IDisplayNameProvider" /> that will be used by default.
        /// </summary>
        /// <returns>An instance of <see cref="IDisplayNameProvider" />.</returns>
        protected virtual BindingInfo<IDisplayNameProvider> GetDisplayNameProvider()
        {
            return BindingInfo<IDisplayNameProvider>.FromType<DisplayNameProvider>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IViewMappingProvider" /> that will be used by default.
        /// </summary>
        /// <returns>An instance of <see cref="IViewMappingProvider" />.</returns>
        protected virtual BindingInfo<IViewMappingProvider> GetViewMappingProvider()
        {
            var assemblies = Context.Assemblies;
            return BindingInfo<IViewMappingProvider>.FromMethod((adapter, list) => new ViewMappingProvider(assemblies), DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IViewManager" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IViewManager" />.</returns>
        protected virtual BindingInfo<IViewManager> GetViewManager()
        {
            return BindingInfo<IViewManager>.FromType<ViewManager>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IViewModelProvider" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IViewModelProvider" />.</returns>
        protected virtual BindingInfo<IViewModelProvider> GetViewModelProvider()
        {
            return BindingInfo<IViewModelProvider>.FromMethod((adapter, list) => new ViewModelProvider(adapter.GetRoot()), DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IViewModelPresenter" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IViewModelPresenter" />.</returns>
        protected virtual BindingInfo<IViewModelPresenter> GetViewModelPresenter()
        {
            return BindingInfo<IViewModelPresenter>.FromType<ViewModelPresenter>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IMessagePresenter" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IMessagePresenter" />.</returns>
        protected virtual BindingInfo<IMessagePresenter> GetMessagePresenter()
        {
            return BindingInfo<IMessagePresenter>.Empty;
        }

        /// <summary>
        ///     Gets the <see cref="IToastPresenter" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IToastPresenter" />.</returns>
        protected virtual BindingInfo<IToastPresenter> GetToastPresenter()
        {
            return BindingInfo<IToastPresenter>.Empty;
        }

        /// <summary>
        ///     Gets the <see cref="IWrapperManager" /> that will be used by default.
        /// </summary>
        /// <returns>An instance of <see cref="IWrapperManager" />.</returns>
        protected virtual BindingInfo<IWrapperManager> GetWrapperManager()
        {
            return BindingInfo<IWrapperManager>.FromType<WrapperManager>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IEventAggregator" /> that will be used by default.
        /// </summary>
        /// <returns>An instance of <see cref="IEventAggregator" />.</returns>
        protected virtual BindingInfo<IEventAggregator> GetEventAggregator()
        {
            return BindingInfo<IEventAggregator>.FromType<EventAggregator>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IValidationElementProvider" /> that will be used by default.
        /// </summary>
        /// <returns>An instance of <see cref="IValidationElementProvider" />.</returns>
        protected virtual BindingInfo<IValidationElementProvider> GetValidationElementProvider()
        {
            return BindingInfo<IValidationElementProvider>
                .FromType<DynamicDataAnnotationsElementProvider>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IEntityStateManager" /> that will be used by default.
        /// </summary>
        /// <returns>An instance of <see cref="IEntityStateManager" />.</returns>
        protected virtual BindingInfo<IEntityStateManager> GetEntityStateProvider()
        {
            return BindingInfo<IEntityStateManager>.FromType<EntityStateManager>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IValidatorProvider" /> that will be used by default.
        /// </summary>
        /// <returns>An instance of <see cref="IValidatorProvider" />.</returns>
        protected virtual BindingInfo<IValidatorProvider> GetValidatorProvider()
        {
            return BindingInfo<IValidatorProvider>
                .FromMethod((adapter, list) => new ValidatorProvider(true, adapter.GetRoot()), DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="ITracer" /> that will be used by default.
        /// </summary>
        /// <returns>An instance of <see cref="ITracer" />.</returns>
        protected virtual BindingInfo<ITracer> GetTracer()
        {
            return BindingInfo<ITracer>.FromInstance(Tracer.Instance);
        }

        /// <summary>
        ///     Gets the <see cref="ITaskExceptionHandler" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="ITaskExceptionHandler" />.</returns>
        protected virtual BindingInfo<ITaskExceptionHandler> GetTaskExceptionHandler()
        {
            return BindingInfo<ITaskExceptionHandler>.FromInstance(Tracer.Instance);
        }

        /// <summary>
        ///     Gets the <see cref="IThreadManager" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IThreadManager" />.</returns>
        protected virtual BindingInfo<IThreadManager> GetThreadManager()
        {
            return GetThreadManagerInternal();
        }

        /// <summary>
        ///     Gets the <see cref="INavigationProvider" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="INavigationProvider" />.</returns>
        protected virtual BindingInfo<INavigationProvider> GetNavigationProvider()
        {
            return BindingInfo<INavigationProvider>.Empty;
        }

        /// <summary>
        ///     Gets the <see cref="INavigationCachePolicy" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="INavigationCachePolicy" />.</returns>
        protected virtual BindingInfo<INavigationCachePolicy> GetNavigationCachePolicy()
        {
            return BindingInfo<INavigationCachePolicy>.FromInstance(new DefaultNavigationCachePolicy());
        }

        /// <summary>
        ///     Gets the <see cref="ISerializer" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="ISerializer" />.</returns>
        protected virtual BindingInfo<ISerializer> GetSerializer()
        {
            var assemblies = Context.Assemblies;
            return BindingInfo<ISerializer>.FromMethod((container, list) => new Serializer(assemblies), DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IOperationCallbackManager" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IOperationCallbackManager" />.</returns>
        protected virtual BindingInfo<IOperationCallbackManager> GetOperationCallbackManager()
        {
            return BindingInfo<IOperationCallbackManager>.FromType<OperationCallbackManager>(DependencyLifecycle.SingleInstance);
        }

        /// <summary>
        ///     Gets the <see cref="IOperationCallbackFactory" /> that will be used in the current application by default.
        /// </summary>
        /// <returns>An instance of <see cref="IOperationCallbackFactory" />.</returns>
        protected virtual BindingInfo<IOperationCallbackFactory> GetOperationCallbackFactory()
        {
            return BindingInfo<IOperationCallbackFactory>.FromInstance(DefaultOperationCallbackFactory.Instance);
        }

        private static BindingInfo<IThreadManager> GetThreadManagerInternal()
        {
            return BindingInfo<IThreadManager>.FromType<SynchronousThreadManager>(DependencyLifecycle.SingleInstance);
        }

        #endregion
    }
}