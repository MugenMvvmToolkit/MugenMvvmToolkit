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
using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using JetBrains.Annotations;
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
                    PlatformExtensions.SetCurrentActivity(activity, false);
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
                    PlatformExtensions.SetCurrentActivity(activity, true);
            }

            public void OnActivityPaused(Activity activity)
            {
            }

            public void OnActivityResumed(Activity activity)
            {
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
        private const string IsBackKey = nameof(IsBackKey);
        private const string IsFinishFromPauseKey = nameof(IsFinishFromPauseKey);

        private bool _isReorder;
        private bool _isPause;
        private IDataContext _newDataContext;
        private IDataContext _backDataContext;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public NavigationService(IEventAggregator eventAggregator)
        {
            Should.NotBeNull(eventAggregator, nameof(eventAggregator));
            EventAggregator = eventAggregator;
            if (Build.VERSION.SdkInt >= BuildVersionCodes.IceCreamSandwich)
            {
                (Application.Context as Application)?.RegisterActivityLifecycleCallbacks(new ActivityLifecycleListener(this));
            }
            _newDataContext = DataContext.Empty;
        }

        #endregion

        #region Properties

        protected IEventAggregator EventAggregator { get; }

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

        protected virtual bool IsNoHistory([CanBeNull] Activity activity)
        {
            if (activity == null)
                return false;
            if ((activity.Intent.Flags & ActivityFlags.NoHistory) == ActivityFlags.NoHistory)
                return true;
            var attribute = activity.GetType().GetCustomAttributes(typeof(ActivityAttribute), false).OfType<ActivityAttribute>().FirstOrDefault();
            return attribute != null && attribute.NoHistory;
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
            var currentActivity = PlatformExtensions.CurrentActivity;
            if (!currentActivity.IsAlive() || IsDestroyed(currentActivity) || currentActivity.IsFinishing)
                return false;
            _backDataContext = context;
            if (_isPause)
            {
                ServiceProvider.AttachedValueProvider.SetValue(currentActivity, IsFinishFromPauseKey, Empty.TrueObject);
                currentActivity.Finish();
            }
            else
                currentActivity.OnBackPressed();
            return true;
        }

        private static string GetParameterFromIntent(Intent intent)
        {
            return intent?.GetStringExtra(ParameterId);
        }

        private static IDataContext MergeContext(IDataContext ctx1, IDataContext ctx2)
        {
            if (ctx1 == null && ctx2 == null)
                return null;
            if (ctx1 != null && ctx2 != null)
            {
                ctx1 = ctx1.ToNonReadOnly();
                ctx1.Merge(ctx2);
                return ctx1;
            }
            if (ctx1 != null)
                return ctx1;
            return ctx2;
        }

        #endregion

        #region Implementation of INavigationService

        public object CurrentContent => PlatformExtensions.CurrentActivity;

        public void OnPauseActivity(Activity activity, IDataContext context = null)
        {
            Should.NotBeNull(activity, nameof(activity));
            var isBack = ServiceProvider.AttachedValueProvider.GetValue<bool>(activity, IsBackKey, false);
            if (_newDataContext != null || isBack || !ReferenceEquals(activity, CurrentContent))
                return;
            _isPause = true;
            EventAggregator.Publish(this, new BackgroundNavigationMessage(context));
        }

        public void OnResumeActivity(Activity activity, IDataContext context = null)
        {
            Should.NotBeNull(activity, nameof(activity));
            var prevContent = CurrentContent as Activity;
            var activityEquals = ReferenceEquals(activity, prevContent);
            if (activityEquals && !_isPause)
                return;
            PlatformExtensions.SetCurrentActivity(activity, false);
            var isPause = _isPause;
            _isPause = false;
            if (_newDataContext == null)
            {
                var dataContext = _backDataContext;
                _backDataContext = null;
                if (isPause && activityEquals)
                    EventAggregator.Publish(this, new ForegroundNavigationMessage(context));
                else
                {
                    RaiseNavigated(activity, NavigationMode.Back, GetParameterFromIntent(activity.Intent), MergeContext(dataContext, context));
                    if (isPause)
                        EventAggregator.Publish(this, new ForegroundNavigationMessage(context));
                }
            }
            else
            {
                var isReorder = _isReorder;
                var dataContext = _newDataContext;
                _newDataContext = null;
                _isReorder = false;
                RaiseNavigated(activity, isReorder ? NavigationMode.Refresh : NavigationMode.New, GetParameterFromIntent(activity.Intent), MergeContext(dataContext, context));
                if (IsNoHistory(prevContent))
                {
                    var viewModel = prevContent.DataContext() as IViewModel;
                    if (viewModel != null)
                    {
                        RaiseNavigated(prevContent, NavigationMode.Remove, null, new DataContext
                        {
                            {NavigationConstants.ViewModel, viewModel}
                        });
                    }
                }
            }
        }

        public void OnStartActivity(Activity activity, IDataContext context = null)
        {
            OnResumeActivity(activity, context);
        }

        public void OnCreateActivity(Activity activity, IDataContext context = null)
        {
            OnResumeActivity(activity, context);
        }

        public bool OnFinishActivity(Activity activity, bool isBackNavigation, IDataContext context = null)
        {
            Should.NotBeNull(activity, nameof(activity));
            var isFinishFromPause = ServiceProvider.AttachedValueProvider.GetValue<bool>(activity, IsFinishFromPauseKey, false);
            if (!isBackNavigation && !isFinishFromPause)
                return true;
            if (isFinishFromPause)
                ServiceProvider.AttachedValueProvider.Clear(activity, IsFinishFromPauseKey);
            if (!RaiseNavigating(new NavigatingCancelEventArgs(NavigationMode.Back, MergeContext(_backDataContext, context))))
                return false;
            //If it's the first activity, we need to raise the back navigation event.
            if (activity.IsTaskRoot)
                RaiseNavigated(null, NavigationMode.Back, null, MergeContext(_backDataContext, context));
            ServiceProvider.AttachedValueProvider.SetValue(activity, IsBackKey, Empty.TrueObject);
            return true;
        }

        public string GetParameterFromArgs(EventArgs args)
        {
            Should.NotBeNull(args, nameof(args));
            var cancelArgs = args as NavigatingCancelEventArgs;
            if (cancelArgs == null)
                return (args as NavigationEventArgs)?.Parameter;
            return cancelArgs.Parameter;
        }

        public bool Navigate(NavigatingCancelEventArgsBase args)
        {
            Should.NotBeNull(args, nameof(args));
            if (!args.IsCancelable)
                return false;
            var eventArgs = (NavigatingCancelEventArgs)args;

            if (args.Context != null && eventArgs.NavigationMode == NavigationMode.Remove)
                return TryClose(args.Context);

            if (eventArgs.NavigationMode != NavigationMode.Back && eventArgs.Mapping != null)
                return Navigate(eventArgs.Mapping, eventArgs.Parameter, eventArgs.Context);

            return GoBack(args.Context);
        }

        public bool Navigate(IViewMappingItem source, string parameter, IDataContext dataContext)
        {
            Should.NotBeNull(source, nameof(source));
            if (dataContext == null)
                dataContext = DataContext.Empty;
            bool bringToFront;
            dataContext.TryGetData(NavigationProviderConstants.BringToFront, out bringToFront);
            if (!RaiseNavigating(new NavigatingCancelEventArgs(source, bringToFront ? NavigationMode.Refresh : NavigationMode.New, parameter, dataContext)))
                return false;

            bool clearBackStack = dataContext.GetData(NavigationConstants.ClearBackStack);
            _isReorder = bringToFront;
            _newDataContext = dataContext;

            var activity = PlatformExtensions.CurrentActivity;
            var context = activity ?? Application.Context;

            var intent = new Intent(context, source.ViewType);
            if (activity == null)
                intent.AddFlags(ActivityFlags.NewTask);
            else if (clearBackStack)
            {
                if (PlatformExtensions.IsApiLessThanOrEqualTo10)
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
                if (!clearBackStack && PlatformExtensions.IsApiGreaterThanOrEqualTo19)
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