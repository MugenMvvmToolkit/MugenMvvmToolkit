using System;
using System.Collections.Generic;
using MugenMvvm.Collections.Components;

namespace MugenMvvm.Collections
{
    internal sealed class SelectedItemClosure<T> where T : class
    {
        private readonly Func<T?> _getSelectedItem;
        private readonly Action<T?> _setSelectedItem;
        private readonly Func<IReadOnlyCollection<T>, T?, T?>? _getDefault;

        public SelectedItemClosure(Func<T?> getSelectedItem, Action<T?> setSelectedItem, Func<IReadOnlyCollection<T>, T?, T?>? getDefault)
        {
            _getSelectedItem = getSelectedItem;
            _setSelectedItem = setSelectedItem;
            _getDefault = getDefault;
        }

        public object? OnAdded(TrackerCollectionDecorator<T, object?> items, T item, object? state, int count)
        {
            if (!items.IsBatchUpdate)
            {
                var selectedItem = _getSelectedItem();
                if (selectedItem == null || !items.ItemsRaw.Comparer.Equals(selectedItem, item) && !items.ItemsRaw.ContainsKey(selectedItem))
                    Set(items, selectedItem);
            }

            return null;
        }

        public object? OnRemoved(TrackerCollectionDecorator<T, object?> items, T item, object? state, int count)
        {
            if (!items.IsBatchUpdate)
            {
                var selectedItem = _getSelectedItem();
                if (selectedItem == null || !items.ItemsRaw.ContainsKey(selectedItem))
                    Set(items, selectedItem);
            }

            return null;
        }

        public void OnEndBatchUpdate(TrackerCollectionDecorator<T, object?> items)
        {
            var selectedItem = _getSelectedItem();
            if (selectedItem == null || !items.ItemsRaw.ContainsKey(selectedItem))
                Set(items, selectedItem);
        }

        private void Set(TrackerCollectionDecorator<T, object?> items, T? selectedItem)
        {
            var item = _getDefault == null ? items.FirstOrDefault() : _getDefault(items, selectedItem);
            if (!ReferenceEquals(selectedItem, item))
                _setSelectedItem(item);
        }
    }
}