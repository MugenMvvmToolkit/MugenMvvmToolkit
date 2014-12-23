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
using System.ComponentModel;
using Android.App;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Mediators;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.ViewModels;
using MugenMvvmToolkit.Views;

namespace MugenMvvmToolkit.Infrastructure.Mediators
{
    public class MvvmActivityMediator : MediatorBase<Activity>, IMvvmActivityMediator
    {
        #region Fields

        private readonly MenuInflater _menuInflater;
        private IMenu _menu;
        private Bundle _bundle;
        private bool _isBackNavigation;
        private View _view;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
        /// </summary>
        public MvvmActivityMediator([NotNull] Activity target)
            : base(target)
        {
            _menuInflater = PlatformExtensions.MenuInflaterFactory(target, Models.DataContext.Empty);
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
            _isBackNavigation = true;
            baseOnBackPressed();
            _isBackNavigation = false;
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
            {
                _menu = menu;
                optionsMenu.Inflate(Activity, menu);
            }
            return baseOnCreateOptionsMenu(menu);
        }

        /// <summary>
        ///     Called after <c>OnCreate(Android.OS.Bundle)</c> or after <c>OnRestart</c> when the activity had been stopped, but is now again being displayed to the user.
        /// </summary>
        public override void OnSaveInstanceState(Bundle outState, Action<Bundle> baseOnSaveInstanceState)
        {
            var handler = SaveInstanceState;
            if (handler != null)
                handler(Activity, new ValueEventArgs<Bundle>(outState));
            base.OnSaveInstanceState(outState, baseOnSaveInstanceState);
        }

        /// <summary>
        ///     Tries to restore instance context.
        /// </summary>
        protected override void RestoreContext(object dataContext)
        {
            base.RestoreContext(dataContext);
            var viewModel = dataContext as IViewModel;
            if (viewModel != null)
            {
                var container = viewModel.GetIocContainer(true, false);
                if (container != null)
                {
                    //Tries to activate navigation provider.
                    INavigationProvider service;
                    container.TryGet(out service);
                }
            }
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
            _view.ClearBindingsHierarchically(true, true);
            _view = null;

            MenuTemplate.Clear(_menu);
            BindingContext.Value = null;
            base.OnDestroy(baseOnDestroy);
            OptionsItemSelected = null;
            ConfigurationChanged = null;
            PostCreate = null;
            BackPressing = null;
            Created = null;
            Started = null;
            Paused = null;
            SaveInstanceState = null;
            Stoped = null;
            Restarted = null;
            Resume = null;
            Destroyed = null;
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
        ///     Called after <c>OnStop</c> when the current activity is being re-displayed to the user (the user has navigated back to it).
        /// </summary>
        public virtual void OnRestart(Action baseOnRestart)
        {
            baseOnRestart();
            var handler = Restarted;
            if (handler != null)
                handler(Activity, EventArgs.Empty);
        }

        /// <summary>
        ///     Called after <c>OnRestoreInstanceState(Android.OS.Bundle)</c>, <c>OnRestart</c>, or <c>OnPause</c>, for your activity to start interacting with the user.
        /// </summary>
        public virtual void OnResume(Action baseOnResume)
        {
            baseOnResume();
            var handler = Resume;
            if (handler != null)
                handler(Activity, EventArgs.Empty);
        }

        /// <summary>
        ///     Called after <c>OnCreate(Android.OS.Bundle)</c> or after <c>OnRestart</c> when the activity had been stopped, but is now again being displayed to the user.
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
            _view = Activity.CreateBindableView(layoutResId, Get<IViewFactory>()).Item1;
            Activity.SetContentView(_view);
            _view = Activity.FindViewById(Android.Resource.Id.Content) ?? _view;
            _view.ListenParentChange();
            PlatformExtensions.NotifyActivityAttached(Activity, _view);
        }

        /// <summary>
        ///     Returns a <c>MenuInflater</c> with this context.
        /// </summary>
        public virtual MenuInflater GetMenuInflater(MenuInflater baseMenuInflater)
        {
            var menuInflater = _menuInflater as IBindableMenuInflater;
            if (menuInflater != null)
                menuInflater.MenuInflater = baseMenuInflater;
            return _menuInflater;
        }

        /// <summary>
        ///     Call this when your activity is done and should be closed.
        /// </summary>
        public virtual void Finish(Action baseFinish)
        {
            var navigationService = Get<INavigationService>();
            if (!navigationService.OnFinishActivity(Activity, _isBackNavigation))
                return;
            ClearContextCache();
            baseFinish();
        }

        /// <summary>
        ///     Called by the system when the device configuration changes while your activity is running.
        /// </summary>
        public void OnConfigurationChanged(Configuration newConfig, Action<Configuration> baseOnConfigurationChanged)
        {
            baseOnConfigurationChanged(newConfig);
            var handler = ConfigurationChanged;
            if (handler != null)
                handler(Activity, new ValueEventArgs<Configuration>(newConfig));
        }

        /// <summary>
        ///     Called when activity start-up is complete (after <c>OnStart</c> and <c>OnRestoreInstanceState</c> have been called).
        /// </summary>
        public void OnPostCreate(Bundle savedInstanceState, Action<Bundle> baseOnPostCreate)
        {
            var handler = PostCreate;
            if (handler != null)
                handler(Activity, new ValueEventArgs<Bundle>(savedInstanceState));
            baseOnPostCreate(savedInstanceState);
        }

        /// <summary>
        ///     This hook is called whenever an item in your options menu is selected.
        /// </summary>
        public bool OnOptionsItemSelected(IMenuItem item, Func<IMenuItem, bool> baseOnOptionsItemSelected)
        {
            var optionsItemSelected = OptionsItemSelected;
            if (optionsItemSelected == null)
                return baseOnOptionsItemSelected(item);
            return optionsItemSelected(item) || baseOnOptionsItemSelected(item);
        }

        /// <summary>
        ///     This hook is called whenever an item in your options menu is selected.
        /// </summary>
        public Func<IMenuItem, bool> OptionsItemSelected { get; set; }

        /// <summary>
        ///     Called by the system when the device configuration changes while your activity is running.
        /// </summary>
        public event EventHandler<Activity, ValueEventArgs<Configuration>> ConfigurationChanged;

        /// <summary>
        ///     Called when activity start-up is complete (after <c>OnStart</c> and <c>OnRestoreInstanceState</c> have been called).
        /// </summary>
        public event EventHandler<Activity, ValueEventArgs<Bundle>> PostCreate;

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