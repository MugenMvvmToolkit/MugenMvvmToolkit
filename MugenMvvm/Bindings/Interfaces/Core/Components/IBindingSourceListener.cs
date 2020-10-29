using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Interfaces.Core.Components
{
    public interface IBindingSourceListener : IComponent<IBinding>
    {
        void OnSourceUpdateFailed(IBinding binding, Exception exception, IReadOnlyMetadataContext metadata);

        void OnSourceUpdateCanceled(IBinding binding, IReadOnlyMetadataContext metadata);

        void OnSourceUpdated(IBinding binding, object? newValue, IReadOnlyMetadataContext metadata);
    }
}