using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Metadata
{
    public class TestMetadataContextListener : IMetadataContextListener, IHasPriority
    {
        public Action<IMetadataContext, IMetadataContextKey, object?>? OnAdded { get; set; }

        public Action<IMetadataContext, IMetadataContextKey, object?, object?>? OnChanged { get; set; }

        public Action<IMetadataContext, IMetadataContextKey, object?>? OnRemoved { get; set; }

        public int Priority { get; set; }

        void IMetadataContextListener.OnAdded(IMetadataContext metadataContext, IMetadataContextKey key, object? newValue) => OnAdded?.Invoke(metadataContext, key, newValue);

        void IMetadataContextListener.OnChanged(IMetadataContext metadataContext, IMetadataContextKey key, object? oldValue, object? newValue) =>
            OnChanged?.Invoke(metadataContext, key, oldValue, newValue);

        void IMetadataContextListener.OnRemoved(IMetadataContext metadataContext, IMetadataContextKey key, object? oldValue) => OnRemoved?.Invoke(metadataContext, key, oldValue);
    }
}