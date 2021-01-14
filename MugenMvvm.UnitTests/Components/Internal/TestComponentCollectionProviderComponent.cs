using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTests.Components.Internal
{
    public class TestComponentCollectionProviderComponent : IComponentCollectionProviderComponent, IHasPriority
    {
        public Func<IComponentCollectionManager, object, IReadOnlyMetadataContext?, IComponentCollection?>? TryGetComponentCollection { get; set; }

        public int Priority { get; set; } = int.MaxValue;

        IComponentCollection? IComponentCollectionProviderComponent.TryGetComponentCollection(IComponentCollectionManager collectionManager, object owner,
            IReadOnlyMetadataContext? metadata) =>
            TryGetComponentCollection?.Invoke(collectionManager, owner, metadata);
    }
}