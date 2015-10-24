#region Copyright

// ****************************************************************************
// <copyright file="NavigationService.cs">
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
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Android.Infrastructure.Mediators;
using MugenMvvmToolkit.Android.Interfaces.Navigation;
using MugenMvvmToolkit.Android.Interfaces.Views;
using MugenMvvmToolkit.Android.Models.EventArg;
using MugenMvvmToolkit.Android.Views.Activities;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
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
                    var application = Application.Context as Application;
                    if (application != null)
                        application.UnregisterActivityLifecycleCallbacks(this);
                    return false;
                }
                return true;
            }

            private void RaiseNew(Activity activity)
            {
                if (IsAlive() && !(activity is IActivityView) && _service.CurrentContent != activity)
                {
                    PlatformExtensions.SetCurrentActivity(activity, false);
                    _service.RaiseNavigated(activity, NavigationMode.New, null);
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

        private bool _isBack;
        private bool _isNew;
        private bool _isPause;
        private string _parameter;

        private const string ParameterString = "viewmodelparameter";

        #endregion

        #region Constructors

        public NavigationService()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.IceCreamSandwich)
            {
                var application = Application.Context as Application;
                if (application != null)
                    application.RegisterActivityLifecycleCallbacks(new ActivityLifecycleListener(this));
            }
        }

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

        protected virtual void RaiseNavigated(object content, NavigationMode mode, string parameter)
        {
            var handler = Navigated;
            if (handler != null)
                handler(this, new NavigationEventArgs(content, parameter, mode));
        }

        private static string GetParameterFromIntent(Intent intent)
        {
            if (intent == null)
                return null;
            return intent.GetStringExtra(ParameterString);
        }

        protected virtual void StartActivity(Context context, Intent intent, IDataContext dataContext)
        {
            var activity = context.GetActivity();
            Action<Context, Intent, IDataContext> startAction = null;
            if (activity != null)
                startAction = activity.GetBindingMemberValue(AttachedMembers.Activity.StartActivityDelegate);
            if (startAction == null)
                context.StartActivity(intent);
            else
                startAction(context, intent, dataContext);
        }

        #endregion

        #region Implementation of INavigationService

        public virtual bool CanGoBack
        {
            get { return PlatformExtensions.CurrentActivity != null; }
        }

        public virtual bool CanGoForward
        {
            get { return false; }
        }

        public virtual object CurrentContent
        {
            get
            {
                if (_isPause)
                    return null;
                return PlatformExtensions.CurrentActivity;
            }
        }

        public virtual void GoBack()
        {
            var currentActivity = PlatformExtensions.CurrentActivity;
            if (currentActivity != null)
                currentActivity.OnBackPressed();
        }

        public virtual void GoForward()
        {
            Should.MethodBeSupported(false, "GoForward()");
        }

        public virtual void OnPauseActivity(Activity activity, IDataContext context = null)
        {
            Should.NotBeNull(activity, "activity");
            if (_isNew || _isBack || !ReferenceEquals(activity, CurrentContent))
                return;
            _isPause = true;
            RaiseNavigating(NavigatingCancelEventArgs.NonCancelableEventArgs);
            RaiseNavigated(null, NavigationMode.New, null);
        }

        public virtual void OnResumeActivity(Activity activity, IDataContext context = null)
        {
            Should.NotBeNull(activity, "activity");
            if (ReferenceEquals(activity, CurrentContent))
                return;
            PlatformExtensions.SetCurrentActivity(activity, false);
            _isPause = false;
            if (_isNew)
            {
                _isNew = false;
                RaiseNavigated(activity, NavigationMode.New, _parameter);
                _parameter = null;
            }
            else
            {
                _isBack = false;
                RaiseNavigated(activity, NavigationMode.Back, GetParameterFromIntent(activity.Intent));
            }
        }

        public virtual void OnStartActivity(Activity activity, IDataContext context = null)
        {
            OnResumeActivity(activity);
        }

        public virtual void OnCreateActivity(Activity activity, IDataContext context = null)
        {
            OnResumeActivity(activity);
        }

        public virtual bool OnFinishActivity(Activity activity, bool isBackNavigation, IDataContext context = null)
        {
            Should.NotBeNull(activity, "activity");
            if (!isBackNavigation)
                return true;
            if (!RaiseNavigating(new NavigatingCancelEventArgs(NavigationMode.Back)))
                return false;
            //If it's the first activity, we need to raise the back navigation event.
            if (activity.IsTaskRoot)
                RaiseNavigated(null, NavigationMode.Back, null);
            _isBack = true;
            return true;
        }

        public virtual string GetParameterFromArgs(EventArgs args)
        {
            Should.NotBeNull(args, "args");
            var cancelArgs = args as NavigatingCancelEventArgs;
            if (cancelArgs == null)
            {
                var eventArgs = args as NavigationEventArgs;
                if (eventArgs == null)
                    return null;
                return eventArgs.Parameter;
            }
            return cancelArgs.Parameter;
        }

        public virtual bool Navigate(NavigatingCancelEventArgsBase args, IDataContext dataContext)
        {
            Should.NotBeNull(args, "args");
            if (!args.IsCancelable)
                return false;
            var eventArgs = ((NavigatingCancelEventArgs)args);
            if (eventArgs.NavigationMode != NavigationMode.Back && eventArgs.Mapping != null)
                return Navigate(eventArgs.Mapping, eventArgs.Parameter, dataContext);

            var activity = PlatformExtensions.CurrentActivity;
            if (activity == null)
                return false;
            GoBack();
            return activity.IsFinishing;
        }

        public virtual bool Navigate(IViewMappingItem source, string parameter, IDataContext dataContext)
        {
            Should.NotBeNull(source, "source");
            if (!RaiseNavigating(new NavigatingCancelEventArgs(source, NavigationMode.New, parameter)))
                return false;
            if (dataContext == null)
                dataContext = DataContext.Empty;
            var activity = PlatformExtensions.CurrentActivity ?? SplashScreenActivityBase.Current;
            var context = activity ?? Application.Context;

            var intent = new Intent(context, source.ViewType);
            if (activity == null)
                intent.AddFlags(ActivityFlags.NewTask);
            else if (dataContext.GetData(NavigationConstants.ClearBackStack))
            {
                if (PlatformExtensions.IsApiLessThanOrEqualTo10)
                {
                    intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);
                    ServiceProvider.EventAggregator.Publish(this, MvvmActivityMediator.FinishActivityMessage.Instance);
                }
                else
                    intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTask);
                dataContext.AddOrUpdate(NavigationProvider.ClearNavigationCache, true);
            }
            if (parameter != null)
                intent.PutExtra(ParameterString, parameter);
            _isNew = true;
            _parameter = parameter;
            StartActivity(context, intent, dataContext);
            return true;
        }

        public virtual bool CanClose(IViewModel viewModel, IDataContext dataContext)
        {
            return true;
        }

        public virtual bool TryClose(IViewModel viewModel, IDataContext dataContext)
        {
            Should.NotBeNull(viewModel, "viewModel");
            if (CurrentContent != null && CurrentContent.DataContext() == viewModel)
            {
                GoBack();
                //Ignore close just in case there backstack fragments.
                return false;
            }
            var message = new MvvmActivityMediator.FinishActivityMessage(viewModel);
            ServiceProvider.EventAggregator.Publish(this, message);
            return message.Finished;
        }

        public virtual event EventHandler<INavigationService, NavigatingCancelEventArgsBase> Navigating;

        public virtual event EventHandler<INavigationService, NavigationEventArgsBase> Navigated;

        #endregion
    }
}
