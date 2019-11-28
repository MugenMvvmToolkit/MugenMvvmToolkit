using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components.Binding
{
    public interface IBindingTargetListener : IComponent<IBinding>
    {
        void OnTargetUpdateFailed(IBinding binding, Exception error, IReadOnlyMetadataContext metadata);

        void OnTargetUpdateCanceled(IBinding binding, IReadOnlyMetadataContext metadata);

        void OnTargetUpdated(IBinding binding, object? newValue, IReadOnlyMetadataContext metadata);
    }
}