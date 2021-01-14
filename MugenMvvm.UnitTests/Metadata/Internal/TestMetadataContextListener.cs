using System;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTests.Metadata.Internal
{
    public class TestMetadataContextListener : IMetadataContextListener, IHasPriority
    {
        private readonly IMetadataContext? _metadataContext;

        public TestMetadataContextListener(IMetadataContext? metadataContext)
        {
            _metadataContext = metadataContext;
        }

        public Action<IMetadataContextKey, object?>? OnAdded { get; set; }

        public Action<IMetadataContextKey, object?, object?>? OnChanged { get; set; }

        public Action<IMetadataContextKey, object?>? OnRemoved { get; set; }

        public int Priority { get; set; }

        void IMetadataContextListener.OnAdded(IMetadataContext metadataContext, IMetadataContextKey key, object? newValue)
        {
            _metadataContext?.ShouldEqual(metadataContext);
            OnAdded?.Invoke(key, newValue);
        }

        void IMetadataContextListener.OnChanged(IMetadataContext metadataContext, IMetadataContextKey key, object? oldValue, object? newValue)
        {
            _metadataContext?.ShouldEqual(metadataContext);
            OnChanged?.Invoke(key, oldValue, newValue);
        }

        void IMetadataContextListener.OnRemoved(IMetadataContext metadataContext, IMetadataContextKey key, object? oldValue)
        {
            _metadataContext?.ShouldEqual(metadataContext);
            OnRemoved?.Invoke(key, oldValue);
        }
    }
}