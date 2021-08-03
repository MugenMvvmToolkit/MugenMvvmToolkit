using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Components
{
    public class TestComponentCollectionChangingListener : IComponentCollectionChangingListener, IHasPriority
    {
        public Action<IComponentCollection, object, IReadOnlyMetadataContext?>? OnAdding { get; set; }

        public Action<IComponentCollection, object, IReadOnlyMetadataContext?>? OnRemoving { get; set; }

        public int Priority { get; set; }

        void IComponentCollectionChangingListener.OnAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            OnAdding?.Invoke(collection, component, metadata);

        void IComponentCollectionChangingListener.OnRemoving(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            OnRemoving?.Invoke(collection, component, metadata);
    }
}