using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Collections.Components
{
    public sealed class CollectionDecoratorManager : ICollectionDecoratorManagerComponent, IHasPriority, ICollectionChangedListener<object?>
    {
        private static readonly CollectionDecoratorManager Instance = new();
        private static readonly Dictionary<Type, ICollectionDecoratorManagerComponent> GenericManagers = new();

        private CollectionDecoratorManager()
        {
        }

        public int Priority { get; set; } = CollectionComponentPriority.DecoratorManager;

        public static ICollectionDecoratorManagerComponent GetOrAdd(IEnumerable collection) =>
            ((IComponentOwner) collection).GetOrAddComponent(collection, (c, context) => TryGetGenericManager(c) ?? Instance);

        private static ICollectionDecoratorManagerComponent? TryGetGenericManager(object owner)
        {
            var collection = (IObservableCollectionBase) owner;
            var itemType = collection.ItemType;
            if (!itemType.IsValueType)
                return null;

            ICollectionDecoratorManagerComponent? component;
            lock (GenericManagers)
            {
                if (!GenericManagers.TryGetValue(itemType, out component))
                {
                    component = (ICollectionDecoratorManagerComponent) Activator.CreateInstance(typeof(GenericManager<>).MakeGenericType(collection.ItemType))!;
                    GenericManagers[itemType] = component;
                }
            }

            return component;
        }

        private static ItemOrArray<ICollectionDecorator> GetDecorators(ICollection collection, ICollectionDecorator? decorator, out int index, bool isLengthDefault = false)
        {
            var components = GetComponents<ICollectionDecorator>(collection);
            index = isLengthDefault ? components.Count : 0;
            if (decorator == null)
                return components;

            for (var i = 0; i < components.Count; i++)
                if (components[i] == decorator)
                {
                    index = i;
                    if (!isLengthDefault)
                        ++index;
                    break;
                }

            return components;
        }

        private static ItemOrArray<TComponent> GetComponents<TComponent>(ICollection collection) where TComponent : class =>
            ((IComponentOwner) collection).Components.Get<TComponent>();

        public IEnumerable<object?> DecorateItems(ICollection collection, ICollectionDecorator? decorator = null)
        {
            IEnumerable<object?> items = collection as IEnumerable<object?> ?? collection.OfType<object?>();
            var decorators = GetDecorators(collection, decorator, out var startIndex, true);
            for (var i = 0; i < startIndex; i++)
                items = decorators[i].DecorateItems(collection, items);

            return items;
        }

        public void OnItemChanged(ICollection collection, ICollectionDecorator? decorator, object? item, int index, object? args)
        {
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Count; i++)
                if (!decorators[i].OnItemChanged(collection, ref item, ref index, ref args))
                    return;

            GetComponents<ICollectionDecoratorListener>(collection).OnItemChanged(collection, item, index, args);
        }

        public void OnAdded(ICollection collection, ICollectionDecorator? decorator, object? item, int index)
        {
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Count; i++)
                if (!decorators[i].OnAdded(collection, ref item, ref index))
                    return;

            GetComponents<ICollectionDecoratorListener>(collection).OnAdded(collection, item, index);
        }

        public void OnReplaced(ICollection collection, ICollectionDecorator? decorator, object? oldItem, object? newItem, int index)
        {
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Count; i++)
                if (!decorators[i].OnReplaced(collection, ref oldItem, ref newItem, ref index))
                    return;

            GetComponents<ICollectionDecoratorListener>(collection).OnReplaced(collection, oldItem, newItem, index);
        }

        public void OnMoved(ICollection collection, ICollectionDecorator? decorator, object? item, int oldIndex, int newIndex)
        {
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Count; i++)
                if (!decorators[i].OnMoved(collection, ref item, ref oldIndex, ref newIndex))
                    return;

            GetComponents<ICollectionDecoratorListener>(collection).OnMoved(collection, item, oldIndex, newIndex);
        }

        public void OnRemoved(ICollection collection, ICollectionDecorator? decorator, object? item, int index)
        {
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Count; i++)
                if (!decorators[i].OnRemoved(collection, ref item, ref index))
                    return;

            GetComponents<ICollectionDecoratorListener>(collection).OnRemoved(collection, item, index);
        }

        public void OnReset(ICollection collection, ICollectionDecorator? decorator, IEnumerable<object?>? items)
        {
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Count; i++)
                if (!decorators[i].OnReset(collection, ref items))
                    return;

            GetComponents<ICollectionDecoratorListener>(collection).OnReset(collection, items);
        }

        void ICollectionChangedListener<object?>.OnItemChanged(IReadOnlyCollection<object?> collection, object? item, int index, object? args) =>
            OnItemChanged((ICollection) collection, null, item, index, args);

        void ICollectionChangedListener<object?>.OnAdded(IReadOnlyCollection<object?> collection, object? item, int index) => OnAdded((ICollection) collection, null, item, index);

        void ICollectionChangedListener<object?>.OnReplaced(IReadOnlyCollection<object?> collection, object? oldItem, object? newItem, int index) =>
            OnReplaced((ICollection) collection, null, oldItem, newItem, index);

        void ICollectionChangedListener<object?>.OnMoved(IReadOnlyCollection<object?> collection, object? item, int oldIndex, int newIndex) =>
            OnMoved((ICollection) collection, null, item, oldIndex, newIndex);

        void ICollectionChangedListener<object?>.OnRemoved(IReadOnlyCollection<object?> collection, object? item, int index) =>
            OnRemoved((ICollection) collection, null, item, index);

        void ICollectionChangedListener<object?>.OnReset(IReadOnlyCollection<object?> collection, IEnumerable<object?>? items) => OnReset((ICollection) collection, null, items);

        private sealed class GenericManager<T> : ICollectionChangedListener<T>, IHasPriority, ICollectionDecoratorManagerComponent
        {
            public int Priority => Instance.Priority;

            public void OnItemChanged(IReadOnlyCollection<T> collection, T item, int index, object? args)
                => Instance.OnItemChanged((ICollection) collection, null, BoxingExtensions.Box(item), index, args);

            public void OnAdded(IReadOnlyCollection<T> collection, T item, int index)
                => Instance.OnAdded((ICollection) collection, null, BoxingExtensions.Box(item), index);

            public void OnReplaced(IReadOnlyCollection<T> collection, T oldItem, T newItem, int index)
                => Instance.OnReplaced((ICollection) collection, null, BoxingExtensions.Box(oldItem), BoxingExtensions.Box(newItem), index);

            public void OnMoved(IReadOnlyCollection<T> collection, T item, int oldIndex, int newIndex)
                => Instance.OnMoved((ICollection) collection, null, BoxingExtensions.Box(item), oldIndex, newIndex);

            public void OnRemoved(IReadOnlyCollection<T> collection, T item, int index)
                => Instance.OnRemoved((ICollection) collection, null, BoxingExtensions.Box(item), index);

            public void OnReset(IReadOnlyCollection<T> collection, IEnumerable<T>? items)
                => Instance.OnReset((ICollection) collection, null, items?.Cast<object?>());

            public IEnumerable<object?> DecorateItems(ICollection collection, ICollectionDecorator? decorator = null) => Instance.DecorateItems(collection, decorator);

            public void OnItemChanged(ICollection collection, ICollectionDecorator decorator, object? item, int index, object? args) =>
                Instance.OnItemChanged(collection, decorator, item, index, args);

            public void OnAdded(ICollection collection, ICollectionDecorator decorator, object? item, int index) => Instance.OnAdded(collection, decorator, item, index);

            public void OnReplaced(ICollection collection, ICollectionDecorator decorator, object? oldItem, object? newItem, int index) =>
                Instance.OnReplaced(collection, decorator, oldItem, newItem, index);

            public void OnMoved(ICollection collection, ICollectionDecorator decorator, object? item, int oldIndex, int newIndex) =>
                Instance.OnMoved(collection, decorator, item, oldIndex, newIndex);

            public void OnRemoved(ICollection collection, ICollectionDecorator decorator, object? item, int index) => Instance.OnRemoved(collection, decorator, item, index);

            public void OnReset(ICollection collection, ICollectionDecorator decorator, IEnumerable<object?>? items) => Instance.OnReset(collection, decorator, items);
        }
    }
}