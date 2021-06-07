using System;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Bindings.Core
{
    public class TestBindingTargetListener : IBindingTargetListener, IHasPriority
    {
        public Action<IBinding, Exception, IReadOnlyMetadataContext>? OnTargetUpdateFailed { get; set; }

        public Action<IBinding, IReadOnlyMetadataContext>? OnTargetUpdateCanceled { get; set; }

        public Action<IBinding, object?, IReadOnlyMetadataContext>? OnTargetUpdated { get; set; }

        public int Priority { get; set; }

        void IBindingTargetListener.OnTargetUpdateFailed(IBinding binding, Exception exception, IReadOnlyMetadataContext metadata) =>
            OnTargetUpdateFailed?.Invoke(binding, exception, metadata);

        void IBindingTargetListener.OnTargetUpdateCanceled(IBinding binding, IReadOnlyMetadataContext metadata) => OnTargetUpdateCanceled?.Invoke(binding, metadata);

        void IBindingTargetListener.OnTargetUpdated(IBinding binding, object? newValue, IReadOnlyMetadataContext metadata) => OnTargetUpdated?.Invoke(binding, newValue, metadata);
    }
}