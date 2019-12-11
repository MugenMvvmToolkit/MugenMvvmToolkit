using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions.Components;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Binding.Core.Components
{
    public sealed class BindingHolderStateDispatcherComponent : AttachableComponentBase<IBindingManager>, IBindingStateDispatcherComponent, IHasPriority
    {
        #region Fields

        private readonly ComponentTracker _componentTracker;
        private IBindingHolderComponent[] _components;

        #endregion

        #region Constructors

        public BindingHolderStateDispatcherComponent()
        {
            _components = Default.EmptyArray<IBindingHolderComponent>();
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IBindingHolderComponent, BindingHolderStateDispatcherComponent>((components, state, _) => state._components = components, this);
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ComponentPriority.PostInitializer;

        #endregion

        #region Implementation of interfaces

        public IReadOnlyMetadataContext? OnLifecycleChanged(IBinding binding, BindingLifecycleState lifecycleState, IReadOnlyMetadataContext? metadata)
        {
            if (metadata != null && metadata.TryGet(BindingMetadata.SuppressHolderRegistration, out var v) && v)
                return null;

            if (lifecycleState == BindingLifecycleState.Initialized)
                _components.TryRegister(binding, metadata);
            else if (lifecycleState == BindingLifecycleState.Disposed)
                _components.TryUnregister(binding, metadata);

            return null;
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