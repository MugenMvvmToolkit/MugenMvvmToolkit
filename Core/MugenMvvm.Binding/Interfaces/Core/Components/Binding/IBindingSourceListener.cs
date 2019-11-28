using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Interfaces.Core.Components.Binding
{
    public interface IBindingSourceListener : IComponent<IBinding>
    {
        void OnSourceUpdateFailed(IBinding binding, Exception error, IReadOnlyMetadataContext metadata);

        void OnSourceUpdateCanceled(IBinding binding, IReadOnlyMetadataContext metadata);

        void OnSourceUpdated(IBinding binding, object? newValue, IReadOnlyMetadataContext metadata);
    }
}