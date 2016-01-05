#region Copyright

// ****************************************************************************
// <copyright file="WrapperViewModelBase.cs">
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.ViewModels
{
    public abstract class WrapperViewModelBase<TViewModel> : ViewModelBase, ICloseableViewModel, INavigableViewModel,
                                                             IHasOperationResult, IHasDisplayName, ISelectable, IWrapperViewModel, IHasState
        where TViewModel : class, IViewModel
    {
        #region Fields

        private readonly object _locker;
        private readonly Dictionary<string, string> _wrappedPropertyNames;

        private string _displayName;
        private TViewModel _viewModel;
        private ICommand _closeCommand;
        private bool? _operationResult;
        private bool _isSelected;

        #endregion

        #region Constructors

        protected WrapperViewModelBase()
        {
            _locker = new object();
            _wrappedPropertyNames = new Dictionary<string, string>
            {
                {"CloseCommand", "CloseCommand"},
                {"DisplayName", "DisplayName"},
                {"IsSelected", "IsSelected"},
                {"OperationResult", "OperationResult"}
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
                CloseCommand = RelayCommandBase.FromAsyncHandler<object>(CloseAsync, false);
            else
            {
                closeableViewModel.Closing += ViewModelOnClosing;
                closeableViewModel.Closed += ViewModelOnClosed;
            }
            ViewModel.Subscribe(this);
            this.Subscribe(_viewModel);
            OnWrapped(context);
            InvalidateProperties();
        }

        Task<bool> ICloseableViewModel.CloseAsync(object parameter)
        {
            var closeableViewModel = ViewModel as ICloseableViewModel;
            if (closeableViewModel != null)
                return closeableViewModel.CloseAsync(parameter);
            if (!RaiseClosing(parameter))
                return Empty.FalseTask;
            RaiseClosed(parameter);
            return Empty.TrueTask;
        }

        public virtual event EventHandler<ICloseableViewModel, ViewModelClosingEventArgs> Closing;

        public virtual event EventHandler<ICloseableViewModel, ViewModelClosedEventArgs> Closed;

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

        public virtual bool? OperationResult
        {
            get { return ViewModelExtensions.GetOperationResult(ViewModel, _operationResult); }
            protected set
            {
                if (OperationResult == value)
                    return;
                _operationResult = value;
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

        void INavigableViewModel.OnNavigatedTo(INavigationContext context)
        {
            var navigableViewModel = ViewModel as INavigableViewModel;
            if (navigableViewModel != null)
                navigableViewModel.OnNavigatedTo(context);
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
            var navigableViewModel = ViewModel as INavigableViewModel;
            if (navigableViewModel != null)
                navigableViewModel.OnNavigatedFrom(context);
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

        protected Task<bool> CloseAsync(object parameter = null)
        {
            var t = this.TryCloseAsync(parameter, null);
            t.WithTaskExceptionHandler(this);
            return t;
        }

        protected virtual void OnWrapped(IDataContext context)
        {
        }

        protected virtual void OnClosed([CanBeNull] object parameter)
        {
        }

        protected virtual void OnShown([CanBeNull] object parameter)
        {
        }

        protected virtual void OnLoadState(IDataContext state)
        {
        }

        protected virtual void OnSaveState(IDataContext state)
        {
        }

        protected virtual bool RaiseClosing(object parameter)
        {
            var handler = Closing;
            if (handler == null)
                return true;
            var args = new ViewModelClosingEventArgs(this, parameter);
            handler(this, args);
            return !args.Cancel;
        }

        protected virtual void RaiseClosed(object parameter)
        {
            Closed?.Invoke(this, new ViewModelClosedEventArgs(this, parameter));
            OnClosed(parameter);
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

        private void ViewModelOnClosing(ICloseableViewModel sender, ViewModelClosingEventArgs args)
        {
            args.Cancel = !RaiseClosing(args.Parameter);
        }

        private void ViewModelOnClosed(ICloseableViewModel sender, ViewModelClosedEventArgs args)
        {
            RaiseClosed(args.Parameter);
        }

        #endregion

        #region Overrides of ViewModelBase

        internal override void OnDisposeInternal(bool disposing)
        {
            if (disposing)
            {
                Closing = null;
                Closed = null;
                if (_viewModel != null)
                {
                    var closeableViewModel = _viewModel as ICloseableViewModel;
                    if (closeableViewModel != null)
                    {
                        closeableViewModel.Closing -= ViewModelOnClosing;
                        closeableViewModel.Closed -= ViewModelOnClosed;
                    }
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
