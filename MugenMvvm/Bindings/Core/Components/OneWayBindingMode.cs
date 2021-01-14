using System;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Core.Components
{
    public sealed class OneWayBindingMode : IAttachableComponent, IBindingSourceObserverListener, IHasPriority
    {
        public static readonly OneWayBindingMode Instance = new();

        private OneWayBindingMode()
        {
        }

        public int Priority { get; set; } = BindingComponentPriority.Mode;

        bool IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata) => true;

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
            var binding = (IBinding) owner;
            binding.UpdateTarget();
            if (!binding.Target.IsAllMembersAvailable())
                binding.AddComponent(OneTimeHandlerComponent.Instance, metadata);
        }

        void IBindingSourceObserverListener.OnSourcePathMembersChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata) => binding.UpdateTarget();

        void IBindingSourceObserverListener.OnSourceLastMemberChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata) => binding.UpdateTarget();

        void IBindingSourceObserverListener.OnSourceError(IBinding binding, IMemberPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata)
        {
        }

        internal sealed class OneTimeHandlerComponent : IBindingTargetObserverListener
        {
            public static readonly OneTimeHandlerComponent Instance = new();

            private OneTimeHandlerComponent()
            {
            }

            public void OnTargetPathMembersChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata) => Invoke(binding);

            public void OnTargetLastMemberChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata) => Invoke(binding);

            public void OnTargetError(IBinding binding, IMemberPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata)
            {
            }

            private void Invoke(IBinding binding)
            {
                if (binding.Target.IsAllMembersAvailable())
                {
                    binding.RemoveComponent(this);
                    binding.UpdateTarget();
                }
            }
        }
    }
}