#region Copyright

// ****************************************************************************
// <copyright file="MultiViewModel.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Annotations;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.Collections;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.ViewModels
{
    [BaseViewModel(Priority = 4)]
    public class MultiViewModel<TViewModel> : CloseableViewModel, IMultiViewModel<TViewModel>
        where TViewModel : class, IViewModel
    {
        #region Fields

        private readonly INotifiableCollection<TViewModel> _itemsSource;
        private readonly EventHandler<ICloseableViewModel, ViewModelClosedEventArgs> _weakEventHandler;
        private readonly PropertyChangedEventHandler _propertyChangedWeakEventHandler;

        private bool _clearing;
        private TViewModel _selectedItem;
        private bool _disposeViewModelOnRemove;
        private bool _closeViewModelsOnClose;
        private INavigationDispatcher _navigationDispatcher;
        private EventHandler<IMultiViewModel, SelectedItemChangedEventArgs<IViewModel>> _selectedItemChangedNonGeneric;
        private EventHandler<IMultiViewModel, ValueEventArgs<IViewModel>> _viewModelAddedNonGeneric;
        private EventHandler<IMultiViewModel, ValueEventArgs<IViewModel>> _viewModelRemovedNonGeneric;

        #endregion

        #region Constructors

        public MultiViewModel()
        {
            var collection = new SynchronizedNotifiableCollection<TViewModel>();
            var list = ServiceProvider.TryDecorate(this, collection);
            Should.BeOfType<INotifiableCollection<TViewModel>>(list, "DecoratedItemsSource");
            _itemsSource = (INotifiableCollection<TViewModel>)list;
            collection.AfterCollectionChanged = OnViewModelsChanged;
            _weakEventHandler = ReflectionExtensions.CreateWeakDelegate<MultiViewModel<TViewModel>, ViewModelClosedEventArgs, EventHandler<ICloseableViewModel, ViewModelClosedEventArgs>>(this,
                (model, o, arg3) => model.ItemsSource.Remove((TViewModel)arg3.ViewModel), UnsubscribeAction, handler => handler.Handle);
            _propertyChangedWeakEventHandler = ReflectionExtensions.MakeWeakPropertyChangedHandler(this, (model, o, arg3) => model.OnItemPropertyChanged(o, arg3));
            DisposeViewModelOnRemove = ApplicationSettings.MultiViewModelDisposeViewModelOnRemove;
            CloseViewModelsOnClose = ApplicationSettings.MultiViewModelCloseViewModelsOnClose;
        }

        #endregion

        #region Implementation of IMultiViewModel

        public bool DisposeViewModelOnRemove
        {
            get { return _disposeViewModelOnRemove; }
            set
            {
                if (value == _disposeViewModelOnRemove) return;
                _disposeViewModelOnRemove = value;
                OnPropertyChanged();
            }
        }

        public bool CloseViewModelsOnClose
        {
            get { return _closeViewModelsOnClose; }
            set
            {
                if (value == _closeViewModelsOnClose) return;
                _closeViewModelsOnClose = value;
                OnPropertyChanged();
            }
        }

        private INavigationDispatcher NavigationDispatcher
        {
            get
            {
                if (_navigationDispatcher == null)
                    _navigationDispatcher = this.GetIocContainer(true).Get<INavigationDispatcher>();
                return _navigationDispatcher;
            }
        }

        Type IMultiViewModel.ViewModelType => typeof(TViewModel);

        IViewModel IMultiViewModel.SelectedItem
        {
            get { return SelectedItem; }
            set
            {
                if (value == null || value is TViewModel)
                    SelectedItem = (TViewModel)value;
            }
        }

        IEnumerable<IViewModel> IMultiViewModel.ItemsSource => ItemsSource;

        public virtual TViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (ReferenceEquals(value, _selectedItem) || (value != null && !ItemsSource.Contains(value)))
                    return;
                TViewModel oldValue = _selectedItem;
                _selectedItem = value;
                OnSelectedItemChangedInternal(oldValue, _selectedItem);
                OnPropertyChanged(Empty.SelectedItemChangedArgs);
            }
        }

        public INotifiableCollection<TViewModel> ItemsSource => _itemsSource;

        public void AddViewModel(TViewModel viewModel, bool setSelected = true)
        {
            EnsureNotDisposed();
            Should.NotBeNull(viewModel, nameof(viewModel));
            AddViewModelInternal(viewModel, null, setSelected);
        }

        public void InsertViewModel(int index, TViewModel viewModel, bool setSelected = true)
        {
            EnsureNotDisposed();
            Should.NotBeNull(viewModel, nameof(viewModel));
            AddViewModelInternal(viewModel, index, setSelected);
        }

        public Task<bool> RemoveViewModelAsync(TViewModel viewModel, object parameter = null)
        {
            EnsureNotDisposed();
            Should.NotBeNull(viewModel, nameof(viewModel));
            return RemoveViewModelInternalAsync(viewModel, parameter);
        }

        public void Clear()
        {
            if (ItemsSource.Count == 0)
                return;
            try
            {
                _clearing = true;
                ClearInternal();
            }
            catch
            {
                _clearing = false;
            }
        }

        void IMultiViewModel.AddViewModel(IViewModel viewModel, bool setSelected)
        {
            AddViewModel((TViewModel)viewModel, setSelected);
        }

        void IMultiViewModel.InsertViewModel(int index, IViewModel viewModel, bool setSelected)
        {
            InsertViewModel(index, (TViewModel)viewModel, setSelected);
        }

        Task<bool> IMultiViewModel.RemoveViewModelAsync(IViewModel viewModel, object parameter)
        {
            return RemoveViewModelAsync((TViewModel)viewModel, parameter);
        }

        event EventHandler<IMultiViewModel, SelectedItemChangedEventArgs<IViewModel>> IMultiViewModel.SelectedItemChanged
        {
            add { _selectedItemChangedNonGeneric += value; }
            remove { _selectedItemChangedNonGeneric -= value; }
        }

        event EventHandler<IMultiViewModel, ValueEventArgs<IViewModel>> IMultiViewModel.ViewModelAdded
        {
            add { _viewModelAddedNonGeneric += value; }
            remove { _viewModelAddedNonGeneric -= value; }
        }

        event EventHandler<IMultiViewModel, ValueEventArgs<IViewModel>> IMultiViewModel.ViewModelRemoved
        {
            add { _viewModelRemovedNonGeneric += value; }
            remove { _viewModelRemovedNonGeneric -= value; }
        }

        public virtual event EventHandler<IMultiViewModel<TViewModel>, SelectedItemChangedEventArgs<TViewModel>> SelectedItemChanged;

        public virtual event EventHandler<IMultiViewModel<TViewModel>, ValueEventArgs<TViewModel>> ViewModelAdded;

        public virtual event EventHandler<IMultiViewModel<TViewModel>, ValueEventArgs<TViewModel>> ViewModelRemoved;

        #endregion

        #region Methods

        public void PreserveViewModels([NotNull] IDataContext context)
        {
            Should.NotBeNull(context, nameof(context));
            context.Remove(MultiViewModelState.ViewModelState);
            if (ItemsSource.Count == 0)
                return;
            var states = new MultiViewModelState { State = new List<IDataContext>() };
            for (int index = 0; index < ItemsSource.Count; index++)
            {
                var viewModel = ItemsSource[index];
                states.State.Add(ViewModelProvider.PreserveViewModel(viewModel, DataContext.Empty));
                if (ReferenceEquals(viewModel, SelectedItem))
                    context.AddOrUpdate(MultiViewModelState.SelectedIndex, index);
            }
            if (states.State.Count != 0)
                context.AddOrUpdate(MultiViewModelState.ViewModelState, states);
        }

        public void RestoreViewModels([NotNull] IDataContext context)
        {
            Should.NotBeNull(context, nameof(context));
            var states = context.GetData(MultiViewModelState.ViewModelState);
            if (states == null)
                return;
            var selectedIndex = context.GetData(MultiViewModelState.SelectedIndex);
            for (int index = 0; index < states.State.Count; index++)
            {
                var state = states.State[index];
                var viewModel = (TViewModel)ViewModelProvider.RestoreViewModel(state, DataContext.Empty, true);
                ItemsSource.Add(viewModel);
                if (selectedIndex == index)
                    SelectedItem = viewModel;
            }
            context.Remove(MultiViewModelState.ViewModelState);
            context.Remove(MultiViewModelState.SelectedIndex);
        }

        protected virtual void AddViewModelInternal([NotNull] TViewModel viewModel, int? index, bool setSelected = true)
        {
            if (!ItemsSource.Contains(viewModel))
            {
                if (index == null)
                    ItemsSource.Add(viewModel);
                else
                    ItemsSource.Insert(index.Value, viewModel);
            }
            if (setSelected)
                SelectedItem = viewModel;
        }

        protected virtual Task<bool> RemoveViewModelInternalAsync([NotNull] TViewModel viewModel, object parameter)
        {
            if (!ItemsSource.Contains(viewModel))
                return Empty.FalseTask;
            var result = NavigationDispatcher
                .NavigatingFromAsync(new NavigationContext(NavigationType.Tab, NavigationMode.Back, viewModel, SelectedItem, this), parameter)
                .TryExecuteSynchronously(task =>
                {
                    if (task.Result)
                        ItemsSource.Remove(viewModel);
                    return task.Result;
                });
            result.WithTaskExceptionHandler(this);
            return result;
        }

        protected virtual void ClearInternal()
        {
            var viewModels = ItemsSource.ToArrayEx();
            ItemsSource.Clear();
            SelectedItem = null;
            OnViewModelsChanged(null, viewModels, 0);
        }

        protected virtual void OnSelectedItemChanged(TViewModel oldValue, TViewModel newValue)
        {
        }

        protected virtual void OnViewModelAdded([NotNull] TViewModel viewModel)
        {
            viewModel.Settings.Metadata.AddOrUpdate(ViewModelConstants.StateNotNeeded, true);
        }

        protected virtual void OnViewModelRemoved([NotNull] TViewModel viewModel)
        {
            viewModel.Settings.Metadata.Remove(ViewModelConstants.StateNotNeeded);
        }

        protected virtual void RaiseViewModelAdded(TViewModel viewModel)
        {
            ViewModelAdded?.Invoke(this, new ValueEventArgs<TViewModel>(viewModel));
            _viewModelAddedNonGeneric?.Invoke(this, new ValueEventArgs<IViewModel>(viewModel));
        }

        protected virtual void RaiseViewModelRemoved(TViewModel viewModel)
        {
            ViewModelRemoved?.Invoke(this, new ValueEventArgs<TViewModel>(viewModel));
            _viewModelRemovedNonGeneric?.Invoke(this, new ValueEventArgs<IViewModel>(viewModel));
        }

        protected virtual void RaiseSelectedItemChanged(TViewModel oldValue, TViewModel newValue)
        {
            if (SelectedItemChanged == null && _selectedItemChangedNonGeneric == null)
                return;
            ThreadManager.Invoke(Settings.EventExecutionMode, this, oldValue, newValue, (model, oldVm, newVm) =>
            {
                model.SelectedItemChanged?.Invoke(model, new SelectedItemChangedEventArgs<TViewModel>(oldVm, newVm));
                model._selectedItemChangedNonGeneric?.Invoke(model, new SelectedItemChangedEventArgs<IViewModel>(oldVm, newVm));
            });
        }

        private void OnViewModelsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (_clearing)
                _clearing = false;
            else
                Should.BeSupported(e.Action != NotifyCollectionChangedAction.Reset, "The MultiViewModel.ItemsSource doesn't support Clear method.");
            OnViewModelsChanged(e.NewItems, e.OldItems, e.OldStartingIndex);
        }

        private void OnViewModelsChanged(IList newItems, IList oldItems, int oldStartingIndex)
        {
            if (newItems != null && newItems.Count != 0)
            {
                for (int index = 0; index < newItems.Count; index++)
                {
                    var viewModel = (TViewModel)newItems[index];
                    // ReSharper disable once NotResolvedInText
                    Should.NotBeNull(viewModel, "newItem");
                    var closeableViewModel = viewModel as ICloseableViewModel;
                    if (closeableViewModel != null)
                        closeableViewModel.Closed += _weakEventHandler;
                    var selectable = viewModel as ISelectable;
                    if (selectable != null)
                        selectable.PropertyChanged += _propertyChangedWeakEventHandler;
                    OnViewModelAdded(viewModel);
                    RaiseViewModelAdded(viewModel);
                }
            }

            if (oldItems != null && oldItems.Count != 0)
            {
                for (int index = 0; index < oldItems.Count; index++)
                {
                    var viewModel = (TViewModel)oldItems[index];
                    if (SelectedItem == null || ReferenceEquals(SelectedItem, viewModel))
                        TryUpdateSelectedValue(oldStartingIndex + index);

                    var closeableViewModel = viewModel as ICloseableViewModel;
                    if (closeableViewModel != null)
                        closeableViewModel.Closed -= _weakEventHandler;

                    NavigationDispatcher.OnNavigated(new NavigationContext(NavigationType.Tab, NavigationMode.Back, viewModel, SelectedItem, this));

                    var selectable = viewModel as ISelectable;
                    if (selectable != null)
                    {
                        selectable.PropertyChanged -= _propertyChangedWeakEventHandler;
                        if (selectable.IsSelected)
                            selectable.IsSelected = false;
                    }
                    OnViewModelRemoved(viewModel);
                    RaiseViewModelRemoved(viewModel);
                    if (DisposeViewModelOnRemove)
                        viewModel.Dispose();
                }
            }
        }

        private void TryUpdateSelectedValue(int oldIndex)
        {
            var maxIndex = ItemsSource.Count - 1;
            while (oldIndex > maxIndex)
                --oldIndex;
            if (oldIndex >= 0 && ItemsSource.Count > oldIndex)
                SelectedItem = ItemsSource[oldIndex];
            else
                SelectedItem = ItemsSource.FirstOrDefault();
        }

        private void OnSelectedItemChangedInternal(TViewModel oldValue, TViewModel newValue)
        {
            ISelectable selectable;
            if (ItemsSource.Contains(oldValue))
            {
                selectable = oldValue as ISelectable;
                if (selectable != null)
                    selectable.IsSelected = false;
            }

            selectable = newValue as ISelectable;
            if (selectable != null)
                selectable.IsSelected = true;
            NavigationDispatcher.OnNavigated(new NavigationContext(NavigationType.Tab, NavigationMode.Refresh, oldValue, newValue, this));

            OnSelectedItemChanged(oldValue, newValue);
            RaiseSelectedItemChanged(oldValue, newValue);
        }

        private bool OnClosingInternal(object parameter)
        {
            var viewModels = ItemsSource.ToList();
            int count = viewModels.Count;
            for (int i = 0; i < count; i++)
            {
                var vm = viewModels[i];
                var closeableViewModel = vm as ICloseableViewModel;
                if (closeableViewModel != null)
                    closeableViewModel.Closed -= _weakEventHandler;
                try
                {
                    if (!NavigationDispatcher.NavigatingFromAsync(new NavigationContext(NavigationType.Tab, NavigationMode.Back, vm, null, this), parameter).Result)
                    {
                        viewModels.RemoveRange(i, count - i);
                        break;
                    }
                }
                finally
                {
                    if (closeableViewModel != null)
                        closeableViewModel.Closed += _weakEventHandler;
                }
            }
            if (viewModels.Count == count)
            {
                Clear();
                return true;
            }
            ItemsSource.RemoveRange(viewModels);
            return false;
        }

        private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            var selectableModel = sender as ISelectable;
            var vm = sender as TViewModel;
            if (vm == null || selectableModel == null || args.PropertyName != "IsSelected")
                return;
            if (selectableModel.IsSelected)
                SelectedItem = vm;
            else if (ReferenceEquals(SelectedItem, vm))
            {
                if (ItemsSource.Count == 0 || ItemsSource.Count == 1)
                {
                    SelectedItem = null;
                    return;
                }
                var oldIndex = ItemsSource.IndexOf(vm);
                if (oldIndex > 0)
                    --oldIndex;
                else
                    ++oldIndex;
                TryUpdateSelectedValue(oldIndex);
            }
        }

        private static void UnsubscribeAction(object sender, EventHandler<ICloseableViewModel, ViewModelClosedEventArgs> eventHandler)
        {
            var closeableViewModel = sender as ICloseableViewModel;
            if (closeableViewModel != null)
                closeableViewModel.Closed -= eventHandler;
        }

        #endregion

        #region Overrides of ViewModelBase

        protected override Task<bool> OnClosing(object parameter)
        {
            if (CloseViewModelsOnClose && ItemsSource.Count != 0)
                return Task.Factory.StartNew(function: OnClosingInternal, state: parameter);
            return base.OnClosing(parameter);
        }

        internal override void OnDisposeInternal(bool disposing)
        {
            if (disposing)
            {
                SelectedItemChanged = null;
                ViewModelAdded = null;
                ViewModelRemoved = null;
                _selectedItemChangedNonGeneric = null;
                _viewModelAddedNonGeneric = null;
                _viewModelRemovedNonGeneric = null;
            }
            base.OnDisposeInternal(disposing);
        }

        #endregion
    }
}
