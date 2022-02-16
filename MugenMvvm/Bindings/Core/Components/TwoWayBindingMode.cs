using System;
using MugenMvvm.Bindings.Constants;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Bindings.Core.Components
{
    public sealed class TwoWayBindingMode : IAttachableComponent, IBindingSourceObserverListener, IBindingTargetObserverListener, IHasPriority
    {
        public static readonly TwoWayBindingMode Target = new(false);
        public static readonly TwoWayBindingMode Source = new(true);
        private readonly bool _source;

        private TwoWayBindingMode(bool source)
        {
            _source = source;
        }

        public int Priority { get; init; } = BindingComponentPriority.Mode;

        void IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata)
        {
        }

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata)
        {
            if (_source)
                ((IBinding) owner).UpdateSource();
            else
                ((IBinding) owner).UpdateTarget();
        }

        void IBindingSourceObserverListener.OnSourcePathMembersChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata) => binding.UpdateTarget();

        void IBindingSourceObserverListener.OnSourceLastMemberChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata) => binding.UpdateTarget();

        void IBindingSourceObserverListener.OnSourceError(IBinding binding, IMemberPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata)
        {
        }

        void IBindingTargetObserverListener.OnTargetPathMembersChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata) => binding.UpdateSource();

        void IBindingTargetObserverListener.OnTargetLastMemberChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata) => binding.UpdateSource();

        void IBindingTargetObserverListener.OnTargetError(IBinding binding, IMemberPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata)
        {
        }
    }
}