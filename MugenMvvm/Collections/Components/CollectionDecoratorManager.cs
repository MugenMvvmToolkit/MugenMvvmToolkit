using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Internal;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public sealed class CollectionDecoratorManager<T> : AttachableComponentBase<IReadOnlyObservableCollection>, ICollectionDecoratorManagerComponent, ICollectionChangedListener<T>,
        IComponentCollectionChangedListener, IDisposableComponent<IReadOnlyObservableCollection>, IHasPriority
    {
        private const int InvalidDecoratorIndex = -1;
        private bool _isInitialized;

        [Preserve]
        public CollectionDecoratorManager() : this(CollectionComponentPriority.DecoratorManager)
        {
        }

        public CollectionDecoratorManager(int priority)
        {
            Priority = priority;
        }

        public int Priority { get; init; }

        public IEnumerable<object?> Decorate(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator = null)
        {
            using var _ = decorator == null || !_isInitialized ? collection.Lock() : default;
            if (!_isInitialized)
                Reset(null, true, false);

            var items = collection.AsEnumerable();
            var decorators = GetDecorators(collection, decorator, out var index, true);
            if (index == InvalidDecoratorIndex)
                yield break;

            for (var i = 0; i < index; i++)
                items = decorators[i].Decorate(collection, items);

            foreach (var item in items)
                yield return item;
        }

        public void RaiseItemChanged(IReadOnlyObservableCollection collection, object? item, object? args)
        {
            using var l = collection.Lock();
            if (!_isInitialized)
                return;

            var indexes = new ItemOrListEditor<int>();
            var items = collection.AsEnumerable();
            items.FindAllIndexOf(item, ref indexes);

            if (indexes.Count != 0)
            {
                OnChanged(collection, collection.GetComponents<ICollectionDecorator>(), 0, item, indexes, args);
                return;
            }

            var decorators = collection.GetComponents<ICollectionDecorator>();
            for (var i = 0; i < decorators.Count; i++)
            {
                var decorator = decorators[i];
                items = decorator.Decorate(collection, items);
                FindAllIndexOf(collection, decorator, items, item, ref indexes);
                if (indexes.Count != 0)
                {
                    OnChanged(collection, decorators, i + 1, item, indexes, args);
                    return;
                }
            }
        }

        public void OnChanged(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, object? item, int index, object? args)
        {
            if (!_isInitialized)
                return;
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            if (startIndex == InvalidDecoratorIndex)
                return;

            using var token = BatchUpdate(collection);
            for (var i = startIndex; i < decorators.Count; i++)
            {
                if (!decorators[i].OnChanged(collection, ref item, ref index, ref args))
                    return;
            }

            collection.GetComponents<IDecoratedCollectionChangedListener>().OnChanged(collection, item, index, args);
        }

        public void OnAdded(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, object? item, int index)
        {
            if (!_isInitialized)
                return;
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            if (startIndex == InvalidDecoratorIndex)
                return;

            using var token = BatchUpdate(collection);
            for (var i = startIndex; i < decorators.Count; i++)
            {
                if (!decorators[i].OnAdded(collection, ref item, ref index))
                    return;
            }

            collection.GetComponents<IDecoratedCollectionChangedListener>().OnAdded(collection, item, index);
        }

        public void OnReplaced(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, object? oldItem, object? newItem, int index)
        {
            if (!_isInitialized)
                return;
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            if (startIndex == InvalidDecoratorIndex)
                return;

            using var token = BatchUpdate(collection);
            for (var i = startIndex; i < decorators.Count; i++)
            {
                if (!decorators[i].OnReplaced(collection, ref oldItem, ref newItem, ref index) || ReferenceEquals(oldItem, newItem))
                    return;
            }

            collection.GetComponents<IDecoratedCollectionChangedListener>().OnReplaced(collection, oldItem, newItem, index);
        }

        public void OnMoved(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, object? item, int oldIndex, int newIndex)
        {
            if (!_isInitialized)
                return;
            if (oldIndex == newIndex)
                return;

            var decorators = GetDecorators(collection, decorator, out var startIndex);
            if (startIndex == InvalidDecoratorIndex)
                return;

            using var token = BatchUpdate(collection);
            for (var i = startIndex; i < decorators.Count; i++)
            {
                if (!decorators[i].OnMoved(collection, ref item, ref oldIndex, ref newIndex) || oldIndex == newIndex)
                    return;
            }

            collection.GetComponents<IDecoratedCollectionChangedListener>().OnMoved(collection, item, oldIndex, newIndex);
        }

        public void OnRemoved(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, object? item, int index)
        {
            if (!_isInitialized)
                return;
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            if (startIndex == InvalidDecoratorIndex)
                return;

            using var token = BatchUpdate(collection);
            for (var i = startIndex; i < decorators.Count; i++)
            {
                if (!decorators[i].OnRemoved(collection, ref item, ref index))
                    return;
            }

            collection.GetComponents<IDecoratedCollectionChangedListener>().OnRemoved(collection, item, index);
        }

        public void OnReset(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, IEnumerable<object?>? items)
        {
            if (!_isInitialized)
                return;
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            if (startIndex == InvalidDecoratorIndex)
                return;

            using var token = BatchUpdate(collection);
            for (var i = startIndex; i < decorators.Count; i++)
            {
                if (!decorators[i].OnReset(collection, ref items))
                    return;
            }

            collection.GetComponents<IDecoratedCollectionChangedListener>().OnReset(collection, items);
        }

        protected override void OnAttaching(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnAttaching(owner, metadata);
            owner.Components.AddComponent(this);
        }

        protected override void OnDetached(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            owner.Components.RemoveComponent(this);
        }

        private static void FindAllIndexOf(IReadOnlyObservableCollection collection, ICollectionDecorator decorator, IEnumerable<object?> items, object? item,
            ref ItemOrListEditor<int> indexes)
        {
            if (!decorator.HasAdditionalItems)
                return;

            indexes.Clear();
            if (!decorator.TryGetIndexes(collection, items, item, ref indexes))
                items.FindAllIndexOf(item, ref indexes);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ActionToken BatchUpdate(IReadOnlyObservableCollection collection) => collection.BatchUpdateDecorators(collection.GetBatchUpdateManager());

        private static void OnChanged(IReadOnlyObservableCollection collection, ItemOrArray<ICollectionDecorator> decorators, int startIndex, object? item,
            ItemOrIReadOnlyList<int> indexes, object? args)
        {
            using var token = BatchUpdate(collection);
            foreach (var currentIndex in indexes)
            {
                var index = currentIndex;
                for (var i = startIndex; i < decorators.Count; i++)
                {
                    if (!decorators[i].OnChanged(collection, ref item, ref index, ref args))
                        return;
                }

                collection.GetComponents<IDecoratedCollectionChangedListener>().OnChanged(collection, item, index, args);
            }
        }

        private static ItemOrArray<ICollectionDecorator> GetDecorators(IReadOnlyObservableCollection collection, ICollectionDecorator? decorator, out int index,
            bool isLengthDefault = false)
        {
            var components = collection.GetComponents<ICollectionDecorator>();
            index = isLengthDefault ? components.Count : 0;
            if (decorator == null)
                return components;

            var found = false;
            for (var i = 0; i < components.Count; i++)
            {
                if (components[i] == decorator)
                {
                    found = true;
                    index = i;
                    if (!isLengthDefault)
                        ++index;
                    break;
                }
            }

            if (!found)
            {
                index = InvalidDecoratorIndex;
                return default;
            }

            return components;
        }

        private void Reset(object? component, bool force, bool remove)
        {
            if (force || CanReset(component))
            {
                var collection = OwnerOptional;
                if (collection == null)
                    return;

                if (!_isInitialized)
                {
                    _isInitialized = true;
                    OnReset(collection, null, collection.AsEnumerable());
                    return;
                }

                if (!remove && component is ICollectionDecorator decorator)
                {
                    var items = Decorate(collection, decorator);
                    if (decorator.OnReset(collection, ref items))
                        OnReset(collection, decorator, items);
                }
                else
                    OnReset(collection, null, collection.AsEnumerable());
            }
        }

        private bool CanReset(object? component)
        {
            if (_isInitialized)
                return component == null || component is ICollectionDecorator && component is not IListenerCollectionDecorator;
            return component is IListenerCollectionDecorator or IDecoratedCollectionChangedListener or ICollectionDecorator { IsLazy: false };
        }

        void ICollectionChangedListener<T>.OnAdded(IReadOnlyObservableCollection<T> collection, T item, int index) => OnAdded(collection, null, item, index);

        void ICollectionChangedListener<T>.OnReplaced(IReadOnlyObservableCollection<T> collection, T oldItem, T newItem, int index) =>
            OnReplaced(collection, null, oldItem, newItem, index);

        void ICollectionChangedListener<T>.OnMoved(IReadOnlyObservableCollection<T> collection, T item, int oldIndex, int newIndex) =>
            OnMoved(collection, null, item, oldIndex, newIndex);

        void ICollectionChangedListener<T>.OnRemoved(IReadOnlyObservableCollection<T> collection, T item, int index) =>
            OnRemoved(collection, null, item, index);

        void ICollectionChangedListener<T>.OnReset(IReadOnlyObservableCollection<T> collection, IEnumerable<T>? items)
        {
            if (items == null)
                OnReset(collection, null, null);
            else
                OnReset(collection, null, items as IEnumerable<object?> ?? items.Cast<object?>());
        }

        void IComponentCollectionChangedListener.OnAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => Reset(component, false, false);

        void IComponentCollectionChangedListener.OnRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata) => Reset(component, false, true);

        void IDisposableComponent<IReadOnlyObservableCollection>.Dispose(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            owner.RemoveComponents<IDecoratedCollectionChangedListener>(metadata);
            owner.RemoveComponents<ICollectionDecoratorManagerComponent>(metadata);
            owner.RemoveComponents<IDecoratedCollectionChangedListener>(metadata);
            owner.ClearComponents(metadata);
        }
    }
}