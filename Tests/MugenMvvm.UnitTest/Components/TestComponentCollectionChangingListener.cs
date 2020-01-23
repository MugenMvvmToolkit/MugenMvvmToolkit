using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Components
{
    public class TestComponentCollectionChangingListener : IComponentCollectionChangingListener
    {
        #region Properties

        public Func<IComponentCollection, object, IReadOnlyMetadataContext?, bool>? OnAdding { get; set; }

        public Func<IComponentCollection, object, IReadOnlyMetadataContext?, bool>? OnRemoving { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IComponentCollectionChangingListener.OnAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            return OnAdding?.Invoke(collection, component, metadata) ?? true;
        }

        bool IComponentCollectionChangingListener.OnRemoving(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            return OnRemoving?.Invoke(collection, component, metadata) ?? true;
        }

        #endregion
    }
}