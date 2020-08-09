using System;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components.Binding;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Binding.Core.Internal
{
    public class TestBindingTargetObserverListener : IBindingTargetObserverListener, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Action<IBinding, IMemberPathObserver, IReadOnlyMetadataContext>? OnTargetPathMembersChanged { get; set; }

        public Action<IBinding, IMemberPathObserver, IReadOnlyMetadataContext>? OnTargetLastMemberChanged { get; set; }

        public Action<IBinding, IMemberPathObserver, Exception, IReadOnlyMetadataContext>? OnTargetError { get; set; }

        #endregion

        #region Implementation of interfaces

        void IBindingTargetObserverListener.OnTargetPathMembersChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata) =>
            OnTargetPathMembersChanged?.Invoke(binding, observer, metadata);

        void IBindingTargetObserverListener.OnTargetLastMemberChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata) => OnTargetLastMemberChanged?.Invoke(binding, observer, metadata);

        void IBindingTargetObserverListener.OnTargetError(IBinding binding, IMemberPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata) =>
            OnTargetError?.Invoke(binding, observer, exception, metadata);

        #endregion
    }
}