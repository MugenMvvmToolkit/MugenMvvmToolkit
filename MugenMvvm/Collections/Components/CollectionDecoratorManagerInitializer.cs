using System;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Collections.Components
{
    public sealed class CollectionDecoratorManagerInitializer : IComponentCollectionManagerListener, IConditionComponentCollectionComponent
    {
        void IComponentCollectionManagerListener.OnComponentCollectionCreated(IComponentCollectionManager collectionManager, IComponentCollection collection,
            IReadOnlyMetadataContext? metadata)
        {
            if (collection.Owner is IReadOnlyObservableCollection)
                collection.AddComponent(this, metadata);
        }

        bool IConditionComponentCollectionComponent.CanAdd(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is ICollectionDecorator or IDecoratedCollectionChangedListener or ICollectionItemChangedListener)
            {
                collection.RemoveComponent(this, metadata);
                var itemType = ((IReadOnlyObservableCollection)collection.Owner).ItemType;
                if (itemType.IsValueType)
                {
                    var instance = Activator.CreateInstance(typeof(CollectionDecoratorManager<>).MakeGenericType(itemType));
                    if (instance != null)
                        collection.TryAdd(instance, metadata);
                }
                else
                    collection.TryAdd(new CollectionDecoratorManager<object>(), metadata);
            }

            return true;
        }

        bool IConditionComponentCollectionComponent.CanRemove(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => true;
    }
}