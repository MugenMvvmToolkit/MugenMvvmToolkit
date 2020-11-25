using MugenMvvm.Attributes;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Extensions.Components;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Metadata;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Core.Components
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

        public int Priority { get; set; } = BindingComponentPriority.PostInitializer;

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged(IBindingManager bindingManager, IBinding binding, BindingLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            if (metadata != null && metadata.TryGet(BindingMetadata.SuppressHolderRegistration, out var v, false) && v)
                return;

            if (lifecycleState == BindingLifecycleState.Initialized)
                _components.TryRegister(bindingManager, binding.Target.Target, binding, metadata);
            else if (lifecycleState == BindingLifecycleState.Disposed)
                _components.TryUnregister(bindingManager, binding.Target.Target, binding, metadata);
        }

        #endregion

        #region Methods

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

        #endregion
    }
}