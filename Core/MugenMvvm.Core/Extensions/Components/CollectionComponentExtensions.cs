using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class CollectionComponentExtensions
    {
        #region Methods

        public static void OnBeginBatchUpdate<T>(this IObservableCollectionBatchUpdateListener<T>[] listeners, IObservableCollection<T> collection)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnBeginBatchUpdate(collection);
        }

        public static void OnEndBatchUpdate<T>(this IObservableCollectionBatchUpdateListener<T>[] listeners, IObservableCollection<T> collection)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnEndBatchUpdate(collection);
        }

        public static bool CanAdd<T>(this IConditionObservableCollectionComponent<T>[] components, IObservableCollection<T> collection, T item, int index)
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

        public static void OnAdding<T>(this IObservableCollectionChangingListener<T>[] listeners, IObservableCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnAdding(collection, item, index);
        }

        public static bool CanReplace<T>(this IConditionObservableCollectionComponent<T>[] components, IObservableCollection<T> collection, T oldItem, T newItem, int index)
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

        public static void OnReplacing<T>(this IObservableCollectionChangingListener<T>[] listeners, IObservableCollection<T> collection, T oldItem, T newItem, int index)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnReplacing(collection, oldItem, newItem, index);
        }

        public static bool CanMove<T>(this IConditionObservableCollectionComponent<T>[] components, IObservableCollection<T> collection, T item, int oldIndex, int newIndex)
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

        public static void OnMoving<T>(this IObservableCollectionChangingListener<T>[] listeners, IObservableCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnMoving(collection, item, oldIndex, newIndex);
        }

        public static bool CanRemove<T>(this IConditionObservableCollectionComponent<T>[] components, IObservableCollection<T> collection, T item, int index)
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

        public static void OnRemoving<T>(this IObservableCollectionChangingListener<T>[] listeners, IObservableCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnRemoving(collection, item, index);
        }

        public static bool CanReset<T>(this IConditionObservableCollectionComponent<T>[] components, IObservableCollection<T> collection, IEnumerable<T> items)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(items, nameof(items));
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanReset(collection, items))
                    return false;
            }

            return true;
        }

        public static void OnResetting<T>(this IObservableCollectionChangingListener<T>[] listeners, IObservableCollection<T> collection, IEnumerable<T> items)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(items, nameof(items));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnResetting(collection, items);
        }

        public static bool CanClear<T>(this IConditionObservableCollectionComponent<T>[] components, IObservableCollection<T> collection)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanClear(collection))
                    return false;
            }

            return true;
        }

        public static void OnClearing<T>(this IObservableCollectionChangingListener<T>[] listeners, IObservableCollection<T> collection)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnClearing(collection);
        }

        public static void OnAdded<T>(this IObservableCollectionChangedListener<T>[] listeners, IObservableCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnAdded(collection, item, index);
        }

        public static void OnReplaced<T>(this IObservableCollectionChangedListener<T>[] listeners, IObservableCollection<T> collection, T oldItem, T newItem, int index)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnReplaced(collection, oldItem, newItem, index);
        }

        public static void OnMoved<T>(this IObservableCollectionChangedListener<T>[] listeners, IObservableCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnMoved(collection, item, oldIndex, newIndex);
        }

        public static void OnRemoved<T>(this IObservableCollectionChangedListener<T>[] listeners, IObservableCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnRemoved(collection, item, index);
        }

        public static void OnReset<T>(this IObservableCollectionChangedListener<T>[] listeners, IObservableCollection<T> collection, IEnumerable<T> items)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(items, nameof(items));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnReset(collection, items);
        }

        public static void OnCleared<T>(this IObservableCollectionChangedListener<T>[] listeners, IObservableCollection<T> collection)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnCleared(collection);
        }

        public static void OnItemChanged<T>(this IObservableCollectionChangedListener<T>[] listeners, IObservableCollection<T> collection, T item, int index, object? args)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnItemChanged(collection, item, index, args);
        }

        public static void OnAdded<T>(this IObservableCollectionChangedDecoratorListener<T>[] listeners, IObservableCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnAdded(collection, item, index);
        }

        public static void OnReplaced<T>(this IObservableCollectionChangedDecoratorListener<T>[] listeners, IObservableCollection<T> collection, T oldItem, T newItem, int index)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnReplaced(collection, oldItem, newItem, index);
        }

        public static void OnMoved<T>(this IObservableCollectionChangedDecoratorListener<T>[] listeners, IObservableCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnMoved(collection, item, oldIndex, newIndex);
        }

        public static void OnRemoved<T>(this IObservableCollectionChangedDecoratorListener<T>[] listeners, IObservableCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnRemoved(collection, item, index);
        }

        public static void OnReset<T>(this IObservableCollectionChangedDecoratorListener<T>[] listeners, IObservableCollection<T> collection, IEnumerable<T> items)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            Should.NotBeNull(items, nameof(items));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnReset(collection, items);
        }

        public static void OnCleared<T>(this IObservableCollectionChangedDecoratorListener<T>[] listeners, IObservableCollection<T> collection)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnCleared(collection);
        }

        public static void OnItemChanged<T>(this IObservableCollectionChangedDecoratorListener<T>[] listeners, IObservableCollection<T> collection, T item, int index, object? args)
        {
            Should.NotBeNull(listeners, nameof(listeners));
            Should.NotBeNull(collection, nameof(collection));
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnItemChanged(collection, item, index, args);
        }

        #endregion
    }
}