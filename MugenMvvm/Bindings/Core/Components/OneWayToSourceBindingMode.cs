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
    public sealed class OneWayToSourceBindingMode : IAttachableComponent, IBindingTargetObserverListener, IBindingSourceObserverListener, IHasPriority
    {
        public static readonly OneWayToSourceBindingMode Instance = new();

        private OneWayToSourceBindingMode()
        {
        }

        public int Priority { get; init; } = BindingComponentPriority.Mode;

        void IAttachableComponent.OnAttaching(object owner, IReadOnlyMetadataContext? metadata)
        {
        }

        void IAttachableComponent.OnAttached(object owner, IReadOnlyMetadataContext? metadata) => ((IBinding) owner).UpdateSource();

        void IBindingSourceObserverListener.OnSourcePathMembersChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata) => binding.UpdateSource();

        void IBindingSourceObserverListener.OnSourceLastMemberChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata)
        {
        }

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