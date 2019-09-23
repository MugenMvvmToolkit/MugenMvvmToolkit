using System;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IBindingTargetObserverListener : IComponent<IBinding>
    {
        void OnTargetPathMembersChanged(IBinding binding, IBindingPathObserver observer, IReadOnlyMetadataContext metadata);

        void OnTargetLastMemberChanged(IBinding binding, IBindingPathObserver observer, IReadOnlyMetadataContext metadata);

        void OnTargetError(IBinding binding, IBindingPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata);
    }
}