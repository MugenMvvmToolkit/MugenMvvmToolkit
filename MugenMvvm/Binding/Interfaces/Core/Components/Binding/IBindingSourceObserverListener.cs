using System;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components.Binding
{
    public interface IBindingSourceObserverListener : IComponent<IBinding>
    {
        void OnSourcePathMembersChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata);

        void OnSourceLastMemberChanged(IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata);

        void OnSourceError(IBinding binding, IMemberPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata);
    }
}