using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.UnitTest.Components
{
    public class TestComponentCollectionProviderListener : IComponentCollectionProviderListener
    {
        #region Properties

        public Action<IComponentCollectionProvider, IComponentCollection, IReadOnlyMetadataContext?>? OnComponentCollectionCreated { get; set; }

        #endregion

        #region Implementation of interfaces

        void IComponentCollectionProviderListener.OnComponentCollectionCreated(IComponentCollectionProvider provider, IComponentCollection componentCollection, IReadOnlyMetadataContext? metadata)
        {
            OnComponentCollectionCreated?.Invoke(provider, componentCollection, metadata);
        }

        #endregion
    }
}