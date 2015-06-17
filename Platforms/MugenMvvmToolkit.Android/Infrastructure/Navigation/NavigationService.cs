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
using System.IO;
using Android.App;
using Android.Content;
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Infrastructure.Mediators;
using MugenMvvmToolkit.Android.Interfaces.Navigation;
using MugenMvvmToolkit.Android.Models.EventArg;
using MugenMvvmToolkit.Android.Views.Activities;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Android.Infrastructure.Navigation
{
    public class NavigationService : INavigationService
    {
        #region Fields

        private const string ParameterSerializer = "viewmodelparameterdata";
        private const string ParameterString = "viewmodelparameter";

        private readonly ISerializer _serializer;
        private bool _isBack;
        private bool _isNew;
        private object _parameter;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="NavigationService" /> class.
        /// </summary>
        public NavigationService([NotNull] ISerializer serializer)
        {
            Should.NotBeNull(serializer, "serializer");
            _serializer = serializer;
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Invokes the <see cref="Navigating" /> event.
        /// </summary>
        protected virtual bool RaiseNavigating(NavigatingCancelEventArgs args)
        {
            EventHandler<INavigationService, NavigatingCancelEventArgsBase> handler = Navigating;
            if (handler != null)
            {
                handler(this, args);
                return !args.Cancel;
            }
            return true;
        }

        /// <summary>
        ///     Invokes the <see cref="Navigated" /> event.
        /// </summary>
        protected virtual void RaiseNavigated(object content, NavigationMode mode, object parameter)
        {
            EventHandler<INavigationService, NavigationEventArgsBase> handler = Navigated;
            if (handler != null)
                handler(this, new NavigationEventArgs(content, parameter, mode));
        }

        private object GetParameterFromIntent(Intent intent)
        {
            if (intent == null)
                return null;
            string value = intent.GetStringExtra(ParameterString);
            if (value != null)
                return value;
            byte[] bytes = intent.GetByteArrayExtra(ParameterSerializer);
            if (bytes == null)
                return null;
            using (var ms = new MemoryStream(bytes))
                return _serializer.Deserialize(ms);
        }

        /// <summary>
        ///     Starts an activity.
        /// </summary>
        protected virtual void StartActivity(Context context, Intent intent, IDataContext dataContext)
        {
            context.StartActivity(intent);
        }

        #endregion

        #region Implementation of INavigationService

        /// <summary>
        ///     Indicates whether the navigator can navigate back.
        /// </summary>
        public virtual bool CanGoBack
        {
            get { return PlatformExtensions.CurrentActivity != null; }
        }

        /// <summary>
        ///     Indicates whether the navigator can navigate forward.
        /// </summary>
        public virtual bool CanGoForward
        {
            get { return false; }
        }

        /// <summary>
        ///     The current content.
        /// </summary>
        public virtual object CurrentContent
        {
            get { return PlatformExtensions.CurrentActivity; }
        }

        /// <summary>
        ///     Navigates back.
        /// </summary>
        public virtual void GoBack()
        {
            var currentActivity = PlatformExtensions.CurrentActivity;
            if (currentActivity != null)
                currentActivity.OnBackPressed();
        }

        /// <summary>
        ///     Navigates forward.
        /// </summary>
        public virtual void GoForward()
        {
            Should.MethodBeSupported(false, "GoForward()");
        }

        /// <summary>
        ///     Raised as part of the activity lifecycle when an activity is going into the background.
        /// </summary>
        public virtual void OnPauseActivity(Activity activity, IDataContext context = null)
        {
            Should.NotBeNull(activity, "activity");
            if (_isNew || _isBack || !ReferenceEquals(activity, PlatformExtensions.CurrentActivity))
                return;
            RaiseNavigating(NavigatingCancelEventArgs.NonCancelableEventArgs);
            RaiseNavigated(null, NavigationMode.New, null);
            PlatformExtensions.SetCurrentActivity(activity, true);
        }

        /// <summary>
        ///     Called when the activity had been stopped, but is now again being displayed to the user.
        /// </summary>
        public virtual void OnStartActivity(Activity activity, IDataContext context = null)
        {
            Should.NotBeNull(activity, "activity");
            if (ReferenceEquals(activity, PlatformExtensions.CurrentActivity))
                return;
            PlatformExtensions.SetCurrentActivity(activity, false);
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

        /// <summary>
        ///     Called when the activity is starting.
        /// </summary>
        public virtual void OnCreateActivity(Activity activity, IDataContext context = null)
        {
            OnStartActivity(activity);
        }

        /// <summary>
        ///     Call this when your activity is done and should be closed.
        /// </summary>
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

        /// <summary>
        ///     Gets a navigation parameter from event args.
        /// </summary>
        public virtual object GetParameterFromArgs(EventArgs args)
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

        /// <summary>
        ///     Navigates using cancel event args.
        /// </summary>
        public virtual bool Navigate(NavigatingCancelEventArgsBase args, IDataContext dataContext)
        {
            Should.NotBeNull(args, "args");
            if (!args.IsCancelable)
                return false;
            var eventArgs = ((NavigatingCancelEventArgs)args);
            if (eventArgs.NavigationMode != NavigationMode.Back && eventArgs.Mapping != null)
                return Navigate(eventArgs.Mapping, eventArgs.Parameter, dataContext);

            Activity activity = PlatformExtensions.CurrentActivity;
            if (activity == null)
                return false;
            GoBack();
            return activity.IsFinishing;
        }

        /// <summary>
        ///     Displays the content located at the specified <see cref="IViewMappingItem" />.
        /// </summary>
        /// <param name="source">
        ///     The <c>IViewPageMappingItem</c> of the content to display.
        /// </param>
        /// <param name="parameter">
        ///     A <see cref="T:System.Object" /> that contains data to be used for processing during
        ///     navigation.
        /// </param>
        /// <param name="dataContext">
        ///     The specified <see cref="IDataContext" />.
        /// </param>
        /// <returns>
        ///     <c>true</c> if the content was successfully displayed; otherwise, <c>false</c>.
        /// </returns>
        public virtual bool Navigate(IViewMappingItem source, object parameter, IDataContext dataContext)
        {
            Should.NotBeNull(source, "source");
            if (!RaiseNavigating(new NavigatingCancelEventArgs(source, NavigationMode.New, parameter)))
                return false;
            if (dataContext == null)
                dataContext = DataContext.Empty;
            Activity activity = PlatformExtensions.CurrentActivity ?? SplashScreenActivityBase.Current;
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
            {
                var s = parameter as string;
                if (s == null)
                {
                    using (Stream stream = _serializer.Serialize(parameter))
                        intent.PutExtra(ParameterSerializer, stream.ToArray());
                }
                else
                    intent.PutExtra(ParameterString, s);
            }
            _isNew = true;
            _parameter = parameter;
            StartActivity(context, intent, dataContext);
            return true;
        }

        /// <summary>
        ///     Determines whether the specified command <c>CloseCommand</c> can be execute.
        /// </summary>
        public virtual bool CanClose(IViewModel viewModel, IDataContext dataContext)
        {
            return true;
        }

        /// <summary>
        ///     Tries to close view-model page.
        /// </summary>
        public virtual bool TryClose(IViewModel viewModel, IDataContext dataContext)
        {
            Should.NotBeNull(viewModel, "viewModel");
            var message = new MvvmActivityMediator.FinishActivityMessage(viewModel);
            ServiceProvider.EventAggregator.Publish(this, message);
            return message.Finished;
        }

        /// <summary>
        ///     Raised prior to navigation.
        /// </summary>
        public virtual event EventHandler<INavigationService, NavigatingCancelEventArgsBase> Navigating;

        /// <summary>
        ///     Raised after navigation.
        /// </summary>
        public virtual event EventHandler<INavigationService, NavigationEventArgsBase> Navigated;

        #endregion
    }
}