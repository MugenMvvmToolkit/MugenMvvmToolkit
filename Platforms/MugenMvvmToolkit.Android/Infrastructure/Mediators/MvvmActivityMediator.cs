#region Copyright

// ****************************************************************************
// <copyright file="MvvmActivityMediator.cs">
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Binding.Models;
using MugenMvvmToolkit.Android.Infrastructure.Navigation;
using MugenMvvmToolkit.Android.Interfaces.Mediators;
using MugenMvvmToolkit.Android.Interfaces.Navigation;
using MugenMvvmToolkit.Android.Interfaces.Views;
using MugenMvvmToolkit.Android.Models.EventArg;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.ViewModels;

namespace MugenMvvmToolkit.Android.Infrastructure.Mediators
{
    public class MvvmActivityMediator : MediatorBase<Activity>, IMvvmActivityMediator, IHandler<MvvmActivityMediator.FinishActivityMessage>,
        IHandler<MvvmActivityMediator.CanFinishActivityMessage>
    {
        #region Nested types

        public sealed class CanFinishActivityMessage
        {
            #region Fields

            public readonly IViewModel ViewModel;

            #endregion

            #region Constructors

            public CanFinishActivityMessage(IViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            #endregion

            #region Methods

            public bool CanFinish { get; set; }

            #endregion
        }

        public sealed class FinishActivityMessage
        {
            #region Fields

            public readonly IViewModel ViewModel;

            #endregion

            #region Constructors

            public FinishActivityMessage(IViewModel viewModel = null)
            {
                ViewModel = viewModel;
            }

            #endregion

            #region Methods

            public bool IsFinished => !FinishedViewModels.IsNullOrEmpty();

            public List<IViewModel> FinishedViewModels { get; private set; }

            public IList<IViewModel> IgnoredViewModels { get; set; }

            public bool CanFinish(MvvmActivityMediator mediator)
            {
                if (IgnoredViewModels != null)
                {
                    var viewModel = mediator.DataContext as IViewModel;
                    if (viewModel != null && IgnoredViewModels.Contains(viewModel))
                        return false;
                }
                return ViewModel == null || ReferenceEquals(mediator.DataContext, ViewModel);
            }

            public void AddFinishedViewModel(IViewModel viewModel)
            {
                if (viewModel == null)
                    return;
                if (FinishedViewModels == null)
                    FinishedViewModels = new List<IViewModel>();
                FinishedViewModels.Add(viewModel);
            }

            #endregion
        }

        #endregion

        #region Fields

        public const string IntentViewModelIdKey = "~vm_id~";
        private MenuInflater _menuInflater;
        private LayoutInflater _layoutInflater;
        private IMenu _menu;
        private Bundle _savedInstanceState;
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
            ToolkitServiceProvider.EventAggregator.Subscribe(this);
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

        public virtual Bundle SavedInstanceState => _savedInstanceState;

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
            AndroidBootstrapperBase.EnsureInitialized(Target, savedInstanceState);
            if (Tracer.TraceInformation)
                Tracer.Info("OnCreate activity({0})", Target);
            _savedInstanceState = savedInstanceState;
            OnCreate(savedInstanceState);

            baseOnCreate(savedInstanceState);
            _isCreated = true;

            var service = Get<INavigationService>();
            service.OnCreateActivity(Target, NavigationContext);

            Created?.Invoke(Target, new ValueEventArgs<Bundle>(savedInstanceState));

            if (viewId.HasValue)
                Target.SetContentView(viewId.Value);
        }

        public virtual void OnNewIntent(Intent intent, Action<Intent> baseOnNewIntent)
        {
            baseOnNewIntent(intent);
            //note android doesn't support reorder activity by instance it will reorder it using type, we need to manually change view model if need
            var oldViewModel = DataContext as IViewModel;
            Guid id;
            if (oldViewModel != null && Guid.TryParse(intent.GetStringExtra(IntentViewModelIdKey), out id))
            {
                var newViewModel = ToolkitServiceProvider.ViewModelProvider.TryGetViewModelById(id);
                if (newViewModel != null && !ReferenceEquals(newViewModel, oldViewModel))
                {
                    var navigationDispatcher = ToolkitServiceProvider.Get<INavigationDispatcher>();
                    var openedViewModels = navigationDispatcher.GetOpenedViewModels(NavigationType.Page).ToList();
                    var viewModels = openedViewModels
                        .Select(info => info.ViewModel)
                        .Where(vm => vm.GetCurrentView<object>()?.GetType() == Target.GetType())
                        .ToList();

                    var from = viewModels.IndexOf(newViewModel);
                    var to = viewModels.IndexOf(oldViewModel);

                    if (from < 0 || to < 0)
                        return;

                    for (int i = to; i > from; i--)
                    {
                        var viewModel = viewModels[i];
                        var currentView = viewModels[i - 1].GetCurrentView<object>();
                        if (currentView != null)
                        {
                            ToolkitServiceProvider.ViewManager.InitializeViewAsync(viewModel, currentView);
                            var activity = currentView as Activity;
                            if (activity != null)
                            {
                                var newIntent = new Intent(activity.Intent);
                                newIntent.PutExtra(NavigationService.IntentParameterKey, NavigationProvider.GenerateNavigationParameter(viewModel));
                                activity.Intent = newIntent;
                            }
                        }
                    }
                    var newParameter = intent.GetStringExtra(NavigationService.IntentParameterKey);
                    var oldParameter = Target.Intent.GetStringExtra(NavigationService.IntentParameterKey);
                    if (newParameter != null && !string.Equals(newParameter, oldParameter))
                    {
                        var newIntent = new Intent(Target.Intent);
                        newIntent.PutExtra(NavigationService.IntentParameterKey, newParameter);
                        Target.Intent = newIntent;
                    }
                    ToolkitServiceProvider.ViewManager.InitializeViewAsync(newViewModel, Target);

                    int freeIndex = -1;
                    for (int i = 0; i < openedViewModels.Count; i++)
                    {
                        var viewModelInfo = openedViewModels[i];
                        if (freeIndex == -1 && ReferenceEquals(viewModelInfo.ViewModel, newViewModel))
                        {
                            ++i;
                            freeIndex = i;
                            openedViewModels.Insert(i, null);
                            continue;
                        }
                        if (freeIndex != -1 && viewModelInfo.ViewModel.GetCurrentView<object>()?.GetType() == Target.GetType())
                        {
                            openedViewModels[freeIndex] = viewModelInfo;
                            openedViewModels[i] = null;
                            freeIndex = i;
                        }
                    }
                    if (freeIndex != -1)
                    {
                        openedViewModels.RemoveAt(freeIndex);
                        navigationDispatcher.UpdateOpenedViewModels(NavigationType.Page, openedViewModels);
                    }
                }
            }
        }

        public virtual bool OnCreateOptionsMenu(IMenu menu, Func<IMenu, bool> baseOnCreateOptionsMenu)
        {
            var optionsMenu = Target.FindViewById(Resource.Id.OptionsMenu) as IOptionsMenu;
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
            outState.PutString(AndroidBootstrapperBase.BootTypeKey, AndroidBootstrapperBase.BootstrapperType);
            base.OnSaveInstanceState(outState, baseOnSaveInstanceState);
        }

        public override void OnDestroy(Action baseOnDestroy)
        {
            if (Tracer.TraceInformation)
                Tracer.Info($"OnDestroy activity({Target})");
            ToolkitServiceProvider.EventAggregator.Unsubscribe(this);
            Destroyed?.Invoke(Target, EventArgs.Empty);
            _view.ClearBindingsRecursively(true, true, AndroidToolkitExtensions.AggressiveViewCleanup);
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
            ToolkitServiceProvider.Get<INavigationService>().OnDestroyActivity(Target, NavigationContext);
            base.OnDestroy(baseOnDestroy);
            ThreadPool.QueueUserWorkItem(state => AndroidToolkitExtensions.CleanupWeakReferences(true));
            AndroidToolkitExtensions.SetCurrentActivity(Target, true);
            Target.ClearBindings(false, true);
            OptionsItemSelected = null;
            ActivityResult = null;
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

        public override void OnPause(Action baseOnPause)
        {
            var service = Get<INavigationService>();
            service.OnPauseActivity(Target, NavigationContext);
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
            service.OnResumeActivity(Target, NavigationContext);
            Resume?.Invoke(Target, EventArgs.Empty);
        }

        public virtual void OnStart(Action baseOnStart)
        {
            if (!_isStarted)
            {
                AndroidToolkitExtensions.NotifyActivityAttached(Target, _view);
                _isStarted = true;
            }
            baseOnStart();

            var service = Get<INavigationService>();
            service.OnStartActivity(Target, NavigationContext);
            Started?.Invoke(Target, EventArgs.Empty);
        }

        public virtual void OnStop(Action baseOnStop)
        {
            baseOnStop();
            Stoped?.Invoke(Target, EventArgs.Empty);
        }

        public virtual void SetContentView(int layoutResId)
        {
            var result = Target.LayoutInflater.InflateEx(layoutResId, null, false);
            _view = result.View;
            Target.SetContentView(_view);
            result.ApplyBindings();
        }

        public virtual MenuInflater GetMenuInflater(MenuInflater baseMenuInflater)
        {
            if (_menuInflater == null)
                _menuInflater = AndroidToolkitExtensions.MenuInflaterFactory(Target, baseMenuInflater, MugenMvvmToolkit.Models.DataContext.Empty);
            return _menuInflater ?? baseMenuInflater;
        }

        public LayoutInflater GetLayoutInflater(LayoutInflater baseLayoutInflater)
        {
            if (!_isCreated)
                return baseLayoutInflater;
            if (_layoutInflater == null)
                _layoutInflater = AndroidToolkitExtensions.LayoutInflaterFactory(Target, null, null, baseLayoutInflater);
            return _layoutInflater ?? baseLayoutInflater;
        }

        public virtual void Finish(Action baseFinish)
        {
            if (!_ignoreFinishNavigation)
            {
                var navigationService = Get<INavigationService>();
                if (!navigationService.OnFinishActivity(Target, _isBackNavigation, NavigationContext))
                    return;
            }
            ClearContextCache();
            baseFinish();
        }

        public virtual void FinishAfterTransition(Action baseFinishAfterTransition)
        {
            if (!_ignoreFinishNavigation)
            {
                var navigationService = Get<INavigationService>();
                if (!navigationService.OnFinishActivity(Target, _isBackNavigation, NavigationContext))
                    return;
                _ignoreFinishNavigation = true;
            }
            baseFinishAfterTransition();
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

        public virtual void OnActivityResult(Action<int, Result, Intent> baseOnActivityResult, int requestCode, Result resultCode, Intent data)
        {
            baseOnActivityResult(requestCode, resultCode, data);
            ActivityResult?.Invoke(Target, new ActivityResultEventArgs(requestCode, resultCode, data));
        }

        public virtual void AddPreferencesFromResource(Action<int> baseAddPreferencesFromResource, int preferencesResId)
        {
            throw new NotSupportedException();
        }

        void IHandler<FinishActivityMessage>.Handle(object sender, FinishActivityMessage message)
        {
            try
            {
                _ignoreFinishNavigation = true;
                if (message.CanFinish(this))
                {
                    Target.Finish();
                    message.AddFinishedViewModel(DataContext as IViewModel);
                }
            }
            finally
            {
                _ignoreFinishNavigation = false;
            }
        }

        void IHandler<CanFinishActivityMessage>.Handle(object sender, CanFinishActivityMessage message)
        {
            if (message.ViewModel == null || ReferenceEquals(DataContext, message.ViewModel))
                message.CanFinish = true;
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

        public virtual event EventHandler<Activity, ActivityResultEventArgs> ActivityResult;

        #endregion
    }
}