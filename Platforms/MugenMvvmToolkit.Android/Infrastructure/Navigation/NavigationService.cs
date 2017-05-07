#region Copyright

// ****************************************************************************
// <copyright file="NavigationService.cs">
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
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Android.Infrastructure.Mediators;
using MugenMvvmToolkit.Android.Interfaces.Navigation;
using MugenMvvmToolkit.Android.Interfaces.Views;
using MugenMvvmToolkit.Android.Models.EventArg;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Models.Messages;
using MugenMvvmToolkit.ViewModels;
using Object = Java.Lang.Object;

namespace MugenMvvmToolkit.Android.Infrastructure.Navigation
{
    public class NavigationService : INavigationService
    {
        #region Nested Types

        private sealed class ActivityLifecycleListener : Object, Application.IActivityLifecycleCallbacks
        {
            #region Fields

            private readonly NavigationService _service;

            #endregion

            #region Constructors

            public ActivityLifecycleListener(NavigationService service)
            {
                _service = service;
            }

            public ActivityLifecycleListener(IntPtr handle, JniHandleOwnership transfer)
                : base(handle, transfer)
            {
            }

            #endregion

            #region Methods

            private bool IsAlive()
            {
                if (_service == null)
                {
                    (Application.Context as Application)?.UnregisterActivityLifecycleCallbacks(this);
                    return false;
                }
                return true;
            }

            private void RaiseNew(Activity activity)
            {
                if (IsAlive() && !(activity is IActivityView) && _service.CurrentContent != activity)
                {
                    AndroidToolkitExtensions.SetCurrentActivity(activity, false);
                    _service.RaiseNavigated(activity, NavigationMode.New, null, null);
                }
            }

            #endregion

            #region Implementation of interfaces

            public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
            {
                RaiseNew(activity);
            }

            public void OnActivityDestroyed(Activity activity)
            {
                if (IsAlive() && !(activity is IActivityView))
                    AndroidToolkitExtensions.SetCurrentActivity(activity, true);
            }

            public void OnActivityPaused(Activity activity)
            {
                if (IsAlive() && !(activity is IActivityView) && ReferenceEquals(activity, _service.CurrentContent) && !activity.IsFinishing)
                    _service.SetBackground(true, null);
            }

            public void OnActivityResumed(Activity activity)
            {
                if (IsAlive() && !(activity is IActivityView))
                    _service.SetBackground(false, null);
            }

            public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
            {
            }

            public void OnActivityStarted(Activity activity)
            {
                RaiseNew(activity);
            }

            public void OnActivityStopped(Activity activity)
            {
            }

            #endregion
        }

        #endregion

        #region Fields

        private const string ParameterId = "viewmodelparameter";
        private const string IsFinishPauseKey = nameof(IsFinishPauseKey);
        private const string IsFinishedKey = nameof(IsFinishedKey);
        private const string IsOpenedKey = nameof(IsOpenedKey);

        private IDataContext _newContext;
        private IDataContext _backContext;
        private bool _isBackground;
        private bool _isBackgroundSet;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public NavigationService(IEventAggregator eventAggregator, IThreadManager threadManager)
        {
            Should.NotBeNull(eventAggregator, nameof(eventAggregator));
            Should.NotBeNull(threadManager, nameof(threadManager));
            ThreadManager = threadManager;
            EventAggregator = eventAggregator;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.IceCreamSandwich)
            {
                (Application.Context as Application)?.RegisterActivityLifecycleCallbacks(new ActivityLifecycleListener(this));
            }
        }

        #endregion

        #region Properties

        protected IThreadManager ThreadManager { get; }

        protected IEventAggregator EventAggregator { get; }

        protected Activity CurrentContent => AndroidToolkitExtensions.CurrentActivity;

        protected bool IsBackground => _isBackground;

        #endregion

        #region Methods

        protected virtual bool RaiseNavigating(NavigatingCancelEventArgs args)
        {
            var handler = Navigating;
            if (handler != null)
            {
                handler(this, args);
                return !args.Cancel;
            }
            return true;
        }

        protected virtual void RaiseNavigated(object content, NavigationMode mode, string parameter, IDataContext context)
        {
            Navigated?.Invoke(this, new NavigationEventArgs(content, parameter, mode, context));
        }

        protected virtual void StartActivity(Context context, Intent intent, IViewMappingItem source, IDataContext dataContext)
        {
            var activity = context.GetActivity();
            Action<Context, Intent, IViewMappingItem, IDataContext> startAction = null;
            if (activity != null)
                startAction = activity.GetBindingMemberValue(AttachedMembers.Activity.StartActivityDelegate);
            if (startAction == null)
            {
                var bundle = dataContext.GetData(NavigationConstants.NavigationParameter) as Bundle;
                if (bundle == null)
                    context.StartActivity(intent);
                else
                    context.StartActivity(intent, bundle);
            }
            else
                startAction(context, intent, source, dataContext);
        }

        protected virtual bool IsDestroyed(Activity activity)
        {
            var activityView = activity as IActivityView;
            if (activityView == null)
                return false;
            return activityView.Mediator.IsDestroyed;
        }

        private bool GoBack(IDataContext context)
        {
            var currentActivity = AndroidToolkitExtensions.CurrentActivity;
            if (!currentActivity.IsAlive() || IsDestroyed(currentActivity) || currentActivity.IsFinishing)
                return false;
            _backContext = context;
            if (IsBackground)
            {
                GetState(currentActivity).PutBoolean(IsFinishPauseKey, true);
                currentActivity.Finish();
            }
            else
                currentActivity.OnBackPressed();
            return currentActivity.IsFinishing;
        }

        private static string GetParameterFromIntent(Intent intent)
        {
            return intent?.GetStringExtra(ParameterId);
        }

        private static Bundle GetState(Activity activity)
        {
            var result = (activity as IActivityView)?.Mediator.State;
            return result ?? ServiceProvider.AttachedValueProvider.GetOrAdd(activity, nameof(GetState), (view, o) => new Bundle(), null);
        }

        private static IDataContext MergeContext(IDataContext ctx1, IDataContext navContext)
        {
            if (ctx1 == null && navContext == null)
                return null;
            if (ctx1 != null && navContext != null)
            {
                ctx1 = ctx1.ToNonReadOnly();
                ctx1.Merge(navContext);
                return ctx1;
            }
            if (ctx1 != null)
                return ctx1;
            return new DataContext(navContext);
        }

        private void SetBackground(bool value, IDataContext context)
        {
            if (_isBackground == value)
                return;
            _isBackground = value;
            if (value)
            {
                var delayTask = Task.Delay(AndroidToolkitExtensions.BackgroundNotificationDelay);
                if (SynchronizationContext.Current == null)
                    delayTask.ContinueWith(RaiseBackground, context);
                else
                    delayTask.ContinueWith(RaiseBackground, context, TaskScheduler.FromCurrentSynchronizationContext());
            }
            else
            {
                if (_isBackgroundSet)
                {
                    _isBackgroundSet = false;
                    EventAggregator.Publish(this, new ForegroundNavigationMessage(context));
                }
            }
        }

        private void RaiseBackground(Task t, object state)
        {
            ThreadManager.Invoke(ExecutionMode.AsynchronousOnUiThread, this, state, (@this, ctx) =>
            {
                if (IsBackground)
                {
                    @this.EventAggregator.Publish(@this, new BackgroundNavigationMessage(ctx as IDataContext));
                    _isBackgroundSet = true;
                }
            }, OperationPriority.Low);
        }

        #endregion

        #region Implementation of INavigationService

        object INavigationService.CurrentContent => AndroidToolkitExtensions.CurrentActivity;

        public void OnPauseActivity(Activity activity, IDataContext context)
        {
            Should.NotBeNull(activity, nameof(activity));
            if (ReferenceEquals(activity, CurrentContent) && !activity.IsFinishing)
                SetBackground(true, context);
        }

        public void OnResumeActivity(Activity activity, IDataContext context)
        {
            Should.NotBeNull(activity, nameof(activity));
            SetBackground(false, context);
            var prevContent = CurrentContent;
            if (ReferenceEquals(activity, prevContent))
                return;
            AndroidToolkitExtensions.SetCurrentActivity(activity, false);
            var bundle = GetState(activity);
            if (bundle.ContainsKey(IsOpenedKey))
            {
                NavigationMode mode;
                IDataContext ctx = _backContext;
                if (ctx != null || prevContent != null && prevContent.IsFinishing)
                {
                    mode = NavigationMode.Back;
                    _backContext = null;
                }
                else
                {
                    ctx = _newContext;
                    _newContext = null;
                    mode = NavigationMode.Refresh;
                }
                RaiseNavigated(activity, mode, GetParameterFromIntent(activity.Intent), MergeContext(ctx, context));
            }
            else
            {
                bundle.PutBoolean(IsOpenedKey, true);
                var newContext = _newContext;
                _newContext = null;
                RaiseNavigated(activity, NavigationMode.New, GetParameterFromIntent(activity.Intent), MergeContext(newContext, context));
            }
        }

        public void OnStartActivity(Activity activity, IDataContext context)
        {
            OnResumeActivity(activity, context);
        }

        public void OnCreateActivity(Activity activity, IDataContext context)
        {
            OnResumeActivity(activity, context);
        }

        public bool OnFinishActivity(Activity activity, bool isBackNavigation, IDataContext context)
        {
            Should.NotBeNull(activity, nameof(activity));
            var bundle = GetState(activity);
            var isFinishFromPause = bundle.ContainsKey(IsFinishPauseKey);
            if (!isBackNavigation && !isFinishFromPause)
            {
                bundle.PutBoolean(IsFinishedKey, true);
                return true;
            }

            if (isFinishFromPause)
                bundle.Remove(IsFinishPauseKey);
            if (!RaiseNavigating(new NavigatingCancelEventArgs(NavigationMode.Back, MergeContext(_backContext, context))))
                return false;

            //If it's the first activity, we need to raise the back navigation event.
            if (activity.IsTaskRoot)
            {
                var backContext = _backContext;
                _backContext = null;
                RaiseNavigated(null, NavigationMode.Back, null, MergeContext(backContext, context));
            }
            bundle.PutBoolean(IsFinishedKey, true);
            return true;
        }

        public void OnDestroyActivity(Activity activity, IDataContext context)
        {
            Should.NotBeNull(activity, nameof(activity));
            var bundle = GetState(activity);
            if (activity.IsFinishing && !bundle.ContainsKey(IsFinishedKey))
            {
                var viewModel = activity.DataContext() as IViewModel;
                if (viewModel != null)
                {
                    RaiseNavigated(activity, NavigationMode.Remove, null, new DataContext
                    {
                        {NavigationConstants.ViewModel, viewModel}
                    });
                }
            }
        }

        public bool Navigate(NavigatingCancelEventArgsBase args)
        {
            Should.NotBeNull(args, nameof(args));
            if (!args.IsCancelable)
                return false;
            if (args.NavigationMode == NavigationMode.Remove)
                return TryClose(args.Context);

            var eventArgs = (NavigatingCancelEventArgs)args;
            if (eventArgs.NavigationMode != NavigationMode.Back && eventArgs.Mapping != null)
                return Navigate(eventArgs.Mapping, eventArgs.Parameter, eventArgs.Context);

            return GoBack(args.Context);
        }

        public bool Navigate(IViewMappingItem source, string parameter, IDataContext dataContext)
        {
            Should.NotBeNull(source, nameof(source));
            Should.NotBeNull(dataContext, nameof(dataContext));
            bool bringToFront;
            dataContext.TryGetData(NavigationProvider.BringToFront, out bringToFront);
            if (!RaiseNavigating(new NavigatingCancelEventArgs(source, bringToFront ? NavigationMode.Refresh : NavigationMode.New, parameter, dataContext)))
                return false;

            _newContext = dataContext;
            bool clearBackStack = dataContext.GetData(NavigationConstants.ClearBackStack);
            var activity = AndroidToolkitExtensions.CurrentActivity;
            var context = activity ?? Application.Context;

            var intent = new Intent(context, source.ViewType);

            if (activity == null)
                intent.AddFlags(ActivityFlags.NewTask);
            else if (clearBackStack)
            {
                if (AndroidToolkitExtensions.IsApiLessThanOrEqualTo10)
                    intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);
                else
                    intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                var message = new MvvmActivityMediator.FinishActivityMessage();
                if (bringToFront)
                {
                    var viewModel = dataContext.GetData(NavigationConstants.ViewModel);
                    if (viewModel != null)
                        message.IgnoredViewModels = new[] { viewModel };
                }
                ServiceProvider.EventAggregator.Publish(this, message);
                if (message.FinishedViewModels != null)
                {
                    message.FinishedViewModels.Reverse();
                    foreach (var vm in message.FinishedViewModels)
                    {
                        var ctx = new DataContext(dataContext);
                        ctx.AddOrUpdate(NavigationConstants.ViewModel, vm);
                        RaiseNavigated(vm, NavigationMode.Remove, null, ctx);
                    }
                }
            }

            if (parameter != null)
                intent.PutExtra(ParameterId, parameter);

            if (bringToFront)
            {
                //http://stackoverflow.com/questions/20695522/puzzling-behavior-with-reorder-to-front
                //http://code.google.com/p/android/issues/detail?id=63570#c2
                bool closed = false;
                if (!clearBackStack && AndroidToolkitExtensions.IsApiGreaterThanOrEqualTo19)
                {
                    var viewModel = dataContext.GetData(NavigationConstants.ViewModel);
                    var activityView = viewModel?.GetCurrentView<object>() as Activity;
                    if (activityView != null && activityView.IsTaskRoot)
                    {
                        var message = new MvvmActivityMediator.FinishActivityMessage(viewModel);
                        ServiceProvider.EventAggregator.Publish(this, message);
                        closed = message.IsFinished;
                    }
                }
                if (!closed)
                    intent.AddFlags(ActivityFlags.ReorderToFront);
            }
            StartActivity(context, intent, source, dataContext);
            return true;
        }

        public bool CanClose(IDataContext dataContext)
        {
            Should.NotBeNull(dataContext, nameof(dataContext));
            var viewModel = dataContext.GetData(NavigationConstants.ViewModel);
            if (viewModel == null)
                return false;
            if (CurrentContent?.DataContext() == viewModel)
                return true;
            var message = new MvvmActivityMediator.CanFinishActivityMessage(viewModel);
            ServiceProvider.EventAggregator.Publish(this, message);
            return message.CanFinish;
        }

        public bool TryClose(IDataContext dataContext)
        {
            Should.NotBeNull(dataContext, nameof(dataContext));
            var viewModel = dataContext.GetData(NavigationConstants.ViewModel);
            if (viewModel == null)
                return false;
            if (CurrentContent?.DataContext() == viewModel)
                return GoBack(dataContext);

            if (!CanClose(dataContext))
                return false;
            if (RaiseNavigating(new NavigatingCancelEventArgs(NavigationMode.Remove, dataContext)))
            {
                var message = new MvvmActivityMediator.FinishActivityMessage(viewModel);
                ServiceProvider.EventAggregator.Publish(this, message);
                if (message.IsFinished)
                    RaiseNavigated(viewModel, NavigationMode.Remove, null, dataContext);
                return message.IsFinished;
            }
            return true;
        }

        public event EventHandler<INavigationService, NavigatingCancelEventArgsBase> Navigating;

        public event EventHandler<INavigationService, NavigationEventArgsBase> Navigated;

        #endregion
    }
}