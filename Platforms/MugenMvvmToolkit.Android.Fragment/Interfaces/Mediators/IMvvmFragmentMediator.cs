#region Copyright

// ****************************************************************************
// <copyright file="IMvvmFragmentMediator.cs">
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
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;
#if APPCOMPAT
using MugenMvvmToolkit.AppCompat.Interfaces.Views;
using Fragment = Android.Support.V4.App.Fragment;
using FragmentTransaction = Android.Support.V4.App.FragmentTransaction;

namespace MugenMvvmToolkit.AppCompat.Interfaces.Mediators
#else
using MugenMvvmToolkit.FragmentSupport.Interfaces.Views;

namespace MugenMvvmToolkit.FragmentSupport.Interfaces.Mediators
#endif
{
    public interface IMvvmFragmentMediator
    {
        /// <summary>
        ///     Gets the <see cref="Fragment" />.
        /// </summary>
        Fragment Fragment { get; }

        /// <summary>
        ///     Gets or sets the data context.
        /// </summary>
        object DataContext { get; set; }

        /// <summary>
        ///     Gets or sets that is responsible for cache view in fragment.
        /// </summary>
        bool CacheFragmentView { get; set; }

        /// <summary>
        ///     Called when a fragment is first attached to its activity.
        /// </summary>
        void OnAttach([NotNull]Activity activity, [NotNull] Action<Activity> baseOnAttach);

        /// <summary>
        ///     Called to do initial creation of a fragment.
        /// </summary>
        void OnCreate(Bundle savedInstanceState, [NotNull] Action<Bundle> baseOnCreate);

        /// <summary>
        ///     Initialize the contents of the Activity's standard options menu.
        /// </summary>
        void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater, Action<IMenu, MenuInflater> baseOnCreateOptionsMenu);

        /// <summary>
        ///     Called to have the fragment instantiate its user interface view.
        /// </summary>
        View OnCreateView(int? viewId, LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState,
            Func<LayoutInflater, ViewGroup, Bundle, View> baseOnCreateView);

        /// <summary>
        ///     Called immediately after <c>OnCreateView(Android.Views.LayoutInflater, Android.Views.ViewGroup, Android.Views.ViewGroup)</c> has returned, but before any saved state has been restored in to the view.
        /// </summary>
        void OnViewCreated([NotNull]View view, Bundle savedInstanceState, [NotNull] Action<View, Bundle> baseOnViewCreated);

        /// <summary>
        ///     Called when the view previously created by <c>OnCreateView</c> has been detached from the fragment.
        /// </summary>
        void OnDestroyView([NotNull]Action baseOnDestroyView);

        /// <summary>
        ///     Called when the fragment is no longer in use.
        /// </summary>
        void OnDestroy([NotNull]Action baseOnDestroy);

        /// <summary>
        ///     Called when the fragment is no longer attached to its activity.
        /// </summary>
        void OnDetach([NotNull]Action baseOnDetach);

        /// <summary>
        ///     Called when a fragment is being created as part of a view layout
        ///     inflation, typically from setting the content view of an activity.
        /// </summary>
        void OnInflate([NotNull]Activity activity, [NotNull] IAttributeSet attrs, Bundle savedInstanceState,
            [NotNull]Action<Activity, IAttributeSet, Bundle> baseOnInflate);

        /// <summary>
        ///     Called when the Fragment is no longer resumed.
        /// </summary>
        void OnPause([NotNull]Action baseOnPause);

        /// <summary>
        ///     Called when the fragment is visible to the user and actively running.
        /// </summary>
        void OnResume([NotNull]Action baseOnResume);

        /// <summary>
        ///     Called to ask the fragment to save its current dynamic state, so it
        ///     can later be reconstructed in a new instance of its process is
        ///     restarted.
        /// </summary>
        void OnSaveInstanceState(Bundle outState, [NotNull] Action<Bundle> baseOnSaveInstanceState);

        /// <summary>
        ///     Called when the Fragment is visible to the user.
        /// </summary>
        void OnStart([NotNull]Action baseOnStart);

        /// <summary>
        ///     Called when the Fragment is no longer started.
        /// </summary>
        void OnStop([NotNull]Action baseOnStop);

        /// <summary>
        ///     This method will be invoked when the dialog is canceled.
        /// </summary>
        void OnCancel([NotNull]IDialogInterface dialog, [NotNull]Action<IDialogInterface> baseOnCancel);

        /// <summary>
        ///     Dismiss the fragment and its dialog.
        /// </summary>
        void Dismiss(Action baseDismiss);

        /// <summary>
        ///     Occurs when the DataContext property changed.
        /// </summary>
        event EventHandler<Fragment, EventArgs> DataContextChanged;

        /// <summary>
        ///     Occurred on destroyed fragment.
        /// </summary>
        event EventHandler<Fragment, EventArgs> Destroyed;

        /// <summary>
        ///     Occurred on closing window.
        /// </summary>
        event EventHandler<IWindowView, CancelEventArgs> Closing;

        /// <summary>
        ///     Occurred on closed window.
        /// </summary>
        event EventHandler<IWindowView, EventArgs> Canceled;
    }
}