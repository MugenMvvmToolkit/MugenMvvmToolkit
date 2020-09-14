using System;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components.Binding;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTests.Binding.Core.Internal
{
    public class TestBindingSourceListener : IBindingSourceListener, IHasPriority
    {
        #region Properties

        public int Priority { get; set; }

        public Action<IBinding, Exception, IReadOnlyMetadataContext>? OnSourceUpdateFailed { get; set; }

        public Action<IBinding, IReadOnlyMetadataContext>? OnSourceUpdateCanceled { get; set; }

        public Action<IBinding, object?, IReadOnlyMetadataContext>? OnSourceUpdated { get; set; }

        #endregion

        #region Implementation of interfaces

        void IBindingSourceListener.OnSourceUpdateFailed(IBinding binding, Exception exception, IReadOnlyMetadataContext metadata) => OnSourceUpdateFailed?.Invoke(binding, exception, metadata);

        void IBindingSourceListener.OnSourceUpdateCanceled(IBinding binding, IReadOnlyMetadataContext metadata) => OnSourceUpdateCanceled?.Invoke(binding, metadata);

        void IBindingSourceListener.OnSourceUpdated(IBinding binding, object? newValue, IReadOnlyMetadataContext metadata) => OnSourceUpdated?.Invoke(binding, newValue, metadata);

        #endregion
    }
}