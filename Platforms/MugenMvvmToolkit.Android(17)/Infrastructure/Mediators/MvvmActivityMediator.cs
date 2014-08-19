#region Copyright
// ****************************************************************************
// <copyright file="MvvmActivityMediator.cs">
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
using System.Collections.Generic;
using System.ComponentModel;
using Android.App;
using Android.OS;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Mediators;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Views;

namespace MugenMvvmToolkit.Infrastructure.Mediators
{
    public class MvvmActivityMediator : MediatorBase<Activity>, IMvvmActivityMediator
    {
        #region Fields

        private readonly BindableMenuInflater _menuInflater;
        private IList<IDataBinding> _bindings;
        private Bundle _bundle;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        public MvvmActivityMediator([NotNull] Activity target)
            : base(target)
        {
            _menuInflater = new BindableMenuInflater(target);
        }

        #endregion

        #region Implementation of IMvvmActivityMediator

        /// <summary>
        ///     Gets the <see cref="IMvvmActivityMediator.Activity" />.
        /// </summary>
        public Activity Activity
        {
            get { return Target; }
        }

        /// <summary>
        /// Gets the current bundle.
        /// </summary>
        public virtual Bundle Bundle
        {
            get { return _bundle; }
        }

        /// <summary>
        ///     Called when the activity has detected the user's press of the back key.
        /// </summary>
        public virtual void OnBackPressed(Action baseOnBackPressed)
        {
            var handler = BackPressing;
            if (handler != null)
            {
                var args = new CancelEventArgs();
                handler(Activity, args);
                if (args.Cancel)
                    return;
            }
            var service = Get<INavigationService>();
            service.GoBack();
        }

        /// <summary>
        ///     Called when the activity is starting.
        /// </summary>
        public void OnCreate(Bundle savedInstanceState, Action<Bundle> baseOnCreate)
        {
            AndroidBootstrapperBase.EnsureInitialized();
            Tracer.Info("OnCreate activity({0})", Target);
            _bundle = savedInstanceState;

            OnCreate(savedInstanceState);

            var service = Get<INavigationService>();
            service.OnCreateActivity(Activity);

            baseOnCreate(savedInstanceState);

            var stateManager = Get<IApplicationStateManager>();
            stateManager.OnCreateActivity(Activity, savedInstanceState);

            var handler = Created;
            if (handler != null)
                handler(Activity, new ValueEventArgs<Bundle>(savedInstanceState));
        }

        /// <summary>
        ///     Initialize the contents of the Activity's standard options menu.
        /// </summary>
        public virtual bool OnCreateOptionsMenu(IMenu menu, Func<IMenu, bool> baseOnCreateOptionsMenu)
        {
            var optionsMenu = Activity.FindViewById<OptionsMenu>(Resource.Id.OptionsMenu);
            if (optionsMenu != null)
                optionsMenu.Inflate(Activity, menu);
            return baseOnCreateOptionsMenu(menu);
        }

        /// <summary>
        ///     Called to ask the view to save its current dynamic state, so it
        ///     can later be reconstructed in a new instance of its process is
        ///     restarted.
        /// </summary>
        public override void OnSaveInstanceState(Bundle outState, Action<Bundle> baseOnSaveInstanceState)
        {
            var stateManager = Get<IApplicationStateManager>();
            stateManager.OnSaveInstanceStateActivity(Activity, outState);

            var handler = SaveInstanceState;
            if (handler != null)
                handler(Activity, new ValueEventArgs<Bundle>(outState));
            base.OnSaveInstanceState(outState, baseOnSaveInstanceState);
        }

        /// <summary>
        ///     Perform any final cleanup before an activity is destroyed.
        /// </summary>
        public override void OnDestroy(Action baseOnDestroy)
        {
            Tracer.Info("OnDestroy activity({0})", Target);
            var handler = Destroyed;
            if (handler != null)
                handler(Activity, EventArgs.Empty);
            if (_bindings != null)
            {
                foreach (IDataBinding dataBinding in _bindings)
                    dataBinding.Dispose();
                _bindings = null;
            }
            base.OnDestroy(baseOnDestroy);
        }

        /// <summary>
        ///     Called as part of the activity lifecycle when an activity is going into
        ///     the background, but has not (yet) been killed.
        /// </summary>
        public virtual void OnPause(Action baseOnPause)
        {
            var service = Get<INavigationService>();
            service.OnPauseActivity(Activity);
            baseOnPause();
            var handler = Paused;
            if (handler != null)
                handler(Activity, EventArgs.Empty);
        }

        /// <summary>
        ///     Called after
        ///     <c>
        ///         <see cref="M:Android.App.Activity.OnStop" />
        ///     </c>
        ///     when the current activity is being
        ///     re-displayed to the user (the user has navigated back to it).
        /// </summary>
        public virtual void OnRestart(Action baseOnRestart)
        {
            baseOnRestart();
            var handler = Restarted;
            if (handler != null)
                handler(Activity, EventArgs.Empty);
        }

        /// <summary>
        ///     Called after
        ///     <c>
        ///         <see cref="M:Android.App.Activity.OnRestoreInstanceState(Android.OS.Bundle)" />
        ///     </c>
        ///     ,
        ///     <c>
        ///         <see cref="M:Android.App.Activity.OnRestart" />
        ///     </c>
        ///     , or
        ///     <c>
        ///         <see cref="M:Android.App.Activity.OnPause" />
        ///     </c>
        ///     , for your activity to start interacting with the user.
        /// </summary>
        public virtual void OnResume(Action baseOnResume)
        {
            baseOnResume();
            var handler = Resume;
            if (handler != null)
                handler(Activity, EventArgs.Empty);
        }

        /// <summary>
        ///     Called after
        ///     <c>
        ///         <see cref="M:Android.App.Activity.OnCreate(Android.OS.Bundle)" />
        ///     </c>
        ///     &amp;mdash; or after
        ///     <c>
        ///         <see cref="M:Android.App.Activity.OnRestart" />
        ///     </c>
        ///     when
        ///     the activity had been stopped, but is now again being displayed to the
        ///     user.
        /// </summary>
        public virtual void OnStart(Action baseOnStart)
        {
            var service = Get<INavigationService>();
            service.OnStartActivity(Activity);

            baseOnStart();
            var handler = Started;
            if (handler != null)
                handler(Activity, EventArgs.Empty);
        }

        /// <summary>
        ///     Called when you are no longer visible to the user.
        /// </summary>
        public virtual void OnStop(Action baseOnStop)
        {
            baseOnStop();
            var handler = Stoped;
            if (handler != null)
                handler(Activity, EventArgs.Empty);
        }

        /// <summary>
        ///     Set the activity content from a layout resource.
        /// </summary>
        /// <param name="layoutResId">Resource ID to be inflated.</param>
        public virtual void SetContentView(int layoutResId)
        {
            var bindableView = Activity.CreateBindableView(layoutResId, Get<IViewFactory>());
            _bindings = bindableView.Item2;
            Activity.SetContentView(bindableView.Item1);
            var view = Activity.FindViewById(Android.Resource.Id.Content) ?? bindableView.Item1;
            view.ListenParentChange();
        }

        /// <summary>
        ///     Returns a
        ///     <c>
        ///         <see cref="T:Android.Views.MenuInflater" />
        ///     </c>
        ///     with this context.
        /// </summary>
        public virtual MenuInflater GetMenuInflater(MenuInflater baseMenuInflater)
        {
            _menuInflater.MenuInflater = baseMenuInflater;
            return _menuInflater;
        }

        /// <summary>
        ///     Call this when your activity is done and should be closed.
        /// </summary>
        public virtual void Finish(Action baseFinish)
        {
            ClearContextCache();
            baseFinish();
        }

        /// <summary>
        /// Occurs when the activity has detected the user's press of the back key.
        /// </summary>
        public virtual event EventHandler<Activity, CancelEventArgs> BackPressing;

        /// <summary>
        ///     Occurred on created activity.
        /// </summary>
        public virtual event EventHandler<Activity, ValueEventArgs<Bundle>> Created;

        /// <summary>
        ///     Occurred on started activity.
        /// </summary>
        public virtual event EventHandler<Activity, EventArgs> Started;

        /// <summary>
        ///     Occurred on paused activity.
        /// </summary>
        public virtual event EventHandler<Activity, EventArgs> Paused;

        /// <summary>
        ///     Occurred on save activity state.
        /// </summary>
        public virtual event EventHandler<Activity, ValueEventArgs<Bundle>> SaveInstanceState;

        /// <summary>
        ///     Occurred on stoped activity.
        /// </summary>
        public virtual event EventHandler<Activity, EventArgs> Stoped;

        /// <summary>
        ///     Occurred on restarted activity.
        /// </summary>
        public virtual event EventHandler<Activity, EventArgs> Restarted;

        /// <summary>
        ///     Occurred on resume activity.
        /// </summary>
        public virtual event EventHandler<Activity, EventArgs> Resume;

        /// <summary>
        ///     Occurred on destroyed activity.
        /// </summary>
        public virtual event EventHandler<Activity, EventArgs> Destroyed;

        #endregion
    }
}