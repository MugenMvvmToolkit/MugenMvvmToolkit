using System.Collections.Generic;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public class HeaderFooterCollectionDecorator : CollectionDecoratorBase
    {
        private const int NoFooterIndex = -1;
        private ItemOrIReadOnlyList<object> _footer;
        private ItemOrIReadOnlyList<object> _header;
        private int _footerIndex;

        public HeaderFooterCollectionDecorator(int priority = CollectionComponentPriority.HeaderFooterDecorator) : base(priority)
        {
            Priority = priority;
            _footerIndex = NoFooterIndex;
        }

        public override bool HasAdditionalItems => _header.Count != 0 || _footer.Count != 0;

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

        protected override void OnDetached(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            _footerIndex = NoFooterIndex;
        }

        protected override IEnumerable<object?> Decorate(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection,
            IEnumerable<object?> items) => Decorate(items);

        protected override bool TryGetIndexes(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, IEnumerable<object?> items,
            object? item, ref ItemOrListEditor<int> indexes)
        {
            FindAllIndexOf(_header, item, 0, ref indexes);
            FindAllIndexOf(_footer, item, _footerIndex, ref indexes);
            return true;
        }

        protected override bool OnChanged(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index,
            ref object? args)
        {
            index += _header.Count;
            return true;
        }

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            index += _header.Count;
            if (_footerIndex > NoFooterIndex)
            {
                if (index > _footerIndex)
                    --index;
                ++_footerIndex;
            }

            return true;
        }

        protected override bool OnReplaced(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? oldItem,
            ref object? newItem, ref int index)
        {
            index += _header.Count;
            return true;
        }

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex)
        {
            oldIndex += _header.Count;
            newIndex += _header.Count;
            return true;
        }

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            index += _header.Count;
            if (_footerIndex > NoFooterIndex)
                --_footerIndex;
            return true;
        }

        protected override bool OnReset(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            if (_footerIndex > NoFooterIndex)
                _footerIndex = items.CountEx() + _header.Count;
            items = Decorate(items);
            return true;
        }

        private static void FindAllIndexOf(ItemOrIReadOnlyList<object> items, object? item, int offset, ref ItemOrListEditor<int> indexes)
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (Equals(items[i], item))
                    indexes.Add(i + offset);
            }
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

            using var _ = Owner.TryLock();
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
    }
}