#region Copyright

// ****************************************************************************
// <copyright file="MvvmActivityMediator.cs">
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
using System.ComponentModel;
using Android.App;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Mediators;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.ViewModels;
using MugenMvvmToolkit.Views;

namespace MugenMvvmToolkit.Infrastructure.Mediators
{
    public class MvvmActivityMediator : MediatorBase<Activity>, IMvvmActivityMediator, IHandler<MvvmActivityMediator.FinishActivityMessage>
    {
        #region Nested types

        internal sealed class FinishActivityMessage
        {
            #region Fields

            public static readonly FinishActivityMessage Instance;

            #endregion

            #region Constructors

            static FinishActivityMessage()
            {
                Instance = new FinishActivityMessage();
            }

            private FinishActivityMessage()
            {
            }

            #endregion
        }

        #endregion

        #region Fields

        private BindableMenuInflater _menuInflater;
        private BindableLayoutInflater _layoutInflater;
        private IMenu _menu;
        private Bundle _bundle;
        private bool _isBackNavigation;
        private View _view;
        private bool _ignoreFinishNavigation;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MvvmActivityMediator" /> class.
        /// </summary>
        public MvvmActivityMediator([NotNull] Activity target)
            : base(target)
        {
            if (PlatformExtensions.IsApiLessThanOrEqualTo10)
                ServiceProvider.EventAggregator.Subscribe(this);
        }

        #endregion

        #region Implementation of IMvvmActivityMediator

        /// <summary>
        ///     Gets the <see cref="IMvvmActivityMediator.Activity" />.
        /// </summary>
        Activity IMvvmActivityMediator.Activity
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
                handler(Target, args);
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
        public virtual void OnCreate(Bundle savedInstanceState, Action<Bundle> baseOnCreate)
        {
            AndroidBootstrapperBase.EnsureInitialized();
            if (Tracer.TraceInformation)
                Tracer.Info("OnCreate activity({0})", Target);
            _bundle = savedInstanceState;
            OnCreate(savedInstanceState);

            var service = Get<INavigationService>();
            service.OnCreateActivity(Target);

            baseOnCreate(savedInstanceState);

            var handler = Created;
            if (handler != null)
                handler(Target, new ValueEventArgs<Bundle>(savedInstanceState));
        }

        /// <summary>
        ///     Initialize the contents of the Activity's standard options menu.
        /// </summary>
        public virtual bool OnCreateOptionsMenu(IMenu menu, Func<IMenu, bool> baseOnCreateOptionsMenu)
        {
            var optionsMenu = Target.FindViewById<OptionsMenu>(Resource.Id.OptionsMenu);
            if (optionsMenu != null)
            {
                _menu = menu;
                optionsMenu.Inflate(Target, menu);
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
                handler(Target, new ValueEventArgs<Bundle>(outState));
            base.OnSaveInstanceState(outState, baseOnSaveInstanceState);
        }

        /// <summary>
        ///     Tries to restore instance context.
        /// </summary>
        protected override void RestoreContext(Activity target, object dataContext)
        {
            base.RestoreContext(target, dataContext);
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
            if (Tracer.TraceInformation)
                Tracer.Info("OnDestroy activity({0})", Target);
            if (PlatformExtensions.IsApiLessThanOrEqualTo10)
                ServiceProvider.EventAggregator.Unsubscribe(this);
            var handler = Destroyed;
            if (handler != null)
                handler(Target, EventArgs.Empty);
            _view.RemoveFromParent();
            _view.ClearBindingsRecursively(true, true);
            _view = null;

            MenuTemplate.Clear(_menu);
            _menu = null;

            if (_menuInflater != null)
            {
                _menuInflater.Dispose();
                _menuInflater = null;
            }
            if (_layoutInflater != null)
            {
                _layoutInflater.Dispose();
                _layoutInflater = null;
            }
            base.OnDestroy(baseOnDestroy);
            PlatformExtensions.UpdateActivity(Target, true);
            Target.ClearBindings(false, true);
            Target.Dispose();
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
            service.OnPauseActivity(Target);
            baseOnPause();
            var handler = Paused;
            if (handler != null)
                handler(Target, EventArgs.Empty);
        }

        /// <summary>
        ///     Called after <c>OnStop</c> when the current activity is being re-displayed to the user (the user has navigated back to it).
        /// </summary>
        public virtual void OnRestart(Action baseOnRestart)
        {
            baseOnRestart();
            var handler = Restarted;
            if (handler != null)
                handler(Target, EventArgs.Empty);
        }

        /// <summary>
        ///     Called after <c>OnRestoreInstanceState(Android.OS.Bundle)</c>, <c>OnRestart</c>, or <c>OnPause</c>, for your activity to start interacting with the user.
        /// </summary>
        public virtual void OnResume(Action baseOnResume)
        {
            baseOnResume();
            var handler = Resume;
            if (handler != null)
                handler(Target, EventArgs.Empty);
        }

        /// <summary>
        ///     Called after <c>OnCreate(Android.OS.Bundle)</c> or after <c>OnRestart</c> when the activity had been stopped, but is now again being displayed to the user.
        /// </summary>
        public virtual void OnStart(Action baseOnStart)
        {
            baseOnStart();
            PlatformExtensions.UpdateActivity(Target, false);

            var service = Get<INavigationService>();
            service.OnStartActivity(Target);

            var handler = Started;
            if (handler != null)
                handler(Target, EventArgs.Empty);
        }

        /// <summary>
        ///     Called when you are no longer visible to the user.
        /// </summary>
        public virtual void OnStop(Action baseOnStop)
        {
            baseOnStop();
            var handler = Stoped;
            if (handler != null)
                handler(Target, EventArgs.Empty);
        }

        /// <summary>
        ///     Set the activity content from a layout resource.
        /// </summary>
        /// <param name="layoutResId">Resource ID to be inflated.</param>
        public virtual void SetContentView(int layoutResId)
        {
            _view = Target.LayoutInflater.Inflate(layoutResId, null);
            Target.SetContentView(_view);
            _view = Target.FindViewById(Android.Resource.Id.Content) ?? _view;
            _view.RootView.ListenParentChange();
            PlatformExtensions.NotifyActivityAttached(Target, _view);
        }

        /// <summary>
        ///     Returns a <c>MenuInflater</c> with this context.
        /// </summary>
        public virtual MenuInflater GetMenuInflater(MenuInflater baseMenuInflater)
        {
            if (_menuInflater == null)
                _menuInflater = PlatformExtensions.MenuInflaterFactory(Target, Models.DataContext.Empty);
            if (_menuInflater != null)
                _menuInflater.NestedMenuInflater = baseMenuInflater;
            return _menuInflater ?? baseMenuInflater;
        }

        /// <summary>
        ///     Returns a <c>LayoutInflater</c> with this context.
        /// </summary>
        public LayoutInflater GetLayoutInflater(LayoutInflater baseLayoutInflater)
        {
            if (_layoutInflater == null)
            {
                _layoutInflater = PlatformExtensions.LayoutInflaterFactory(Target, Models.DataContext.Empty, Get<IViewFactory>(), baseLayoutInflater);
                if (_layoutInflater != null)
                    _layoutInflater = new BindableLayoutInflaterProxy(_layoutInflater);
            }
            return _layoutInflater ?? baseLayoutInflater;
        }

        /// <summary>
        ///     Call this when your activity is done and should be closed.
        /// </summary>
        public virtual void Finish(Action baseFinish)
        {
            if (!_ignoreFinishNavigation)
            {
                var navigationService = Get<INavigationService>();
                if (!navigationService.OnFinishActivity(Target, _isBackNavigation))
                    return;
            }
            ClearContextCache();
            baseFinish();
        }

        /// <summary>
        ///     Called by the system when the device configuration changes while your activity is running.
        /// </summary>
        public virtual void OnConfigurationChanged(Configuration newConfig, Action<Configuration> baseOnConfigurationChanged)
        {
            baseOnConfigurationChanged(newConfig);
            var handler = ConfigurationChanged;
            if (handler != null)
                handler(Target, new ValueEventArgs<Configuration>(newConfig));
        }

        /// <summary>
        ///     Called when activity start-up is complete (after <c>OnStart</c> and <c>OnRestoreInstanceState</c> have been called).
        /// </summary>
        public virtual void OnPostCreate(Bundle savedInstanceState, Action<Bundle> baseOnPostCreate)
        {
            var handler = PostCreate;
            if (handler != null)
                handler(Target, new ValueEventArgs<Bundle>(savedInstanceState));
            baseOnPostCreate(savedInstanceState);
        }

        /// <summary>
        ///     This hook is called whenever an item in your options menu is selected.
        /// </summary>
        public virtual bool OnOptionsItemSelected(IMenuItem item, Func<IMenuItem, bool> baseOnOptionsItemSelected)
        {
            var optionsItemSelected = OptionsItemSelected;
            if (optionsItemSelected == null)
                return baseOnOptionsItemSelected(item);
            return optionsItemSelected(item) || baseOnOptionsItemSelected(item);
        }

        void IHandler<FinishActivityMessage>.Handle(object sender, FinishActivityMessage message)
        {
            try
            {
                _ignoreFinishNavigation = true;
                Target.Finish();
            }
            finally
            {
                _ignoreFinishNavigation = false;
            }
        }

        /// <summary>
        ///     This hook is called whenever an item in your options menu is selected.
        /// </summary>
        public virtual Func<IMenuItem, bool> OptionsItemSelected { get; set; }

        /// <summary>
        ///     Called by the system when the device configuration changes while your activity is running.
        /// </summary>
        public virtual event EventHandler<Activity, ValueEventArgs<Configuration>> ConfigurationChanged;

        /// <summary>
        ///     Called when activity start-up is complete (after <c>OnStart</c> and <c>OnRestoreInstanceState</c> have been called).
        /// </summary>
        public virtual event EventHandler<Activity, ValueEventArgs<Bundle>> PostCreate;

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