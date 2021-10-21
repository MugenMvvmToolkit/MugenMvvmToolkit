using System.Collections.Generic;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Collections.Components;
using MugenMvvm.Interfaces.Metadata;

// ReSharper disable PossibleMultipleEnumeration

namespace MugenMvvm.Collections.Components
{
    public sealed class HeaderFooterCollectionDecorator : CollectionDecoratorBase
    {
        private const int NoFooterIndex = -1;
        private List<object>? _footer;
        private List<object>? _header;
        private int _footerIndex;

        public HeaderFooterCollectionDecorator(int priority) : base(priority)
        {
            Priority = priority;
            _footerIndex = NoFooterIndex;
        }

        public ItemOrIReadOnlyList<object> Header
        {
            get => _header;
            set => Update(value, false);
        }

        public ItemOrIReadOnlyList<object> Footer
        {
            get => _footer;
            set => Update(value, true);
        }

        protected override bool HasAdditionalItems => HeaderCount != 0 || FooterCount != 0;

        private int HeaderCount => _header == null ? 0 : _header.Count;

        private int FooterCount => _footer == null ? 0 : _footer.Count;

        protected override void OnDetached(IReadOnlyObservableCollection owner, IReadOnlyMetadataContext? metadata)
        {
            base.OnDetached(owner, metadata);
            _footerIndex = NoFooterIndex;
        }

        protected override IEnumerable<object?> Decorate(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection,
            IEnumerable<object?> items) => Decorate(items);

        protected override bool TryGetIndexes(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, IEnumerable<object?> items,
            object? item, bool ignoreDuplicates, ref ItemOrListEditor<int> indexes)
        {
            FindAllIndexOf(_header, item, 0, ignoreDuplicates, ref indexes);
            FindAllIndexOf(_footer, item, _footerIndex, ignoreDuplicates, ref indexes);
            return true;
        }

        protected override bool OnChanged(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index,
            ref object? args)
        {
            index += HeaderCount;
            return true;
        }

        protected override bool OnAdded(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            index += HeaderCount;
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
            index += HeaderCount;
            return true;
        }

        protected override bool OnMoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int oldIndex,
            ref int newIndex)
        {
            oldIndex += HeaderCount;
            newIndex += HeaderCount;
            return true;
        }

        protected override bool OnRemoved(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref object? item, ref int index)
        {
            index += HeaderCount;
            if (_footerIndex > NoFooterIndex)
                --_footerIndex;
            return true;
        }

        protected override bool OnReset(ICollectionDecoratorManagerComponent decoratorManager, IReadOnlyObservableCollection collection, ref IEnumerable<object?>? items)
        {
            if (FooterCount == 0)
                _footerIndex = NoFooterIndex;
            else
                _footerIndex = items.CountEx() + HeaderCount;
            items = Decorate(items);
            return true;
        }

        private static void FindAllIndexOf(ItemOrIReadOnlyList<object> items, object? item, int offset, bool ignoreDuplicates, ref ItemOrListEditor<int> indexes)
        {
            for (var i = 0; i < items.Count; i++)
            {
                if (Equals(items[i], item))
                {
                    indexes.Add(i + offset);
                    if (ignoreDuplicates)
                        return;
                }
            }
        }

        private static void SetValue(ref List<object>? field, ItemOrIReadOnlyList<object> value)
        {
            if (value.IsEmpty && field == null)
                return;
            field ??= new List<object>(value.Count);
            field.Clear();
            if (value.List != null)
                field.AddRange(value.List);
            else if (value.HasItem)
                field.Add(value.Item!);
        }

        private IEnumerable<object?> Decorate(IEnumerable<object?>? items)
        {
            if (_header != null)
            {
                for (var i = 0; i < _header.Count; i++)
                    yield return _header[i];
            }

            if (items != null)
            {
                foreach (var item in items)
                    yield return item;
            }

            if (_footer != null)
            {
                for (var i = 0; i < _footer.Count; i++)
                    yield return _footer[i];
            }
        }

        private void SetValue(ItemOrIReadOnlyList<object> value, bool isFooter)
        {
            if (isFooter)
                SetValue(ref _footer, value);
            else
                SetValue(ref _header, value);
        }

        private void Update(ItemOrIReadOnlyList<object> value, bool isFooter)
        {
            var decoratorManager = DecoratorManager;
            var owner = OwnerOptional;
            if (decoratorManager == null || owner == null)
            {
                SetValue(value, isFooter);
                return;
            }

            using var _ = owner.Lock();
            if (DecoratorManager == null)
            {
                SetValue(value, isFooter);
                return;
            }

            using var __ = owner.BatchUpdateDecorators(owner.GetBatchUpdateManager());
            if (!value.IsEmpty)
            {
                if (isFooter)
                    _footer ??= new List<object>(value.Count);
                else
                    _header ??= new List<object>(value.Count);
            }

            var oldValue = isFooter ? _footer : _header;
            var offset = isFooter ? _footerIndex : 0;
            if (value.IsEmpty)
            {
                if (isFooter)
                    _footerIndex = NoFooterIndex;

                if (oldValue != null)
                {
                    for (var i = oldValue.Count - 1; i >= 0; i--)
                    {
                        if (!isFooter && _footerIndex > NoFooterIndex)
                            --_footerIndex;
                        var item = oldValue[i];
                        oldValue.RemoveAt(i);
                        decoratorManager.OnRemoved(owner, this, item, offset + i);
                    }
                }
            }
            else
            {
                if (oldValue!.Count == 0)
                {
                    if (isFooter)
                    {
                        _footerIndex = decoratorManager.Decorate(owner, this).CountEx() + HeaderCount;
                        offset = _footerIndex;
                    }

                    for (var i = 0; i < value.Count; i++)
                    {
                        if (!isFooter && _footerIndex > NoFooterIndex)
                            ++_footerIndex;
                        var item = value[i];
                        oldValue.Add(item);
                        decoratorManager.OnAdded(owner, this, item, i + offset);
                    }
                }
                else
                {
                    if (value.Count >= oldValue.Count)
                    {
                        for (var i = 0; i < oldValue.Count; i++)
                        {
                            var oldItem = oldValue[i];
                            var newItem = value[i];
                            if (!Equals(oldItem, newItem))
                            {
                                oldValue[i] = newItem;
                                decoratorManager.OnReplaced(owner, this, oldItem, newItem, i + offset);
                            }
                        }

                        var startIndex = oldValue.Count;
                        for (var i = startIndex; i < value.Count; i++)
                        {
                            if (!isFooter && _footerIndex > NoFooterIndex)
                                ++_footerIndex;
                            var item = value[i];
                            oldValue.Add(item);
                            decoratorManager.OnAdded(owner, this, item, i + offset);
                        }
                    }
                    else
                    {
                        for (var i = 0; i < value.Count; i++)
                        {
                            var oldItem = oldValue[i];
                            var newItem = value[i];
                            if (!Equals(oldItem, newItem))
                            {
                                oldValue[i] = newItem;
                                decoratorManager.OnReplaced(owner, this, oldItem, newItem, i + offset);
                            }
                        }

                        for (var i = oldValue.Count - 1; i >= value.Count; i--)
                        {
                            if (!isFooter && _footerIndex > NoFooterIndex)
                                --_footerIndex;
                            var item = oldValue[i];
                            oldValue.RemoveAt(i);
                            decoratorManager.OnRemoved(owner, this, item, offset + i);
                            --i;
                        }
                    }
                }
            }
        }
    }
}