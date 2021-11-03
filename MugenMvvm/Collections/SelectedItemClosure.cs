using System;
using System.Collections.Generic;
using MugenMvvm.Collections.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Internal;
using MugenMvvm.Models;

namespace MugenMvvm.Collections
{
    internal sealed class SelectedItemClosure<T, TState> : NotifyPropertyChangedBase, ISelectedItemTracker<T> where T : class
    {
        private readonly IReadOnlyObservableCollection _collection;
        private readonly Func<IReadOnlyCollection<T>, T?, T?>? _getDefault;
        private readonly Action<T?, TState>? _onChanged;
        private readonly TState _state;
        private T? _selectedItem;

        public SelectedItemClosure(IReadOnlyObservableCollection collection, Func<IReadOnlyCollection<T>, T?, T?>? getDefault, Action<T?, TState>? onChanged, TState state)
        {
            _collection = collection;
            _getDefault = getDefault;
            _onChanged = onChanged;
            _state = state;
            Tracker = null!;
        }

        public TrackerCollectionDecorator<T, object?> Tracker { get; set; }

        public T? SelectedItem
        {
            get => _selectedItem;
            set => SetSelectedItem(value);
        }

        public object? OnAdded(TrackerCollectionDecorator<T, object?> items, T item, object? state, int count)
        {
            if (!items.IsBatchUpdate)
            {
                if (SelectedItem == null || !items.ItemsRaw.Comparer.Equals(SelectedItem, item) && !items.ItemsRaw.ContainsKey(SelectedItem))
                    Reset();
            }

            return null;
        }

        public object? OnRemoved(TrackerCollectionDecorator<T, object?> items, T item, object? state, int count)
        {
            if (!items.IsBatchUpdate)
            {
                if (SelectedItem == null || !items.ItemsRaw.ContainsKey(SelectedItem))
                    Reset();
            }

            return null;
        }

        public void OnEndBatchUpdate(TrackerCollectionDecorator<T, object?> items)
        {
            if (SelectedItem == null || !items.ItemsRaw.ContainsKey(SelectedItem))
                Reset();
        }

        public void Dispose() => _collection.RemoveComponent(Tracker);

        public bool SetSelectedItem(T? value)
        {
            using var _ = _collection.Lock();
            if (Tracker.ItemsRaw.Comparer.Equals(SelectedItem, value) || !Tracker.Contains(value))
                return false;
            SetValue(value);
            return true;
        }

        private void SetValue(T? value)
        {
            _selectedItem = value;
            _onChanged?.Invoke(value, _state);
            OnPropertyChanged(Default.SelectedItemChangedArgs);
        }

        private void Reset()
        {
            var item = _getDefault == null ? Tracker.FirstOrDefault() : _getDefault(Tracker, SelectedItem);
            if (!Tracker.ItemsRaw.Comparer.Equals(SelectedItem, item))
                SetValue(item);
        }
    }
}