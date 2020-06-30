using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Components.Internal
{
    public class TestComponentCollectionManagerListener : IComponentCollectionManagerListener
    {
        #region Properties

        public Action<IComponentCollectionManager, IComponentCollection, IReadOnlyMetadataContext?>? OnComponentCollectionCreated { get; set; }

        #endregion

        #region Implementation of interfaces

        void IComponentCollectionManagerListener.OnComponentCollectionCreated(IComponentCollectionManager provider, IComponentCollection componentCollection, IReadOnlyMetadataContext? metadata)
        {
            OnComponentCollectionCreated?.Invoke(provider, componentCollection, metadata);
        }

        #endregion
    }
}