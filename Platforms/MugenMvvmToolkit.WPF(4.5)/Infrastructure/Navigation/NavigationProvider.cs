#region Copyright

// ****************************************************************************
// <copyright file="NavigationProvider.cs">
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
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
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
#elif WINDOWS_UWP
using System.Reflection;
using MugenMvvmToolkit.UWP.Interfaces.Navigation;

namespace MugenMvvmToolkit.UWP.Infrastructure.Navigation
#endif
{
    public class NavigationProvider : INavigationProvider
    {
        #region Fields

        private readonly INavigationCachePolicy _cachePolicy;
        private readonly EventHandler<IDisposableObject, EventArgs> _disposeViewModelHandler;

        private bool _ignoreNavigating;
        private TaskCompletionSource<bool> _navigatedTcs;
        private NavigatingCancelEventArgsBase _navigatingCancelArgs;
        private WeakReference _vmReference;

        protected static readonly DataConstant<Type> ViewModelTypeConstant;
        private static readonly DataConstant<object> IsNavigatedConstant;
        private static readonly DataConstant<IDataContext> CloseContextConstant;
        private static readonly DataConstant<TaskCompletionSource<bool>> NavigatedTaskConstant;

        #endregion

        #region Constructors

        static NavigationProvider()
        {
            var type = typeof(NavigationProvider);
            ViewModelTypeConstant = DataConstant.Create<Type>(type, nameof(ViewModelTypeConstant), true);
            IsNavigatedConstant = DataConstant.Create<object>(type, nameof(IsNavigatedConstant), false);
            NavigatedTaskConstant = DataConstant.Create<TaskCompletionSource<bool>>(type, nameof(NavigatedTaskConstant), false);
            CloseContextConstant = DataConstant.Create<IDataContext>(type, nameof(CloseContextConstant), true);
        }

        [Preserve(Conditional = true)]
        public NavigationProvider([NotNull] INavigationService navigationService, [NotNull] IThreadManager threadManager,
            [NotNull] IViewMappingProvider mappingProvider, [NotNull] IViewManager viewManager, [NotNull] IViewModelProvider viewModelProvider,
            [NotNull] INavigationDispatcher navigationDispatcher, INavigationCachePolicy cachePolicy = null)
        {
            Should.NotBeNull(navigationService, nameof(navigationService));
            Should.NotBeNull(threadManager, nameof(threadManager));
            Should.NotBeNull(mappingProvider, nameof(mappingProvider));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(viewModelProvider, nameof(viewModelProvider));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            NavigationService = navigationService;
            ThreadManager = threadManager;
            ViewMappingProvider = mappingProvider;
            ViewManager = viewManager;
            ViewModelProvider = viewModelProvider;
            NavigationDispatcher = navigationDispatcher;
            _cachePolicy = cachePolicy;
            _vmReference = Empty.WeakReference;
            _disposeViewModelHandler = ViewModelOnDisposed;

            NavigationService.Navigating += NavigationServiceOnNavigating;
            NavigationService.Navigated += NavigationServiceOnNavigated;
        }

        #endregion

        #region Properties

        protected INavigationService NavigationService { get; }

        protected IViewManager ViewManager { get; }

        protected IViewMappingProvider ViewMappingProvider { get; }

        protected IThreadManager ThreadManager { get; }

        protected IViewModelProvider ViewModelProvider { get; }

        protected INavigationDispatcher NavigationDispatcher { get; }

        public IViewModel CurrentViewModel
        {
            get { return (IViewModel)_vmReference.Target; }
            protected set { _vmReference = ToolkitExtensions.GetWeakReferenceOrDefault(value, Empty.WeakReference, true); }
        }

        public virtual object CurrentContent => NavigationService.CurrentContent;

        public INavigationCachePolicy CachePolicy => _cachePolicy;

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            ClearCacheIfNeed(new DataContext(NavigationProviderConstants.InvalidateAllCache.ToValue(true)), null);
            NavigationService.Navigating -= NavigationServiceOnNavigating;
            NavigationService.Navigated -= NavigationServiceOnNavigated;
            OnDispose();
        }

        public Task NavigateAsync(IDataContext context)
        {
            var viewModel = GetViewModelFromContext(context);
            var tcs = new TaskCompletionSource<bool>();
            var currentTask = _navigatedTcs?.Task ?? Empty.Task;
            currentTask.TryExecuteSynchronously(task => ThreadManager.InvokeOnUiThreadAsync(() => NavigateInternal(viewModel, context.ToNonReadOnly(), tcs)));
            return tcs.Task;
        }

        public Task<bool> TryCloseAsync(IDataContext context)
        {
            var viewModel = GetViewModelFromContext(context, false);
            if (viewModel == null || !viewModel.Settings.State.Contains(IsNavigatedConstant))
                return null;
            context = context.ToNonReadOnly();
            if (!NavigationService.CanClose(context))
                return Empty.FalseTask;

            var tcs = new TaskCompletionSource<bool>();
            if (TryCloseInternal(context, tcs))
                return tcs.Task;
            return Empty.FalseTask;
        }

        public void Restore(IDataContext context)
        {
            var viewModel = GetViewModelFromContext(context);
            context = context.ToNonReadOnly();
            RestoreInternal(viewModel, context);
        }

        #endregion

        #region Methods

        public static string GenerateNavigationParameter([NotNull] Type vmType)
        {
            Should.NotBeNull(vmType, nameof(vmType));
            return vmType.AssemblyQualifiedName;
        }

        protected static Type GetViewModelTypeFromParameter(string parameter)
        {
            if (string.IsNullOrEmpty(parameter))
                return null;
            return Type.GetType(parameter, false);
        }

        private static bool HasViewModel(object view, Type viewModelType)
        {
            if (view == null)
                return false;
            var viewModel = ToolkitExtensions.GetDataContext(view) as IViewModel;
            if (viewModel == null)
                return false;

            var vmType = viewModel.GetType();
#if WINDOWS_UWP || XAMARIN_FORMS
            if (!viewModelType.GetTypeInfo().IsGenericType)
#else
            if (!viewModelType.IsGenericType)
#endif
                return vmType.Equals(viewModelType);
#if WINDOWS_UWP || XAMARIN_FORMS
            if (!vmType.GetTypeInfo().IsGenericType)
#else
            if (!vmType.IsGenericType)
#endif
                return false;
            return vmType.GetGenericTypeDefinition().Equals(viewModelType.GetGenericTypeDefinition());
        }

        private IViewModel GetViewModelFromContext(IDataContext context, bool throwOnError = true)
        {
            Should.NotBeNull(context, nameof(context));
            IViewModel viewModel = context.GetData(NavigationConstants.ViewModel);
            if (viewModel == null && throwOnError)
                throw new InvalidOperationException($"The '{GetType()}' provider doesn't support the DataContext without navigation target.");
            return viewModel;
        }

        protected virtual void NavigateInternal(IViewModel viewModel, IDataContext context, TaskCompletionSource<bool> tcs)
        {
            if (ReferenceEquals(viewModel, CurrentViewModel))
            {
                tcs.SetResult(true);
                return;
            }
            //The view model is already shown as page and we need to bring it to front
            if (viewModel.Settings.State.Contains(IsNavigatedConstant))
                context.AddOrUpdate(NavigationProviderConstants.BringToFront, true);

            context.AddOrUpdate(NavigatedTaskConstant, tcs);
            string viewName = viewModel.GetViewName(context);
            var vmType = viewModel.GetType();
            var mappingItem = ViewMappingProvider.FindMappingForViewModel(vmType, viewName, true);
            var parameter = GenerateNavigationParameter(vmType);

            _navigatedTcs = tcs;
            if (NavigationService.Navigate(mappingItem, parameter, context))
                ClearCacheIfNeed(context, viewModel);
        }

        protected virtual bool TryCloseInternal(IDataContext context, TaskCompletionSource<bool> tcs)
        {
            context.AddOrUpdate(NavigatedTaskConstant, tcs);
            return NavigationService.TryClose(context);
        }

        protected virtual void RestoreInternal(IViewModel viewModel, IDataContext context)
        {
            var navigationContext = new NavigationContext(NavigationType.Page, NavigationMode.Refresh, CurrentViewModel, viewModel, this, context);
            var currentViewModel = CurrentViewModel;
            if (currentViewModel != null)
                TryCacheViewModel(navigationContext, CurrentContent ?? currentViewModel.Settings.Metadata.GetData(ViewModelConstants.View), currentViewModel);
            OnNavigated(navigationContext);
        }

        protected virtual void OnDispose()
        {
        }

        protected virtual IViewModel GetViewModelForView([NotNull] NavigationEventArgsBase args,
            [CanBeNull] IViewModel navigationViewModel, [NotNull] INavigationContext context, [NotNull] Type vmType)
        {
            var view = args.Content;
            if (navigationViewModel != null)
            {
                ViewManager.InitializeViewAsync(navigationViewModel, view, context);
                return navigationViewModel;
            }

            IViewModel vm = null;
            if (CachePolicy != null)
                vm = CachePolicy.TryTakeViewModelFromCache(context, view);

            if (HasViewModel(view, vmType))
                return (IViewModel)ToolkitExtensions.GetDataContext(view);
            if (vm == null)
            {
                IDataContext viewModelState = null;
#if WINDOWS_UWP
                viewModelState = PlatformExtensions.GetViewModelState(view);
                if (viewModelState != null)
                    PlatformExtensions.SetViewModelState(view, null);
#endif
                vm = ViewModelProvider.RestoreViewModel(viewModelState, new DataContext
                {
                    {InitializationConstants.ViewModelType, vmType}
                }, true);
            }

            if (vm != null)
                ViewManager.InitializeViewAsync(vm, view, context);
            return vm;
        }

        [CanBeNull]
        protected virtual INavigationContext CreateContextNavigateFrom(NavigatingCancelEventArgsBase args)
        {
            IViewModel viewModelFrom = null, viewModelTo = null;
            if (args.NavigationMode.IsClose())
                viewModelFrom = args.Context?.GetData(NavigationConstants.ViewModel);
            else
                viewModelTo = args.Context?.GetData(NavigationConstants.ViewModel);

            if (args.NavigationMode == NavigationMode.Remove)
            {
                if (viewModelFrom != null)
                    return new NavigationContext(NavigationType.Page, NavigationMode.Remove, viewModelFrom, null, this, args.Context);
                Tracer.Error("Possible bug in navigation, navigate with mode Remove mode without ViewModel");
                return null;
            }

            if (viewModelFrom == null)
                viewModelFrom = CurrentViewModel;
            if (viewModelFrom == null)
                return null;

            GetViewModelTypeFromParameter(NavigationService.GetParameterFromArgs(args));
            if (viewModelTo == null && args.NavigationMode == NavigationMode.Back)
                viewModelTo = viewModelFrom.GetParentViewModel();
            return new NavigationContext(NavigationType.Page, args.NavigationMode, viewModelFrom, viewModelTo, this, args.Context)
            {
                {NavigationProviderConstants.NavigatingCancelArgs, args}
            };
        }

        [NotNull]
        protected virtual INavigationContext CreateContextNavigateTo(NavigationEventArgsBase args)
        {
            IViewModel viewModelFrom = null, viewModelTo = null;
            if (args.NavigationMode.IsClose())
                viewModelFrom = args.Context?.GetData(NavigationConstants.ViewModel);
            else
                viewModelTo = args.Context?.GetData(NavigationConstants.ViewModel);
            if (viewModelFrom == null)
                viewModelFrom = CurrentViewModel;

            if (args.NavigationMode == NavigationMode.Remove)
            {
                if (viewModelFrom != null)
                    return new NavigationContext(NavigationType.Page, NavigationMode.Remove, viewModelFrom, null, this, args.Context);
                Tracer.Error("Possible bug in navigation, navigate with mode Remove mode without ViewModel");
            }

            var vmType = GetViewModelTypeFromParameter(NavigationService.GetParameterFromArgs(args));
            if (vmType == null)
            {
                if (args.Content != null)
                {
                    var items = ViewMappingProvider.FindMappingsForView(args.Content.GetType(), false);
                    if (items.Count == 1)
                    {
                        var type = items[0].ViewModelType;
#if WINDOWS_UWP || XAMARIN_FORMS
                        if (!type.GetTypeInfo().IsGenericTypeDefinition)
#else
                        if (!type.IsGenericTypeDefinition)
#endif

                            vmType = type;
                    }
                }
                if (vmType == null)
                    return new NavigationContext(NavigationType.Page, args.NavigationMode, viewModelFrom, viewModelTo, this, args.Context)
                        {
                            {NavigationProviderConstants.NavigationArgs, args}
                        };
            }
            return new NavigationContext(NavigationType.Page, args.NavigationMode, viewModelFrom, viewModelTo, this, args.Context)
            {
                {NavigationProviderConstants.NavigationArgs, args},
                {ViewModelTypeConstant, vmType}
            };
        }

        protected virtual void OnNavigating(NavigatingCancelEventArgsBase args)
        {
            if (_ignoreNavigating)
                return;
            var context = CreateContextNavigateFrom(args);
            if (context == null)
                return;

            args.Cancel = true;
            var navigateTask = !args.IsCancelable
                ? Empty.TrueTask
                : NavigationDispatcher.OnNavigatingFromAsync(context);
            navigateTask.TryExecuteSynchronously(task =>
            {
                if (!task.IsCanceled && task.IsFaulted)
                {
                    args.Context?.GetData(NavigatedTaskConstant)?.TrySetResult(false);
                    NavigationDispatcher.OnNavigationFailed(context, task.Exception);
                    return;
                }

                if (task.IsCanceled || !task.Result)
                {
                    args.Context?.GetData(NavigatedTaskConstant)?.TrySetResult(false);
                    if (!context.NavigationMode.IsClose())
                        NavigationDispatcher.OnNavigationCanceled(context);
                    return;
                }
                ThreadManager.InvokeOnUiThreadAsync(() =>
                {
                    try
                    {
                        _ignoreNavigating = true;
                        Renavigate(context, args);
                    }
                    finally
                    {
                        _ignoreNavigating = false;
                    }
                });
            });
        }

        protected virtual void OnNavigated(INavigationContext context)
        {
            var vmFrom = context.ViewModelFrom;
            var vmTo = context.ViewModelTo;
            var mode = context.NavigationMode;
            //only this mode allows to renavigate.
            if (ReferenceEquals(vmFrom, vmTo) && mode != NavigationMode.Refresh && mode != NavigationMode.Undefined)
            {
                if (vmFrom != null)
                    Tracer.Error("Possible bug in navigation, navigate to the same view model with mode " + mode);
                return;
            }
            if (mode != NavigationMode.Remove)
            {
                CurrentViewModel = vmTo;
                if (vmTo != null)
                {
                    if (!vmTo.Settings.State.Contains(IsNavigatedConstant))
                    {
                        vmTo.Disposed += _disposeViewModelHandler;
                        vmTo.Settings.State.AddOrUpdate(IsNavigatedConstant, null);
                        vmTo.Settings.Metadata.AddOrUpdate(ViewModelConstants.CanCloseHandler, CanCloseViewModel);
                    }
                }
            }

            NavigationDispatcher.OnNavigated(context);
            if (context.NavigationMode.IsClose())
                OnViewModelClosed(vmFrom, context);
        }

        private void Renavigate([NotNull] INavigationContext context, NavigatingCancelEventArgsBase args)
        {
            var currentViewModel = CurrentViewModel;
            var currentContent = CurrentContent;
            if (_navigatingCancelArgs == null)
            {
                if (NavigationService.Navigate(args))
                {
                    if (currentContent != null && currentViewModel != null)
                        TryCacheViewModel(context, currentContent, currentViewModel);

                    ClearCacheIfNeed(args.Context ?? DataContext.Empty, args.Context?.GetData(NavigationConstants.ViewModel));
                }
                else
                {
                    args.Context?.GetData(NavigatedTaskConstant)?.TrySetResult(false);
                    NavigationDispatcher.OnNavigationCanceled(context);
                }
            }
            else
            {
                if (currentContent != null && currentViewModel != null)
                    TryCacheViewModel(context, currentContent, currentViewModel);

                _navigatingCancelArgs.Cancel = false;
            }
        }

        private void NavigationServiceOnNavigating(object sender, NavigatingCancelEventArgsBase args)
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

        private void NavigationServiceOnNavigated(object sender, NavigationEventArgsBase e)
        {
            try
            {
                var context = CreateContextNavigateTo(e);
                UpdateNavigationContext(e, ref context);
                OnNavigated(context);
            }
            finally
            {
                e.Context?.GetData(NavigatedTaskConstant)?.TrySetResult(true);
            }
        }

        private void UpdateNavigationContext(NavigationEventArgsBase args, ref INavigationContext context)
        {
            var vmType = context.GetData(ViewModelTypeConstant);
            if (vmType == null)
                return;

            var viewModel = GetViewModelForView(args, context.ViewModelTo, context, vmType);
            if (!ReferenceEquals(context.ViewModelTo, viewModel))
                context = new NavigationContext(NavigationType.Page, context.NavigationMode, context.ViewModelFrom, viewModel, context.NavigationProvider, args.Context);
        }

        private void ViewModelOnDisposed(IDisposableObject sender, EventArgs args)
        {
            if (CachePolicy != null && CachePolicy.Invalidate((IViewModel)sender, DataContext.Empty))
                Tracer.Warn("The disposed view model " + sender.GetType().Name + " was in the navigation cache");
        }

        private void OnViewModelClosed(IViewModel viewModel, object parameter)
        {
            if (viewModel == null)
                return;
            viewModel.Disposed -= _disposeViewModelHandler;
            viewModel.Settings.State.Remove(IsNavigatedConstant);
            viewModel.Settings.Metadata.Remove(ViewModelConstants.CanCloseHandler);
            CachePolicy?.Invalidate(viewModel, parameter as IDataContext);
        }

        private void TryCacheViewModel(INavigationContext context, object view, IViewModel viewModel)
        {
            if (CachePolicy != null && view != null && viewModel != null && !context.NavigationMode.IsClose())
                CachePolicy.TryCacheViewModel(context, view, viewModel);
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
                if (ReferenceEquals(viewModelFrom, viewModelTo))
                    continue;
                var navigationContext = new NavigationContext(NavigationType.Page, NavigationMode.Remove, viewModelFrom, null, this, context);
                NavigationDispatcher.OnNavigated(navigationContext);
                OnViewModelClosed(viewModelFrom, navigationContext);
            }
        }

        private bool CanCloseViewModel(IViewModel viewModel, object parameter)
        {
            IDataContext context;
            if (!viewModel.Settings.Metadata.TryGetData(CloseContextConstant, out context))
            {
                context = new DataContext
                {
                    {NavigationConstants.ViewModel, viewModel},
                };
                viewModel.Settings.Metadata.AddOrUpdate(CloseContextConstant, context);
            }
            if (parameter != null)
                context.AddOrUpdate(NavigationConstants.CloseParameter, parameter);
            return NavigationService.CanClose(context);
        }

        #endregion
    }
}