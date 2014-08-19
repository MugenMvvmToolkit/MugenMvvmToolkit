#region Copyright
// ****************************************************************************
// <copyright file="IMvvmActivityMediator.cs">
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
using Android.OS;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Interfaces.Mediators
{
    public interface IMvvmActivityMediator
    {
        /// <summary>
        ///     Gets the <see cref="Activity" />.
        /// </summary>
        Activity Activity { get; }

        /// <summary>
        ///     Gets or sets the data context.
        /// </summary>
        [CanBeNull]
        object DataContext { get; set; }

        /// <summary>
        /// Gets the current bundle.
        /// </summary>
        [CanBeNull]
        Bundle Bundle { get; }

        /// <summary>
        /// Called when the activity has detected the user's press of the back key.
        /// </summary>
        void OnBackPressed(Action baseOnBackPressed);

        /// <summary>
        ///     Called when the activity is starting.
        /// </summary>
        void OnCreate(Bundle savedInstanceState, [NotNull] Action<Bundle> baseOnCreate);

        /// <summary>
        ///     Initialize the contents of the Activity's standard options menu.
        /// </summary>
        bool OnCreateOptionsMenu(IMenu menu, Func<IMenu, bool> baseOnCreateOptionsMenu);

        /// <summary>
        ///     Perform any final cleanup before an activity is destroyed.
        /// </summary>
        void OnDestroy([NotNull] Action baseOnDestroy);

        /// <summary>
        ///     Called as part of the activity lifecycle when an activity is going into
        ///     the background, but has not (yet) been killed.
        /// </summary>
        void OnPause([NotNull] Action baseOnPause);

        /// <summary>
        ///     Called after
        ///     <c>
        ///         <see cref="M:Android.App.Activity.OnStop" />
        ///     </c>
        ///     when the current activity is being
        ///     re-displayed to the user (the user has navigated back to it).
        /// </summary>
        void OnRestart([NotNull] Action baseOnRestart);

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
        void OnResume([NotNull] Action baseOnResume);

        /// <summary>
        ///     Called to retrieve per-instance state from an activity before being killed
        ///     so that the state can be restored in
        ///     <c>
        ///         <see cref="M:Android.App.Activity.OnCreate(Android.OS.Bundle)" />
        ///     </c>
        ///     or
        ///     <c>
        ///         <see cref="M:Android.App.Activity.OnRestoreInstanceState(Android.OS.Bundle)" />
        ///     </c>
        ///     (the
        ///     <c>
        ///         <see cref="T:Android.OS.Bundle" />
        ///     </c>
        ///     populated by this method
        ///     will be passed to both).
        /// </summary>
        void OnSaveInstanceState(Bundle outState, [NotNull] Action<Bundle> baseOnSaveInstanceState);

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
        void OnStart([NotNull] Action baseOnStart);

        /// <summary>
        ///     Called when you are no longer visible to the user.
        /// </summary>
        void OnStop([NotNull] Action baseOnStop);

        /// <summary>
        ///     Set the activity content from a layout resource.
        /// </summary>
        /// <param name="layoutResId">Resource ID to be inflated.</param>
        void SetContentView(int layoutResId);

        /// <summary>
        ///     Returns a
        ///     <c>
        ///         <see cref="T:Android.Views.MenuInflater" />
        ///     </c>
        ///     with this context.
        /// </summary>
        MenuInflater GetMenuInflater(MenuInflater baseMenuInflater);

        /// <summary>
        /// Call this when your activity is done and should be closed.
        /// </summary>
        void Finish(Action baseFinish);

        /// <summary>
        /// Occurs when the activity has detected the user's press of the back key.
        /// </summary>
        event EventHandler<Activity, CancelEventArgs> BackPressing;

        /// <summary>
        ///     Occurred on created activity.
        /// </summary>
        event EventHandler<Activity, ValueEventArgs<Bundle>> Created;

        /// <summary>
        ///     Occurred on started activity.
        /// </summary>
        event EventHandler<Activity, EventArgs> Started;

        /// <summary>
        ///     Occurred on paused activity.
        /// </summary>
        event EventHandler<Activity, EventArgs> Paused;

        /// <summary>
        ///     Occurred on save activity state.
        /// </summary>
        event EventHandler<Activity, ValueEventArgs<Bundle>> SaveInstanceState;

        /// <summary>
        ///     Occurred on stoped activity.
        /// </summary>
        event EventHandler<Activity, EventArgs> Stoped;

        /// <summary>
        ///     Occurred on restarted activity.
        /// </summary>
        event EventHandler<Activity, EventArgs> Restarted;

        /// <summary>
        ///     Occurred on resumed activity.
        /// </summary>
        event EventHandler<Activity, EventArgs> Resume;

        /// <summary>
        ///     Occurred on destroyed activity.
        /// </summary>
        event EventHandler<Activity, EventArgs> Destroyed;

        /// <summary>
        ///     Occurs when the DataContext property changed.
        /// </summary>
        event EventHandler<Activity, EventArgs> DataContextChanged;
    }
}