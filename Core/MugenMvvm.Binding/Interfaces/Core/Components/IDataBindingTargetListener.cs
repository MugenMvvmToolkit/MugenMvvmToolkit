using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components
{
    public interface IDataBindingTargetListener : IComponent<IDataBinding>
    {
        void OnTargetUpdateFailed(IDataBinding dataBinding, Exception error, IReadOnlyMetadataContext metadata);

        void OnTargetUpdateCanceled(IDataBinding dataBinding, IReadOnlyMetadataContext metadata);

        void OnTargetUpdated(IDataBinding dataBinding, object? newValue, IReadOnlyMetadataContext metadata);
    }
}