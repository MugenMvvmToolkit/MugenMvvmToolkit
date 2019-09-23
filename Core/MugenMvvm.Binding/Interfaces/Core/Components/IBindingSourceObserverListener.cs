using System;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingSourceObserverListener : IComponent<IBinding>
    {
        void OnSourcePathMembersChanged(IBinding binding, IBindingPathObserver observer, IReadOnlyMetadataContext metadata);

        void OnSourceLastMemberChanged(IBinding binding, IBindingPathObserver observer, IReadOnlyMetadataContext metadata);

        void OnSourceError(IBinding binding, IBindingPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata);
    }
}