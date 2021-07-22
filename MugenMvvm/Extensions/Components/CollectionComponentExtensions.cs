using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Extensions.Components
{
    public static class CollectionComponentExtensions
    {
        public static void OnBeginBatchUpdate(this ItemOrArray<ICollectionBatchUpdateListener> listeners, IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnBeginBatchUpdate(collection, batchUpdateType);
        }

        public static void OnEndBatchUpdate(this ItemOrArray<ICollectionBatchUpdateListener> listeners, IReadOnlyObservableCollection collection, BatchUpdateType batchUpdateType)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnEndBatchUpdate(collection, batchUpdateType);
        }

        public static bool CanAdd<T>(this ItemOrArray<IConditionCollectionComponent<T>> components, IReadOnlyObservableCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in components)
            {
                if (!c.CanAdd(collection, item, index))
                    return false;
            }

            return true;
        }

        public static bool CanReplace<T>(this ItemOrArray<IConditionCollectionComponent<T>> components, IReadOnlyObservableCollection<T> collection, T oldItem, T newItem,
            int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in components)
            {
                if (!c.CanReplace(collection, oldItem, newItem, index))
                    return false;
            }

            return true;
        }

        public static bool CanMove<T>(this ItemOrArray<IConditionCollectionComponent<T>> components, IReadOnlyObservableCollection<T> collection, T item, int oldIndex,
            int newIndex)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in components)
            {
                if (!c.CanMove(collection, item, oldIndex, newIndex))
                    return false;
            }

            return true;
        }

        public static bool CanRemove<T>(this ItemOrArray<IConditionCollectionComponent<T>> components, IReadOnlyObservableCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in components)
            {
                if (!c.CanRemove(collection, item, index))
                    return false;
            }

            return true;
        }

        public static bool CanReset<T>(this ItemOrArray<IConditionCollectionComponent<T>> components, IReadOnlyObservableCollection<T> collection, IReadOnlyCollection<T>? items)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in components)
            {
                if (!c.CanReset(collection, items))
                    return false;
            }

            return true;
        }

        public static void OnAdding<T>(this ItemOrArray<ICollectionChangingListener<T>> listeners, IReadOnlyObservableCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnAdding(collection, item, index);
        }

        public static void OnReplacing<T>(this ItemOrArray<ICollectionChangingListener<T>> listeners, IReadOnlyObservableCollection<T> collection, T oldItem, T newItem, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnReplacing(collection, oldItem, newItem, index);
        }

        public static void OnMoving<T>(this ItemOrArray<ICollectionChangingListener<T>> listeners, IReadOnlyObservableCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnMoving(collection, item, oldIndex, newIndex);
        }

        public static void OnRemoving<T>(this ItemOrArray<ICollectionChangingListener<T>> listeners, IReadOnlyObservableCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnRemoving(collection, item, index);
        }

        public static void OnResetting<T>(this ItemOrArray<ICollectionChangingListener<T>> listeners, IReadOnlyObservableCollection<T> collection, IReadOnlyCollection<T>? items)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnResetting(collection, items);
        }

        public static void OnAdded<T>(this ItemOrArray<ICollectionChangedListener<T>> listeners, IReadOnlyObservableCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnAdded(collection, item, index);
        }

        public static void OnReplaced<T>(this ItemOrArray<ICollectionChangedListener<T>> listeners, IReadOnlyObservableCollection<T> collection, T oldItem, T newItem, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnReplaced(collection, oldItem, newItem, index);
        }

        public static void OnMoved<T>(this ItemOrArray<ICollectionChangedListener<T>> listeners, IReadOnlyObservableCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnMoved(collection, item, oldIndex, newIndex);
        }

        public static void OnRemoved<T>(this ItemOrArray<ICollectionChangedListener<T>> listeners, IReadOnlyObservableCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnRemoved(collection, item, index);
        }

        public static void OnReset<T>(this ItemOrArray<ICollectionChangedListener<T>> listeners, IReadOnlyObservableCollection<T> collection, IReadOnlyCollection<T>? items)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnReset(collection, items);
        }

        public static void OnChanged(this ItemOrArray<ICollectionDecoratorListener> listeners, IReadOnlyObservableCollection collection, object? item, int index, object? args)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnChanged(collection, item, index, args);
        }

        public static void OnAdded(this ItemOrArray<ICollectionDecoratorListener> listeners, IReadOnlyObservableCollection collection, object? item, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnAdded(collection, item, index);
        }

        public static void OnReplaced(this ItemOrArray<ICollectionDecoratorListener> listeners, IReadOnlyObservableCollection collection, object? oldItem, object? newItem,
            int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnReplaced(collection, oldItem, newItem, index);
        }

        public static void OnMoved(this ItemOrArray<ICollectionDecoratorListener> listeners, IReadOnlyObservableCollection collection, object? item, int oldIndex, int newIndex)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnMoved(collection, item, oldIndex, newIndex);
        }

        public static void OnRemoved(this ItemOrArray<ICollectionDecoratorListener> listeners, IReadOnlyObservableCollection collection, object? item, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnRemoved(collection, item, index);
        }

        public static void OnReset(this ItemOrArray<ICollectionDecoratorListener> listeners, IReadOnlyObservableCollection collection, IEnumerable<object?>? items)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnReset(collection, items);
        }

        public static void Initialize<T>(this ItemOrArray<ICollectionItemPreInitializerComponent<T>> components, IReadOnlyObservableCollection<T> collection, T item)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in components)
                c.Initialize(collection, item);
        }
    }
}