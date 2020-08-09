using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Components.Internal
{
    public class TestComponentCollectionChangingListener : IComponentCollectionChangingListener, IHasPriority
    {
        #region Properties

        public Func<IComponentCollection, object, IReadOnlyMetadataContext?, bool>? OnAdding { get; set; }

        public Func<IComponentCollection, object, IReadOnlyMetadataContext?, bool>? OnRemoving { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IComponentCollectionChangingListener.OnAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => OnAdding?.Invoke(collection, component, metadata) ?? true;

        bool IComponentCollectionChangingListener.OnRemoving(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => OnRemoving?.Invoke(collection, component, metadata) ?? true;

        #endregion
    }
}