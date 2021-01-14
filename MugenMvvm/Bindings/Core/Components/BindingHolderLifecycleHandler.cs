using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Core.Components
{
    public sealed class BindingHolderLifecycleHandler : AttachableComponentBase<IBindingManager>, IBindingLifecycleListener, IHasPriority
    {
        private readonly ComponentTracker _componentTracker;
        private ItemOrArray<IBindingHolderComponent> _components;

        [Preserve(Conditional = true)]
        public BindingHolderLifecycleHandler()
        {
            _componentTracker = new ComponentTracker();
            _componentTracker.AddListener<IBindingHolderComponent, BindingHolderLifecycleHandler>((components, state, _) => state._components = components, this);
        }

        public int Priority { get; set; } = BindingComponentPriority.LifecyclePostInitializer;

        public void OnLifecycleChanged(IBindingManager bindingManager, IBinding binding, BindingLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (metadata != null && metadata.TryGet(BindingMetadata.SuppressHolderRegistration, out var v) && v)
                return;

            if (lifecycleState == BindingLifecycleState.Initialized)
                _components.TryRegister(bindingManager, binding.Target.Target, binding, metadata);
            else if (lifecycleState == BindingLifecycleState.Disposed)
                _components.TryUnregister(bindingManager, binding.Target.Target, binding, metadata);
        }

        protected override void OnAttached(IBindingManager owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttached(owner, metadata);
            _componentTracker.Attach(owner, metadata);
        }

        protected override void OnDetached(IBindingManager owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            _componentTracker.Detach(owner, metadata);
        }
    }
}