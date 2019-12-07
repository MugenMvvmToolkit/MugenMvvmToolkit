using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static void ObservableCollectionOnBeginBatchUpdate<T>(IObservableCollection<T> collection)
        {
            Should.NotBeNull(collection, nameof(collection));
            var components = collection.Components.Get<IObservableCollectionBatchUpdateListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnBeginBatchUpdate(collection);
        }

        public static void ObservableCollectionOnEndBatchUpdate<T>(IObservableCollection<T> collection)
        {
            Should.NotBeNull(collection, nameof(collection));
            var components = collection.Components.Get<IObservableCollectionBatchUpdateListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnEndBatchUpdate(collection);
        }

        public static bool ObservableCollectionOnAdding<T>(IObservableCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            var conditionComponents = collection.Components.Get<IConditionObservableCollectionComponent<T>>();
            for (var i = 0; i < conditionComponents.Length; i++)
            {
                if (!conditionComponents[i].CanAdd(collection, item, index))
                    return false;
            }

            var components = collection.Components.Get<IObservableCollectionChangingListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnAdding(collection, item, index);

            return true;
        }

        public static bool ObservableCollectionOnReplacing<T>(IObservableCollection<T> collection, T oldItem, T newItem, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            var conditionComponents = collection.Components.Get<IConditionObservableCollectionComponent<T>>();
            for (var i = 0; i < conditionComponents.Length; i++)
            {
                if (!conditionComponents[i].CanReplace(collection, oldItem, newItem, index))
                    return false;
            }

            var components = collection.Components.Get<IObservableCollectionChangingListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnReplacing(collection, oldItem, newItem, index);

            return true;
        }

        public static bool ObservableCollectionOnMoving<T>(IObservableCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            Should.NotBeNull(collection, nameof(collection));
            var conditionComponents = collection.Components.Get<IConditionObservableCollectionComponent<T>>();
            for (var i = 0; i < conditionComponents.Length; i++)
            {
                if (!conditionComponents[i].CanMove(collection, item, oldIndex, newIndex))
                    return false;
            }

            var components = collection.Components.Get<IObservableCollectionChangingListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnMoving(collection, item, oldIndex, newIndex);

            return true;
        }

        public static bool ObservableCollectionOnRemoving<T>(IObservableCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            var conditionComponents = collection.Components.Get<IConditionObservableCollectionComponent<T>>();
            for (var i = 0; i < conditionComponents.Length; i++)
            {
                if (!conditionComponents[i].CanRemove(collection, item, index))
                    return false;
            }

            var components = collection.Components.Get<IObservableCollectionChangingListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnRemoving(collection, item, index);

            return true;
        }

        public static bool ObservableCollectionOnResetting<T>(IObservableCollection<T> collection, IEnumerable<T> items)
        {
            Should.NotBeNull(collection, nameof(collection));
            var conditionComponents = collection.Components.Get<IConditionObservableCollectionComponent<T>>();
            for (var i = 0; i < conditionComponents.Length; i++)
            {
                if (!conditionComponents[i].CanReset(collection, items))
                    return false;
            }

            var components = collection.Components.Get<IObservableCollectionChangingListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnResetting(collection, items);

            return true;
        }

        public static bool ObservableCollectionOnClearing<T>(IObservableCollection<T> collection)
        {
            Should.NotBeNull(collection, nameof(collection));
            var conditionComponents = collection.Components.Get<IConditionObservableCollectionComponent<T>>();
            for (var i = 0; i < conditionComponents.Length; i++)
            {
                if (!conditionComponents[i].CanClear(collection))
                    return false;
            }

            var components = collection.Components.Get<IObservableCollectionChangingListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnClearing(collection);

            return true;
        }

        public static void ObservableCollectionOnAdded<T>(IObservableCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            var components = collection.Components.Get<IObservableCollectionChangedListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnAdded(collection, item, index);
        }

        public static void ObservableCollectionOnReplaced<T>(IObservableCollection<T> collection, T oldItem, T newItem, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            var components = collection.Components.Get<IObservableCollectionChangedListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnReplaced(collection, oldItem, newItem, index);
        }

        public static void ObservableCollectionOnMoved<T>(IObservableCollection<T> collection, T item, int oldIndex, int newIndex)
        {
            Should.NotBeNull(collection, nameof(collection));
            var components = collection.Components.Get<IObservableCollectionChangedListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnMoved(collection, item, oldIndex, newIndex);
        }

        public static void ObservableCollectionOnRemoved<T>(IObservableCollection<T> collection, T item, int index)
        {
            Should.NotBeNull(collection, nameof(collection));
            var components = collection.Components.Get<IObservableCollectionChangedListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnRemoved(collection, item, index);
        }

        public static void ObservableCollectionOnReset<T>(IObservableCollection<T> collection, IEnumerable<T> items)
        {
            Should.NotBeNull(collection, nameof(collection));
            var components = collection.Components.Get<IObservableCollectionChangedListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnReset(collection, items);
        }

        public static void ObservableCollectionOnCleared<T>(IObservableCollection<T> collection)
        {
            Should.NotBeNull(collection, nameof(collection));
            var components = collection.Components.Get<IObservableCollectionChangedListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnCleared(collection);
        }

        public static void ObservableCollectionOnItemChanged<T>(IObservableCollection<T> collection, IDecoratorObservableCollectionComponent<T>? decorator, T item, int index, object? args)
        {
            Should.NotBeNull(collection, nameof(collection));
            var components = collection.Components.Get<IObservableCollectionChangedListener<T>>();
            for (var i = 0; i < components.Length; i++)
                components[i].OnItemChanged(collection, item, index, args);
        }

        #endregion
    }
}