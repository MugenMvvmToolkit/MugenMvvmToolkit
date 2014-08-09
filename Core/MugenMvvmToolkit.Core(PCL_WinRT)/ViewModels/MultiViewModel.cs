#region Copyright
// ****************************************************************************
// <copyright file="MultiViewModel.cs">
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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using MugenMvvmToolkit.Annotations;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Utils;

namespace MugenMvvmToolkit.ViewModels
{
    /// <summary>
    ///     Represents the view model that can contains a collection of other <see cref="IViewModel" />.
    /// </summary>
    [BaseViewModel(Priority = 4)]
    public class MultiViewModel : CloseableViewModel, IMultiViewModel
    {
        #region Fields

        private static readonly string IsSelectedPropertyName;

        private readonly IList<IViewModel> _itemsSource;
        private IViewModel _selectedItem;
        private int _prevIndex;
        private readonly EventHandler<ICloseableViewModel, ViewModelClosedEventArgs> _weakEventHandler;
        private readonly PropertyChangedEventHandler _propertyChangedWeakEventHandler;

        #endregion

        #region Constructors

        static MultiViewModel()
        {
            IsSelectedPropertyName = MvvmExtensions.GetPropertyName<ISelectable>(selectable => selectable.IsSelected);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="MultiViewModel" /> class.
        /// </summary>
        public MultiViewModel()
        {
            _prevIndex = -1;
            DisposeViewModelOnRemove = true;

            var collection = new SynchronizedNotifiableCollection<IViewModel>();
            _itemsSource = ApplicationSettings.ItemsSourceDecorator == null
                ? collection
                : (IList<IViewModel>)ApplicationSettings.ItemsSourceDecorator(collection);
            collection.CollectionChanged += OnViewModelsChanged;
            _weakEventHandler = ReflectionExtensions.CreateWeakDelegate<MultiViewModel, ViewModelClosedEventArgs, EventHandler<ICloseableViewModel, ViewModelClosedEventArgs>>(this,
                (model, o, arg3) => model.OnViewModelClosed(arg3), UnsubscribeAction, handler => handler.Handle, true);
            _propertyChangedWeakEventHandler = ReflectionExtensions.MakeWeakPropertyChangedHandler(this,
                (model, o, arg3) => model.OnItemPropertyChanged(o, arg3), true);
        }

        #endregion

        #region Implementation of IMultiViewModel

        /// <summary>
        ///     Gets or sets the value.
        ///     If <c>true</c> the view-model will disposed view model when it closed.
        /// </summary>
        public bool DisposeViewModelOnRemove { get; set; }

        /// <summary>
        ///     Gets or sets the selected view-model.
        /// </summary>
        public virtual IViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (ReferenceEquals(value, _selectedItem) || (value != null && !ItemsSource.Contains(value)))
                    return;
                int oldValueIndex = -1;
                if (_selectedItem != null)
                    oldValueIndex = ItemsSource.IndexOf(_selectedItem);
                _prevIndex = oldValueIndex;
                IViewModel oldValue = _selectedItem;
                _selectedItem = value;
                OnSelectedItemChangedInternal(oldValueIndex, oldValue, _selectedItem);
                OnPropertyChanged("SelectedItem");
            }
        }

        /// <summary>
        ///     Gets the collection of <see cref="IViewModel" />s.
        /// </summary>
        public IList<IViewModel> ItemsSource
        {
            get { return _itemsSource; }
        }

        /// <summary>
        ///     Adds the specified <see cref="IViewModel" /> to <see cref="IMultiViewModel.ItemsSource" />.
        /// </summary>
        /// <param name="viewModel">
        ///     The specified <see cref="IViewModel" />.
        /// </param>
        public virtual void AddViewModel(IViewModel viewModel)
        {
            EnsureNotDisposed();
            Should.NotBeNull(viewModel, "viewModel");
            if (!ItemsSource.Contains(viewModel))
                ItemsSource.Add(viewModel);
            SelectedItem = viewModel;
        }

        /// <summary>
        ///     Removes the specified <see cref="IViewModel" /> from <see cref="IMultiViewModel.ItemsSource" />.
        /// </summary>
        /// <param name="viewModel">
        ///     The specified <see cref="IViewModel" />.
        /// </param>
        /// <param name="parameter">The specified parameter, if any.</param>
        public virtual Task<bool> RemoveViewModelAsync(IViewModel viewModel, object parameter = null)
        {
            EnsureNotDisposed();
            Should.NotBeNull(viewModel, "viewModel");
            if (!ItemsSource.Contains(viewModel))
                return MvvmUtils.FalseTaskResult;
            var result = MvvmUtils
                .TryCloseAsync(viewModel, parameter, null)
                .TryExecuteSynchronously(task =>
                {
                    if (task.Result)
                        ItemsSource.Remove(viewModel);
                    return task.Result;
                });
            result.WithTaskExceptionHandler(this);
            return result;
        }

        /// <summary>
        ///     Clears all view models from <see cref="IMultiViewModel.ItemsSource" />.
        /// </summary>
        public virtual void Clear()
        {
            EnsureNotDisposed();
            while (ItemsSource.Count != 0)
                ItemsSource.RemoveAt(0);
            SelectedItem = null;
        }

        /// <summary>
        ///     Occurs when the <c>SelectedItem</c> property changed.
        /// </summary>
        public virtual event EventHandler<IMultiViewModel, SelectedItemChangedEventArgs<IViewModel>> SelectedItemChanged;

        /// <summary>
        ///     Occurs when a view model is added.
        /// </summary>
        public virtual event EventHandler<IMultiViewModel, ValueEventArgs<IViewModel>> ViewModelAdded;

        /// <summary>
        ///     Occurs when a view model is removed.
        /// </summary>
        public virtual event EventHandler<IMultiViewModel, ValueEventArgs<IViewModel>> ViewModelRemoved;

        #endregion

        #region Methods

        /// <summary>
        ///     Occurs when the <c>SelectedItem</c> property is changed.
        /// </summary>
        /// <param name="oldValue">The old value.</param>
        /// <param name="newValue">The new value.</param>
        protected virtual void OnSelectedItemChanged(IViewModel oldValue, IViewModel newValue)
        {
        }

        /// <summary>
        ///     Occurs when a view model is added.
        /// </summary>
        protected virtual void OnViewModelAdded(IViewModel viewModel)
        {
        }

        /// <summary>
        ///     Occurs when a view model is removed.
        /// </summary>
        protected virtual void OnViewModelRemoved(IViewModel viewModel)
        {
        }

        /// <summary>
        ///     Occurs when any workspace view model is changed.
        /// </summary>
        private void OnViewModelsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Should.BeSupported(e.Action != NotifyCollectionChangedAction.Reset, "The IMultiViewModel.ItemsSource doesn't support Clear method.");
            if (e.NewItems != null && e.NewItems.Count != 0)
            {
                for (int index = 0; index < e.NewItems.Count; index++)
                {
                    var viewModel = (IViewModel)e.NewItems[index];
                    // ReSharper disable once NotResolvedInText
                    Should.NotBeNull(viewModel, "newItem");
                    var closeableViewModel = viewModel as ICloseableViewModel;
                    if (closeableViewModel != null)
                        closeableViewModel.Closed += _weakEventHandler;
                    var selectable = viewModel as ISelectable;
                    if (selectable != null)
                        selectable.PropertyChanged += _propertyChangedWeakEventHandler;
                    OnViewModelAddedEvent(viewModel);
                }
            }

            if (e.OldItems != null && e.OldItems.Count != 0)
            {
                for (int index = 0; index < e.OldItems.Count; index++)
                {
                    var viewModel = (IViewModel)e.OldItems[index];
                    TrySetPreviousValue(viewModel, e.OldStartingIndex + index);

                    var closeableViewModel = viewModel as ICloseableViewModel;
                    if (closeableViewModel != null)
                        closeableViewModel.Closed -= _weakEventHandler;

                    var navigableViewModel = viewModel as INavigableViewModel;
                    if (navigableViewModel != null)
                        navigableViewModel.OnNavigatedFrom(new NavigationContext(NavigationMode.Back, viewModel,
                            SelectedItem, this));

                    var selectable = viewModel as ISelectable;
                    if (selectable != null)
                    {
                        if (selectable.IsSelected)
                            selectable.IsSelected = false;
                        selectable.PropertyChanged -= _propertyChangedWeakEventHandler;
                    }
                    OnViewModelRemovedEvent(viewModel);
                    if (DisposeViewModelOnRemove)
                        viewModel.Dispose();
                }
            }
        }

        private void OnViewModelAddedEvent(IViewModel vm)
        {
            OnViewModelAdded(vm);
            var handler = ViewModelAdded;
            if (handler != null) handler(this, new ValueEventArgs<IViewModel>(vm));
        }

        private void OnViewModelRemovedEvent(IViewModel vm)
        {
            OnViewModelRemoved(vm);
            var handler = ViewModelRemoved;
            if (handler != null) handler(this, new ValueEventArgs<IViewModel>(vm));
        }

        private void OnViewModelClosed(ViewModelClosedEventArgs args)
        {
            ItemsSource.Remove(args.ViewModel);
        }

        private void TrySetPreviousValue(IViewModel oldVm, int oldIndex)
        {
            if (!ReferenceEquals(oldVm, SelectedItem))
                return;
            if (_prevIndex >= 0 && oldIndex <= _prevIndex)
                --_prevIndex;
            var prevIndex = _prevIndex;
            if (prevIndex < 0)
            {
                if (oldIndex == 0)
                    prevIndex = oldIndex + 1;
                else
                    prevIndex = oldIndex - 1;
            }
            if (prevIndex >= 0 && ItemsSource.Count > prevIndex)
                SelectedItem = ItemsSource[prevIndex];
            else
                SelectedItem = ItemsSource.FirstOrDefault();
        }

        private void OnSelectedItemChangedInternal(int oldValueIndex, IViewModel oldValue, IViewModel newValue)
        {
            ISelectable selectable;
            INavigableViewModel navigableViewModel;
            NavigationContext ctx = null;
            if (oldValueIndex >= 0)
            {
                selectable = oldValue as ISelectable;
                if (selectable != null)
                    selectable.IsSelected = false;

                navigableViewModel = oldValue as INavigableViewModel;
                if (navigableViewModel != null)
                {
                    ctx = new NavigationContext(NavigationMode.Refresh, oldValue, newValue, this);
                    navigableViewModel.OnNavigatedFrom(ctx);
                }
            }

            selectable = newValue as ISelectable;
            if (selectable != null)
                selectable.IsSelected = true;
            navigableViewModel = newValue as INavigableViewModel;
            if (navigableViewModel != null)
                navigableViewModel.OnNavigatedTo(ctx ??
                                                 new NavigationContext(NavigationMode.Refresh, oldValue, newValue, this));

            OnSelectedItemChanged(oldValue, newValue);
            if (SelectedItemChanged == null)
                return;
            var args = new SelectedItemChangedEventArgs<IViewModel>(oldValue, newValue);
            ThreadManager.Invoke(Settings.EventExecutionMode, this, args, (model, eventArgs) =>
            {
                var handler = model.SelectedItemChanged;
                if (handler != null)
                    handler(model, eventArgs);
            });
        }

        private bool OnClosing()
        {
            while (ItemsSource.Count != 0)
            {
                var vm = ItemsSource[0];
                if (!RemoveViewModelAsync(vm).Result)
                    return false;
            }
            return true;
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            var selectableModel = sender as ISelectable;
            var vm = sender as IViewModel;
            if (vm == null || selectableModel == null || selectableModel.IsSelected ||
                args.PropertyName != IsSelectedPropertyName)
                return;
            if (selectableModel.IsSelected)
                SelectedItem = vm;
            else
                TrySetPreviousValue(vm, ItemsSource.IndexOf(vm));
        }

        private static void UnsubscribeAction(object sender, EventHandler<ICloseableViewModel, ViewModelClosedEventArgs> eventHandler)
        {
            var closeableViewModel = sender as ICloseableViewModel;
            if (closeableViewModel != null)
                closeableViewModel.Closed -= eventHandler;
        }

        #endregion

        #region Overrides of ViewModelBase

        /// <summary>
        ///     Occurs after the initialization of the current <see cref="ViewModelBase" />.
        /// </summary>
        internal override void OnInitializedInternal()
        {
            var notifiableCollection = _itemsSource as SynchronizedNotifiableCollection<IViewModel>;
            if (notifiableCollection != null)
                notifiableCollection.ThreadManager = ThreadManager;
            base.OnInitializedInternal();
        }

        /// <summary>
        ///     Occurs when view model is closing.
        /// </summary>
        /// <returns>
        ///     If <c>true</c> - close, otherwise <c>false</c>.
        /// </returns>
        protected override Task<bool> OnClosing(object parameter)
        {
            if (ItemsSource.Count == 0)
                return base.OnClosing(parameter);
            return Task.Factory.StartNew(function: OnClosing);
        }

        /// <summary>
        ///     Occurs after current view model disposed, use for clear resource and event listeners(Internal only).
        /// </summary>
        internal override void OnDisposeInternal(bool disposing)
        {
            if (disposing)
            {
                Clear();
                SelectedItemChanged = null;
                ViewModelAdded = null;
                ViewModelRemoved = null;
            }
            base.OnDisposeInternal(disposing);
        }

        #endregion
    }
}