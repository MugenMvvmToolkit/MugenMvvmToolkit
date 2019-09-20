using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IDataBindingSourceListener : IComponent<IDataBinding>
    {
        void OnSourceUpdateFailed(IDataBinding dataBinding, Exception error, IReadOnlyMetadataContext metadata);

        void OnSourceUpdateCanceled(IDataBinding dataBinding, IReadOnlyMetadataContext metadata);

        void OnSourceUpdated(IDataBinding dataBinding, object? newValue, IReadOnlyMetadataContext metadata);
    }
}