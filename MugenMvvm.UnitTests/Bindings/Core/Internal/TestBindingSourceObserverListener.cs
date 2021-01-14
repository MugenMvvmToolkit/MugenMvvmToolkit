using System;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTests.Bindings.Core.Internal
{
    public class TestBindingSourceObserverListener : IBindingSourceObserverListener, IHasPriority
    {
        public Action<IBinding, IMemberPathObserver, IReadOnlyMetadataContext>? OnSourcePathMembersChanged { get; set; }

        public Action<IBinding, IMemberPathObserver, IReadOnlyMetadataContext>? OnSourceLastMemberChanged { get; set; }

        public Action<IBinding, IMemberPathObserver, Exception, IReadOnlyMetadataContext>? OnSourceError { get; set; }

        public int Priority { get; set; }

        void IBindingSourceObserverListener.OnSourcePathMembersChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata) =>
            OnSourcePathMembersChanged?.Invoke(binding, observer, metadata);

        void IBindingSourceObserverListener.OnSourceLastMemberChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata) =>
            OnSourceLastMemberChanged?.Invoke(binding, observer, metadata);

        void IBindingSourceObserverListener.OnSourceError(IBinding binding, IMemberPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata) =>
            OnSourceError?.Invoke(binding, observer, exception, metadata);
    }
}