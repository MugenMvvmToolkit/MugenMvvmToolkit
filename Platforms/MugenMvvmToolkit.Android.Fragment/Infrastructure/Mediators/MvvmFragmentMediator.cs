#region Copyright
// ****************************************************************************
// <copyright file="MvvmFragmentMediator.cs">
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
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Mediators;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Views;
#if APPCOMPAT
using MugenMvvmToolkit.AppCompat.Interfaces.Mediators;
using MugenMvvmToolkit.AppCompat.Interfaces.Views;
using MugenMvvmToolkit.AppCompat.Infrastructure.Presenters;
using Fragment = Android.Support.V4.App.Fragment;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;

namespace MugenMvvmToolkit.AppCompat.Infrastructure.Mediators
#else
using MugenMvvmToolkit.FragmentSupport.Interfaces.Mediators;
using MugenMvvmToolkit.FragmentSupport.Interfaces.Views;
using MugenMvvmToolkit.FragmentSupport.Infrastructure.Presenters;

namespace MugenMvvmToolkit.FragmentSupport.Infrastructure.Mediators
#endif
{
    public class MvvmFragmentMediator : MediatorBase<Fragment>, IMvvmFragmentMediator
    {
        #region Nested types

        private sealed class DialogInterfaceOnKeyListener : Java.Lang.Object, IDialogInterfaceOnKeyListener
        {
            #region Fields

            private readonly MvvmFragmentMediator _mediator;

            #endregion

            #region Constructors

            public DialogInterfaceOnKeyListener(MvvmFragmentMediator mediator)
            {
                _mediator = mediator;
            }

            #endregion

            #region Implementation of IDialogInterfaceOnKeyListener

            bool IDialogInterfaceOnKeyListener.OnKey(IDialogInterface dialog, Keycode keyCode, KeyEvent e)
            {
                if (keyCode != Keycode.Back || e.Action != KeyEventActions.Up)
                    return false;
                _mediator._dialogFragment.Dismiss();
                return true;
            }

            #endregion
        }

        #endregion

        #region Fields

        private readonly DialogFragment _dialogFragment;
        private DialogInterfaceOnKeyListener _keyListener;
        private View _view;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="MediatorBase{TTarget}" /> class.
        /// </summary>
        public MvvmFragmentMediator([NotNull] Fragment target)
            : base(target)
        {
            _dialogFragment = target as DialogFragment;
        }

        #endregion

        #region Implementation of IMvvmFragmentMediator

        /// <summary>
        ///     Gets the <see cref="Fragment" />.
        /// </summary>
        Fragment IMvvmFragmentMediator.Fragment
        {
            get { return Target; }
        }

        /// <summary>
        ///     Called when a fragment is first attached to its activity.
        /// </summary>
        public virtual void OnAttach(Activity activity, Action<Activity> baseOnAttach)
        {
            baseOnAttach(activity);
        }

        /// <summary>
        ///     Initialize the contents of the Activity's standard options menu.
        /// </summary>
        public virtual void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater,
            Action<IMenu, MenuInflater> baseOnCreateOptionsMenu)
        {
            if (Target.Activity == null || Target.View == null)
                baseOnCreateOptionsMenu(menu, inflater);
            else
            {
                var optionsMenu = Target.View.FindViewById<OptionsMenu>(Resource.Id.OptionsMenu);
                if (optionsMenu != null)
                    optionsMenu.Inflate(Target.Activity, menu);
            }
        }

        /// <summary>
        ///     Called to have the fragment instantiate its user interface view.
        /// </summary>
        public virtual View OnCreateView(int? viewId, LayoutInflater inflater, ViewGroup container,
            Bundle savedInstanceState, Func<LayoutInflater, ViewGroup, Bundle, View> baseOnCreateView)
        {
            _view.ClearBindingsHierarchically(true, true);
            if (viewId.HasValue)
            {
                _view = inflater.CreateBindableView(viewId.Value, container, false).Item1;
                FragmentExtensions.FragmentViewMember.SetValue(_view, Target);
                BindingServiceProvider.ContextManager.GetBindingContext(_view).Value = DataContext;
                return _view;
            }
            return baseOnCreateView(inflater, container, savedInstanceState);
        }

        /// <summary>
        ///     Called when the target is starting.
        /// </summary>
        public void OnCreate(Bundle savedInstanceState, Action<Bundle> baseOnCreate)
        {
            Tracer.Info("OnCreate fragment({0})", Target);
            OnCreate(savedInstanceState);
            baseOnCreate(savedInstanceState);

            var viewModel = BindingContext.Value as IViewModel;
            if (viewModel != null)
            {
                if (!viewModel.Settings.Metadata.Contains(ViewModelConstants.StateNotNeeded) && !viewModel.Settings.Metadata.Contains(ViewModelConstants.StateManager))
                    viewModel.Settings.Metadata.AddOrUpdate(ViewModelConstants.StateManager, this);
                viewModel.Settings.Metadata.AddOrUpdate(PlatformExtensions.CurrentFragment, Target);
            }
        }

        /// <summary>
        ///     Called immediately after <c>OnCreateView(Android.Views.LayoutInflater, Android.Views.ViewGroup, Android.Views.ViewGroup)</c> has returned, but before any saved state has been restored in to the view.
        /// </summary>
        public virtual void OnViewCreated(View view, Bundle savedInstanceState, Action<View, Bundle> baseOnViewCreated)
        {
            if (_dialogFragment == null)
                PlatformExtensions.NotifyActivityAttached(Target.Activity, view);
            else
            {
                var dialog = _dialogFragment.Dialog;
                if (dialog != null)
                {
                    TrySetTitleBinding();
                    if (_keyListener == null)
                        _keyListener = new DialogInterfaceOnKeyListener(this);
                    dialog.SetOnKeyListener(_keyListener);
                }
            }
            baseOnViewCreated(view, savedInstanceState);
        }

        /// <summary>
        ///     Called when the fragment is no longer in use.
        /// </summary>
        public override void OnDestroy(Action baseOnDestroy)
        {
            Tracer.Info("OnDestroy fragment({0})", Target);
            var handler = Destroyed;
            if (handler != null)
                handler((IWindowView)Target, EventArgs.Empty);

            _view.ClearBindingsHierarchically(true, true);
            if (_dialogFragment != null)
                _dialogFragment.Dialog.ClearBindings(true, true);
            Target.ClearBindings(false, true);

            var viewModel = BindingContext.Value as IViewModel;
            if (viewModel != null)
            {
                viewModel.Settings.Metadata.Remove(PlatformExtensions.CurrentFragment);
                object stateManager;
                if (viewModel.Settings.Metadata.TryGetData(ViewModelConstants.StateManager, out stateManager) &&
                    stateManager == this)
                    viewModel.Settings.Metadata.Remove(ViewModelConstants.StateManager);
            }
            _view = null;
            BindingContext.Value = null;
            base.OnDestroy(baseOnDestroy);
            Closing = null;
            Canceled = null;
            Destroyed = null;
        }

        /// <summary>
        ///     Called when the fragment is no longer attached to its activity.
        /// </summary>
        public virtual void OnDetach(Action baseOnDetach)
        {
            baseOnDetach();
        }

        /// <summary>
        ///     Called when a fragment is being created as part of a view layout
        ///     inflation, typically from setting the content view of an activity.
        /// </summary>
        public virtual void OnInflate(Activity activity, IAttributeSet attrs, Bundle savedInstanceState,
            Action<Activity, IAttributeSet, Bundle> baseOnInflate)
        {
            Target.ClearBindings(false, false);
            List<string> strings = ViewFactory.ReadStringAttributeValue(activity, attrs, MugenMvvmToolkit.Resource.Styleable.Binding, null);
            if (strings != null && strings.Count != 0)
            {
                foreach (string bind in strings)
                    BindingServiceProvider.BindingProvider.CreateBindingsFromString(Target, bind, null);
            }
            baseOnInflate(activity, attrs, savedInstanceState);
        }

        /// <summary>
        ///     Called when the Fragment is no longer resumed.
        /// </summary>
        public virtual void OnPause(Action baseOnPause)
        {
            baseOnPause();
        }

        /// <summary>
        ///     Called when the fragment is visible to the user and actively running.
        /// </summary>
        public virtual void OnResume(Action baseOnResume)
        {
            baseOnResume();
        }

        /// <summary>
        ///     Called when the Fragment is visible to the user.
        /// </summary>
        public virtual void OnStart(Action baseOnStart)
        {
            baseOnStart();
            var view = Target.View;
            if (view != null)
                view.ListenParentChange();
        }

        /// <summary>
        ///     Called when the Fragment is no longer started.
        /// </summary>
        public virtual void OnStop(Action baseOnStop)
        {
            baseOnStop();
        }

        /// <summary>
        ///     This method will be invoked when the dialog is canceled.
        /// </summary>
        public virtual void OnCancel(IDialogInterface dialog, Action<IDialogInterface> baseOnCancel)
        {
            var handler = Canceled;
            if (handler != null)
                handler((IWindowView)Target, EventArgs.Empty);
            baseOnCancel(dialog);
        }

        /// <summary>
        ///     Dismiss the fragment and its dialog.
        /// </summary>
        public virtual void Dismiss(Action baseDismiss)
        {
            if (OnClosing())
                baseDismiss();
        }

        /// <summary>
        ///     Occurred on closing window.
        /// </summary>
        public virtual event EventHandler<IWindowView, CancelEventArgs> Closing;

        /// <summary>
        ///     Occurred on closed window.
        /// </summary>
        public virtual event EventHandler<IWindowView, EventArgs> Canceled;

        /// <summary>
        ///     Occurred on destroyed view.
        /// </summary>
        public event EventHandler<IWindowView, EventArgs> Destroyed;

        #endregion

        #region Overrides of MediatorBase<Fragment>

        /// <summary>
        ///     Occurs when the DataContext property changed.
        /// </summary>
        protected override void OnDataContextChanged(object oldValue, object newValue)
        {
            base.OnDataContextChanged(oldValue, newValue);
            View view = Target.View;
            if (view != null)
                BindingServiceProvider.ContextManager
                    .GetBindingContext(view)
                    .Value = DataContext;
        }

        protected override IDataContext CreateRestorePresenterContext()
        {
            return new DataContext
            {
                {DynamicViewModelWindowPresenter.IsOpenViewConstant, true},
                {DynamicViewModelWindowPresenter.RestoredViewConstant, Target},
                {NavigationConstants.SuppressPageNavigation, true}
            };
        }

        #endregion

        #region Methods

        private bool OnClosing()
        {
            var closing = Closing;
            if (closing == null)
                return true;
            var args = new CancelEventArgs();
            closing((IWindowView)Target, args);
            return !args.Cancel;
        }

        private void TrySetTitleBinding()
        {
            var hasDisplayName = BindingContext.Value as IHasDisplayName;
            if (_dialogFragment == null || hasDisplayName == null)
                return;
            var dialog = _dialogFragment.Dialog;
            if (dialog != null)
                BindingServiceProvider.BindingProvider.CreateBindingsFromString(dialog, "Title DisplayName", new object[] { hasDisplayName });
        }

        #endregion
    }
}