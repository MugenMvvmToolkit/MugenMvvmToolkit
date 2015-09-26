#region Copyright

// ****************************************************************************
// <copyright file="InitializationModuleBase.cs">
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
    public abstract class InitializationModuleBase : ModuleBase
    {
        #region Constructors

        protected InitializationModuleBase(LoadMode mode = LoadMode.All, int priority = InitializationModulePriority)
            : base(false, mode, priority)
        {
        }

        #endregion

        #region Overrides of IocModule

        protected override bool LoadInternal()
        {
            IocContainer.BindToBindingInfo(GetAttachedValueProvider());
            IAttachedValueProvider attachedValueProvider;
            if (IocContainer.TryGet(out attachedValueProvider))
                ServiceProvider.AttachedValueProvider = attachedValueProvider;
            IocContainer.BindToBindingInfo(Mode.IsRuntimeMode() ? GetThreadManager() : GetThreadManagerInternal());
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
            IocContainer.BindToBindingInfo(GetEntityStateProvider());
            IocContainer.BindToBindingInfo(GetValidatorProvider());
            IocContainer.BindToBindingInfo(GetTracer());
            IocContainer.BindToBindingInfo(GetReflectionManager());
            IocContainer.BindToBindingInfo(GetNavigationCachePolicy());
            IocContainer.BindToBindingInfo(GetNavigationProvider());
            return true;
        }

        protected override void UnloadInternal()
        {
        }

        #endregion

        #region Methods

        protected virtual BindingInfo<IAttachedValueProvider> GetAttachedValueProvider()
        {
            return BindingInfo<IAttachedValueProvider>.FromType<AttachedValueProviderDefault>(DependencyLifecycle.SingleInstance);
        }

        protected virtual BindingInfo<IReflectionManager> GetReflectionManager()
        {
            return BindingInfo<IReflectionManager>.FromMethod((container, list) => ServiceProvider.ReflectionManagerField ?? new ExpressionReflectionManager(), DependencyLifecycle.SingleInstance);
        }

        protected virtual BindingInfo<IVisualStateManager> GetVisualStateManager()
        {
            return BindingInfo<IVisualStateManager>.Empty;
        }

        protected virtual BindingInfo<IDisplayNameProvider> GetDisplayNameProvider()
        {
            return BindingInfo<IDisplayNameProvider>.FromType<DisplayNameProvider>(DependencyLifecycle.SingleInstance);
        }

        protected virtual BindingInfo<IViewMappingProvider> GetViewMappingProvider()
        {
            var assemblies = Context.Assemblies;
            return BindingInfo<IViewMappingProvider>.FromMethod((adapter, list) => new ViewMappingProvider(assemblies), DependencyLifecycle.SingleInstance);
        }

        protected virtual BindingInfo<IViewManager> GetViewManager()
        {
            return BindingInfo<IViewManager>.FromType<ViewManager>(DependencyLifecycle.SingleInstance);
        }

        protected virtual BindingInfo<IViewModelProvider> GetViewModelProvider()
        {
            return BindingInfo<IViewModelProvider>.FromMethod((adapter, list) => new ViewModelProvider(adapter.GetRoot()), DependencyLifecycle.SingleInstance);
        }

        protected virtual BindingInfo<IViewModelPresenter> GetViewModelPresenter()
        {
            return BindingInfo<IViewModelPresenter>.FromType<ViewModelPresenter>(DependencyLifecycle.SingleInstance);
        }

        protected virtual BindingInfo<IMessagePresenter> GetMessagePresenter()
        {
            return BindingInfo<IMessagePresenter>.Empty;
        }

        protected virtual BindingInfo<IToastPresenter> GetToastPresenter()
        {
            return BindingInfo<IToastPresenter>.Empty;
        }

        protected virtual BindingInfo<IWrapperManager> GetWrapperManager()
        {
            return BindingInfo<IWrapperManager>.FromType<WrapperManager>(DependencyLifecycle.SingleInstance);
        }

        protected virtual BindingInfo<IEventAggregator> GetEventAggregator()
        {
            return BindingInfo<IEventAggregator>.FromMethod((container, list) => ServiceProvider.EventAggregatorField ?? new EventAggregator(), DependencyLifecycle.SingleInstance);
        }

        protected virtual BindingInfo<IEntityStateManager> GetEntityStateProvider()
        {
            return BindingInfo<IEntityStateManager>.FromType<EntityStateManager>(DependencyLifecycle.SingleInstance);
        }

        protected virtual BindingInfo<IValidatorProvider> GetValidatorProvider()
        {
            return BindingInfo<IValidatorProvider>.FromMethod((container, list) => ServiceProvider.ValidatorProviderField ?? new ValidatorProvider(), DependencyLifecycle.SingleInstance);
        }

        protected virtual BindingInfo<ITracer> GetTracer()
        {
            return BindingInfo<ITracer>.FromInstance(Tracer.Instance);
        }

        protected virtual BindingInfo<ITaskExceptionHandler> GetTaskExceptionHandler()
        {
            return BindingInfo<ITaskExceptionHandler>.FromInstance(Tracer.Instance);
        }

        protected virtual BindingInfo<IThreadManager> GetThreadManager()
        {
            return GetThreadManagerInternal();
        }

        protected virtual BindingInfo<INavigationProvider> GetNavigationProvider()
        {
            return BindingInfo<INavigationProvider>.Empty;
        }

        protected virtual BindingInfo<INavigationCachePolicy> GetNavigationCachePolicy()
        {
            return BindingInfo<INavigationCachePolicy>.FromType<DefaultNavigationCachePolicy>(DependencyLifecycle.SingleInstance);
        }

        protected virtual BindingInfo<ISerializer> GetSerializer()
        {
            var assemblies = Context.Assemblies;
            return BindingInfo<ISerializer>.FromMethod((container, list) => new Serializer(assemblies), DependencyLifecycle.SingleInstance);
        }

        protected virtual BindingInfo<IOperationCallbackManager> GetOperationCallbackManager()
        {
            return BindingInfo<IOperationCallbackManager>.FromType<OperationCallbackManager>(DependencyLifecycle.SingleInstance);
        }

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
