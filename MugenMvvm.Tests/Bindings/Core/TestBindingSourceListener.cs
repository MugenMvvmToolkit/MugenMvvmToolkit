﻿using System;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Bindings.Core
{
    public class TestBindingSourceListener : IBindingSourceListener, IHasPriority
    {
        public Action<IBinding, Exception, IReadOnlyMetadataContext>? OnSourceUpdateFailed { get; set; }

        public Action<IBinding, IReadOnlyMetadataContext>? OnSourceUpdateCanceled { get; set; }

        public Action<IBinding, object?, IReadOnlyMetadataContext>? OnSourceUpdated { get; set; }

        public int Priority { get; set; }

        void IBindingSourceListener.OnSourceUpdateFailed(IBinding binding, Exception exception, IReadOnlyMetadataContext metadata) =>
            OnSourceUpdateFailed?.Invoke(binding, exception, metadata);

        void IBindingSourceListener.OnSourceUpdateCanceled(IBinding binding, IReadOnlyMetadataContext metadata) => OnSourceUpdateCanceled?.Invoke(binding, metadata);

        void IBindingSourceListener.OnSourceUpdated(IBinding binding, object? newValue, IReadOnlyMetadataContext metadata) => OnSourceUpdated?.Invoke(binding, newValue, metadata);
    }
}