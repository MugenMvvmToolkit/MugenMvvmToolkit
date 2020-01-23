using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Components
{
    public class TestComponentCollectionChangedListener : IComponentCollectionChangedListener
    {
        #region Properties

        public Action<IComponentCollection, object, IReadOnlyMetadataContext?>? OnAdded { get; set; }

        public Action<IComponentCollection, object, IReadOnlyMetadataContext?>? OnRemoved { get; set; }

        #endregion

        #region Implementation of interfaces

        void IComponentCollectionChangedListener.OnAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            OnAdded?.Invoke(collection, component, metadata);
        }

        void IComponentCollectionChangedListener.OnRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            OnRemoved?.Invoke(collection, component, metadata);
        }

        #endregion
    }
}