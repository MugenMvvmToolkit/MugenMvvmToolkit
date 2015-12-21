#region Copyright

// ****************************************************************************
// <copyright file="NavigationProvider.cs">
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.ViewModels;

#if WPF
using MugenMvvmToolkit.WPF.Interfaces.Navigation;

namespace MugenMvvmToolkit.WPF.Infrastructure.Navigation
#elif ANDROID
using MugenMvvmToolkit.Android.Interfaces.Navigation;

namespace MugenMvvmToolkit.Android.Infrastructure.Navigation
#elif TOUCH
using MugenMvvmToolkit.iOS.Interfaces.Navigation;

namespace MugenMvvmToolkit.iOS.Infrastructure.Navigation
#elif XAMARIN_FORMS
using System.Reflection;
using MugenMvvmToolkit.Xamarin.Forms.Interfaces.Navigation;

namespace MugenMvvmToolkit.Xamarin.Forms.Infrastructure.Navigation
#elif SILVERLIGHT
using MugenMvvmToolkit.Silverlight.Interfaces.Navigation;

namespace MugenMvvmToolkit.Silverlight.Infrastructure.Navigation
#elif WINDOWSCOMMON
using System.Reflection;
using MugenMvvmToolkit.WinRT.Interfaces.Navigation;

namespace MugenMvvmToolkit.WinRT.Infrastructure.Navigation
#elif WINDOWS_PHONE
using MugenMvvmToolkit.WinPhone.Interfaces.Navigation;

namespace MugenMvvmToolkit.WinPhone.Infrastructure.Navigation
#endif
{
    public class NavigationProvider : INavigationProviderEx
    {
        #region Nested types

        private sealed class CloseCommandWrapper : IRelayCommand
        {
            #region Fields

            private static readonly IDisposable EmptyDisposable;
            public readonly ICommand NestedCommand;
            private readonly NavigationProvider _provider;
            private readonly WeakReference _reference;

            #endregion

            #region Constructors

            static CloseCommandWrapper()
            {
                EmptyDisposable = new ActionToken(() => { });
            }

            public CloseCommandWrapper(ICommand nestedCommand, NavigationProvider provider, IViewModel viewModel)
            {
                NestedCommand = nestedCommand;
                _provider = provider;
                _reference = ToolkitExtensions.GetWeakReference(viewModel);
            }

            #endregion

            #region Properties

            private IRelayCommand RelayCommand
            {
                get { return NestedCommand as IRelayCommand; }
            }

            #endregion

            #region Implementation of ICommand

            public bool CanExecute(object parameter)
            {
                var target = _reference.Target as IViewModel;
                return target != null && _provider.NavigationService.CanClose(target, parameter as IDataContext) &&
                       (NestedCommand == null || NestedCommand.CanExecute(parameter));
            }

            public void Execute(object parameter)
            {
                var target = _reference.Target as IViewModel;
                if (target == null)
                    return;
                if (NestedCommand == null)
                    _provider.TryCloseViewModel(target, parameter);
                else
                    NestedCommand.Execute(parameter);
            }

            event EventHandler ICommand.CanExecuteChanged
            {
                add
                {
                    if (NestedCommand != null)
                        NestedCommand.CanExecuteChanged += value;
                }
                remove
                {
                    if (NestedCommand != null)
                        NestedCommand.CanExecuteChanged -= value;
                }
            }

            public void Dispose()
            {
                if (RelayCommand != null)
                    RelayCommand.Dispose();
            }

            public event PropertyChangedEventHandler PropertyChanged
            {
                add
                {
                    if (RelayCommand != null)
                        RelayCommand.PropertyChanged += value;
                }
                remove
                {
                    if (RelayCommand != null)
                        RelayCommand.PropertyChanged -= value;
                }
            }

            public bool IsNotificationsSuspended
            {
                get { return RelayCommand != null && RelayCommand.IsNotificationsSuspended; }
            }

            public IDisposable SuspendNotifications()
            {
                if (RelayCommand == null)
                    return EmptyDisposable;
                return RelayCommand.SuspendNotifications();
            }

            public bool HasCanExecuteImpl
            {
                get { return true; }
            }

            public CommandExecutionMode ExecutionMode
            {
                get
                {
                    if (RelayCommand == null)
                        return CommandExecutionMode.None;
                    return RelayCommand.ExecutionMode;
                }
                set
                {
                    if (RelayCommand != null)
                        RelayCommand.ExecutionMode = value;
                }
            }

            public ExecutionMode CanExecuteMode
            {
                get
                {
                    if (RelayCommand == null)
                        return MugenMvvmToolkit.Models.ExecutionMode.None;
                    return RelayCommand.CanExecuteMode;
                }
                set
                {
                    if (RelayCommand != null)
                        RelayCommand.CanExecuteMode = value;
                }
            }

            public IList<object> GetNotifiers()
            {
                if (RelayCommand == null)
                    return Empty.Array<object>();
                return RelayCommand.GetNotifiers();
            }

            public bool AddNotifier(object item)
            {
                if (RelayCommand == null)
                    return false;
                return RelayCommand.AddNotifier(item);
            }

            public bool RemoveNotifier(object item)
            {
                if (RelayCommand == null)
                    return false;
                return RelayCommand.RemoveNotifier(item);
            }

            public void ClearNotifiers()
            {
                if (RelayCommand != null)
                    RelayCommand.ClearNotifiers();
            }

            public void RaiseCanExecuteChanged()
            {
                if (RelayCommand != null)
                    RelayCommand.RaiseCanExecuteChanged();
            }

            #endregion
        }

        #endregion

        #region Fields

        protected static readonly DataConstant<Type> ViewModelTypeConstant;
        private static readonly DataConstant<object> IsNavigatedConstant;
        private static readonly string[] IdSeparator = { "~n|v~" };

        private readonly IViewMappingProvider _mappingProvider;
        private readonly IViewManager _viewManager;
        private readonly IViewModelProvider _viewModelProvider;
        private readonly IOperationCallbackManager _callbackManager;
        private readonly INavigationService _navigationService;
        private readonly IThreadManager _threadManager;
        private readonly INavigationCachePolicy _cachePolicy;
        private readonly EventHandler<ICloseableViewModel, ViewModelClosedEventArgs> _closeViewModelHandler;
        private readonly EventHandler<IDisposableObject, EventArgs> _disposeViewModelHandler;

        private WeakReference _vmReference;
        private bool _closedFromViewModel;
        private bool _ignoreNavigating;
        private IViewModel _closingViewModel;
        private NavigatingCancelEventArgsBase _navigatingCancelArgs;
        private IViewModel _navigationTargetVm;
        private IOperationCallback _currentCallback;
        private string _currentOperationId;
        private IDataContext _lastContext;
        private TaskCompletionSource<object> _navigatedTcs;

        #endregion

        #region Constructors

        static NavigationProvider()
        {
            var type = typeof(NavigationProvider);
            ViewModelTypeConstant = DataConstant.Create<Type>(type, nameof(ViewModelTypeConstant), true);
            IsNavigatedConstant = DataConstant.Create<object>(type, nameof(IsNavigatedConstant), false);
        }

        public NavigationProvider([NotNull] INavigationService navigationService, [NotNull] IThreadManager threadManager,
            [NotNull] IViewMappingProvider mappingProvider, [NotNull] IViewManager viewManager,
            [NotNull] IViewModelProvider viewModelProvider, IOperationCallbackManager callbackManager, INavigationCachePolicy cachePolicy = null)
        {
            Should.NotBeNull(navigationService, nameof(navigationService));
            Should.NotBeNull(threadManager, nameof(threadManager));
            Should.NotBeNull(mappingProvider, nameof(mappingProvider));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(viewModelProvider, nameof(viewModelProvider));
            Should.NotBeNull(callbackManager, nameof(callbackManager));
            _navigationService = navigationService;
            _threadManager = threadManager;
            _mappingProvider = mappingProvider;
            _viewManager = viewManager;
            _viewModelProvider = viewModelProvider;
            _callbackManager = callbackManager;
            _cachePolicy = cachePolicy;
            _vmReference = Empty.WeakReference;
            _closeViewModelHandler = CloseableViewModelOnClosed;
            _disposeViewModelHandler = ViewModelOnDisposed;

            NavigationService.Navigating += NavigationServiceOnNavigating;
            NavigationService.Navigated += NavigationServiceOnNavigated;
        }

        #endregion

        #region Properties

        [NotNull]
        protected IViewManager ViewManager
        {
            get { return _viewManager; }
        }

        [NotNull]
        protected IViewMappingProvider ViewMappingProvider
        {
            get { return _mappingProvider; }
        }

        [NotNull]
        protected IThreadManager ThreadManager
        {
            get { return _threadManager; }
        }

        [NotNull]
        protected IViewModelProvider ViewModelProvider
        {
            get { return _viewModelProvider; }
        }

        [NotNull]
        protected IOperationCallbackManager CallbackManager
        {
            get { return _callbackManager; }
        }

        #endregion

        #region Implementation of INavigationProvider

        public INavigationService NavigationService
        {
            get { return _navigationService; }
        }

        public IViewModel CurrentViewModel
        {
            get { return (IViewModel)_vmReference.Target; }
            protected set { _vmReference = ToolkitExtensions.GetWeakReferenceOrDefault(value, Empty.WeakReference, true); }
        }

        public virtual object CurrentContent
        {
            get { return _navigationService.CurrentContent; }
        }

        public Task CurrentNavigationTask
        {
            get
            {
                var tcs = _navigatedTcs;
                if (tcs == null)
                    return Empty.Task;
                return tcs.Task;
            }
        }

        public INavigationCachePolicy CachePolicy
        {
            get { return _cachePolicy; }
        }

        public virtual bool CanGoBack
        {
            get { return _navigationService.CanGoBack; }
        }

        public virtual bool CanGoForward
        {
            get { return _navigationService.CanGoForward; }
        }

        public virtual void GoBack()
        {
            _navigationService.GoBack();
        }

        public virtual void GoForward()
        {
            _navigationService.GoForward();
        }

        public Task NavigateAsync(IOperationCallback callback, IDataContext context)
        {
            Should.NotBeNull(context, nameof(context));
            IViewModel viewModel = context.GetData(NavigationConstants.ViewModel);
            if (viewModel == null)
                throw new InvalidOperationException($"The '{GetType()}' provider doesn't support the DataContext without navigation target.");
            context = context.ToNonReadOnly();
            if (ReferenceEquals(viewModel, CurrentViewModel))
            {
                if (callback != null)
                    CallbackManager.Register(OperationType.PageNavigation, viewModel, callback, context);
                return Empty.Task;
            }
            //The view model is already shown as page and we need to bring it to front
            if (viewModel.Settings.State.Contains(IsNavigatedConstant))
                context.AddOrUpdate(NavigationProviderConstants.BringToFront, true);

            string viewName = viewModel.GetViewName(context);
            var vmType = viewModel.GetType();
            var mappingItem = ViewMappingProvider.FindMappingForViewModel(vmType, viewName, true);
            var id = Guid.NewGuid().ToString("n");
            var parameter = GenerateNavigationParameter(vmType, id);

            var tcs = new TaskCompletionSource<object>();
            CurrentNavigationTask.TryExecuteSynchronously(_ =>
                ThreadManager.InvokeOnUiThreadAsync(() =>
                {
                    _navigatedTcs = tcs;
                    _navigationTargetVm = viewModel;
                    _currentCallback = callback;
                    _lastContext = context;
                    _currentOperationId = id;
                    if (_navigationService.Navigate(mappingItem, parameter, context))
                        ClearCacheIfNeed(context, viewModel);
                }));
            return tcs.Task;
        }

        public virtual void OnNavigated(IViewModel viewModel, NavigationMode mode, IDataContext context)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            var navigationContext = new NavigationContext(NavigationType.Page, mode, CurrentViewModel, viewModel, this);
            var currentViewModel = CurrentViewModel;
            if (currentViewModel != null)
                TryCacheViewModel(navigationContext, CurrentContent ?? currentViewModel.Settings.Metadata.GetData(ViewModelConstants.View), currentViewModel);
            OnNavigated(navigationContext);
        }

        public virtual event EventHandler<INavigationProvider, NavigatedEventArgs> Navigated;

        public virtual void Dispose()
        {
            ClearCacheIfNeed(new DataContext(NavigationProviderConstants.InvalidateAllCache.ToValue(true)), null);
            NavigationService.Navigating -= NavigationServiceOnNavigating;
            NavigationService.Navigated -= NavigationServiceOnNavigated;
            Navigated = null;
        }

        #endregion

        #region Methods

        public static string GenerateNavigationParameter([NotNull] Type vmType, string value)
        {
            Should.NotBeNull(vmType, nameof(vmType));
            return vmType.AssemblyQualifiedName + IdSeparator[0] + value;
        }

        protected virtual void NavigationServiceOnNavigating(object sender, NavigatingCancelEventArgsBase args)
        {
            try
            {
                _navigatingCancelArgs = args;
                OnNavigating(args);
            }
            finally
            {
                _navigatingCancelArgs = null;
            }
        }

        protected virtual void NavigationServiceOnNavigated(object sender, NavigationEventArgsBase e)
        {
            string idOperation = null;
            try
            {
                var context = CreateContextNavigateTo(CurrentViewModel, e);
                idOperation = context.GetData(NavigationProviderConstants.OperationId);
                IOperationCallback callback = null;
                if (idOperation == _currentOperationId)
                {
                    callback = _currentCallback;
                    _currentCallback = null;
                    _navigationTargetVm = null;
                    _lastContext = null;
                }
                UpdateNavigationContext(callback, context.ViewModelTo, e, ref context);
                OnNavigated(context);
            }
            finally
            {
                if (idOperation == _currentOperationId)
                {
                    _currentOperationId = null;
                    var tcs = _navigatedTcs;
                    if (tcs != null)
                    {
                        _navigatedTcs = null;
                        tcs.TrySetResult(null);
                    }
                }
            }
        }

        protected virtual Task<bool> OnNavigatingFrom([NotNull] IViewModel viewModel, INavigationContext context)
        {
            return viewModel.TryCloseAsync(context, context);
        }

        protected virtual void OnNavigatedTo(IViewModel viewModel, INavigationContext context)
        {
            var navVm = viewModel as INavigableViewModel;
            if (navVm != null)
                navVm.OnNavigatedTo(context);
        }

        protected virtual INavigationContext CreateContextNavigateFrom(IViewModel viewModelFrom, NavigatingCancelEventArgsBase args)
        {
            string idOperation;
            GetViewModelTypeFromParameter(NavigationService.GetParameterFromArgs(args), out idOperation);
            var viewModelTo = idOperation == _currentOperationId ? _navigationTargetVm : null;
            if (viewModelTo == null && viewModelFrom != null && args.NavigationMode == NavigationMode.Back)
                viewModelTo = viewModelFrom.GetParentViewModel();
            return new NavigationContext(NavigationType.Page, args.NavigationMode, viewModelFrom, viewModelTo, this)
            {
                {NavigationProviderConstants.NavigatingCancelArgs, args},
                {NavigationProviderConstants.OperationId, idOperation}
            };
        }

        protected virtual INavigationContext CreateContextNavigateTo(IViewModel viewModelFrom, NavigationEventArgsBase args)
        {
            string idOperation;
            var vmType = GetViewModelTypeFromParameter(NavigationService.GetParameterFromArgs(args), out idOperation);
            var viewModelTo = idOperation == _currentOperationId ? _navigationTargetVm : null;
            if (vmType == null)
            {
                if (args.Content != null)
                {
                    var items = _mappingProvider.FindMappingsForView(args.Content.GetType(), false);
                    if (items.Count == 1)
                    {
                        var type = items[0].ViewModelType;
#if WINDOWSCOMMON || XAMARIN_FORMS
                        if (!type.GetTypeInfo().IsGenericTypeDefinition)
#else
                        if (!type.IsGenericTypeDefinition)
#endif

                            vmType = type;
                    }
                }
                if (vmType == null)
                    return new NavigationContext(NavigationType.Page, args.Mode, viewModelFrom, viewModelTo, this)
                    {
                        {NavigationProviderConstants.NavigationArgs, args},
                        {NavigationProviderConstants.OperationId, idOperation}
                    };
            }
            return new NavigationContext(NavigationType.Page, args.Mode, viewModelFrom, viewModelTo, this)
            {
                {NavigationProviderConstants.NavigationArgs, args},
                {ViewModelTypeConstant, vmType},
                {NavigationProviderConstants.OperationId, idOperation}
            };
        }

        protected virtual IViewModel GetViewModelForView([NotNull] NavigationEventArgsBase args,
            [CanBeNull] IViewModel navigationViewModel, [NotNull] INavigationContext context, [NotNull] Type vmType)
        {
            var view = args.Content;
            if (navigationViewModel != null)
            {
                ViewManager.InitializeViewAsync(navigationViewModel, view, context).WithTaskExceptionHandler(this);
                return navigationViewModel;
            }

            IViewModel vm = null;
            if (CachePolicy != null)
                vm = CachePolicy.TryTakeViewModelFromCache(context, view);

            if (HasViewModel(view, vmType))
                return (IViewModel)MugenMvvmToolkit.Infrastructure.ViewManager.GetDataContext(view);
            if (vm == null)
            {
                IDataContext viewModelState = null;
#if WINDOWS_PHONE || WINDOWSCOMMON
                viewModelState = PlatformExtensions.GetViewModelState(view);
                if (viewModelState != null)
                    PlatformExtensions.SetViewModelState(view, null);
#endif
                vm = ViewModelProvider.RestoreViewModel(viewModelState, new DataContext
                {
                    {InitializationConstants.ViewModelType, vmType}
                }, false);
            }

            if (vm != null)
                ViewManager.InitializeViewAsync(vm, view, context).WithTaskExceptionHandler(this);
            return vm;
        }

        protected virtual void RaiseNavigated(INavigationContext ctx)
        {
            var handler = Navigated;
            if (handler != null)
                handler(this, new NavigatedEventArgs(ctx));
        }

        protected bool TryCompleteOperationCallback([NotNull] IViewModel viewModel, [NotNull] INavigationContext context)
        {
            if (context.NavigationMode != NavigationMode.Back)
                return false;
            CompleteOperationCallback(viewModel, context);
            return true;
        }

        protected static Type GetViewModelTypeFromParameter(string parameter, out string idOperation)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                idOperation = null;
                return null;
            }
            var items = parameter.Split(IdSeparator, StringSplitOptions.RemoveEmptyEntries);
            idOperation = items.Length == 2 ? items[1] : null;
            return Type.GetType(items[0], false);
        }

        private void Renavigate(IViewModel currentViewModel, [NotNull] INavigationContext context, NavigatingCancelEventArgsBase args)
        {
            if (CurrentContent != null)
                TryCacheViewModel(context, CurrentContent, currentViewModel);
            if (_navigatingCancelArgs == null)
            {
                if (NavigationService.Navigate(args, _lastContext))
                    ClearCacheIfNeed(_lastContext ?? DataContext.Empty, _navigationTargetVm);
                else
                    CancelCurrentNavigation(context);
            }
            else
                _navigatingCancelArgs.Cancel = false;
        }

        private void OnNavigating(NavigatingCancelEventArgsBase args)
        {
            if (_ignoreNavigating)
                return;
            var currentViewModel = CurrentViewModel;
            if (currentViewModel == null)
                return;
            try
            {
                _closingViewModel = currentViewModel;
                args.Cancel = true;
                var context = CreateContextNavigateFrom(currentViewModel, args);
                var navigateTask = (_closedFromViewModel || !args.IsCancelable)
                    ? Empty.TrueTask
                    : OnNavigatingFrom(currentViewModel, context);
                var t = navigateTask.TryExecuteSynchronously(task =>
                {
                    if (!task.IsCanceled && task.IsFaulted)
                    {
                        _closingViewModel = null;
                        var callback = _currentCallback;
                        if (callback != null)
                        {
                            callback.Invoke(OperationResult.CreateErrorResult<bool?>(OperationType.PageNavigation,
                                currentViewModel, task.Exception, context));
                            _currentCallback = null;
                        }
                        return;
                    }
                    if (task.IsCanceled || !task.Result)
                    {
                        _closingViewModel = null;
                        CancelCurrentNavigation(context);
                        return;
                    }
                    ThreadManager.InvokeOnUiThreadAsync(() =>
                    {
                        try
                        {
                            _ignoreNavigating = true;
                            Renavigate(currentViewModel, context, args);
                        }
                        finally
                        {
                            _closingViewModel = null;
                            _ignoreNavigating = false;
                        }
                    });
                });
                t.WithTaskExceptionHandler(this);
            }
            catch (Exception)
            {
                _closingViewModel = null;
                throw;
            }
        }

        private void OnNavigated(INavigationContext context)
        {
            var vmFrom = context.ViewModelFrom;
            var vmTo = context.ViewModelTo;
            var mode = context.NavigationMode;
            //only this mode allows to renavigate.
            if (ReferenceEquals(vmFrom, vmTo) && mode != NavigationMode.Refresh && mode != NavigationMode.Reset && mode != NavigationMode.Undefined)
            {
                if (vmFrom != null)
                    Tracer.Warn("Possible bug in navigation, navigate to the same view model with mode " + mode);
                return;
            }
            CurrentViewModel = vmTo;
            if (vmFrom != null)
            {
                var navigableViewModel = vmFrom as INavigableViewModel;
                if (navigableViewModel != null)
                    navigableViewModel.OnNavigatedFrom(context);
            }

            if (vmTo != null)
            {
                if (!vmTo.Settings.State.Contains(IsNavigatedConstant))
                {
                    vmTo.Disposed += _disposeViewModelHandler;
                    vmTo.Settings.State.AddOrUpdate(IsNavigatedConstant, null);
                }

                var closeableViewModel = vmTo as ICloseableViewModel;
                if (closeableViewModel != null && !(closeableViewModel.CloseCommand is CloseCommandWrapper))
                {
                    closeableViewModel.Closed += _closeViewModelHandler;
                    closeableViewModel.CloseCommand = new CloseCommandWrapper(closeableViewModel.CloseCommand, this, closeableViewModel);
                }
                OnNavigatedTo(vmTo, context);
            }

            RaiseNavigated(context);
            if (vmFrom != null && TryCompleteOperationCallback(vmFrom, context))
                OnViewModelClosed(vmFrom, context, false);
            if (Tracer.TraceInformation)
                Tracer.Info("Navigated from '{0}' to '{1}', navigation mode '{2}'", vmFrom, vmTo, mode);
        }

        private void UpdateNavigationContext(IOperationCallback callback, IViewModel navigationViewModel, NavigationEventArgsBase args, ref INavigationContext context)
        {
            var vmType = context.GetData(ViewModelTypeConstant);
            if (vmType == null)
                return;

            var viewModel = GetViewModelForView(args, navigationViewModel, context, vmType);
            if (!ReferenceEquals(context.ViewModelTo, viewModel))
                context = new NavigationContext(NavigationType.Page, context.NavigationMode, context.ViewModelFrom, viewModel, context.NavigationProvider);
            if (viewModel != null && callback != null)
                CallbackManager.Register(OperationType.PageNavigation, viewModel, callback, context);
        }

        private void ViewModelOnDisposed(IDisposableObject sender, EventArgs args)
        {
            if (CachePolicy != null && CachePolicy.Invalidate((IViewModel)sender, DataContext.Empty))
                Tracer.Warn("The disposed view model " + sender.GetType().Name + " was in the navigation cache");
        }

        private void CloseableViewModelOnClosed(ICloseableViewModel sender, ViewModelClosedEventArgs e)
        {
            TryCloseViewModel(e.ViewModel, e.Parameter);
        }

        private void TryCloseViewModel(IViewModel viewModel, object parameter)
        {
            _threadManager.Invoke(ExecutionMode.AsynchronousOnUiThread, this, viewModel, parameter, (provider, vm, p) =>
            {
                if (ReferenceEquals(provider._closingViewModel, vm))
                    return;
                try
                {
                    provider._closedFromViewModel = true;
                    if (provider.NavigationService.TryClose(vm, p as IDataContext))
                        provider.OnViewModelClosed(vm, p, true);
                }
                finally
                {
                    provider._closedFromViewModel = false;
                }
            });
        }

        private bool OnViewModelClosed(IViewModel viewModel, object parameter, bool completeCallback)
        {
            viewModel.Disposed -= _disposeViewModelHandler;
            viewModel.Settings.State.Remove(IsNavigatedConstant);
            if (CachePolicy != null)
                CachePolicy.Invalidate(viewModel, parameter as IDataContext);
            var closeableViewModel = viewModel as ICloseableViewModel;
            if (closeableViewModel != null)
            {
                var wrapper = closeableViewModel.CloseCommand as CloseCommandWrapper;
                if (wrapper != null)
                    closeableViewModel.CloseCommand = wrapper.NestedCommand;
                closeableViewModel.Closed -= _closeViewModelHandler;
            }
            if (completeCallback && CurrentViewModel != viewModel)
            {
                CompleteOperationCallback(viewModel, parameter as IDataContext ?? DataContext.Empty);
                return true;
            }
            return false;
        }

        private void TryCacheViewModel(INavigationContext context, object view, IViewModel viewModel)
        {
            if (CachePolicy != null && view != null && viewModel != null)
                CachePolicy.TryCacheViewModel(context, view, viewModel);
        }

        private void CancelCurrentNavigation(INavigationContext context)
        {
            var callback = _currentCallback;
            if (callback != null)
            {
                callback.Invoke(OperationResult.CreateCancelResult<bool?>(OperationType.PageNavigation, context.ViewModelTo ?? (object)this, context));
                _currentCallback = null;
                _navigationTargetVm = null;
            }
            var tcs = _navigatedTcs;
            if (tcs != null)
            {
                _navigatedTcs = null;
                tcs.TrySetCanceled();
            }
        }

        private static bool HasViewModel(object view, Type viewModelType)
        {
            if (view == null)
                return false;
            var viewModel = MugenMvvmToolkit.Infrastructure.ViewManager.GetDataContext(view) as IViewModel;
            if (viewModel == null)
                return false;

            var vmType = viewModel.GetType();
#if WINDOWSCOMMON || XAMARIN_FORMS
            if (!viewModelType.GetTypeInfo().IsGenericType)
#else
            if (!viewModelType.IsGenericType)
#endif
                return vmType.Equals(viewModelType);
#if WINDOWSCOMMON || XAMARIN_FORMS
            if (!vmType.GetTypeInfo().IsGenericType)
#else
            if (!vmType.IsGenericType)
#endif
                return false;
            return vmType.GetGenericTypeDefinition().Equals(viewModelType.GetGenericTypeDefinition());
        }

        private void CompleteOperationCallback(IViewModel viewModel, IDataContext context)
        {
            var result = ViewModelExtensions.GetOperationResult(viewModel);
            var operationResult = OperationResult.CreateResult(OperationType.PageNavigation, viewModel, result, context);
            CallbackManager.SetResult(operationResult);
        }

        private void ClearCacheIfNeed(IDataContext context, IViewModel viewModelTo)
        {
            if (CachePolicy == null)
                return;
            if (!context.GetData(NavigationProviderConstants.InvalidateAllCache))
            {
                if (context.GetData(NavigationProviderConstants.InvalidateCache))
                    CachePolicy.Invalidate(viewModelTo, context);
                return;
            }
            var viewModels = CachePolicy.Invalidate(context);
            foreach (var viewModelFrom in viewModels.Reverse())
            {
                var navigationContext = new NavigationContext(NavigationType.Page, NavigationMode.Reset, viewModelFrom, viewModelTo, this);
                if (!OnViewModelClosed(viewModelFrom, navigationContext, true))
                    CompleteOperationCallback(viewModelFrom, navigationContext);
            }
        }

        #endregion
    }
}
