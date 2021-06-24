using System;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Collections.Components
{
    public sealed class CollectionDecoratorManagerInitializer : IComponentCollectionManagerListener, IComponentCollectionChangingListener
    {
        bool IComponentCollectionChangingListener.OnAdding(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is ICollectionDecorator || component is ICollectionDecoratorListener)
            {
                collection.RemoveComponent(this, metadata);
                var itemType = MugenExtensions.GetCollectionItemType(collection.Owner);
                var instance = Activator.CreateInstance(typeof(CollectionDecoratorManager<>).MakeGenericType(itemType));
                if (instance != null)
                    collection.TryAdd(instance, metadata);
            }

            return true;
        }

        bool IComponentCollectionChangingListener.OnRemoving(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => true;

        void IComponentCollectionManagerListener.OnComponentCollectionCreated(IComponentCollectionManager collectionManager, IComponentCollection collection,
            IReadOnlyMetadataContext? metadata)
        {
            if (collection.Owner is IReadOnlyObservableCollection)
                collection.AddComponent(this, metadata);
        }
    }
}