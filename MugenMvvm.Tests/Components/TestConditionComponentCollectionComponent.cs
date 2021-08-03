using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Components
{
    public class TestConditionComponentCollectionComponent : IConditionComponentCollectionComponent, IHasPriority
    {
        public Func<IComponentCollection, object, IReadOnlyMetadataContext?, bool>? CanAdd { get; set; }

        public Func<IComponentCollection, object, IReadOnlyMetadataContext?, bool>? CanRemove { get; set; }

        public int Priority { get; set; }

        bool IConditionComponentCollectionComponent.CanAdd(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            CanAdd?.Invoke(collection, component, metadata) ?? true;

        bool IConditionComponentCollectionComponent.CanRemove(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) =>
            CanRemove?.Invoke(collection, component, metadata) ?? true;
    }
}