using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Collections.Components
{
    public class HeaderFooterCollectionDecorator : AttachableComponentBase<ICollection>, ICollectionDecorator, IHasPriority
    {
        private ICollectionDecoratorManagerComponent? _decoratorManager;
        private ItemOrIReadOnlyList<object> _footer;
        private ItemOrIReadOnlyList<object> _header;
        private int _footerIndex;

        public HeaderFooterCollectionDecorator(int priority = CollectionComponentPriority.HeaderFooterDecorator)
        {
            Priority = priority;
            _footerIndex = -1;
        }

        public ItemOrIReadOnlyList<object> Header
        {
            get => _header;
            set
            {
                using var l = OwnerOptional.TryLock();
                var oldValue = _header;
                _header = value;
                if (_decoratorManager != null)
                    Update(value, oldValue, false);
            }
        }

        public ItemOrIReadOnlyList<object> Footer
        {
            get => _footer;
            set
            {
                using var l = OwnerOptional.TryLock();
                var oldValue = _footer;
                _footer = value;
                if (_decoratorManager != null)
                    Update(value, oldValue, true);
            }
        }

        public int Priority { get; }

        protected override void OnAttached(ICollection owner, IReadOnlyMetadataContext? metadata) => _decoratorManager = CollectionDecoratorManager.GetOrAdd(owner);

        protected override void OnDetached(ICollection owner, IReadOnlyMetadataContext? metadata)
        {
            _decoratorManager = null;
            _footerIndex = -1;
        }

        private IEnumerable<object?> DecorateItems(IEnumerable<object?>? items)
        {
            foreach (var item in _header)
                yield return item;

            if (items != null)
            {
                foreach (var item in items)
                    yield return item;
            }

            foreach (object item in _footer)
                yield return item;
        }

        private void Update(ItemOrIReadOnlyList<object> value, ItemOrIReadOnlyList<object> oldValue, bool isFooter)
        {
            var offset = isFooter ? _footerIndex : 0;
            if (value.IsEmpty)
            {
                if (isFooter)
                    _footerIndex = -1;
                for (var i = 0; i < oldValue.Count; i++)
                {
                    if (!isFooter && _footerIndex > 0)
                        --_footerIndex;
                    _decoratorManager!.OnRemoved(Owner, this, oldValue[i], offset);
                }
            }
            else
            {
                if (oldValue.IsEmpty)
                {
                    if (isFooter)
                    {
                        _footerIndex = _decoratorManager!.DecorateItems(Owner, this).CountEx() + _header.Count;
                        offset = _footerIndex;
                    }

                    for (var i = 0; i < value.Count; i++)
                    {
                        if (!isFooter && _footerIndex > 0)
                            ++_footerIndex;
                        _decoratorManager!.OnAdded(Owner, this, value[i], i + offset);
                    }
                }
                else
                {
                    if (value.Count >= oldValue.Count)
                    {
                        for (var i = 0; i < oldValue.Count; i++)
                        {
                            if (!Equals(oldValue[i], value[i]))
                                _decoratorManager!.OnReplaced(Owner, this, oldValue[i], value[i], i + offset);
                        }

                        for (var i = oldValue.Count; i < value.Count; i++)
                        {
                            if (!isFooter && _footerIndex > 0)
                                ++_footerIndex;
                            _decoratorManager!.OnAdded(Owner, this, value[i], i + offset);
                        }
                    }
                    else
                    {
                        for (var i = 0; i < value.Count; i++)
                        {
                            if (!Equals(oldValue[i], value[i]))
                                _decoratorManager!.OnReplaced(Owner, this, oldValue[i], value[i], i + offset);
                        }

                        for (var i = value.Count; i < oldValue.Count; i++)
                        {
                            if (!isFooter && _footerIndex > 0)
                                --_footerIndex;
                            _decoratorManager!.OnRemoved(Owner, this, oldValue[i], value.Count + offset);
                        }
                    }
                }
            }
        }

        IEnumerable<object?> ICollectionDecorator.DecorateItems(ICollection collection, IEnumerable<object?> items) => DecorateItems(items);

        bool ICollectionDecorator.OnItemChanged(ICollection collection, ref object? item, ref int index, ref object? args)
        {
            index += _header.Count;
            return true;
        }

        bool ICollectionDecorator.OnAdded(ICollection collection, ref object? item, ref int index)
        {
            index += _header.Count;
            if (_footerIndex > 0)
            {
                if (index > _footerIndex)
                    --index;
                ++_footerIndex;
            }

            return true;
        }

        bool ICollectionDecorator.OnReplaced(ICollection collection, ref object? oldItem, ref object? newItem, ref int index)
        {
            index += _header.Count;
            return true;
        }

        bool ICollectionDecorator.OnMoved(ICollection collection, ref object? item, ref int oldIndex, ref int newIndex)
        {
            oldIndex += _header.Count;
            newIndex += _header.Count;
            return true;
        }

        bool ICollectionDecorator.OnRemoved(ICollection collection, ref object? item, ref int index)
        {
            index += _header.Count;
            if (_footerIndex > 0)
                --_footerIndex;
            return true;
        }

        bool ICollectionDecorator.OnReset(ICollection collection, ref IEnumerable<object?>? items)
        {
            if (_footerIndex > 0)
                _footerIndex = items.CountEx() + _header.Count;
            items = DecorateItems(items);
            return true;
        }
    }
}