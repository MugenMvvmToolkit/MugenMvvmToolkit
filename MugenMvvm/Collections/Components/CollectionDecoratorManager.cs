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
    public sealed class CollectionDecoratorManager : ICollectionChangedListener, ICollectionDecoratorManagerComponent, IHasPriority
    {
        #region Fields

        public static readonly CollectionDecoratorManager Instance = new CollectionDecoratorManager();

        #endregion

        #region Constructors

        private CollectionDecoratorManager()
        {
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = CollectionComponentPriority.DecoratorManager;

        #endregion

        #region Implementation of interfaces

        void ICollectionChangedListenerBase.OnItemChanged(IObservableCollection collection, object? item, int index, object? args)
        {
            OnItemChanged(collection, null, item, index, args);
        }

        void ICollectionChangedListenerBase.OnAdded(IObservableCollection collection, object? item, int index)
        {
            OnAdded(collection, null, item, index);
        }

        void ICollectionChangedListenerBase.OnReplaced(IObservableCollection collection, object? oldItem, object? newItem, int index)
        {
            OnReplaced(collection, null, oldItem, newItem, index);
        }

        void ICollectionChangedListenerBase.OnMoved(IObservableCollection collection, object? item, int oldIndex, int newIndex)
        {
            OnMoved(collection, null, item, oldIndex, newIndex);
        }

        void ICollectionChangedListenerBase.OnRemoved(IObservableCollection collection, object? item, int index)
        {
            OnRemoved(collection, null, item, index);
        }

        void ICollectionChangedListenerBase.OnReset(IObservableCollection collection, IEnumerable<object?> items)
        {
            OnReset(collection, null, items);
        }

        void ICollectionChangedListenerBase.OnCleared(IObservableCollection collection)
        {
            OnCleared(collection, null);
        }

        public IEnumerable<object?> DecorateItems(IObservableCollection collection, ICollectionDecorator? decorator = null)
        {
            IEnumerable<object?> items = collection as IEnumerable<object?> ?? collection.OfType<object?>();
            var decorators = GetDecorators(collection, decorator, out var startIndex, true);
            for (var i = 0; i < startIndex; i++)
                items = decorators[i].DecorateItems(collection, items);

            return items;
        }

        public void OnItemChanged(IObservableCollection collection, ICollectionDecorator? decorator, object? item, int index, object? args)
        {
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Length; i++)
            {
                if (!decorators[i].OnItemChanged(collection, ref item, ref index, ref args))
                    return;
            }

            GetComponents<ICollectionDecoratorListener>(collection).OnItemChanged(collection, item, index, args);
        }

        public void OnAdded(IObservableCollection collection, ICollectionDecorator? decorator, object? item, int index)
        {
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Length; i++)
            {
                if (!decorators[i].OnAdded(collection, ref item, ref index))
                    return;
            }

            GetComponents<ICollectionDecoratorListener>(collection).OnAdded(collection, item, index);
        }

        public void OnReplaced(IObservableCollection collection, ICollectionDecorator? decorator, object? oldItem, object? newItem, int index)
        {
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Length; i++)
            {
                if (!decorators[i].OnReplaced(collection, ref oldItem, ref newItem, ref index))
                    return;
            }

            GetComponents<ICollectionDecoratorListener>(collection).OnReplaced(collection, oldItem, newItem, index);
        }

        public void OnMoved(IObservableCollection collection, ICollectionDecorator? decorator, object? item, int oldIndex, int newIndex)
        {
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Length; i++)
            {
                if (!decorators[i].OnMoved(collection, ref item, ref oldIndex, ref newIndex))
                    return;
            }

            GetComponents<ICollectionDecoratorListener>(collection).OnMoved(collection, item, oldIndex, newIndex);
        }

        public void OnRemoved(IObservableCollection collection, ICollectionDecorator? decorator, object? item, int index)
        {
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Length; i++)
            {
                if (!decorators[i].OnRemoved(collection, ref item, ref index))
                    return;
            }

            GetComponents<ICollectionDecoratorListener>(collection).OnRemoved(collection, item, index);
        }

        public void OnReset(IObservableCollection collection, ICollectionDecorator? decorator, IEnumerable<object?> items)
        {
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Length; i++)
            {
                if (!decorators[i].OnReset(collection, ref items))
                    return;
            }

            GetComponents<ICollectionDecoratorListener>(collection).OnReset(collection, items);
        }

        public void OnCleared(IObservableCollection collection, ICollectionDecorator? decorator)
        {
            var decorators = GetDecorators(collection, decorator, out var startIndex);
            for (var i = startIndex; i < decorators.Length; i++)
            {
                if (!decorators[i].OnCleared(collection))
                    return;
            }

            GetComponents<ICollectionDecoratorListener>(collection).OnCleared(collection);
        }

        #endregion

        #region Methods

        private static ICollectionDecorator[] GetDecorators(IComponentOwner collection, ICollectionDecorator? decorator, out int index, bool isLengthDefault = false)
        {
            var components = GetComponents<ICollectionDecorator>(collection);
            index = isLengthDefault ? components.Length : 0;
            if (decorator == null)
                return components;

            for (var i = 0; i < components.Length; i++)
            {
                if (ReferenceEquals(components[i], decorator))
                {
                    index = i;
                    if (!isLengthDefault)
                        ++index;
                    break;
                }
            }

            return components;
        }

        private static TComponent[] GetComponents<TComponent>(IComponentOwner collection) where TComponent : class
        {
            return collection.Components.Get<TComponent>();
        }

        #endregion
    }
}