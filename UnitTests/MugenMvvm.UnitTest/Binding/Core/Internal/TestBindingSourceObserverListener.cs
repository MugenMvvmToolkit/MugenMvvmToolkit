using System;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components.Binding;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Binding.Core.Internal
{
    public class TestBindingSourceObserverListener : IBindingSourceObserverListener, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Action<IBinding, IMemberPathObserver, IReadOnlyMetadataContext>? OnSourcePathMembersChanged { get; set; }

        public Action<IBinding, IMemberPathObserver, IReadOnlyMetadataContext>? OnSourceLastMemberChanged { get; set; }

        public Action<IBinding, IMemberPathObserver, Exception, IReadOnlyMetadataContext>? OnSourceError { get; set; }

        #endregion

        #region Implementation of interfaces

        void IBindingSourceObserverListener.OnSourcePathMembersChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata)
        {
            OnSourcePathMembersChanged?.Invoke(binding, observer, metadata);
        }

        void IBindingSourceObserverListener.OnSourceLastMemberChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata)
        {
            OnSourceLastMemberChanged?.Invoke(binding, observer, metadata);
        }

        void IBindingSourceObserverListener.OnSourceError(IBinding binding, IMemberPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata)
        {
            OnSourceError?.Invoke(binding, observer, exception, metadata);
        }

        #endregion
    }
}