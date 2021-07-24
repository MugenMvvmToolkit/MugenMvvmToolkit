using System.Collections.Generic;
using MugenMvvm.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Collections.Components
{
    public abstract class CollectionDecoratorBase : AttachableComponentBase<IReadOnlyObservableCollection>, ICollectionDecorator, IHasPriority
    {
        protected CollectionDecoratorBase(int priority)
        {
            Priority = priority;
        }

        public abstract bool HasAdditionalItems { get; }

        public int Priority { get; set; }

        protected internal ICollectionDecoratorManagerComponent? DecoratorManager { get; private set; }

        protected abstract IEnumerable<object?> Decorate(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection,
            IEnumerable<object?> items);

        protected abstract bool OnChanged(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index,
            ref object? args);

        protected abstract bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index);

        protected abstract bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index);

        protected abstract bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex);

        protected abstract bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index);

        protected abstract bool OnReset(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items);

        protected virtual bool TryGetIndexes(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, IEnumerable<object?> items,
            object? item, ref ItemOrListEditor<int> indexes) => false;

        protected override void OnDetached(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            DecoratorManager = null;
        }

        protected override void OnAttached(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttached(owner, metadata);
            DecoratorManager = owner.GetComponent<ICollectionDecoratorManagerComponent>();
        }

        bool ICollectionDecorator.TryGetIndexes(IReadOnlyObservableCollection collection, IEnumerable<object?> items, object? item, ref ItemOrListEditor<int> indexes)
        {
            var decoratorManager = DecoratorManager;
            if (decoratorManager == null)
                return false;

            return TryGetIndexes(decoratorManager, collection, items, item, ref indexes);
        }

        IEnumerable<object?> ICollectionDecorator.Decorate(IReadOnlyObservableCollection collection, IEnumerable<object?> items)
        {
            var decoratorManager = DecoratorManager;
            return decoratorManager == null ? items : Decorate(decoratorManager, collection, items);
        }

        bool ICollectionDecorator.OnChanged(IReadOnlyObservableCollection collection, ref object? item, ref int index, ref object? args)
        {
            var decoratorManager = DecoratorManager;
            return decoratorManager == null || OnChanged(decoratorManager, collection, ref item, ref index, ref args);
        }

        bool ICollectionDecorator.OnAdded(IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            var decoratorManager = DecoratorManager;
            return decoratorManager == null || OnAdded(decoratorManager, collection, ref item, ref index);
        }

        bool ICollectionDecorator.OnReplaced(IReadOnlyObservableCollection collection, ref object? oldItem, ref object? newItem, ref int index)
        {
            var decoratorManager = DecoratorManager;
            return decoratorManager == null || OnReplaced(decoratorManager, collection, ref oldItem, ref newItem, ref index);
        }

        bool ICollectionDecorator.OnMoved(IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex, ref int newIndex)
        {
            var decoratorManager = DecoratorManager;
            return decoratorManager == null || OnMoved(decoratorManager, collection, ref item, ref oldIndex, ref newIndex);
        }

        bool ICollectionDecorator.OnRemoved(IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            var decoratorManager = DecoratorManager;
            return decoratorManager == null || OnRemoved(decoratorManager, collection, ref item, ref index);
        }

        bool ICollectionDecorator.OnReset(IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            var decoratorManager = DecoratorManager;
            return decoratorManager == null || OnReset(decoratorManager, collection, ref items);
        }
    }
}