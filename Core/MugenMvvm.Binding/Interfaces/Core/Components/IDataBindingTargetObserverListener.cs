using System;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IDataBindingTargetObserverListener : IComponent<IDataBinding>
    {
        void OnTargetPathMembersChanged(IDataBinding dataBinding, IBindingPathObserver observer, IReadOnlyMetadataContext metadata);

        void OnTargetLastMemberChanged(IDataBinding dataBinding, IBindingPathObserver observer, IReadOnlyMetadataContext metadata);

        void OnTargetError(IDataBinding dataBinding, IBindingPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata);
    }
}