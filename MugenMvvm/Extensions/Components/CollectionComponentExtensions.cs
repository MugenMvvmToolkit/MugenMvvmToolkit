using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Collections.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class CollectionComponentExtensions
    {
        #region Methods

        public static void OnBeginBatchUpdate(this ItemOrArray<ICollectionBatchUpdateListener> listeners, ICollection collection)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnBeginBatchUpdate(collection);
        }

        public static void OnEndBatchUpdate(this ItemOrArray<ICollectionBatchUpdateListener> listeners, ICollection collection)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnEndBatchUpdate(collection);
        }

        public static bool CanAdd<T>(this ItemOrArray<IConditionCollectionComponent<T>> components, IReadOnlyCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in components)
            {
                if (!c.CanAdd(collection, item, index))
                    return false;
            }

            return true;
        }

        public static bool CanReplace<T>(this ItemOrArray<IConditionCollectionComponent<T>> components, IReadOnlyCollection<T> collection, T oldItem, T newItem, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in components)
            {
                if (!c.CanReplace(collection, oldItem, newItem, index))
                    return false;
            }

            return true;
        }

        public static bool CanMove<T>(this ItemOrArray<IConditionCollectionComponent<T>> components, IReadOnlyCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in components)
            {
                if (!c.CanMove(collection, item, oldIndex, newIndex))
                    return false;
            }

            return true;
        }

        public static bool CanRemove<T>(this ItemOrArray<IConditionCollectionComponent<T>> components, IReadOnlyCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in components)
            {
                if (!c.CanRemove(collection, item, index))
                    return false;
            }

            return true;
        }

        public static bool CanReset<T>(this ItemOrArray<IConditionCollectionComponent<T>> components, IReadOnlyCollection<T> collection, IEnumerable<T>? items)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in components)
            {
                if (!c.CanReset(collection, items))
                    return false;
            }

            return true;
        }

        public static void OnAdding<T>(this ItemOrArray<ICollectionChangingListener<T>> listeners, IReadOnlyCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnAdding(collection, item, index);
        }

        public static void OnReplacing<T>(this ItemOrArray<ICollectionChangingListener<T>> listeners, IReadOnlyCollection<T> collection, T oldItem, T newItem, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnReplacing(collection, oldItem, newItem, index);
        }

        public static void OnMoving<T>(this ItemOrArray<ICollectionChangingListener<T>> listeners, IReadOnlyCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnMoving(collection, item, oldIndex, newIndex);
        }

        public static void OnRemoving<T>(this ItemOrArray<ICollectionChangingListener<T>> listeners, IReadOnlyCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnRemoving(collection, item, index);
        }

        public static void OnResetting<T>(this ItemOrArray<ICollectionChangingListener<T>> listeners, IReadOnlyCollection<T> collection, IEnumerable<T>? items)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnResetting(collection, items);
        }

        public static void OnItemChanged<T>(this ItemOrArray<ICollectionChangedListener<T>> listeners, IReadOnlyCollection<T> collection, T item, int index, object? args)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnItemChanged(collection, item, index, args);
        }

        public static void OnAdded<T>(this ItemOrArray<ICollectionChangedListener<T>> listeners, IReadOnlyCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnAdded(collection, item, index);
        }

        public static void OnReplaced<T>(this ItemOrArray<ICollectionChangedListener<T>> listeners, IReadOnlyCollection<T> collection, T oldItem, T newItem, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnReplaced(collection, oldItem, newItem, index);
        }

        public static void OnMoved<T>(this ItemOrArray<ICollectionChangedListener<T>> listeners, IReadOnlyCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnMoved(collection, item, oldIndex, newIndex);
        }

        public static void OnRemoved<T>(this ItemOrArray<ICollectionChangedListener<T>> listeners, IReadOnlyCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnRemoved(collection, item, index);
        }

        public static void OnReset<T>(this ItemOrArray<ICollectionChangedListener<T>> listeners, IReadOnlyCollection<T> collection, IEnumerable<T>? items)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnReset(collection, items);
        }

        public static void OnItemChanged(this ItemOrArray<ICollectionDecoratorListener> listeners, ICollection collection, object? item, int index, object? args)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnItemChanged(collection, item, index, args);
        }

        public static void OnAdded(this ItemOrArray<ICollectionDecoratorListener> listeners, ICollection collection, object? item, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnAdded(collection, item, index);
        }

        public static void OnReplaced(this ItemOrArray<ICollectionDecoratorListener> listeners, ICollection collection, object? oldItem, object? newItem, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnReplaced(collection, oldItem, newItem, index);
        }

        public static void OnMoved(this ItemOrArray<ICollectionDecoratorListener> listeners, ICollection collection, object? item, int oldIndex, int newIndex)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnMoved(collection, item, oldIndex, newIndex);
        }

        public static void OnRemoved(this ItemOrArray<ICollectionDecoratorListener> listeners, ICollection collection, object? item, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnRemoved(collection, item, index);
        }

        public static void OnReset(this ItemOrArray<ICollectionDecoratorListener> listeners, ICollection collection, IEnumerable<object?>? items)
        {
            Should.NotBeNull(collection, nameof(collection));
            foreach (var c in listeners)
                c.OnReset(collection, items);
        }

        #endregion
    }
}