#region Copyright

// ****************************************************************************
// <copyright file="MultiViewModel.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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
using System.Runtime.Serialization;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Annotations;
using MugenMvvmToolkit.Collections;
using MugenMvvmToolkit.Interfaces.Collections;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.ViewModels
{
    [BaseViewModel(Priority = 4)]
    public class MultiViewModel : CloseableViewModel, IMultiViewModel
    {
        #region Nested types

        //NOTE we cannot use default list, because MONO cannot deserialize it correctly.
        [DataContract(Namespace = ApplicationSettings.DataContractNamespace, IsReference = true), Serializable]
        internal sealed class StateList
        {
            [DataMember]
            public List<IDataContext> State;
        }

        #endregion

        #region Fields

        private static readonly DataConstant<StateList> ViewModelState;
        private static readonly DataConstant<int> SelectedIndex;

        private readonly INotifiableCollection<IViewModel> _itemsSource;
        private readonly EventHandler<ICloseableViewModel, ViewModelClosedEventArgs> _weakEventHandler;
        private readonly PropertyChangedEventHandler _propertyChangedWeakEventHandler;

        private bool _clearing;
        private IViewModel _selectedItem;

        #endregion

        #region Constructors

        static MultiViewModel()
        {
            ViewModelState = DataConstant.Create(() => ViewModelState, true);
            SelectedIndex = DataConstant.Create(() => SelectedIndex);
        }

        public MultiViewModel()
        {
            var collection = new SynchronizedNotifiableCollection<IViewModel>();
            var list = ServiceProvider.TryDecorate(collection);
            Should.BeOfType<INotifiableCollection<IViewModel>>(list, "DecoratedItemsSource");
            _itemsSource = (INotifiableCollection<IViewModel>)list;
            collection.AfterCollectionChanged = OnViewModelsChanged;
            _weakEventHandler = ReflectionExtensions.CreateWeakDelegate<MultiViewModel, ViewModelClosedEventArgs, EventHandler<ICloseableViewModel, ViewModelClosedEventArgs>>(this,
                (model, o, arg3) => model.ItemsSource.Remove(arg3.ViewModel), UnsubscribeAction, handler => handler.Handle);
            _propertyChangedWeakEventHandler = ReflectionExtensions.MakeWeakPropertyChangedHandler(this, (model, o, arg3) => model.OnItemPropertyChanged(o, arg3));
            DisposeViewModelOnRemove = true;
        }

        #endregion

        #region Implementation of IMultiViewModel

        public bool DisposeViewModelOnRemove { get; set; }

        public virtual IViewModel SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (ReferenceEquals(value, _selectedItem) || (value != null && !ItemsSource.Contains(value)))
                    return;
                IViewModel oldValue = _selectedItem;
                _selectedItem = value;
                OnSelectedItemChangedInternal(oldValue, _selectedItem);
                OnPropertyChanged(Empty.SelectedItemChangedArgs);
            }
        }

        public INotifiableCollection<IViewModel> ItemsSource
        {
            get { return _itemsSource; }
        }

        public virtual void AddViewModel(IViewModel viewModel, bool setSelected = true)
        {
            EnsureNotDisposed();
            Should.NotBeNull(viewModel, "viewModel");
            if (!ItemsSource.Contains(viewModel))
                ItemsSource.Add(viewModel);
            if (setSelected)
                SelectedItem = viewModel;
        }

        public virtual Task<bool> RemoveViewModelAsync(IViewModel viewModel, object parameter = null)
        {
            EnsureNotDisposed();
            Should.NotBeNull(viewModel, "viewModel");
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
            EnsureNotDisposed();
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

        public virtual event EventHandler<IMultiViewModel, SelectedItemChangedEventArgs<IViewModel>> SelectedItemChanged;

        public virtual event EventHandler<IMultiViewModel, ValueEventArgs<IViewModel>> ViewModelAdded;

        public virtual event EventHandler<IMultiViewModel, ValueEventArgs<IViewModel>> ViewModelRemoved;

        #endregion

        #region Methods

        public void PreserveViewModels([NotNull] IDataContext context)
        {
            Should.NotBeNull(context, "context");
            context.Remove(ViewModelState);
            if (ItemsSource.Count == 0)
                return;
            var states = new StateList { State = new List<IDataContext>() };
            for (int index = 0; index < ItemsSource.Count; index++)
            {
                var viewModel = ItemsSource[index];
                states.State.Add(ViewModelProvider.PreserveViewModel(viewModel, DataContext.Empty));
                if (ReferenceEquals(viewModel, SelectedItem))
                    context.AddOrUpdate(SelectedIndex, index);
            }
            if (states.State.Count != 0)
                context.AddOrUpdate(ViewModelState, states);
        }

        public void RestoreViewModels([NotNull] IDataContext context)
        {
            Should.NotBeNull(context, "context");
            var states = context.GetData(ViewModelState);
            if (states == null)
                return;
            var selectedIndex = context.GetData(SelectedIndex);
            for (int index = 0; index < states.State.Count; index++)
            {
                var state = states.State[index];
                var viewModel = ViewModelProvider.RestoreViewModel(state, DataContext.Empty, true);
                ItemsSource.Add(viewModel);
                if (selectedIndex == index)
                    SelectedItem = viewModel;
            }
            context.Remove(ViewModelState);
            context.Remove(SelectedIndex);
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
                    var viewModel = (IViewModel)newItems[index];
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
                    var viewModel = (IViewModel)oldItems[index];
                    if (SelectedItem == null || ReferenceEquals(SelectedItem, viewModel))
                        TryUpdateSelectedValue(oldStartingIndex + index);

                    var closeableViewModel = viewModel as ICloseableViewModel;
                    if (closeableViewModel != null)
                        closeableViewModel.Closed -= _weakEventHandler;

                    var navigableViewModel = viewModel as INavigableViewModel;
                    if (navigableViewModel != null)
                        navigableViewModel.OnNavigatedFrom(new NavigationContext(NavigationType.Tab, NavigationMode.Back, viewModel, SelectedItem, this));

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

        private void RaiseViewModelAdded(IViewModel vm)
        {
            OnViewModelAdded(vm);
            var handler = ViewModelAdded;
            if (handler != null) handler(this, new ValueEventArgs<IViewModel>(vm));
        }

        private void RaiseViewModelRemoved(IViewModel vm)
        {
            OnViewModelRemoved(vm);
            var handler = ViewModelRemoved;
            if (handler != null) handler(this, new ValueEventArgs<IViewModel>(vm));
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

        private void OnSelectedItemChangedInternal(IViewModel oldValue, IViewModel newValue)
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
            if (navigableViewModel != null)
                navigableViewModel.OnNavigatedTo(ctx ??
                                                 new NavigationContext(NavigationType.Tab, NavigationMode.Refresh, oldValue, newValue, this));

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
            var vm = sender as IViewModel;
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
            if (ItemsSource.Count == 0)
                return base.OnClosing(parameter);
            return Task.Factory.StartNew(function: OnClosingInternal, state: parameter);
        }

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
