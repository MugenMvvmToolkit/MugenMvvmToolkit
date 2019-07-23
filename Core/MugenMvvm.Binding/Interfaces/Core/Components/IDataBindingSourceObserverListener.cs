using System;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IDataBindingSourceObserverListener : IComponent<IDataBinding>
    {
        void OnSourcePathMembersChanged(IDataBinding dataBinding, IBindingPathObserver observer, IReadOnlyMetadataContext metadata);

        void OnSourceLastMemberChanged(IDataBinding dataBinding, IBindingPathObserver observer, IReadOnlyMetadataContext metadata);

        void OnSourceError(IDataBinding dataBinding, IBindingPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata);
    }
}