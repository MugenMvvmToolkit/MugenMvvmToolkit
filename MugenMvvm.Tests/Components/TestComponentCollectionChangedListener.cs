using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Components
{
    public class TestComponentCollectionChangedListener : IComponentCollectionChangedListener, IHasPriority
    {
        public Action<IComponentCollection, object, IReadOnlyMetadataContext?>? OnAdded { get; set; }

        public Action<IComponentCollection, object, IReadOnlyMetadataContext?>? OnRemoved { get; set; }

        public int Priority { get; set; }

        void IComponentCollectionChangedListener.OnAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            OnAdded?.Invoke(collection, component, metadata);

        void IComponentCollectionChangedListener.OnRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            OnRemoved?.Invoke(collection, component, metadata);
    }
}