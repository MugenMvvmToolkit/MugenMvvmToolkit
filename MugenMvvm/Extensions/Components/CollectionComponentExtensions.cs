using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class CollectionComponentExtensions
    {
        #region Methods

        public static void OnBeginBatchUpdate(this ICollectionBatchUpdateListener[] listeners, ICollection collection)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnBeginBatchUpdate(collection);
        }

        public static void OnEndBatchUpdate(this ICollectionBatchUpdateListener[] listeners, ICollection collection)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnEndBatchUpdate(collection);
        }

        public static bool CanAdd<T>(this IConditionCollectionComponent<T>[] components, IReadOnlyCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanAdd(collection, item, index))
                    return false;
            }

            return true;
        }

        public static bool CanReplace<T>(this IConditionCollectionComponent<T>[] components, IReadOnlyCollection<T> collection, T oldItem, T newItem, int index)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanReplace(collection, oldItem, newItem, index))
                    return false;
            }

            return true;
        }

        public static bool CanMove<T>(this IConditionCollectionComponent<T>[] components, IReadOnlyCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanMove(collection, item, oldIndex, newIndex))
                    return false;
            }

            return true;
        }

        public static bool CanRemove<T>(this IConditionCollectionComponent<T>[] components, IReadOnlyCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanRemove(collection, item, index))
                    return false;
            }

            return true;
        }

        public static bool CanReset<T>(this IConditionCollectionComponent<T>[] components, IReadOnlyCollection<T> collection, IEnumerable<T>? items)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanReset(collection, items))
                    return false;
            }

            return true;
        }

        public static void OnAdding<T>(this ICollectionChangingListener<T>[] listeners, IReadOnlyCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnAdding(collection, item, index);
        }

        public static void OnReplacing<T>(this ICollectionChangingListener<T>[] listeners, IReadOnlyCollection<T> collection, T oldItem, T newItem, int index)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnReplacing(collection, oldItem, newItem, index);
        }

        public static void OnMoving<T>(this ICollectionChangingListener<T>[] listeners, IReadOnlyCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnMoving(collection, item, oldIndex, newIndex);
        }

        public static void OnRemoving<T>(this ICollectionChangingListener<T>[] listeners, IReadOnlyCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnRemoving(collection, item, index);
        }

        public static void OnResetting<T>(this ICollectionChangingListener<T>[] listeners, IReadOnlyCollection<T> collection, IEnumerable<T>? items)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnResetting(collection, items);
        }

        public static void OnItemChanged<T>(this ICollectionChangedListener<T>[] listeners, IReadOnlyCollection<T> collection, T item, int index, object? args)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnItemChanged(collection, item, index, args);
        }

        public static void OnAdded<T>(this ICollectionChangedListener<T>[] listeners, IReadOnlyCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnAdded(collection, item, index);
        }

        public static void OnReplaced<T>(this ICollectionChangedListener<T>[] listeners, IReadOnlyCollection<T> collection, T oldItem, T newItem, int index)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnReplaced(collection, oldItem, newItem, index);
        }

        public static void OnMoved<T>(this ICollectionChangedListener<T>[] listeners, IReadOnlyCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnMoved(collection, item, oldIndex, newIndex);
        }

        public static void OnRemoved<T>(this ICollectionChangedListener<T>[] listeners, IReadOnlyCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnRemoved(collection, item, index);
        }

        public static void OnReset<T>(this ICollectionChangedListener<T>[] listeners, IReadOnlyCollection<T> collection, IEnumerable<T>? items)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnReset(collection, items);
        }

        public static void OnItemChanged(this ICollectionDecoratorListener[] listeners, ICollection collection, object? item, int index, object? args)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnItemChanged(collection, item, index, args);
        }

        public static void OnAdded(this ICollectionDecoratorListener[] listeners, ICollection collection, object? item, int index)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnAdded(collection, item, index);
        }

        public static void OnReplaced(this ICollectionDecoratorListener[] listeners, ICollection collection, object? oldItem, object? newItem, int index)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnReplaced(collection, oldItem, newItem, index);
        }

        public static void OnMoved(this ICollectionDecoratorListener[] listeners, ICollection collection, object? item, int oldIndex, int newIndex)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnMoved(collection, item, oldIndex, newIndex);
        }

        public static void OnRemoved(this ICollectionDecoratorListener[] listeners, ICollection collection, object? item, int index)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnRemoved(collection, item, index);
        }

        public static void OnReset(this ICollectionDecoratorListener[] listeners, ICollection collection, IEnumerable<object?>? items)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnReset(collection, items);
        }

        #endregion
    }
}