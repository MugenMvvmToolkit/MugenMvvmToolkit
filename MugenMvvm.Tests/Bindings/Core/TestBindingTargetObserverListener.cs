using System;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Bindings.Core
{
    public class TestBindingTargetObserverListener : IBindingTargetObserverListener, IHasPriority
    {
        public Action<IBinding, IMemberPathObserver, IReadOnlyMetadataContext>? OnTargetPathMembersChanged { get; set; }

        public Action<IBinding, IMemberPathObserver, IReadOnlyMetadataContext>? OnTargetLastMemberChanged { get; set; }

        public Action<IBinding, IMemberPathObserver, Exception, IReadOnlyMetadataContext>? OnTargetError { get; set; }

        public int Priority { get; set; }

        void IBindingTargetObserverListener.OnTargetPathMembersChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata) =>
            OnTargetPathMembersChanged?.Invoke(binding, observer, metadata);

        void IBindingTargetObserverListener.OnTargetLastMemberChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata) =>
            OnTargetLastMemberChanged?.Invoke(binding, observer, metadata);

        void IBindingTargetObserverListener.OnTargetError(IBinding binding, IMemberPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata) =>
            OnTargetError?.Invoke(binding, observer, exception, metadata);
    }
}