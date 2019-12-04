using MugenMvvm.Binding.Enums;
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

        public IReadOnlyMetadataContext? OnLifecycleChanged(IBinding binding, BindingLifecycleState lifecycle, IReadOnlyMetadataContext? metadata)
        {
            if (metadata != null && metadata.TryGet(BindingMetadata.SuppressHolderRegistration, out var v) && v)
                return null;

            if (lifecycle == BindingLifecycleState.Initialized)
            {
                var holders = Owner.GetComponents<IBindingHolderComponent>(metadata);
                for (var i = 0; i < holders.Length; i++)
                {
                    if (holders[i].TryRegister(binding, metadata))
                        break;
                }
            }
            else if (lifecycle == BindingLifecycleState.Disposed)
            {
                var holders = Owner.GetComponents<IBindingHolderComponent>(metadata);
                for (var i = 0; i < holders.Length; i++)
                {
                    if (holders[i].TryUnregister(binding, metadata))
                        break;
                }
            }

            return null;
        }

        #endregion
    }
}