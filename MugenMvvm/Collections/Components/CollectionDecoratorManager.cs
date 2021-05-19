using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections.Components
{
    public sealed class CollectionDecoratorManager : ICollectionDecoratorManagerComponent, IHasPriority, ICollectionChangedListener<object?>, IComponentCollectionChangedListener
    {
        private static readonly CollectionDecoratorManager Instance = new();
        private static readonly Dictionary<Type, ICollectionDecoratorManagerComponent> GenericManagers = new();

        private CollectionDecoratorManager()
        {
        }

        public int Priority { get; set; } = CollectionComponentPriority.DecoratorManager;

        public static ICollectionDecoratorManagerComponent GetOrAdd(IEnumerable collection) =>
            ((IComponentOwner) collection).GetOrAddComponent(collection, (c, _) => Initialize(c));

        private static ICollectionDecoratorManagerComponent Initialize(object owner)
        {
            ((IComponentOwner) owner).Components.AddComponent(Instance);
            return TryGetGenericManager(owner) ?? Instance;
        }

        private static ICollectionDecoratorManagerComponent? TryGetGenericManager(object owner)
        {
            var itemType = GetItemType(owner);
            if (!itemType.IsValueType)
                return null;

            ICollectionDecoratorManagerComponent? component;
            lock (GenericManagers)
            {
                if (!GenericManagers.TryGetValue(itemType, out component))
                {
                    component = (ICollectionDecoratorManagerComponent) Activator.CreateInstance(typeof(GenericManager<>).MakeGenericType(itemType))!;
                    GenericManagers[itemType] = component;
                }
            }

            return component;
        }

        private static Type GetItemType(object owner)
        {
            if (owner is IReadOnlyObservableCollection c)
                return c.ItemType;

            foreach (Type interfaceType in owner.GetType().GetInterfaces())
            {
                if (!interfaceType.IsGenericType)
                    continue;

                var typeDefinition = interfaceType.GetGenericTypeDefinition();
                if (typeDefinition == typeof(IEnumerable<>) || typeDefinition == typeof(ICollection<>) || typeDefinition == typeof(IList<>))
                    return interfaceType.GetGenericArguments()[0];
            }

            return typeof(object);
        }

        private static ItemOrArray<ICollectionDecorator> GetDecorators(ICollection collection, ICollectionDecorator? decorator, out int index, bool isLengthDefault = false)
        {
            var components = GetComponents<ICollectionDecorator>(collection);
            index = isLengthDefault ? components.Count : 0;
            if (decorator == null)
                return components;

            for (var i = 0; i < components.Count; i++)
            {
                if (components[i] == decorator)
                {
                    index = i;
                    if (!isLengthDefault)
                        ++index;
                    break;
                }
            }

            return components;
        }

        private static ItemOrArray<TComponent> GetComponents<TComponent>(ICollection collection) where TComponent : class =>
            ((IComponentOwner) collection).Components.Get<TComponent>();

        private static void Reset(ICollection collection)
        {
            using var _ = MugenExtensions.TryLock(collection);
            Instance.OnReset(collection, null, collection.AsEnumerable());
        }

        public ActionToken TryLock(ICollection collection, ICollectionDecorator? decorator = null) => MugenExtensions.TryLock(collection);

        public ActionToken BatchUpdate(ICollection collection, ICollectionDecorator? decorator = null)
        {
            var listeners = GetComponents<ICollectionBatchUpdateListener>(collection);
            if (listeners.Count == 0)
                return default;

            listeners.OnBeginBatchUpdate(collection, BatchUpdateType.Decorators);
            return new ActionToken((o, l) => ItemOrArray.FromRawValue<ICollectionBatchUpdateListener>(l).OnEndBatchUpdate((ICollection) o!, BatchUpdateType.Decorators), collection,
                listeners.GetRawValue());
        }

        public IEnumerable<object?> Decorate(ICollection collection, ICollectionDecorator? decorator = null)
        {
            using var _ = decorator == null ? MugenExtensions.TryLock(collection) : default;
            var items = collection.AsEnumerable();
            var decorators = GetDecorators(collection, decorator, out var index, true);
            for (var i = 0; i < index; i++)
                items = decorators[i].Decorate(collection, items);

            foreach (var item in items)
                yield return item;
        }

        public void OnChanged(ICollection collection, ICollectionDecorator? decorator, object? item, int index, object? args)
        {
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Count; i++)
            {
                if (!decorators[i].OnChanged(collection, ref item, ref index, ref args))
                    return;
            }

            GetComponents<ICollectionDecoratorListener>(collection).OnChanged(collection, item, index, args);
        }

        public void OnAdded(ICollection collection, ICollectionDecorator? decorator, object? item, int index)
        {
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Count; i++)
            {
                if (!decorators[i].OnAdded(collection, ref item, ref index))
                    return;
            }

            GetComponents<ICollectionDecoratorListener>(collection).OnAdded(collection, item, index);
        }

        public void OnReplaced(ICollection collection, ICollectionDecorator? decorator, object? oldItem, object? newItem, int index)
        {
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Count; i++)
            {
                if (!decorators[i].OnReplaced(collection, ref oldItem, ref newItem, ref index))
                    return;
            }

            GetComponents<ICollectionDecoratorListener>(collection).OnReplaced(collection, oldItem, newItem, index);
        }

        public void OnMoved(ICollection collection, ICollectionDecorator? decorator, object? item, int oldIndex, int newIndex)
        {
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Count; i++)
            {
                if (!decorators[i].OnMoved(collection, ref item, ref oldIndex, ref newIndex))
                    return;
            }

            GetComponents<ICollectionDecoratorListener>(collection).OnMoved(collection, item, oldIndex, newIndex);
        }

        public void OnRemoved(ICollection collection, ICollectionDecorator? decorator, object? item, int index)
        {
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Count; i++)
            {
                if (!decorators[i].OnRemoved(collection, ref item, ref index))
                    return;
            }

            GetComponents<ICollectionDecoratorListener>(collection).OnRemoved(collection, item, index);
        }

        public void OnReset(ICollection collection, ICollectionDecorator? decorator, IEnumerable<object?>? items)
        {
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Count; i++)
            {
                if (!decorators[i].OnReset(collection, ref items))
                    return;
            }

            GetComponents<ICollectionDecoratorListener>(collection).OnReset(collection, items);
        }

        void ICollectionChangedListener<object?>.OnChanged(IReadOnlyCollection<object?> collection, object? item, int index, object? args) =>
            OnChanged((ICollection) collection, null, item, index, args);

        void ICollectionChangedListener<object?>.OnAdded(IReadOnlyCollection<object?> collection, object? item, int index) => OnAdded((ICollection) collection, null, item, index);

        void ICollectionChangedListener<object?>.OnReplaced(IReadOnlyCollection<object?> collection, object? oldItem, object? newItem, int index) =>
            OnReplaced((ICollection) collection, null, oldItem, newItem, index);

        void ICollectionChangedListener<object?>.OnMoved(IReadOnlyCollection<object?> collection, object? item, int oldIndex, int newIndex) =>
            OnMoved((ICollection) collection, null, item, oldIndex, newIndex);

        void ICollectionChangedListener<object?>.OnRemoved(IReadOnlyCollection<object?> collection, object? item, int index) =>
            OnRemoved((ICollection) collection, null, item, index);

        void ICollectionChangedListener<object?>.OnReset(IReadOnlyCollection<object?> collection, IEnumerable<object?>? items) => OnReset((ICollection) collection, null, items);

        void IComponentCollectionChangedListener.OnAdded(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is ICollectionDecorator || component is ICollectionDecoratorManagerComponent)
                Reset((ICollection) collection.Owner);
        }

        void IComponentCollectionChangedListener.OnRemoved(IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (component is ICollectionDecorator)
                Reset((ICollection) collection.Owner);
        }

        private sealed class GenericManager<T> : ICollectionChangedListener<T>, IHasPriority, ICollectionDecoratorManagerComponent
        {
            public int Priority => Instance.Priority;

            public void OnChanged(IReadOnlyCollection<T> collection, T item, int index, object? args) =>
                Instance.OnChanged((ICollection) collection, null, BoxingExtensions.Box(item), index, args);

            public void OnAdded(IReadOnlyCollection<T> collection, T item, int index) => Instance.OnAdded((ICollection) collection, null, BoxingExtensions.Box(item), index);

            public void OnReplaced(IReadOnlyCollection<T> collection, T oldItem, T newItem, int index) =>
                Instance.OnReplaced((ICollection) collection, null, BoxingExtensions.Box(oldItem), BoxingExtensions.Box(newItem), index);

            public void OnMoved(IReadOnlyCollection<T> collection, T item, int oldIndex, int newIndex) =>
                Instance.OnMoved((ICollection) collection, null, BoxingExtensions.Box(item), oldIndex, newIndex);

            public void OnRemoved(IReadOnlyCollection<T> collection, T item, int index) => Instance.OnRemoved((ICollection) collection, null, BoxingExtensions.Box(item), index);

            public void OnReset(IReadOnlyCollection<T> collection, IEnumerable<T>? items) => Instance.OnReset((ICollection) collection, null, items?.Cast<object?>());

            public ActionToken TryLock(ICollection collection, ICollectionDecorator? decorator = null) => Instance.TryLock(collection, decorator);

            public ActionToken BatchUpdate(ICollection collection, ICollectionDecorator? decorator = null) => Instance.BatchUpdate(collection, decorator);

            public IEnumerable<object?> Decorate(ICollection collection, ICollectionDecorator? decorator = null) => Instance.Decorate(collection, decorator);

            public void OnChanged(ICollection collection, ICollectionDecorator? decorator, object? item, int index, object? args) =>
                Instance.OnChanged(collection, decorator, item, index, args);

            public void OnAdded(ICollection collection, ICollectionDecorator? decorator, object? item, int index) => Instance.OnAdded(collection, decorator, item, index);

            public void OnReplaced(ICollection collection, ICollectionDecorator? decorator, object? oldItem, object? newItem, int index) =>
                Instance.OnReplaced(collection, decorator, oldItem, newItem, index);

            public void OnMoved(ICollection collection, ICollectionDecorator? decorator, object? item, int oldIndex, int newIndex) =>
                Instance.OnMoved(collection, decorator, item, oldIndex, newIndex);

            public void OnRemoved(ICollection collection, ICollectionDecorator? decorator, object? item, int index) => Instance.OnRemoved(collection, decorator, item, index);

            public void OnReset(ICollection collection, ICollectionDecorator? decorator, IEnumerable<object?>? items) => Instance.OnReset(collection, decorator, items);
        }
    }
}