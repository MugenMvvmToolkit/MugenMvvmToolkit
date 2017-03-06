#region Copyright

// ****************************************************************************
// <copyright file="WrapperViewModelBase.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.ViewModels
{
    public abstract class WrapperViewModelBase<TViewModel> : ViewModelBase, ICloseableViewModel, INavigableViewModel,
                                                             IHasDisplayName, ISelectable, IWrapperViewModel, IHasState
        where TViewModel : class, IViewModel
    {
        #region Fields

        private readonly object _locker;
        private readonly Dictionary<string, string> _wrappedPropertyNames;

        private string _displayName;
        private TViewModel _viewModel;
        private ICommand _closeCommand;
        private bool _isSelected;

        #endregion

        #region Constructors

        protected WrapperViewModelBase()
        {
            _locker = new object();
            _wrappedPropertyNames = new Dictionary<string, string>
            {
                {nameof(CloseCommand), nameof(CloseCommand)},
                {nameof(DisplayName), nameof(DisplayName)},
                {nameof(IsSelected), nameof(IsSelected)}
            };
            Settings.HandleBusyMessageMode |= HandleMode.Handle;
        }

        #endregion

        #region Properties

        public TViewModel ViewModel => _viewModel;

        protected IDictionary<string, string> WrappedPropertyNames => _wrappedPropertyNames;

        #endregion

        #region Implementation of interfaces

        IViewModel IWrapperViewModel.ViewModel => _viewModel;

        public virtual ICommand CloseCommand
        {
            get
            {
                var closeableViewModel = ViewModel as ICloseableViewModel;
                if (closeableViewModel == null)
                    return _closeCommand;
                return closeableViewModel.CloseCommand;
            }
            set
            {
                if (CloseCommand == value)
                    return;
                _closeCommand = value;
                var closeableViewModel = ViewModel as ICloseableViewModel;
                if (closeableViewModel == null)
                    _closeCommand = value;
                else
                    closeableViewModel.CloseCommand = value;
                OnPropertyChanged();
            }
        }

        public void Wrap(IViewModel viewModel, IDataContext context = null)
        {
            EnsureNotDisposed();
            Should.NotBeNull(viewModel, nameof(viewModel));
            lock (_locker)
            {
                if (_viewModel != null)
                    throw ExceptionManager.ObjectInitialized("ViewModel", viewModel);
                _viewModel = (TViewModel)viewModel;
            }
            //It indicates that wrapper is responsible for the view model state.
            _viewModel.Settings.Metadata.AddOrUpdate(ViewModelConstants.StateNotNeeded, true);
            ViewModel.PropertyChanged += ViewModelOnPropertyChanged;
            var closeableViewModel = ViewModel as ICloseableViewModel;
            if (closeableViewModel == null)
                CloseCommand = RelayCommandBase.FromAsyncHandler<object>(ExecuteCloseAsync, CanClose, false, this);
            ViewModel.Subscribe(this);
            this.Subscribe(_viewModel);
            OnWrapped(context);
            InvalidateProperties();
        }

        public virtual string DisplayName
        {
            get
            {
                var hasDisplayName = ViewModel as IHasDisplayName;
                if (hasDisplayName == null)
                    return _displayName;
                return hasDisplayName.DisplayName;
            }
            set
            {
                if (DisplayName == value)
                    return;
                _displayName = value;
                OnPropertyChanged();
            }
        }

        public virtual bool IsSelected
        {
            get
            {
                var selectable = ViewModel as ISelectable;
                if (selectable == null)
                    return _isSelected;
                return selectable.IsSelected;
            }
            set
            {
                if (value == IsSelected)
                    return;
                var selectable = ViewModel as ISelectable;
                if (selectable == null)
                    _isSelected = value;
                else
                    selectable.IsSelected = value;
                OnPropertyChanged();
            }
        }

        Task<bool> ICloseableViewModel.OnClosingAsync(IDataContext context)
        {
            return (ViewModel as ICloseableViewModel)?.OnClosingAsync(context);
        }

        void ICloseableViewModel.OnClosed(IDataContext context)
        {
            (ViewModel as ICloseableViewModel)?.OnClosed(context);
            OnClosed(context, context?.GetData(NavigationConstants.CloseParameter));
        }

        void INavigableViewModel.OnNavigatedTo(INavigationContext context)
        {
            (ViewModel as INavigableViewModel)?.OnNavigatedTo(context);
            OnShown(context);
        }

        Task<bool> INavigableViewModel.OnNavigatingFrom(INavigationContext context)
        {
            var navigableViewModel = ViewModel as INavigableViewModel;
            if (navigableViewModel == null)
                return Empty.TrueTask;
            return navigableViewModel.OnNavigatingFrom(context);
        }

        void INavigableViewModel.OnNavigatedFrom(INavigationContext context)
        {
            (ViewModel as INavigableViewModel)?.OnNavigatedFrom(context);
        }

        void IHasState.LoadState(IDataContext state)
        {
            if (ViewModel == null)
            {
                string typeName;
                if (state.TryGetData(ViewModelConstants.ViewModelTypeName, out typeName))
                {
                    var vmType = Type.GetType(typeName, false);
                    var vmState = state.GetData(ViewModelConstants.ViewModelState);
                    if (vmType != null)
                    {
                        var viewModel = ViewModelProvider.RestoreViewModel(vmState, new DataContext
                        {
                            {InitializationConstants.ViewModelType, vmType}
                        }, true);
                        Wrap(viewModel);
                    }
                }
            }
            OnLoadState(state);
        }

        void IHasState.SaveState(IDataContext state)
        {
            state.AddOrUpdate(ViewModelConstants.ViewModelTypeName, ViewModel.GetType().AssemblyQualifiedName);
            state.AddOrUpdate(ViewModelConstants.ViewModelState, ViewModelProvider.PreserveViewModel(ViewModel, DataContext.Empty));
            OnSaveState(state);
        }

        #endregion

        #region Methods

        private Task ExecuteCloseAsync(object parameter)
        {
            return this.CloseAsync(parameter);
        }

        protected virtual void OnWrapped(IDataContext context)
        {
        }

        protected virtual void OnClosed(IDataContext context, object parameter)
        {
        }

        protected virtual void OnShown(IDataContext context)
        {
        }

        protected virtual void OnLoadState(IDataContext state)
        {
        }

        protected virtual void OnSaveState(IDataContext state)
        {
        }

        private void ViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            string value;
            if (string.IsNullOrEmpty(args.PropertyName))
                OnPropertyChanged(args, ExecutionMode.None);
            else if (WrappedPropertyNames.TryGetValue(args.PropertyName, out value))
            {
                if (args.PropertyName != value)
                    args = new PropertyChangedEventArgs(value);
                OnPropertyChanged(args, ExecutionMode.None);
            }
        }

        private bool CanClose(object parameter)
        {
            var func = Settings.Metadata.GetData(ViewModelConstants.CanCloseHandler);
            if (func != null && !func(this, parameter))
                return false;
            return true;
        }

        #endregion

        #region Overrides of ViewModelBase

        internal override void OnDisposeInternal(bool disposing)
        {
            if (disposing)
            {
                if (_viewModel != null)
                {
                    _viewModel.PropertyChanged -= ViewModelOnPropertyChanged;
                    _viewModel.Settings.Metadata.Remove(ViewModelConstants.StateNotNeeded);
                    _viewModel = null;
                    OnPropertyChanged(nameof(ViewModel));
                }
            }
            base.OnDisposeInternal(disposing);
        }

        #endregion
    }
}
