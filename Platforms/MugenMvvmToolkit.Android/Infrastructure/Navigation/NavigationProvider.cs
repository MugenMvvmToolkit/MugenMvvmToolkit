#region Copyright

// ****************************************************************************
// <copyright file="NavigationProvider.cs">
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
using MugenMvvmToolkit.Models.Messages;
using MugenMvvmToolkit.ViewModels;
#if ANDROID
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
using System.Collections.Generic;
using System.Reflection;
using MugenMvvmToolkit.UWP.Interfaces.Navigation;

namespace MugenMvvmToolkit.UWP.Infrastructure.Navigation
#endif
{
    public class NavigationProvider : INavigationProvider, IHandler<ForegroundNavigationMessage>, IHandler<BackgroundNavigationMessage>
    {
        #region Fields

#if WINDOWS_UWP
        private readonly Dictionary<Guid, IViewModel> _openedViewModels;
#endif
        private static readonly string[] IdSeparator = { "~n|v~" };

        private TaskCompletionSource<bool> _navigationTcs;
        private TaskCompletionSource<bool> _unobservedNavigationTcs;
        private NavigatingCancelEventArgsBase _navigatingCancelArgs;
        private WeakReference _vmReference;

        public static readonly DataConstant<bool> BringToFront;
        protected static readonly DataConstant<object> IsNavigatedConstant;
        protected static readonly DataConstant<Type> ViewModelTypeConstant;
        protected static readonly DataConstant<object> IgnoreNavigatingConstant;
        protected static readonly DataConstant<IDataContext> CloseContextConstant;
        protected static readonly DataConstant<TaskCompletionSource<bool>> NavigatedTaskConstant;

        #endregion

        #region Constructors

        static NavigationProvider()
        {
            var type = typeof(NavigationProvider);
            ViewModelTypeConstant = DataConstant.Create<Type>(type, nameof(ViewModelTypeConstant), true);
            IsNavigatedConstant = DataConstant.Create<object>(type, nameof(IsNavigatedConstant), false);
            IgnoreNavigatingConstant = DataConstant.Create<object>(type, nameof(IgnoreNavigatingConstant), false);
            NavigatedTaskConstant = DataConstant.Create<TaskCompletionSource<bool>>(type, nameof(NavigatedTaskConstant), false);
            CloseContextConstant = DataConstant.Create<IDataContext>(type, nameof(CloseContextConstant), true);
            BringToFront = DataConstant.Create<bool>(type, nameof(BringToFront));
        }

        [Preserve(Conditional = true)]
        public NavigationProvider([NotNull] INavigationService navigationService, [NotNull] IThreadManager threadManager, [NotNull] IViewMappingProvider mappingProvider,
            [NotNull] IViewManager viewManager, [NotNull] IViewModelProvider viewModelProvider, [NotNull] INavigationDispatcher navigationDispatcher, IEventAggregator eventAggregator)
        {
            Should.NotBeNull(navigationService, nameof(navigationService));
            Should.NotBeNull(threadManager, nameof(threadManager));
            Should.NotBeNull(mappingProvider, nameof(mappingProvider));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(viewModelProvider, nameof(viewModelProvider));
            Should.NotBeNull(navigationDispatcher, nameof(navigationDispatcher));
            Should.NotBeNull(eventAggregator, nameof(eventAggregator));
            NavigationService = navigationService;
            ThreadManager = threadManager;
            ViewMappingProvider = mappingProvider;
            ViewManager = viewManager;
            ViewModelProvider = viewModelProvider;
            NavigationDispatcher = navigationDispatcher;
            _vmReference = Empty.WeakReference;

            NavigationService.Navigating += NavigationServiceOnNavigating;
            NavigationService.Navigated += NavigationServiceOnNavigated;
            eventAggregator.Subscribe(this);
#if WINDOWS_UWP
            _openedViewModels = new Dictionary<Guid, IViewModel>();
#elif XAMARIN_FORMS
            NavigationService.RootPageChanged += NavigationServiceOnRootPageChanged;
#endif
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

        public Task CurrentNavigationTask => _navigationTcs?.Task ?? _unobservedNavigationTcs?.Task ?? Empty.Task;

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
#if WINDOWS_UWP
            _openedViewModels.Clear();
#elif XAMARIN_FORMS
            NavigationService.RootPageChanged -= NavigationServiceOnRootPageChanged;
#endif
            NavigationService.Navigating -= NavigationServiceOnNavigating;
            NavigationService.Navigated -= NavigationServiceOnNavigated;
            OnDispose();
        }

        public Task<bool> NavigateAsync(IDataContext context)
        {
            var viewModel = GetViewModelFromContext(context);
            var tcs = new TaskCompletionSource<bool>();
            context = new DataContext(context.ToNonReadOnly());
            CurrentNavigationTask.TryExecuteSynchronously(task => ThreadManager.InvokeOnUiThreadAsync(() =>
            {
                try
                {
                    _navigationTcs = tcs;
                    NavigateInternal(viewModel, context, tcs);
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                    throw;
                }
            }));
            return tcs.Task;
        }

        public Task<bool> TryCloseAsync(IDataContext context)
        {
            var viewModel = GetViewModelFromContext(context, false);
            if (viewModel == null || !viewModel.Settings.State.Contains(IsNavigatedConstant))
                return null;
            context = new DataContext(context.ToNonReadOnly());
            var tcs = new TaskCompletionSource<bool>();
            CurrentNavigationTask.TryExecuteSynchronously(task => ThreadManager.InvokeOnUiThreadAsync(() =>
            {
                try
                {
                    _navigationTcs = tcs;
                    if (!TryCloseInternal(context, tcs))
                        tcs.TrySetResult(false);
                }
                catch (Exception e)
                {
                    tcs.TrySetException(e);
                    throw;
                }
            }));
            return tcs.Task;
        }

        public void Restore(IDataContext context)
        {
            var viewModel = GetViewModelFromContext(context);
            context = context.ToNonReadOnly();
            RestoreInternal(viewModel, context);
        }

        void IHandler<ForegroundNavigationMessage>.Handle(object sender, ForegroundNavigationMessage message)
        {
            var currentViewModel = CurrentViewModel;
            if (currentViewModel != null)
                OnNavigated(new NavigationContext(NavigationType.Page, NavigationMode.Foreground, null, currentViewModel, this, message.Context));
        }

        void IHandler<BackgroundNavigationMessage>.Handle(object sender, BackgroundNavigationMessage message)
        {
            var currentViewModel = CurrentViewModel;
            if (currentViewModel != null)
                OnNavigated(new NavigationContext(NavigationType.Page, NavigationMode.Background, currentViewModel, null, this, message.Context));
        }

        #endregion

        #region Methods

        public static string GenerateNavigationParameter([NotNull] IViewModel viewModel)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            return viewModel.GetType().AssemblyQualifiedName + IdSeparator[0] + viewModel.GetViewModelId().ToString("N");
        }

        public static Type GetViewModelTypeFromParameter(string parameter, out Guid viewModelId)
        {
            if (string.IsNullOrEmpty(parameter))
            {
                viewModelId = Guid.Empty;
                return null;
            }
            var items = parameter.Split(IdSeparator, StringSplitOptions.RemoveEmptyEntries);
            viewModelId = items.Length == 2 ? Guid.Parse(items[1]) : Guid.Empty;
            return Type.GetType(items[0], false);
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
            //The view model is already shown as page and we need to bring it to front
            if (viewModel.Settings.State.Contains(IsNavigatedConstant))
                context.AddOrUpdate(BringToFront, true);

            context.AddOrUpdate(NavigatedTaskConstant, tcs);
            string viewName = viewModel.GetViewName(context);
            var mappingItem = ViewMappingProvider.FindMappingForViewModel(viewModel.GetType(), viewName, true);
            var parameter = GenerateNavigationParameter(viewModel);
            NavigationService.Navigate(mappingItem, parameter, context);
        }

        protected virtual bool TryCloseInternal(IDataContext context, TaskCompletionSource<bool> tcs)
        {
            context.AddOrUpdate(NavigatedTaskConstant, tcs);
            return NavigationService.TryClose(context);
        }

        protected virtual void RestoreInternal(IViewModel viewModel, IDataContext context)
        {
            var opened = viewModel.Settings.State.Contains(IsNavigatedConstant);
            var navigationContext = new NavigationContext(NavigationType.Page, opened ? NavigationMode.Refresh : NavigationMode.New, CurrentViewModel, viewModel, this, context);
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


            if (HasViewModel(view, vmType))
                return (IViewModel)ToolkitExtensions.GetDataContext(view);

            IDataContext viewModelState = null;
#if WINDOWS_UWP
            viewModelState = UwpToolkitExtensions.GetViewModelState(view);
            if (viewModelState != null)
                UwpToolkitExtensions.SetViewModelState(view, null);
#endif
            var vm = ViewModelProvider.RestoreViewModel(viewModelState, new DataContext
            {
                {InitializationConstants.ViewModelType, vmType}
            }, true);

            if (vm != null)
                ViewManager.InitializeViewAsync(vm, view, context);
            return vm;
        }

        [CanBeNull]
        protected virtual INavigationContext CreateContextNavigateFrom(NavigatingCancelEventArgsBase args)
        {
            IViewModel viewModelFrom = null, viewModelTo = null;
            if (args.NavigationMode.IsClose())
                viewModelFrom = args.Context.GetData(NavigationConstants.ViewModel);
            else
                viewModelTo = args.Context.GetData(NavigationConstants.ViewModel);

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

            Guid viewModelId;
            if (viewModelTo == null && GetViewModelTypeFromParameter(args.Parameter, out viewModelId) != null)
                viewModelTo = ViewModelProvider.TryGetViewModelById(viewModelId);

            bool doNotTrackViewModelTo = false;
            if (viewModelTo == null && args.NavigationMode == NavigationMode.Back)
                viewModelTo = NavigationDispatcher.GetPreviousOpenedViewModelOrParent(viewModelFrom, NavigationType.Page, out doNotTrackViewModelTo, this);
            return new NavigationContext(NavigationType.Page, args.NavigationMode, viewModelFrom, viewModelTo, this, args.Context)
            {
                {NavigationConstants.DoNotTrackViewModelTo, doNotTrackViewModelTo}
            };
        }

        [NotNull]
        protected virtual INavigationContext CreateContextNavigateTo(NavigationEventArgsBase args)
        {
            IViewModel viewModelFrom = null, viewModelTo = null;
            if (args.NavigationMode.IsClose())
                viewModelFrom = args.Context.GetData(NavigationConstants.ViewModel);
            else
                viewModelTo = args.Context.GetData(NavigationConstants.ViewModel);

            if (viewModelFrom == null)
                viewModelFrom = CurrentViewModel;
            if (args.NavigationMode == NavigationMode.Remove)
            {
                if (viewModelFrom != null)
                    return new NavigationContext(NavigationType.Page, NavigationMode.Remove, viewModelFrom, null, this, args.Context);
                Tracer.Error("Possible bug in navigation, navigate with mode Remove mode without ViewModel");
            }

            Guid viewModelId;
            var vmType = GetViewModelTypeFromParameter(args.Parameter, out viewModelId);
            if (viewModelTo == null && vmType != null)
                viewModelTo = ViewModelProvider.TryGetViewModelById(viewModelId);
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
                    else if (viewModelTo == null || items.Count > 1)
                    {
                        var viewModel = ToolkitExtensions.GetDataContext(args.Content) as IViewModel;
                        var type = viewModel?.GetType();
                        if (type != null && items.Any(item => item.ViewModelType == type))
                        {
                            viewModelTo = viewModel;
                            vmType = type;
                        }
                    }
                }
                if (vmType == null)
                    return new NavigationContext(NavigationType.Page, args.NavigationMode, viewModelFrom, viewModelTo, this, args.Context);
            }
            return new NavigationContext(NavigationType.Page, args.NavigationMode, viewModelFrom, viewModelTo, this, args.Context)
            {
                {ViewModelTypeConstant, vmType}
            };
        }

        protected virtual void OnNavigating(NavigatingCancelEventArgsBase args)
        {
            if (args.Context.Contains(IgnoreNavigatingConstant))
                return;
            var context = CreateContextNavigateFrom(args);
            if (context == null)
                return;

            args.Cancel = true;
            var navigateTask = !args.IsCancelable
                ? Empty.TrueTask
                : NavigationDispatcher.OnNavigatingAsync(context);
            navigateTask.TryExecuteSynchronously(task =>
            {
                if (!task.IsCanceled && task.IsFaulted)
                {
                    TryCompleteNavigationTask(args.Context, false);
                    NavigationDispatcher.OnNavigationFailed(context, task.Exception);
                    return;
                }

                if (task.IsCanceled || !task.Result)
                {
                    TryCompleteNavigationTask(args.Context, false);
                    if (!context.NavigationMode.IsClose())
                        NavigationDispatcher.OnNavigationCanceled(context);
                    return;
                }
                ThreadManager.Invoke(ExecutionMode.AsynchronousOnUiThread, this, context, args, (@this, ctx, e) =>
                {
                    e.Context.AddOrUpdate(IgnoreNavigatingConstant, null);
                    @this.Renavigate(ctx, e);
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
            if (mode != NavigationMode.Remove && mode != NavigationMode.Background && mode != NavigationMode.Foreground)
            {
                CurrentViewModel = vmTo;
                if (vmTo != null)
                {
                    if (!vmTo.Settings.State.Contains(IsNavigatedConstant))
                    {
                        vmTo.Settings.State.AddOrUpdate(IsNavigatedConstant, null);
                        vmTo.Settings.Metadata.AddOrUpdate(ViewModelConstants.CanCloseHandler, CanCloseViewModel);
                    }
                }
            }

            NavigationDispatcher.OnNavigated(context);
            if (context.NavigationMode.IsClose())
            {
                if (vmFrom != null)
                {
                    vmFrom.Settings.State.Remove(IsNavigatedConstant);
                    vmFrom.Settings.Metadata.Remove(ViewModelConstants.CanCloseHandler);
#if WINDOWS_UWP                
                    lock (_openedViewModels)
                        _openedViewModels.Remove(vmFrom.GetViewModelId());
#endif
                }
            }
            else if (vmTo != null)
            {
#if WINDOWS_UWP
                lock (_openedViewModels)
                    _openedViewModels[vmTo.GetViewModelId()] = vmTo;
#endif
            }
        }

        private void Renavigate([NotNull] INavigationContext context, NavigatingCancelEventArgsBase args)
        {
            if (_navigatingCancelArgs == null)
            {
                if (!NavigationService.Navigate(args))
                {
                    TryCompleteNavigationTask(args.Context, false);
                    NavigationDispatcher.OnNavigationCanceled(context);
                }
            }
            else
                _navigatingCancelArgs.Cancel = false;
        }

        private void NavigationServiceOnNavigating(object sender, NavigatingCancelEventArgsBase args)
        {
            try
            {
                if (!args.Context.Contains(NavigatedTaskConstant) && _unobservedNavigationTcs == null)
                    _unobservedNavigationTcs = new TaskCompletionSource<bool>();
                _navigatingCancelArgs = args;
                OnNavigating(args);
            }
            finally
            {
                _navigatingCancelArgs = null;
            }
        }

        private void NavigationServiceOnNavigated(object sender, NavigationEventArgsBase args)
        {
            try
            {
                if (!args.Context.Contains(NavigatedTaskConstant) && _unobservedNavigationTcs == null)
                    _unobservedNavigationTcs = new TaskCompletionSource<bool>();
                var context = CreateContextNavigateTo(args);
                UpdateNavigationContext(args, ref context);
                OnNavigated(context);
            }
            finally
            {
                TryCompleteNavigationTask(args.Context, true);
            }
        }

#if XAMARIN_FORMS
        private void NavigationServiceOnRootPageChanged(INavigationService sender, ValueEventArgs<IViewModel> args)
        {
            CurrentViewModel = args.Value;
        }
#endif

        private void UpdateNavigationContext(NavigationEventArgsBase args, ref INavigationContext context)
        {
            var vmType = context.GetData(ViewModelTypeConstant);
            if (vmType == null)
                return;

            var viewModel = GetViewModelForView(args, context.ViewModelTo, context, vmType);
            if (!ReferenceEquals(context.ViewModelTo, viewModel))
                context = new NavigationContext(NavigationType.Page, context.NavigationMode, context.ViewModelFrom, viewModel, context.NavigationProvider, args.Context);
        }

        private void TryCompleteNavigationTask(IDataContext context, bool result)
        {
            var unobservedNavigationTcs = _unobservedNavigationTcs;
            if (unobservedNavigationTcs != null)
            {
                _unobservedNavigationTcs = null;
                unobservedNavigationTcs.TrySetResult(result);
            }
            if (context != null)
            {
                context.GetData(NavigatedTaskConstant)?.TrySetResult(result);
                context.Clear();
            }
        }

        private bool CanCloseViewModel(IViewModel viewModel, object parameter)
        {
            IDataContext context;
            if (!viewModel.Settings.Metadata.TryGetData(CloseContextConstant, out context))
            {
                context = new DataContext
                {
                    {NavigationConstants.ViewModel, viewModel}
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