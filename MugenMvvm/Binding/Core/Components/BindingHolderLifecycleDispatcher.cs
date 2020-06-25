using MugenMvvm.Attributes;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingHolderLifecycleDispatcher : AttachableComponentBase<IBindingManager>, IBindingLifecycleDispatcherComponent, IHasPriority
    {
        #region Fields

        private readonly ComponentTracker _componentTracker;
        private IBindingHolderComponent[] _components;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public BindingHolderLifecycleDispatcher()
        {
            _components = Default.Array<IBindingHolderComponent>();
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IBindingHolderComponent, BindingHolderLifecycleDispatcher>((components, state, _) => state._components = components, this);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.PostInitializer;

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged<TState>(IBinding binding, BindingLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata)
        {
            if (metadata != null && metadata.TryGet(BindingMetadata.SuppressHolderRegistration, out var v, false) && v)
                return;

            if (lifecycleState == BindingLifecycleState.Initialized)
                _components.TryRegister(typeof(TState) == typeof(BindingTargetSourceState) ? MugenExtensions.CastGeneric<TState, BindingTargetSourceState>(state).Target : binding.Target.Target, binding, metadata);
            else if (lifecycleState == BindingLifecycleState.Disposed)
                _components.TryUnregister(binding.Target.Target, binding, metadata);
        }

        #endregion

        #region Methods

        protected override void OnAttachedInternal(IBindingManager owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttachedInternal(owner, metadata);
            _componentTracker.Attach(owner, metadata);
        }

        protected override void OnDetachedInternal(IBindingManager owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetachedInternal(owner, metadata);
            _componentTracker.Detach(owner, metadata);
        }

        #endregion
    }
}