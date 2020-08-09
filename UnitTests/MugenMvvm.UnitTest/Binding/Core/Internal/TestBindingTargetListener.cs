using System;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components.Binding;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Binding.Core.Internal
{
    public class TestBindingTargetListener : IBindingTargetListener, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Action<IBinding, Exception, IReadOnlyMetadataContext>? OnTargetUpdateFailed { get; set; }

        public Action<IBinding, IReadOnlyMetadataContext>? OnTargetUpdateCanceled { get; set; }

        public Action<IBinding, object?, IReadOnlyMetadataContext>? OnTargetUpdated { get; set; }

        #endregion

        #region Implementation of interfaces

        void IBindingTargetListener.OnTargetUpdateFailed(IBinding binding, Exception exception, IReadOnlyMetadataContext metadata) => OnTargetUpdateFailed?.Invoke(binding, exception, metadata);

        void IBindingTargetListener.OnTargetUpdateCanceled(IBinding binding, IReadOnlyMetadataContext metadata) => OnTargetUpdateCanceled?.Invoke(binding, metadata);

        void IBindingTargetListener.OnTargetUpdated(IBinding binding, object? newValue, IReadOnlyMetadataContext metadata) => OnTargetUpdated?.Invoke(binding, newValue, metadata);

        #endregion
    }
}