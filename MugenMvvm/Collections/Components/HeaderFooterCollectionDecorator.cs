using System.Collections;
using System.Collections.Generic;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public class HeaderFooterCollectionDecorator : AttachableComponentBase<ICollection>, ICollectionDecorator, IHasPriority
    {
        private const int NoFooterIndex = -1;
        private ItemOrIReadOnlyList<object> _footer;
        private ItemOrIReadOnlyList<object> _header;
        private int _footerIndex;

        public HeaderFooterCollectionDecorator(int priority = CollectionComponentPriority.HeaderFooterDecorator)
        {
            Priority = priority;
            _footerIndex = NoFooterIndex;
        }

        public ItemOrIReadOnlyList<object> Header
        {
            get => _header;
            set => Update(value, _header, false);
        }

        public ItemOrIReadOnlyList<object> Footer
        {
            get => _footer;
            set => Update(value, _footer, true);
        }

        public int Priority { get; set; }

        protected ICollectionDecoratorManagerComponent? DecoratorManager { get; private set; }

        protected override void OnAttached(ICollection owner, IReadOnlyMetadataContext? metadata) => DecoratorManager = CollectionDecoratorManager.GetOrAdd(owner);

        protected override void OnDetached(ICollection owner, IReadOnlyMetadataContext? metadata)
        {
            DecoratorManager = null;
            _footerIndex = NoFooterIndex;
        }

        private IEnumerable<object?> Decorate(IEnumerable<object?>? items)
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

        private void SetValue(ItemOrIReadOnlyList<object> value, bool isFooter)
        {
            if (isFooter)
                _footer = value;
            else
                _header = value;
        }

        private void Update(ItemOrIReadOnlyList<object> value, ItemOrIReadOnlyList<object> oldValue, bool isFooter)
        {
            if (DecoratorManager == null)
            {
                SetValue(value, isFooter);
                return;
            }

            using var _ = DecoratorManager.TryLock(Owner, this);
            using var __ = DecoratorManager.BatchUpdate(Owner, this);
            SetValue(value, isFooter);
            var offset = isFooter ? _footerIndex : 0;
            if (value.IsEmpty)
            {
                if (isFooter)
                    _footerIndex = NoFooterIndex;
                for (var i = 0; i < oldValue.Count; i++)
                {
                    if (!isFooter && _footerIndex > NoFooterIndex)
                        --_footerIndex;
                    DecoratorManager.OnRemoved(Owner, this, oldValue[i], offset);
                }
            }
            else
            {
                if (oldValue.IsEmpty)
                {
                    if (isFooter)
                    {
                        _footerIndex = DecoratorManager.Decorate(Owner, this).CountEx() + _header.Count;
                        offset = _footerIndex;
                    }

                    for (var i = 0; i < value.Count; i++)
                    {
                        if (!isFooter && _footerIndex > NoFooterIndex)
                            ++_footerIndex;
                        DecoratorManager.OnAdded(Owner, this, value[i], i + offset);
                    }
                }
                else
                {
                    if (value.Count >= oldValue.Count)
                    {
                        for (var i = 0; i < oldValue.Count; i++)
                        {
                            if (!Equals(oldValue[i], value[i]))
                                DecoratorManager.OnReplaced(Owner, this, oldValue[i], value[i], i + offset);
                        }

                        for (var i = oldValue.Count; i < value.Count; i++)
                        {
                            if (!isFooter && _footerIndex > NoFooterIndex)
                                ++_footerIndex;
                            DecoratorManager.OnAdded(Owner, this, value[i], i + offset);
                        }
                    }
                    else
                    {
                        for (var i = 0; i < value.Count; i++)
                        {
                            if (!Equals(oldValue[i], value[i]))
                                DecoratorManager.OnReplaced(Owner, this, oldValue[i], value[i], i + offset);
                        }

                        for (var i = value.Count; i < oldValue.Count; i++)
                        {
                            if (!isFooter && _footerIndex > NoFooterIndex)
                                --_footerIndex;
                            DecoratorManager.OnRemoved(Owner, this, oldValue[i], value.Count + offset);
                        }
                    }
                }
            }
        }

        IEnumerable<object?> ICollectionDecorator.Decorate(ICollection collection, IEnumerable<object?> items) => DecoratorManager == null ? items : Decorate(items);

        bool ICollectionDecorator.OnChanged(ICollection collection, ref object? item, ref int index, ref object? args)
        {
            if (DecoratorManager == null)
                return false;

            index += _header.Count;
            return true;
        }

        bool ICollectionDecorator.OnAdded(ICollection collection, ref object? item, ref int index)
        {
            if (DecoratorManager == null)
                return false;

            index += _header.Count;
            if (_footerIndex > NoFooterIndex)
            {
                if (index > _footerIndex)
                    --index;
                ++_footerIndex;
            }

            return true;
        }

        bool ICollectionDecorator.OnReplaced(ICollection collection, ref object? oldItem, ref object? newItem, ref int index)
        {
            if (DecoratorManager == null)
                return false;

            index += _header.Count;
            return true;
        }

        bool ICollectionDecorator.OnMoved(ICollection collection, ref object? item, ref int oldIndex, ref int newIndex)
        {
            if (DecoratorManager == null)
                return false;

            oldIndex += _header.Count;
            newIndex += _header.Count;
            return true;
        }

        bool ICollectionDecorator.OnRemoved(ICollection collection, ref object? item, ref int index)
        {
            if (DecoratorManager == null)
                return false;

            index += _header.Count;
            if (_footerIndex > NoFooterIndex)
                --_footerIndex;
            return true;
        }

        bool ICollectionDecorator.OnReset(ICollection collection, ref IEnumerable<object?>? items)
        {
            if (DecoratorManager == null)
                return false;

            if (_footerIndex > NoFooterIndex)
                _footerIndex = items.CountEx() + _header.Count;
            items = Decorate(items);
            return true;
        }
    }
}