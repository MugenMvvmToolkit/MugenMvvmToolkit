using System;
using System.Collections.Generic;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Collections.Components
{
    public sealed class FirstLastTrackerCollectionDecorator : CollectionDecoratorBase, IListenerCollectionDecorator, ActionToken.IHandler
    {
        private readonly Action<object?> _setter;
        private readonly bool _isFirstTracker;
        private int _index;

        public FirstLastTrackerCollectionDecorator(int priority, Action<object?> setter, bool isFirstTracker) : base(priority)
        {
            Should.NotBeNull(setter, nameof(setter));
            _setter = setter;
            _isFirstTracker = isFirstTracker;
        }

        protected override bool IsLazy => false;

        protected override bool HasAdditionalItems => false;

        protected override IEnumerable<object?> Decorate(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection,
            IEnumerable<object?> items) => items;

        protected override bool OnChanged(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index,
            ref object? args) => true;

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (!_isFirstTracker)
                ++_index;
            if (index == _index)
                _setter(item);
            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            if (index == _index)
                _setter(newItem);
            return true;
        }

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex)
        {
            if (newIndex == _index)
                _setter(item);
            else if (oldIndex == _index)
            {
                var items = decoratorManager.Decorate(collection, this);
                _setter(_isFirstTracker ? items.FirstOrDefaultEx() : items.LastOrDefaultEx());
            }

            return true;
        }

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            if (_isFirstTracker)
            {
                if (index == 0)
                    _setter(decoratorManager.Decorate(collection, this).FirstOrDefaultEx());
                return true;
            }

            if (index == _index)
                _setter(decoratorManager.Decorate(collection, this).LastOrDefaultEx());
            --_index;
            return true;
        }

        protected override bool OnReset(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            if (items == null)
            {
                _index = 0;
                _setter(null);
                return true;
            }

            if (_isFirstTracker)
            {
                _setter(items.FirstOrDefaultEx());
                return true;
            }

            object? lastItem = null;
            int index;
            if (items is IReadOnlyList<object?> readOnlyList)
            {
                index = readOnlyList.Count;
                if (index != 0)
                    lastItem = readOnlyList[index - 1];
            }
            else if (items is IList<object?> list)
            {
                index = list.Count;
                if (index != 0)
                    lastItem = list[index - 1];
            }
            else
            {
                index = 0;
                foreach (var item in items)
                {
                    lastItem = item;
                    ++index;
                }
            }

            _setter(lastItem);
            _index = index - 1;
            return true;
        }

        void ActionToken.IHandler.Invoke(object? state1, object? state2)
        {
            ((IReadOnlyObservableCollection) state1!).RemoveComponent(this);
            if (state2 != null)
                ((IReadOnlyObservableCollection) state1!).RemoveComponent((IComponent<IReadOnlyObservableCollection>) state2);
        }
    }
}