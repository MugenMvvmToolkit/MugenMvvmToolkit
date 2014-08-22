#region Copyright
// ****************************************************************************
// <copyright file="GridViewModel.cs">
// Copyright © Vyacheslav Volkov 2012-2014
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************
#endregion
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using MugenMvvmToolkit.Annotations;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Interfaces.Collections;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.ViewModels
{
    /// <summary>
    ///     Represents the base class for linear lists.
    /// </summary>
    /// <typeparam name="T">The type of model.</typeparam>
    [BaseViewModel(Priority = 7)]
    public class GridViewModel<T> : ViewModelBase, IGridViewModel<T> where T : class
    {
        #region Fields

        private readonly object _locker;
        private FilterDelegate<T> _filter;
        private IList<T> _itemsSource;
        private IList<T> _originalData;
        private FilterableNotifiableCollection<T> _filterableItemsSource;

        private T _selectedItem;
        private readonly PropertyChangedEventHandler _weakPropertyHandler;

        private EventHandler<IGridViewModel, SelectedItemChangedEventArgs> _selectedItemChangedNonGeneric;
        private EventHandler<IGridViewModel, ItemsSourceChangedEventArgs> _itemsSourceChangedNonGeneric;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="GridViewModel{T}" /> class.
        /// </summary>
        public GridViewModel()
        {
            _locker = new object();
            SetOriginalItemsSource(new SynchronizedNotifiableCollection<T>());
            _weakPropertyHandler = ReflectionExtensions
                .MakeWeakPropertyChangedHandler(this, (model, o, arg3) => model.OnSelectedItemPropertyChanged(o, arg3));
            ChangeItemSelectedState = true;
        }

        #endregion

        #region Propreties

        /// <summary>
        ///     Gets the filterable items source.
        /// </summary>
        protected FilterableNotifiableCollection<T> FilterableItemsSource
        {
            get { return _filterableItemsSource; }
        }

        /// <summary>
        /// Gets or sets the value that indicates that the current view model will change the IsSelected property in <see cref="ISelectable"/> model.
        /// </summary>
        public bool ChangeItemSelectedState { get; set; }

        #endregion

        #region Implementation of IGridViewModel

        /// <summary>
        ///     Gets the type of model.
        /// </summary>
        Type IGridViewModel.ModelType
        {
            get { return typeof(T); }
        }

        /// <summary>
        ///     Gets the original collection of items source without the filter.
        /// </summary>
        IList IGridViewModel.OriginalItemsSource
        {
            get { return (IList)OriginalItemsSource; }
        }

        /// <summary>
        ///     Gets the collection of objects.
        /// </summary>
        IEnumerable IGridViewModel.ItemsSource
        {
            get { return ItemsSource; }
        }

        /// <summary>
        ///     Gets or sets the selected item.
        /// </summary>
        object IGridViewModel.SelectedItem
        {
            get { return SelectedItem; }
            set { SelectedItem = (T)value; }
        }

        /// <summary>
        ///     Gets or sets the filter.
        /// </summary>
        FilterDelegate<object> IGridViewModel.Filter
        {
            set { Filter = value; }
        }

        /// <summary>
        ///     Gets or sets the filter.
        /// </summary>
        public virtual FilterDelegate<T> Filter
        {
            get { return _filter; }
            set
            {
                if (Equals(value, _filter)) return;
                _filter = value;
                UpdateFilter();
                OnPropertyChanged("Filter");
            }
        }

        /// <summary>
        ///     Updates the current <see cref="IGridViewModel.ItemsSource" />.
        /// </summary>
        /// <param name="value">The new item source value.</param>
        void IGridViewModel.UpdateItemsSource(IEnumerable value)
        {
            UpdateItemsSource((IEnumerable<T>)value);
        }

        /// <summary>
        ///     Gets the original collection of items source without the filter.
        /// </summary>
        public virtual IList<T> OriginalItemsSource
        {
            get { return FilterableItemsSource.SourceCollection; }
        }

        /// <summary>
        ///     Gets or sets the collection of objects.
        /// </summary>
        public virtual IList<T> ItemsSource
        {
            get { return _itemsSource; }
        }

        /// <summary>
        ///     Gets or sets the selected item.
        /// </summary>
        public virtual T SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (Equals(_selectedItem, value))
                    return;
                T oldValue = _selectedItem;
                _selectedItem = OnSelectedItemChanging(value);
                if (Equals(_selectedItem, oldValue))
                    return;

                if (_selectedItem != null)
                {
                    FilterDelegate<T> filter = Filter;
                    if (filter != null && !filter(_selectedItem))
                        _selectedItem = null;
                }

                TryUpdatePropertyChanged(oldValue, false);
                TryUpdatePropertyChanged(_selectedItem, true);

                if (ChangeItemSelectedState)
                {
                    var selectable = oldValue as ISelectable;
                    if (selectable != null)
                        selectable.IsSelected = false;

                    selectable = _selectedItem as ISelectable;
                    if (selectable != null)
                        selectable.IsSelected = true;
                }

                OnSelectedItemChanged(oldValue, _selectedItem);
                RaiseSelectedItemChanged(oldValue, _selectedItem);
                OnPropertyChanged("SelectedItem");
            }
        }

        /// <summary>
        ///     Updates the current <see cref="IGridViewModel{T}.ItemsSource" />.
        /// </summary>
        /// <param name="value">The new items source value.</param>
        public void UpdateItemsSource(IEnumerable<T> value)
        {
            EnsureNotDisposed();
            UpdateItemsSourceInternal(value);
        }

        /// <summary>
        ///     Sets the original collection of items.
        /// </summary>
        /// <param name="originalItemsSource">The source collection.</param>
        public void SetOriginalItemsSource<TItemsSource>(TItemsSource originalItemsSource)
            where TItemsSource : IList<T>, INotifyCollectionChanged, IList
        {
            EnsureNotDisposed();
            Should.NotBeNull(originalItemsSource, "originalItemsSource");
            lock (_locker)
            {
                INotifyCollectionChanging collectionChanging;
                if (_originalData != null)
                {
                    collectionChanging = _originalData as INotifyCollectionChanging;
                    if (collectionChanging != null)
                        collectionChanging.CollectionChanging -= RaiseCollectionChanging;
                    ((INotifyCollectionChanged)(_originalData)).CollectionChanged -= RaiseCollectionChanged;
                    if (_originalData.Count != 0)
                        originalItemsSource.AddRange(_originalData);
                }
                _filterableItemsSource = new FilterableNotifiableCollection<T>(originalItemsSource);
                collectionChanging = originalItemsSource as INotifyCollectionChanging;
                if (collectionChanging != null)
                    collectionChanging.CollectionChanging += RaiseCollectionChanging;
                originalItemsSource.CollectionChanged += RaiseCollectionChanged;

                _originalData = originalItemsSource;
                _itemsSource = ServiceProvider.TryDecorate(FilterableItemsSource);
            }
            UpdateFilter();
            OnPropertyChanged("ItemsSource");
            OnPropertyChanged("OriginalItemsSource");
        }

        /// <summary>
        ///     Updates the filter state.
        /// </summary>
        public void UpdateFilter()
        {
            UpdateFilterInternal();
        }

        /// <summary>
        ///     Occurs when the <c>SelectedItem</c> property changed.
        /// </summary>
        event EventHandler<IGridViewModel, SelectedItemChangedEventArgs> IGridViewModel.SelectedItemChanged
        {
            add { _selectedItemChangedNonGeneric += value; }
            remove { _selectedItemChangedNonGeneric -= value; }
        }

        /// <summary>
        ///     Occurs when the <c>ItemsSource</c> property changed.
        /// </summary>
        event EventHandler<IGridViewModel, ItemsSourceChangedEventArgs> IGridViewModel.ItemsSourceChanged
        {
            add { _itemsSourceChangedNonGeneric += value; }
            remove { _itemsSourceChangedNonGeneric -= value; }
        }

        /// <summary>
        ///     Occurs when the <c>SelectedItem</c> property changed.
        /// </summary>
        public virtual event EventHandler<IGridViewModel, SelectedItemChangedEventArgs<T>> SelectedItemChanged;

        /// <summary>
        ///     Occurs when the <c>ItemsSource</c> property changed.
        /// </summary>
        public virtual event EventHandler<IGridViewModel, ItemsSourceChangedEventArgs<T>> ItemsSourceChanged;

        /// <summary>
        ///     Occurs before the collection changes.
        /// </summary>
        public virtual event NotifyCollectionChangingEventHandler CollectionChanging;

        /// <summary>
        ///     Occurs when the collection changes.
        /// </summary>
        public virtual event NotifyCollectionChangedEventHandler CollectionChanged;

        #endregion

        #region Methods

        /// <summary>
        ///     Updates the current <see cref="IGridViewModel{T}.ItemsSource" />.
        /// </summary>
        /// <param name="value">The new items source value.</param>
        protected virtual void UpdateItemsSourceInternal(IEnumerable<T> value)
        {
            value = OnItemsSourceChanging(value);
            SelectedItem = null;

            if (value == null)
                _originalData.Clear();
            else
            {
                using (FilterableItemsSource.SuspendNotifications())
                {
                    _originalData.Clear();
                    _originalData.AddRange(value);
                }
            }
            UpdateFilter();
            OnItemsSourceChanged(value);
            RaiseItemsSourceChanged(value);
            OnPropertyChanged("ItemsSource");
            OnPropertyChanged("OriginalItemsSource");
        }

        /// <summary>
        ///     Updates the filter state.
        /// </summary>
        protected virtual void UpdateFilterInternal()
        {
            FilterDelegate<T> filter = Filter;
            if (filter == FilterableItemsSource.Filter)
                FilterableItemsSource.UpdateFilter();
            else
                FilterableItemsSource.Filter = filter;
            if (SelectedItem != null && filter != null && !filter(SelectedItem))
                SelectedItem = null;
        }

        /// <summary>
        ///     Occurs when the <c>SelectedItem</c> property changing.
        /// </summary>
        /// <param name="newValue">The new value.</param>
        /// <returns>The value to set as selected item.</returns>
        protected virtual T OnSelectedItemChanging(T newValue)
        {
            return newValue;
        }

        /// <summary>
        ///     Occurs when the <c>SelectedItem</c> property changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnSelectedItemChanged(T oldValue, T newValue)
        {
        }

        /// <summary>
        ///     Occurs when the <c>ItemsSource</c> property changing.
        /// </summary>
        /// <param name="data">The new item source data.</param>
        /// <returns>
        ///     An instance of <see cref="IEnumerable{T}" />.
        /// </returns>
        protected virtual IEnumerable<T> OnItemsSourceChanging(IEnumerable<T> data)
        {
            return data;
        }

        /// <summary>
        ///     Occurs when the <c>ItemsSource</c> property changed.
        /// </summary>
        /// <param name="data">The new item source data.</param>
        protected virtual void OnItemsSourceChanged(IEnumerable<T> data)
        {
        }

        /// <summary>
        ///     Invokes the <c>SelectedItemChanged</c> event.
        /// </summary>
        protected void RaiseSelectedItemChanged(T oldValue, T newValue)
        {
            if (SelectedItemChanged == null && _selectedItemChangedNonGeneric == null)
                return;
            var args = new SelectedItemChangedEventArgs<T>(oldValue, newValue);
            ThreadManager.Invoke(Settings.EventExecutionMode, this, args, (model, eventArgs) =>
            {
                var genericHandler = model.SelectedItemChanged;
                var nonGenericHandler = model._selectedItemChangedNonGeneric;
                if (genericHandler != null)
                    genericHandler(model, eventArgs);
                if (nonGenericHandler != null)
                    nonGenericHandler(model, eventArgs);
            });
        }

        /// <summary>
        ///     Invokes the event <c>ItemsSourceChanged</c>.
        /// </summary>
        protected void RaiseItemsSourceChanged(IEnumerable<T> data)
        {
            if (ItemsSourceChanged == null && _itemsSourceChangedNonGeneric == null)
                return;
            var args = new ItemsSourceChangedEventArgs<T>(data);
            ThreadManager.Invoke(Settings.EventExecutionMode, this, args, (model, eventArgs) =>
            {
                var genericHandler = model.ItemsSourceChanged;
                var nonGenericHandler = model._itemsSourceChangedNonGeneric;
                if (genericHandler != null)
                    genericHandler(model, eventArgs);
                if (nonGenericHandler != null)
                    nonGenericHandler(model, eventArgs);
            });
        }

        /// <summary>
        ///     Occurs when selected item property changed.
        /// </summary>
        protected virtual void OnSelectedItemPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
        }

        /// <summary>
        ///     Occurs when collection changing.
        /// </summary>
        protected virtual void OnCollectionChanging(object sender, NotifyCollectionChangingEventArgs args)
        {
        }

        /// <summary>
        ///     Occurs when collection changed
        /// </summary>
        protected virtual void OnCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
        }

        private void TryUpdatePropertyChanged(object item, bool subcribe)
        {
            var notifyPropertyChanged = item as INotifyPropertyChanged;
            if (notifyPropertyChanged == null)
                return;
            if (subcribe)
                notifyPropertyChanged.PropertyChanged += _weakPropertyHandler;
            else
                notifyPropertyChanged.PropertyChanged -= _weakPropertyHandler;
        }

        private void RaiseCollectionChanging(object sender, NotifyCollectionChangingEventArgs e)
        {
            OnCollectionChanging(sender, e);
            var eventHandler = CollectionChanging;
            if (eventHandler != null)
                eventHandler(this, e);
        }

        private void RaiseCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChanged(sender, e);
            var eventHandler = CollectionChanged;
            if (eventHandler != null)
                eventHandler(this, e);
        }

        #endregion

        #region Overrides of ViewModelBase

        /// <summary>
        ///     Occurs after the initialization of the current <see cref="ViewModelBase" />.
        /// </summary>
        internal override void OnInitializedInternal()
        {
            if (ThreadManager != null)
                FilterableItemsSource.ThreadManager = ThreadManager;
            base.OnInitializedInternal();
        }

        /// <summary>
        ///     Occurs after current view model disposed, use for clear resource and event listeners(Internal only).
        /// </summary>
        internal override void OnDisposeInternal(bool disposing)
        {
            if (disposing)
            {
                _itemsSourceChangedNonGeneric = null;
                _selectedItemChangedNonGeneric = null;
                SelectedItemChanged = null;
                ItemsSourceChanged = null;
                CollectionChanging = null;
                CollectionChanging = null;
                TryUpdatePropertyChanged(SelectedItem, false);
            }
            base.OnDisposeInternal(disposing);
        }

        #endregion
    }
}