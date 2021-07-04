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
    public sealed class OneWayToSourceBindingMode : IAttachableComponent, IBindingTargetObserverListener, IHasPriority
    {
        public static readonly OneWayToSourceBindingMode Instance = new();

        private OneWayToSourceBindingMode()
        {
        }

        public int Priority { get; init; } = BindingComponentPriority.Mode;

        bool IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata) => true;

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
            var binding = (IBinding) owner;
            binding.UpdateSource();
            if (!BindingMugenExtensions.IsAllMembersAvailable(binding.Source))
                binding.AddComponent(OneTimeHandlerComponent.Instance, metadata);
        }

        void IBindingTargetObserverListener.OnTargetPathMembersChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata) => binding.UpdateSource();

        void IBindingTargetObserverListener.OnTargetLastMemberChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata) => binding.UpdateSource();

        void IBindingTargetObserverListener.OnTargetError(IBinding binding, IMemberPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata)
        {
        }

        internal sealed class OneTimeHandlerComponent : IBindingSourceObserverListener
        {
            public static readonly OneTimeHandlerComponent Instance = new();

            private OneTimeHandlerComponent()
            {
            }

            public void OnSourcePathMembersChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata) => Invoke(binding);

            public void OnSourceLastMemberChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata) => Invoke(binding);

            public void OnSourceError(IBinding binding, IMemberPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata)
            {
            }

            private void Invoke(IBinding binding)
            {
                if (BindingMugenExtensions.IsAllMembersAvailable(binding.Source))
                {
                    binding.RemoveComponent(this);
                    binding.UpdateSource();
                }
            }
        }
    }
}