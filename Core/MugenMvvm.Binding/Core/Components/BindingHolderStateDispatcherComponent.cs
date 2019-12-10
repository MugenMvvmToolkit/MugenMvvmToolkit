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
        #region Properties

        public int Priority { get; set; } = ComponentPriority.PostInitializer;

        #endregion

        #region Implementation of interfaces

        public IReadOnlyMetadataContext? OnLifecycleChanged(IBinding binding, BindingLifecycleState lifecycleState, IReadOnlyMetadataContext? metadata)
        {
            if (metadata != null && metadata.TryGet(BindingMetadata.SuppressHolderRegistration, out var v) && v)
                return null;

            if (lifecycleState == BindingLifecycleState.Initialized)
                Owner.Components.Get<IBindingHolderComponent>(metadata).TryRegister(binding, metadata);
            else if (lifecycleState == BindingLifecycleState.Disposed)
                Owner.Components.Get<IBindingHolderComponent>(metadata).TryUnregister(binding, metadata);

            return null;
        }

        #endregion
    }
}