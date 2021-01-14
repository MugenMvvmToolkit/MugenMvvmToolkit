using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTests.Components.Internal
{
    public class TestComponentCollectionManagerListener : IComponentCollectionManagerListener
    {
        public Action<IComponentCollectionManager, IComponentCollection, IReadOnlyMetadataContext?>? OnComponentCollectionCreated { get; set; }

        void IComponentCollectionManagerListener.OnComponentCollectionCreated(IComponentCollectionManager provider, IComponentCollection componentCollection,
            IReadOnlyMetadataContext? metadata) =>
            OnComponentCollectionCreated?.Invoke(provider, componentCollection, metadata);
    }
}