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
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.ViewModels
{
    [BaseViewModel(Priority = 4)]
    public class MultiViewModel<TViewModel> : CloseableViewModel, IMultiViewModel<TViewModel>
        where TViewModel : class, IViewModel
    {
        #region Nested types

        #endregion

        #region Fields

        private readonly INotifiableCollection<TViewModel> _itemsSource;
        private readonly EventHandler<ICloseableViewModel, ViewModelClosedEventArgs> _weakEventHandler;
        private readonly PropertyChangedEventHandler _propertyChangedWeakEventHandler;

        private bool _clearing;
        private TViewModel _selectedItem;
        private bool _disposeViewModelOnRemove;
        private bool _closeViewModelsOnClose;

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

        public virtual void AddViewModel(TViewModel viewModel, bool setSelected = true)
        {
            EnsureNotDisposed();
            Should.NotBeNull(viewModel, nameof(viewModel));
            if (!ItemsSource.Contains(viewModel))
                ItemsSource.Add(viewModel);
            if (setSelected)
                SelectedItem = viewModel;
        }

        public virtual Task<bool> RemoveViewModelAsync(TViewModel viewModel, object parameter = null)
        {
            EnsureNotDisposed();
            Should.NotBeNull(viewModel, nameof(viewModel));
            if (!ItemsSource.Contains(viewModel))
                return Empty.FalseTask;
            var result = viewModel
                .TryCloseAsync(parameter, null, NavigationType.Tab)
                .TryExecuteSynchronously(task =>
                {
                    if (task.Result)
                        ItemsSource.Remove(viewModel);
                    return task.Result;
                });
            result.WithTaskExceptionHandler(this);
            return result;
        }

        public virtual void Clear()
        {
            if (ItemsSource.Count == 0)
                return;
            try
            {
                _clearing = true;
                var viewModels = ItemsSource.ToArrayEx();
                ItemsSource.Clear();
                SelectedItem = null;
                OnViewModelsChanged(null, viewModels, 0);
            }
            catch
            {
                _clearing = false;
            }
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

        protected virtual void OnSelectedItemChanged(IViewModel oldValue, IViewModel newValue)
        {
        }

        protected virtual void OnViewModelAdded(IViewModel viewModel)
        {
        }

        protected virtual void OnViewModelRemoved(IViewModel viewModel)
        {
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

                    (viewModel as INavigableViewModel)?.OnNavigatedFrom(new NavigationContext(NavigationType.Tab, NavigationMode.Back, viewModel, SelectedItem, this));

                    var selectable = viewModel as ISelectable;
                    if (selectable != null)
                    {
                        selectable.PropertyChanged -= _propertyChangedWeakEventHandler;
                        if (selectable.IsSelected)
                            selectable.IsSelected = false;
                    }
                    RaiseViewModelRemoved(viewModel);
                    if (DisposeViewModelOnRemove)
                        viewModel.Dispose();
                }
            }
        }

        private void RaiseViewModelAdded(TViewModel vm)
        {
            vm.Settings.Metadata.AddOrUpdate(ViewModelConstants.StateNotNeeded, true);
            OnViewModelAdded(vm);
            ViewModelAdded?.Invoke(this, new ValueEventArgs<TViewModel>(vm));
        }

        private void RaiseViewModelRemoved(TViewModel vm)
        {
            vm.Settings.Metadata.Remove(ViewModelConstants.StateNotNeeded);
            OnViewModelRemoved(vm);
            ViewModelRemoved?.Invoke(this, new ValueEventArgs<TViewModel>(vm));
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
            INavigableViewModel navigableViewModel;
            NavigationContext ctx = null;
            if (ItemsSource.Contains(oldValue))
            {
                selectable = oldValue as ISelectable;
                if (selectable != null)
                    selectable.IsSelected = false;

                navigableViewModel = oldValue as INavigableViewModel;
                if (navigableViewModel != null)
                {
                    ctx = new NavigationContext(NavigationType.Tab, NavigationMode.Refresh, oldValue, newValue, this);
                    navigableViewModel.OnNavigatedFrom(ctx);
                }
            }

            selectable = newValue as ISelectable;
            if (selectable != null)
                selectable.IsSelected = true;
            navigableViewModel = newValue as INavigableViewModel;
            navigableViewModel?.OnNavigatedTo(ctx ?? new NavigationContext(NavigationType.Tab, NavigationMode.Refresh, oldValue, newValue, this));

            OnSelectedItemChanged(oldValue, newValue);
            if (SelectedItemChanged == null)
                return;
            var args = new SelectedItemChangedEventArgs<TViewModel>(oldValue, newValue);
            ThreadManager.Invoke(Settings.EventExecutionMode, this, args, (model, eventArgs) =>
            {
                model.SelectedItemChanged?.Invoke(model, eventArgs);
            });
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
                    if (!vm.TryCloseAsync(parameter, null, NavigationType.Tab).Result)
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
            }
            base.OnDisposeInternal(disposing);
        }

        #endregion
    }
}
