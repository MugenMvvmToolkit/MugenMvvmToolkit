#region Copyright
// ****************************************************************************
// <copyright file="NavigationProvider.cs">
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
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure.Callbacks;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Callbacks;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.ViewModels;
using NavigationMode = MugenMvvmToolkit.Models.NavigationMode;
using NavigationContext = MugenMvvmToolkit.Models.NavigationContext;

namespace MugenMvvmToolkit.Infrastructure.Navigation
{
    public class NavigationProvider : DisposableObject, INavigationProviderEx
    {
        #region Nested types

        private sealed class CloseCommandWrapper : ICommand
        {
            #region Fields

            public readonly ICommand NestedCommand;
            private readonly INavigationProvider _provider;

            #endregion

            #region Constructors

            public CloseCommandWrapper(ICommand nestedCommand, INavigationProvider provider)
            {
                NestedCommand = nestedCommand;
                _provider = provider;
            }

            #endregion

            #region Implementation of ICommand

            public bool CanExecute(object parameter)
            {
                return _provider.CanGoBack && (NestedCommand == null || NestedCommand.CanExecute(parameter));
            }

            public void Execute(object parameter)
            {
                if (NestedCommand == null)
                    _provider.GoBack();
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

            #endregion
        }

        #endregion

        #region Fields

        public static readonly DataConstant<NavigationEventArgsBase> NavigationArgsConstant;
        public static readonly DataConstant<NavigatingCancelEventArgsBase> NavigatingCancelArgsConstant;

        protected static readonly DataConstant<Type> ViewModelType;
        private static readonly DataConstant<string> VmTypeConstant;

        private readonly IViewMappingProvider _mappingProvider;
        private readonly IViewManager _viewManager;
        private readonly IViewModelProvider _viewModelProvider;
        private readonly IOperationCallbackManager _callbackManager;
        private readonly INavigationService _navigationService;
        private readonly IThreadManager _threadManager;
        private readonly INavigationCachePolicy _cachePolicy;

        private readonly WeakReference _vmReference;
        private bool _closedFromViewModel;
        private bool _ignoreCloseFromViewModel;
        private bool _ignoreNavigating;
        private NavigatingCancelEventArgsBase _navigatingCancelArgs;
        private IViewModel _navigationTargetVm;
        private IOperationCallback _currentCallback;

        #endregion

        #region Constructors

        static NavigationProvider()
        {
            NavigationArgsConstant = DataConstant.Create(() => NavigationArgsConstant, true);
            NavigatingCancelArgsConstant = DataConstant.Create(() => NavigatingCancelArgsConstant, true);
            ViewModelType = DataConstant.Create(() => ViewModelType, true);
            VmTypeConstant = DataConstant.Create(() => VmTypeConstant, true);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="NavigationProvider" /> class.
        /// </summary>
        public NavigationProvider([NotNull] INavigationService navigationService, [NotNull] IThreadManager threadManager,
            [NotNull] IViewMappingProvider mappingProvider, [NotNull] IViewManager viewManager,
            [NotNull] IViewModelProvider viewModelProvider, IOperationCallbackManager callbackManager, INavigationCachePolicy cachePolicy = null)
        {
            Should.NotBeNull(navigationService, "navigationService");
            Should.NotBeNull(threadManager, "threadManager");
            Should.NotBeNull(mappingProvider, "mappingProvider");
            Should.NotBeNull(viewManager, "viewManager");
            Should.NotBeNull(viewModelProvider, "viewModelProvider");
            Should.NotBeNull(callbackManager, "callbackManager");
            _navigationService = navigationService;
            _threadManager = threadManager;
            _mappingProvider = mappingProvider;
            _viewManager = viewManager;
            _viewModelProvider = viewModelProvider;
            _callbackManager = callbackManager;
            _cachePolicy = cachePolicy;
            _vmReference = new WeakReference(null);

            NavigationService.Navigating += NavigationServiceOnNavigating;
            NavigationService.Navigated += NavigationServiceOnNavigated;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets the <see cref="IViewManager"/>.
        /// </summary>
        [NotNull]
        protected IViewManager ViewManager
        {
            get { return _viewManager; }
        }

        /// <summary>
        ///     Gets the <see cref="IViewMappingProvider"/>.
        /// </summary>
        [NotNull]
        protected IViewMappingProvider ViewMappingProvider
        {
            get { return _mappingProvider; }
        }

        /// <summary>
        ///     Gets the current <see cref="IThreadManager"/>.
        /// </summary>
        [NotNull]
        protected IThreadManager ThreadManager
        {
            get { return _threadManager; }
        }

        /// <summary>
        /// Gets the <see cref="IViewModelProvider"/>.
        /// </summary>
        [NotNull]
        protected IViewModelProvider ViewModelProvider
        {
            get { return _viewModelProvider; }
        }

        /// <summary>
        /// Gets the <see cref="IOperationCallbackManager"/>.
        /// </summary>
        [NotNull]
        protected IOperationCallbackManager CallbackManager
        {
            get { return _callbackManager; }
        }

        #endregion

        #region Implementation of INavigationProvider

        /// <summary>
        ///     Gets the <see cref="INavigationService" />.
        /// </summary>
        public INavigationService NavigationService
        {
            get { return _navigationService; }
        }

        /// <summary>
        ///     Gets the active view model.
        /// </summary>
        public IViewModel CurrentViewModel
        {
            get { return (IViewModel)_vmReference.Target; }
            protected set { _vmReference.Target = value; }
        }

        /// <summary>
        ///     Gets the current content.
        /// </summary>
        public virtual object CurrentContent
        {
            get { return _navigationService.CurrentContent; }
        }

        /// <summary>
        ///     Gets the cache policy, if any.
        /// </summary>
        public INavigationCachePolicy CachePolicy
        {
            get { return _cachePolicy; }
        }

        /// <summary>
        ///     Indicates whether the navigator can navigate back.
        /// </summary>
        public virtual bool CanGoBack
        {
            get { return _navigationService.CanGoBack; }
        }

        /// <summary>
        ///     Indicates whether the navigator can navigate forward.
        /// </summary>
        public virtual bool CanGoForward
        {
            get { return _navigationService.CanGoForward; }
        }

        /// <summary>
        ///     Navigates back.
        /// </summary>
        public virtual void GoBack()
        {
            _navigationService.GoBack();
        }

        /// <summary>
        ///     Navigates forward.
        /// </summary>
        public virtual void GoForward()
        {
            _navigationService.GoForward();
        }

        /// <summary>
        ///     Navigates using the specified data context.
        /// </summary>
        /// <param name="callback">The specified callback, if any.</param>
        /// <param name="context">
        ///     The specified <see cref="IDataContext" />.
        /// </param>
        public void Navigate(IOperationCallback callback, IDataContext context)
        {
            Should.NotBeNull(context, "context");
            IViewModel viewModel = context.GetData(NavigationConstants.ViewModel);
            if (viewModel == null)
                throw new InvalidOperationException(string.Format("The '{0}' provider doesn't support the DataContext without navigation target.", GetType()));
            if (ReferenceEquals(viewModel, CurrentViewModel))
            {
                if (callback != null)
                    callback.Invoke(OperationResult.CreateResult<bool?>(OperationType.Navigation, CurrentViewModel, true, context));
                return;
            }

            string viewName = viewModel.GetViewName(context);
            var parameters = context.GetData(NavigationConstants.Parameters);

            var vmType = viewModel.GetType();
            var mappingItem = FindMappingForViewModel(vmType, viewName);
            object parameter;
            if (parameters != null)
            {
                parameters = parameters.ToNonReadOnly();
                parameters.Add(VmTypeConstant, vmType.AssemblyQualifiedName);
                parameter = parameters;
            }
            else
                parameter = vmType.AssemblyQualifiedName;

            ThreadManager.InvokeOnUiThreadAsync(() =>
            {
                CancelCurrentNavigation(null);
                _navigationTargetVm = viewModel;
                _currentCallback = callback;
                _navigationService.Navigate(mappingItem, parameter, context);
            });
        }

        /// <summary>
        ///     Occurs after view model was navigated.
        /// </summary>
        public virtual event EventHandler<INavigationProvider, NavigatedEventArgs> Navigated;

        #endregion

        #region Methods

        /// <summary>
        ///     Subscriber to the Navigating event.
        /// </summary>
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

        /// <summary>
        ///     Subscriber to the Navigated event.
        /// </summary>
        protected virtual void NavigationServiceOnNavigated(object sender, NavigationEventArgsBase e)
        {
            var callback = _currentCallback;
            var vm = _navigationTargetVm;
            try
            {
                var context = CreateContextNavigateTo(CurrentViewModel, vm, e);
                CurrentViewModel = OnNavigated(callback, vm, e, ref context);

                if (context.ViewModelFrom != null)
                {
                    var navigableViewModel = context.ViewModelFrom as INavigableViewModel;
                    if (navigableViewModel != null)
                        navigableViewModel.OnNavigatedFrom(context);
                    TryCompleteOperationCallback(context.ViewModelFrom, context);
                }

                if (!ReferenceEquals(context.ViewModelFrom, context.ViewModelTo))
                {
                    var closeableViewModel = context.ViewModelFrom as ICloseableViewModel;
                    if (closeableViewModel != null)
                    {
                        closeableViewModel.Closed -= CloseableViewModelOnClosed;
                        var wrapper = closeableViewModel.CloseCommand as CloseCommandWrapper;
                        if (wrapper != null)
                            closeableViewModel.CloseCommand = wrapper.NestedCommand;
                    }

                    closeableViewModel = context.ViewModelTo as ICloseableViewModel;
                    if (closeableViewModel != null)
                    {
                        closeableViewModel.Closed += CloseableViewModelOnClosed;
                        closeableViewModel.CloseCommand = new CloseCommandWrapper(closeableViewModel.CloseCommand, this);
                    }
                }
                Tracer.Info("Navigated from '{0}' to '{1}', navigation mode '{2}'", context.ViewModelFrom,
                    context.ViewModelTo, context.NavigationMode);
            }
            catch (Exception ex)
            {
                Tracer.Error(ex.Flatten(true));
                throw;
            }
            finally
            {
                _currentCallback = null;
                _navigationTargetVm = null;
            }
        }

        /// <summary>
        ///     Called just before a view-model is no longer the active view-model in a frame.
        /// </summary>
        /// <param name="viewModel">The specified <see cref="IViewModel"/></param>
        /// <param name="context">
        ///     The specified <see cref="INavigationContext" />.
        /// </param>
        protected virtual Task<bool> OnNavigatingFrom([NotNull] IViewModel viewModel, INavigationContext context)
        {
            return viewModel.TryCloseAsync(context, context);
        }

        /// <summary>
        ///     Called when a view-model becomes the active view-model in a frame.
        /// </summary>
        /// <param name="viewModel">The specified <see cref="IViewModel"/></param>
        /// <param name="context">
        ///     The specified <see cref="INavigationContext" />.
        /// </param>
        protected virtual void OnNavigatedTo(IViewModel viewModel, INavigationContext context)
        {
            var navVm = viewModel as INavigableViewModel;
            if (navVm != null)
                navVm.OnNavigatedTo(context);
        }

        protected virtual INavigationContext CreateContextNavigateFrom(IViewModel viewModelFrom, IViewModel viewModelTo, NavigatingCancelEventArgsBase args)
        {
            IDataContext parameters;
            GetViewModelTypeFromParameter(NavigationService.GetParameterFromArgs(args), out parameters);
            return new NavigationContext(args.NavigationMode, viewModelFrom, viewModelTo, this, parameters)
            {
                {NavigatingCancelArgsConstant, args}
            };
        }

        protected virtual INavigationContext CreateContextNavigateTo(IViewModel viewModelFrom, IViewModel viewModelTo, NavigationEventArgsBase args)
        {
            IDataContext parameters;
            var vmType = GetViewModelTypeFromParameter(NavigationService.GetParameterFromArgs(args), out parameters);
            if (vmType == null)
            {
                if (args.Content != null)
                {
                    var items = _mappingProvider.FindMappingsForView(args.Content.GetType(), false);
                    if (items.Count == 1)
                    {
                        var type = items[0].ViewModelType;
#if NETFX_CORE || WINDOWSCOMMON
                        if (!type.GetTypeInfo().IsGenericTypeDefinition)
#else
                        if (!type.IsGenericTypeDefinition)
#endif

                            vmType = type;
                    }
                }
                if (vmType == null)
                    return new NavigationContext(args.Mode, viewModelFrom, viewModelTo, this, parameters);
            }
            return new NavigationContext(args.Mode, viewModelFrom, viewModelTo, this, parameters)
            {
                {NavigationArgsConstant, args},
                {ViewModelType, vmType}
            };
        }

        protected virtual void RegisterOperationCallback([NotNull]IViewModel viewModel, [NotNull] IOperationCallback callback, [NotNull]INavigationContext context)
        {
            CallbackManager.Register(OperationType.Navigation, viewModel, callback, context);
        }

        protected virtual void TryCompleteOperationCallback([NotNull] IViewModel viewModel, [NotNull] INavigationContext context)
        {
            if (context.NavigationMode != NavigationMode.Back)
                return;
            bool? result = null;
            var hasOperationResult = viewModel as IHasOperationResult;
            if (hasOperationResult != null)
                result = hasOperationResult.OperationResult;
            var operationResult = OperationResult.CreateResult(OperationType.Navigation, viewModel, result, context);
            //NOTE: to make sure that the callback is called after the navigation.
#if WINDOWS_PHONE
            ThreadManager.InvokeAsync(() => ThreadManager.InvokeOnUiThreadAsync(() =>
            {
                var vm = context.ViewModelTo;
                Task data;
                if (vm != null && vm.Settings.Metadata.TryGetData(FrameStateManager.RestoreStateConstant, out data))
                    data.TryExecuteSynchronously(task => CallbackManager.SetResult(viewModel, operationResult)).WithTaskExceptionHandler(vm);
                else
                    CallbackManager.SetResult(viewModel, operationResult);
            }, OperationPriority.Low));
#else
            ThreadManager.InvokeAsync(() => ThreadManager.InvokeOnUiThreadAsync(() => CallbackManager.SetResult(viewModel, operationResult), OperationPriority.Low));
#endif
        }

        protected static Type GetViewModelTypeFromParameter(object parameter, out IDataContext parameters)
        {
            var st = parameter as string;
            if (!string.IsNullOrEmpty(st))
            {
                parameters = null;
                return Type.GetType(st, false);
            }
            parameters = parameter as IDataContext;
            if (parameters == null)
                return null;
            return Type.GetType(parameters.GetData(VmTypeConstant), true);
        }

        /// <summary>
        ///     Invokes the <see cref="Navigated" /> event.
        /// </summary>
        protected void RaiseNavigated(INavigationContext ctx, IViewModel vm)
        {
            var handler = Navigated;
            if (handler != null)
                handler(this, new NavigatedEventArgs(ctx, vm));
        }

        private void Renavigate(IViewModel currentViewModel, INavigationContext context, NavigatingCancelEventArgsBase args)
        {
            if (CurrentContent != null)
                TryCacheViewModel(context, ViewManager.WrapToView(CurrentContent, DataContext.Empty), currentViewModel);
            if (_navigatingCancelArgs == null)
            {
                if (args.IsCancelable && args.NavigationMode == NavigationMode.Back)
                    NavigationService.GoBack();
                else
                {
                    if (!NavigationService.Navigate(args))
                        CancelCurrentNavigation(context);
                }
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
            _ignoreCloseFromViewModel = true;
            args.Cancel = true;
            var context = CreateContextNavigateFrom(currentViewModel, _navigationTargetVm, args);
            var navigateTask = (_closedFromViewModel || !args.IsCancelable)
                ? Empty.TrueTask
                : OnNavigatingFrom(currentViewModel, context);
            var t = navigateTask.TryExecuteSynchronously(task =>
            {
                if (task.IsCanceled || !task.Result)
                {
                    CancelCurrentNavigation(context);
                    return;
                }
                if (task.IsFaulted)
                {
                    var callback = _currentCallback;
                    if (callback != null)
                    {
                        callback.Invoke(OperationResult.CreateErrorResult<bool?>(OperationType.Navigation,
                            currentViewModel, task.Exception, context));
                        _currentCallback = null;
                    }
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
                        _ignoreNavigating = false;
                    }
                });
            });
            t.TryExecuteSynchronously(task => _ignoreCloseFromViewModel = false);
            t.WithTaskExceptionHandler(this);
        }

        protected virtual IViewModel OnNavigated(IOperationCallback callback, IViewModel navigationViewModel, NavigationEventArgsBase args, ref INavigationContext context)
        {
            var vmType = context.GetData(ViewModelType);
            if (vmType == null)
                return null;

            var view = ViewManager.WrapToView(args.Content, DataContext.Empty);
            var viewModel = GetViewModelForView(view, navigationViewModel, context, vmType);
            if (context.ViewModelTo == null)
                context = new NavigationContext(context.NavigationMode, context.ViewModelFrom, viewModel,
                    context.NavigationProvider, context.Parameters);
            OnNavigatedTo(viewModel, context);
            RaiseNavigated(context, viewModel);
            if (viewModel != null && callback != null)
                RegisterOperationCallback(viewModel, callback, context);
            return viewModel;
        }

        private IViewModel GetViewModelForView(IView view, IViewModel navigationViewModel, INavigationContext context, Type vmType)
        {
            if (navigationViewModel != null)
            {
                ViewManager.InitializeViewAsync(navigationViewModel, view).WithTaskExceptionHandler(this);
                return navigationViewModel;
            }
            //Trying to get from cache.
            IViewModel vm = TryTakeViewModelFromCache(context, view);
            if (HasViewModel(view, vmType))
                return (IViewModel)view.DataContext;

            if (vm == null)
                vm = ViewModelProvider.GetViewModel(vmType, InitializationConstants.IsRestored.ToValue(true));
            ViewManager.InitializeViewAsync(vm, view).WithTaskExceptionHandler(this);
            return vm;
        }

        private void CloseableViewModelOnClosed(object sender, ViewModelClosedEventArgs args)
        {
            if (_ignoreCloseFromViewModel)
                return;
            _threadManager.InvokeOnUiThreadAsync(() =>
            {
                try
                {
                    _closedFromViewModel = true;
                    if (NavigationService.CanGoBack)
                        NavigationService.GoBack();
                }
                finally
                {
                    _closedFromViewModel = false;
                }
            });
        }

        private void TryCacheViewModel(INavigationContext context, IView view, IViewModel viewModel)
        {
            if (CachePolicy != null)
                CachePolicy.TryCacheViewModel(context, view, viewModel);
        }

        private IViewModel TryTakeViewModelFromCache(INavigationContext context, IView view)
        {
            if (CachePolicy == null)
                return null;
            return CachePolicy.TryTakeViewModelFromCache(context, view);
        }

        private IViewMappingItem FindMappingForViewModel(Type viewModelType, string viewName)
        {
            return ViewMappingProvider.FindMappingForViewModel(viewModelType, viewName, true);
        }

        private void CancelCurrentNavigation(INavigationContext context)
        {
            var callback = _currentCallback;
            if (callback != null)
            {
                callback.Invoke(OperationResult.CreateCancelResult<bool?>(OperationType.Navigation, this, context));
                _currentCallback = null;
                _navigationTargetVm = null;
            }
        }

        private bool HasViewModel(IView view, Type viewModelType)
        {
            if (_navigationTargetVm != null && _navigationTargetVm.GetType().Equals(viewModelType))
                return true;
            if (view == null)
                return false;
            var viewModel = view.DataContext as IViewModel;
            if (viewModel == null)
                return false;

            var vmType = viewModel.GetType();
#if NETFX_CORE || WINDOWSCOMMON
            if (!viewModelType.GetTypeInfo().IsGenericType)
#else
            if (!viewModelType.IsGenericType)
#endif
                return vmType.Equals(viewModelType);
#if NETFX_CORE || WINDOWSCOMMON
            if (!vmType.GetTypeInfo().IsGenericType)
#else
            if (!vmType.IsGenericType)
#endif
                return false;
            return vmType.GetGenericTypeDefinition().Equals(viewModelType.GetGenericTypeDefinition());
        }

        #endregion

        #region Overrides of DisposableObject

        /// <summary>
        ///     Releases resources held by the object.
        /// </summary>
        protected override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                NavigationService.Navigating -= NavigationServiceOnNavigating;
                NavigationService.Navigated -= NavigationServiceOnNavigated;
                Navigated = null;
            }
            base.OnDispose(disposing);
        }

        #endregion
    }
}