using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class ObservableCollectionComponentExtensions
    {
        #region Methods

        public static void OnBeginBatchUpdate<T>(this IObservableCollection<T> collection, IObservableCollectionBatchUpdateListener<T>[]? listeners = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (listeners == null)
                listeners = collection.Components.Get<IObservableCollectionBatchUpdateListener<T>>();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnBeginBatchUpdate(collection);
        }

        public static void OnEndBatchUpdate<T>(this IObservableCollection<T> collection, IObservableCollectionBatchUpdateListener<T>[]? listeners = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (listeners == null)
                listeners = collection.Components.Get<IObservableCollectionBatchUpdateListener<T>>();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnEndBatchUpdate(collection);
        }

        public static bool CanAdd<T>(this IObservableCollection<T> collection, T item, int index, IConditionObservableCollectionComponent<T>[]? components = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (components == null)
                components = collection.Components.Get<IConditionObservableCollectionComponent<T>>();
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanAdd(collection, item, index))
                    return false;
            }

            return true;
        }

        public static void OnAdding<T>(this IObservableCollection<T> collection, T item, int index, IObservableCollectionChangingListener<T>[]? listeners = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (listeners == null)
                listeners = collection.Components.Get<IObservableCollectionChangingListener<T>>();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnAdding(collection, item, index);
        }

        public static bool CanReplace<T>(this IObservableCollection<T> collection, T oldItem, T newItem, int index, IConditionObservableCollectionComponent<T>[]? components = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (components == null)
                components = collection.Components.Get<IConditionObservableCollectionComponent<T>>();
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanReplace(collection, oldItem, newItem, index))
                    return false;
            }

            return true;
        }

        public static void OnReplacing<T>(this IObservableCollection<T> collection, T oldItem, T newItem, int index, IObservableCollectionChangingListener<T>[]? listeners = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (listeners == null)
                listeners = collection.Components.Get<IObservableCollectionChangingListener<T>>();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnReplacing(collection, oldItem, newItem, index);
        }

        public static bool CanMove<T>(this IObservableCollection<T> collection, T item, int oldIndex, int newIndex, IConditionObservableCollectionComponent<T>[]? components = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (components == null)
                components = collection.Components.Get<IConditionObservableCollectionComponent<T>>();
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanMove(collection, item, oldIndex, newIndex))
                    return false;
            }

            return true;
        }

        public static void OnMoving<T>(this IObservableCollection<T> collection, T item, int oldIndex, int newIndex, IObservableCollectionChangingListener<T>[]? listeners = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (listeners == null)
                listeners = collection.Components.Get<IObservableCollectionChangingListener<T>>();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnMoving(collection, item, oldIndex, newIndex);
        }

        public static bool CanRemove<T>(this IObservableCollection<T> collection, T item, int index, IConditionObservableCollectionComponent<T>[]? components = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (components == null)
                components = collection.Components.Get<IConditionObservableCollectionComponent<T>>();
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanRemove(collection, item, index))
                    return false;
            }

            return true;
        }

        public static void OnRemoving<T>(this IObservableCollection<T> collection, T item, int index, IObservableCollectionChangingListener<T>[]? listeners = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (listeners == null)
                listeners = collection.Components.Get<IObservableCollectionChangingListener<T>>();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnRemoving(collection, item, index);
        }

        public static bool CanReset<T>(this IObservableCollection<T> collection, IEnumerable<T> items, IConditionObservableCollectionComponent<T>[]? components = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (components == null)
                components = collection.Components.Get<IConditionObservableCollectionComponent<T>>();
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanReset(collection, items))
                    return false;
            }

            return true;
        }

        public static void OnResetting<T>(this IObservableCollection<T> collection, IEnumerable<T> items, IObservableCollectionChangingListener<T>[]? listeners = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (listeners == null)
                listeners = collection.Components.Get<IObservableCollectionChangingListener<T>>();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnResetting(collection, items);
        }

        public static bool CanClear<T>(this IObservableCollection<T> collection, IConditionObservableCollectionComponent<T>[]? components = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (components == null)
                components = collection.Components.Get<IConditionObservableCollectionComponent<T>>();
            for (var i = 0; i < components.Length; i++)
            {
                if (!components[i].CanClear(collection))
                    return false;
            }

            return true;
        }

        public static void OnClearing<T>(this IObservableCollection<T> collection, IObservableCollectionChangingListener<T>[]? listeners = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (listeners == null)
                listeners = collection.Components.Get<IObservableCollectionChangingListener<T>>();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnClearing(collection);
        }

        public static void OnAdded<T>(this IObservableCollection<T> collection, T item, int index, IObservableCollectionChangedListener<T>[]? listeners = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (listeners == null)
                listeners = collection.Components.Get<IObservableCollectionChangedListener<T>>();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnAdded(collection, item, index);
        }

        public static void OnReplaced<T>(this IObservableCollection<T> collection, T oldItem, T newItem, int index, IObservableCollectionChangedListener<T>[]? listeners = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (listeners == null)
                listeners = collection.Components.Get<IObservableCollectionChangedListener<T>>();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnReplaced(collection, oldItem, newItem, index);
        }

        public static void OnMoved<T>(this IObservableCollection<T> collection, T item, int oldIndex, int newIndex, IObservableCollectionChangedListener<T>[]? listeners = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (listeners == null)
                listeners = collection.Components.Get<IObservableCollectionChangedListener<T>>();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnMoved(collection, item, oldIndex, newIndex);
        }

        public static void OnRemoved<T>(this IObservableCollection<T> collection, T item, int index, IObservableCollectionChangedListener<T>[]? listeners = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (listeners == null)
                listeners = collection.Components.Get<IObservableCollectionChangedListener<T>>();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnRemoved(collection, item, index);
        }

        public static void OnReset<T>(this IObservableCollection<T> collection, IEnumerable<T> items, IObservableCollectionChangedListener<T>[]? listeners = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (listeners == null)
                listeners = collection.Components.Get<IObservableCollectionChangedListener<T>>();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnReset(collection, items);
        }

        public static void OnCleared<T>(this IObservableCollection<T> collection, IObservableCollectionChangedListener<T>[]? listeners = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (listeners == null)
                listeners = collection.Components.Get<IObservableCollectionChangedListener<T>>();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnCleared(collection);
        }

        public static void OnItemChanged<T>(this IObservableCollection<T> collection, T item, int index, object? args, IObservableCollectionChangedListener<T>[]? listeners = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (listeners == null)
                listeners = collection.Components.Get<IObservableCollectionChangedListener<T>>();
            for (var i = 0; i < listeners.Length; i++)
                listeners[i].OnItemChanged(collection, item, index, args);
        }

        #endregion
    }
}