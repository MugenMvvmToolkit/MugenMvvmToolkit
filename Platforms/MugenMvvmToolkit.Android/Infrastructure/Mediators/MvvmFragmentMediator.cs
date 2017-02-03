#region Copyright

// ****************************************************************************
// <copyright file="MvvmFragmentMediator.cs">
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
using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Android.Interfaces.Views;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
#if APPCOMPAT
using MugenMvvmToolkit.Android.AppCompat.Interfaces.Mediators;
using MugenMvvmToolkit.Android.Infrastructure;
using MugenMvvmToolkit.Android.Infrastructure.Mediators;
using Fragment = Android.Support.V4.App.Fragment;
using DialogFragment = Android.Support.V4.App.DialogFragment;
using IWindowView = MugenMvvmToolkit.Android.AppCompat.Interfaces.Views.IWindowView;

namespace MugenMvvmToolkit.Android.AppCompat.Infrastructure.Mediators
#else
using MugenMvvmToolkit.Android.Interfaces.Mediators;

namespace MugenMvvmToolkit.Android.Infrastructure.Mediators
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
                (_mediator.Target as DialogFragment)?.Dismiss();
                return true;
            }

            #endregion
        }

        #endregion

        #region Fields

        private DialogInterfaceOnKeyListener _keyListener;
        private View _view;
        private bool _removed;
        private bool _isStarted;

        #endregion

        #region Constructors

        public MvvmFragmentMediator([NotNull] Fragment target)
            : base(target)
        {
        }

        #endregion

        #region Implementation of IMvvmFragmentMediator

        Fragment IMvvmFragmentMediator.Fragment => Target;

        public virtual void OnAttach(Activity activity, Action<Activity> baseOnAttach)
        {
            baseOnAttach(activity);
        }

        public virtual void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater,
            Action<IMenu, MenuInflater> baseOnCreateOptionsMenu)
        {
            if (Target.Activity == null || Target.View == null)
                baseOnCreateOptionsMenu(menu, inflater);
            else
                (Target.View.FindViewById(Resource.Id.OptionsMenu) as IOptionsMenu)?.Inflate(Target.Activity, menu);
        }

        public virtual View OnCreateView(int? viewId, LayoutInflater inflater, ViewGroup container,
            Bundle savedInstanceState, Func<LayoutInflater, ViewGroup, Bundle, View> baseOnCreateView)
        {
            if (_removed)
                return null;
            ClearView();
            if (viewId.HasValue)
            {
                _view = inflater.ToBindableLayoutInflater().Inflate(viewId.Value, container, false);
                _view.SetBindingMemberValue(AttachedMembers.View.Fragment, Target);
                _view.SetDataContext(DataContext);
                return _view;
            }
            return baseOnCreateView(inflater, container, savedInstanceState);
        }

        public virtual void OnCreate(Bundle savedInstanceState, Action<Bundle> baseOnCreate)
        {
            if (Tracer.TraceInformation)
                Tracer.Info("OnCreate fragment({0})", Target);
            OnCreate(savedInstanceState);
            baseOnCreate(savedInstanceState);

            var viewModel = DataContext as IViewModel;
            if (viewModel != null)
                viewModel.Settings.Metadata.AddOrUpdate(PlatformExtensions.FragmentConstant, Target);
            else if (DataContext == null)
            {
                if (savedInstanceState != null && savedInstanceState.ContainsKey(IgnoreStateKey))
                {
                    //prevent child fragments restore
                    savedInstanceState.RemoveFragmentsState();
                    _removed = true;
                    Target.FragmentManager
                        .BeginTransaction()
                        .Remove(Target)
                        .CommitAllowingStateLoss();
                }
            }
        }

        public virtual void OnViewCreated(View view, Bundle savedInstanceState, Action<View, Bundle> baseOnViewCreated)
        {
            if (!(Target is DialogFragment))
                PlatformExtensions.NotifyActivityAttached(Target.Activity, view);
            baseOnViewCreated(view, savedInstanceState);
        }

        public virtual void OnDestroyView(Action baseOnDestroyView)
        {
            baseOnDestroyView();
            ClearView();
        }

        public override void OnDestroy(Action baseOnDestroy)
        {
            if (Tracer.TraceInformation)
                Tracer.Info("OnDestroy fragment({0})", Target);
            RaiseDestroy();
            ClearView();

            (Target as DialogFragment)?.Dialog.ClearBindings(true, true);

            if (_keyListener != null)
            {
                _keyListener.Dispose();
                _keyListener = null;
            }

            (DataContext as IViewModel)?.Settings.Metadata.Remove(PlatformExtensions.FragmentConstant);
            base.OnDestroy(baseOnDestroy);
            Closing = null;
            Canceled = null;
            Destroyed = null;
        }

        public virtual void OnDetach(Action baseOnDetach)
        {
            baseOnDetach();
            Target.ClearBindings(false, true);
        }

        public virtual void OnInflate(Activity activity, IAttributeSet attrs, Bundle savedInstanceState,
            Action<Activity, IAttributeSet, Bundle> baseOnInflate)
        {
            Target.ClearBindings(false, false);
            var bind = ViewFactory.ReadStringAttributeValue(activity, attrs, Resource.Styleable.Binding, Resource.Styleable.Binding_Bind, null, null, null);
            if (!string.IsNullOrEmpty(bind))
                BindingServiceProvider.BindingProvider.CreateBindingsFromString(Target, bind, null);
            baseOnInflate(activity, attrs, savedInstanceState);
        }

        public virtual void OnStart(Action baseOnStart)
        {
            baseOnStart();
            if (!_isStarted)
            {
                _isStarted = true;
                var dialogFragment = Target as DialogFragment;
                var dialog = dialogFragment?.Dialog;
                if (dialog != null)
                {
                    if (DataContext != null)
                        dialog.BindFromExpression("Title DisplayName, Optional=true", new[] { DataContext });
                    if (_keyListener == null)
                        _keyListener = new DialogInterfaceOnKeyListener(this);
                    dialog.SetOnKeyListener(_keyListener);
                }
            }
        }

        public virtual void OnStop(Action baseOnStop)
        {
            baseOnStop();
        }

        public virtual void OnCancel(IDialogInterface dialog, Action<IDialogInterface> baseOnCancel)
        {
            Canceled?.Invoke((IWindowView)Target, EventArgs.Empty);
            baseOnCancel(dialog);
        }

        public virtual void Dismiss(Action baseDismiss)
        {
            if (OnClosing())
                baseDismiss();
        }

        public virtual void AddPreferencesFromResource(Action<int> baseAddPreferencesFromResource, int preferencesResId)
        {
            throw new NotSupportedException();
        }

        public virtual event EventHandler<IWindowView, CancelEventArgs> Closing;

        public virtual event EventHandler<IWindowView, EventArgs> Canceled;

        public virtual event EventHandler<Fragment, EventArgs> Destroyed;

        #endregion

        #region Overrides of MediatorBase<Fragment>

        protected override void OnDataContextChanged(object oldValue, object newValue)
        {
            base.OnDataContextChanged(oldValue, newValue);
            Target.View?.SetDataContext(DataContext);
        }

        protected override IDataContext CreateRestorePresenterContext(Fragment target)
        {
            return new DataContext
            {
                {WindowPresenterConstants.IsViewOpened, true},
                {WindowPresenterConstants.RestoredView, target},
                {NavigationConstants.SuppressPageNavigation, true}
            };
        }

        #endregion

        #region Methods

        private void ClearView()
        {
            if (_view != null)
            {
                _view.ClearBindingsRecursively(true, true, PlatformExtensions.AggressiveViewCleanup);
                _view = null;
                PlatformExtensions.CleanupWeakReferences(false);
            }
        }

        private void RaiseDestroy()
        {
            Destroyed?.Invoke(Target, EventArgs.Empty);
        }

        private bool OnClosing()
        {
            var closing = Closing;
            if (closing == null)
                return true;
            var args = new CancelEventArgs();
            closing((IWindowView)Target, args);
            return !args.Cancel;
        }

        #endregion
    }
}
