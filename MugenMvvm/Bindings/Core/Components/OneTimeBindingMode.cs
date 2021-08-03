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
    public sealed class OneTimeBindingMode : IHasAttachConditionComponent, IBindingSourceObserverListener, IHasPriority
    {
        public static readonly OneTimeBindingMode Instance = new(true);
        public static readonly OneTimeBindingMode NonDisposeInstance = new(false);

        private readonly bool _disposeBinding;

        private OneTimeBindingMode(bool disposeBinding)
        {
            _disposeBinding = disposeBinding;
        }

        public int Priority { get; init; } = BindingComponentPriority.Mode;

        private bool Invoke(IBinding binding, bool attached)
        {
            if (!binding.Target.IsAllMembersAvailable() || !BindingMugenExtensions.IsAllMembersAvailable(binding.Source))
                return false;

            binding.UpdateTarget();
            if (_disposeBinding)
                binding.Dispose();
            else if (attached)
                binding.RemoveComponent(this);
            return true;
        }

        void IBindingSourceObserverListener.OnSourcePathMembersChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata) => Invoke(binding, true);

        void IBindingSourceObserverListener.OnSourceLastMemberChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata) => Invoke(binding, true);

        void IBindingSourceObserverListener.OnSourceError(IBinding binding, IMemberPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata)
        {
        }

        bool IHasAttachConditionComponent.CanAttach(object owner, IReadOnlyMetadataContext? metadata) => !Invoke((IBinding) owner, false);
    }
}