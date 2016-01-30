#region Copyright

// ****************************************************************************
// <copyright file="MvvmActivityMediator.cs">
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Preferences;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Android.Binding.Models;
using MugenMvvmToolkit.Android.Interfaces.Mediators;
using MugenMvvmToolkit.Android.Interfaces.Navigation;
using MugenMvvmToolkit.Android.Models.EventArg;
using MugenMvvmToolkit.Android.Views;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Android.Infrastructure.Mediators
{
    public class MvvmActivityMediator : MediatorBase<Activity>, IMvvmActivityMediator, IHandler<MvvmActivityMediator.FinishActivityMessage>
    {
        #region Nested types

        public sealed class FinishActivityMessage
        {
            #region Fields

            public static readonly FinishActivityMessage Instance;

            public readonly IViewModel ViewModel;

            #endregion

            #region Constructors

            static FinishActivityMessage()
            {
                Instance = new FinishActivityMessage();
            }

            public FinishActivityMessage(IViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            private FinishActivityMessage()
            {
            }

            #endregion

            #region Methods

            public bool Finished { get; set; }

            #endregion
        }

        #endregion

        #region Fields

        private BindableMenuInflater _menuInflater;
        private LayoutInflater _layoutInflater;
        private IMenu _menu;
        private Bundle _bundle;
        private bool _isBackNavigation;
        private View _view;
        private bool _ignoreFinishNavigation;
        private bool _isStarted;
        private bool _isCreated;
        private IDictionary<string, object> _metadata;

        #endregion

        #region Constructors

        public MvvmActivityMediator([NotNull] Activity target)
            : base(target)
        {
            ServiceProvider.EventAggregator.Subscribe(this);
        }

        #endregion

        #region Implementation of IMvvmActivityMediator

        public virtual IDictionary<string, object> Metadata
        {
            get
            {
                if (_metadata == null)
                    Interlocked.CompareExchange(ref _metadata, new ConcurrentDictionary<string, object>(StringComparer.Ordinal), null);
                return _metadata;
            }
        }

        Activity IMvvmActivityMediator.Activity => Target;

        public virtual Bundle Bundle => _bundle;

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

        public virtual void OnCreate(int? viewId, Bundle savedInstanceState, Action<Bundle> baseOnCreate)
        {
            AndroidBootstrapperBase.EnsureInitialized();
            if (Tracer.TraceInformation)
                Tracer.Info("OnCreate activity({0})", Target);
            _bundle = savedInstanceState;
            OnCreate(savedInstanceState);

            baseOnCreate(savedInstanceState);
            _isCreated = true;

            var service = Get<INavigationService>();
            service.OnCreateActivity(Target);

            Created?.Invoke(Target, new ValueEventArgs<Bundle>(savedInstanceState));

            if (viewId.HasValue)
                Target.SetContentView(viewId.Value);
        }

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

        public override void OnSaveInstanceState(Bundle outState, Action<Bundle> baseOnSaveInstanceState)
        {
            SaveInstanceState?.Invoke(Target, new ValueEventArgs<Bundle>(outState));
            base.OnSaveInstanceState(outState, baseOnSaveInstanceState);
        }

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

        public override void OnDestroy(Action baseOnDestroy)
        {
            if (Tracer.TraceInformation)
                Tracer.Info("OnDestroy activity({0})", Target);
            ServiceProvider.EventAggregator.Unsubscribe(this);
            Destroyed?.Invoke(Target, EventArgs.Empty);
            _view.RemoveFromParent();
            _view.ClearBindingsRecursively(true, true);
            ThreadPool.QueueUserWorkItem(state => PlatformExtensions.CleanupWeakReferences());
            _view = null;

            if (_metadata != null)
            {
                _metadata.Clear();
                _metadata = null;
            }

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
            PlatformExtensions.SetCurrentActivity(Target, true);
            Target.ClearBindings(false, true);
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

        protected override PreferenceManager PreferenceManager
        {
            get
            {
                var activity = Target as PreferenceActivity;
                if (activity == null)
                    return null;
                return activity.PreferenceManager;
            }
        }

        public override void OnPause(Action baseOnPause)
        {
            var service = Get<INavigationService>();
            service.OnPauseActivity(Target);
            Paused?.Invoke(Target, EventArgs.Empty);
            base.OnPause(baseOnPause);
        }

        public virtual void OnRestart(Action baseOnRestart)
        {
            baseOnRestart();
            Restarted?.Invoke(Target, EventArgs.Empty);
        }

        public override void OnResume(Action baseOnResume)
        {
            base.OnResume(baseOnResume);

            var service = Get<INavigationService>();
            service.OnResumeActivity(Target);
            Resume?.Invoke(Target, EventArgs.Empty);
        }

        public virtual void OnStart(Action baseOnStart)
        {
            if (!_isStarted)
            {
                PlatformExtensions.NotifyActivityAttached(Target, _view);
                _isStarted = true;
            }
            baseOnStart();

            var service = Get<INavigationService>();
            service.OnStartActivity(Target);
            Started?.Invoke(Target, EventArgs.Empty);
        }

        public virtual void OnStop(Action baseOnStop)
        {
            baseOnStop();
            Stoped?.Invoke(Target, EventArgs.Empty);
        }

        public virtual void SetContentView(int layoutResId)
        {
            _view = Target.LayoutInflater.Inflate(layoutResId, null);
            Target.SetContentView(_view);
            _view = Target.FindViewById(global::Android.Resource.Id.Content) ?? _view;
            _view.RootView.ListenParentChange();
        }

        public virtual MenuInflater GetMenuInflater(MenuInflater baseMenuInflater)
        {
            if (_menuInflater == null)
                _menuInflater = PlatformExtensions.MenuInflaterFactory(Target, MugenMvvmToolkit.Models.DataContext.Empty);
            if (_menuInflater != null)
                _menuInflater.NestedMenuInflater = baseMenuInflater;
            return _menuInflater ?? baseMenuInflater;
        }

        public LayoutInflater GetLayoutInflater(LayoutInflater baseLayoutInflater)
        {
            if (!_isCreated)
                return baseLayoutInflater;
            if (_layoutInflater == null)
                _layoutInflater = PlatformExtensions.LayoutInflaterFactory(Target, null, null, baseLayoutInflater);
            return _layoutInflater ?? baseLayoutInflater;
        }

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

        public virtual void OnConfigurationChanged(Configuration newConfig, Action<Configuration> baseOnConfigurationChanged)
        {
            baseOnConfigurationChanged(newConfig);
            ConfigurationChanged?.Invoke(Target, new ValueEventArgs<Configuration>(newConfig));
        }

        public virtual void OnPostCreate(Bundle savedInstanceState, Action<Bundle> baseOnPostCreate)
        {
            PostCreate?.Invoke(Target, new ValueEventArgs<Bundle>(savedInstanceState));
            baseOnPostCreate(savedInstanceState);
        }

        public virtual bool OnOptionsItemSelected(IMenuItem item, Func<IMenuItem, bool> baseOnOptionsItemSelected)
        {
            var optionsItemSelected = OptionsItemSelected;
            if (optionsItemSelected == null)
                return baseOnOptionsItemSelected(item);
            return optionsItemSelected(item) || baseOnOptionsItemSelected(item);
        }

        public void OnActivityResult(Action<int, Result, Intent> baseOnActivityResult, int requestCode, Result resultCode, Intent data)
        {
            baseOnActivityResult(requestCode, resultCode, data);
            ActivityResult?.Invoke(Target, new ActivityResultEventArgs(requestCode, resultCode, data));
        }

        public virtual void AddPreferencesFromResource(Action<int> baseAddPreferencesFromResource, int preferencesResId)
        {
            var activity = Target as PreferenceActivity;
            if (activity == null)
            {
                Tracer.Error("The AddPreferencesFromResource method supported only for PreferenceActivity");
                return;
            }
            baseAddPreferencesFromResource(preferencesResId);
            InitializePreferences(activity.PreferenceScreen, preferencesResId);
        }

        void IHandler<FinishActivityMessage>.Handle(object sender, FinishActivityMessage message)
        {
            try
            {
                _ignoreFinishNavigation = true;
                if (message.ViewModel == null)
                {
                    if (PlatformExtensions.IsApiLessThanOrEqualTo10)
                        Target.Finish();
                }
                else if (ReferenceEquals(DataContext, message.ViewModel))
                {
                    Target.Finish();
                    message.Finished = true;
                }
            }
            finally
            {
                _ignoreFinishNavigation = false;
            }
        }

        public virtual Func<IMenuItem, bool> OptionsItemSelected { get; set; }

        public virtual event EventHandler<Activity, ValueEventArgs<Configuration>> ConfigurationChanged;

        public virtual event EventHandler<Activity, ValueEventArgs<Bundle>> PostCreate;

        public virtual event EventHandler<Activity, CancelEventArgs> BackPressing;

        public virtual event EventHandler<Activity, ValueEventArgs<Bundle>> Created;

        public virtual event EventHandler<Activity, EventArgs> Started;

        public virtual event EventHandler<Activity, EventArgs> Paused;

        public virtual event EventHandler<Activity, ValueEventArgs<Bundle>> SaveInstanceState;

        public virtual event EventHandler<Activity, EventArgs> Stoped;

        public virtual event EventHandler<Activity, EventArgs> Restarted;

        public virtual event EventHandler<Activity, EventArgs> Resume;

        public virtual event EventHandler<Activity, EventArgs> Destroyed;
        public event EventHandler<Activity, ActivityResultEventArgs> ActivityResult;

        #endregion
    }
}
