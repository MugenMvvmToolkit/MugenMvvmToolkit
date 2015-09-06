#region Copyright

// ****************************************************************************
// <copyright file="IMvvmActivityMediator.cs">
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
using System.Collections.Generic;
using System.ComponentModel;
using Android.App;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Android.Interfaces.Mediators
{
    public interface IMvvmActivityMediator
    {
        /// <summary>
        ///     Gets the current activity metadata.
        /// </summary>
        IDictionary<string, object> Metadata { get; }

            /// <summary>
        ///     Gets the <see cref="Activity" />.
        /// </summary>
        Activity Activity { get; }

        /// <summary>
        ///     Returns true if the final <c>OnDestroy</c> call has been made on the Target, so this instance is now dead.
        /// </summary>
        bool IsDestroyed { get; }

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
        ///     Called when the activity has detected the user's press of the back key.
        /// </summary>
        void OnBackPressed(Action baseOnBackPressed);

        /// <summary>
        ///     Called when the activity is starting.
        /// </summary>
        void OnCreate(int? viewId, Bundle savedInstanceState, [NotNull] Action<Bundle> baseOnCreate);

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
        ///     Called after <c>OnStop</c> when the current activity is being re-displayed to the user (the user has navigated back to it).
        /// </summary>
        void OnRestart([NotNull] Action baseOnRestart);

        /// <summary>
        ///     Called after <c>OnRestoreInstanceState(Android.OS.Bundle)</c>, <c>OnRestart</c>, or <c>OnPause</c>, for your activity to start interacting with the user.
        /// </summary>
        void OnResume([NotNull] Action baseOnResume);

        /// <summary>
        ///     Called to retrieve per-instance state from an activity before being killed
        ///     so that the state can be restored in <c>OnCreate(Android.OS.Bundle)</c> or <c>OnRestoreInstanceState(Android.OS.Bundle)</c> (the <c>Bundle</c> populated by this method will be passed to both).
        /// </summary>
        void OnSaveInstanceState(Bundle outState, [NotNull] Action<Bundle> baseOnSaveInstanceState);

        /// <summary>
        ///     Called after <c>OnCreate(Android.OS.Bundle)</c> or after <c>OnRestart</c> when the activity had been stopped, but is now again being displayed to the user.
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
        ///     Returns a <c>MenuInflater</c> with this context.
        /// </summary>
        MenuInflater GetMenuInflater(MenuInflater baseMenuInflater);

        /// <summary>
        ///     Returns a <c>LayoutInflater</c> with this context.
        /// </summary>
        LayoutInflater GetLayoutInflater(LayoutInflater baseLayoutInflater);

        /// <summary>
        ///     Call this when your activity is done and should be closed.
        /// </summary>
        void Finish(Action baseFinish);

        /// <summary>
        ///     Called by the system when the device configuration changes while your activity is running.
        /// </summary>
        void OnConfigurationChanged(Configuration newConfig, Action<Configuration> baseOnConfigurationChanged);

        /// <summary>
        ///     Called when activity start-up is complete (after <c>OnStart</c> and <c>OnRestoreInstanceState</c> have been called).
        /// </summary>
        void OnPostCreate(Bundle savedInstanceState, Action<Bundle> baseOnPostCreate);

        /// <summary>
        ///     This hook is called whenever an item in your options menu is selected.
        /// </summary>
        bool OnOptionsItemSelected(IMenuItem item, Func<IMenuItem, bool> baseOnOptionsItemSelected);

        /// <summary>
        ///     Inflates the given XML resource and adds the preference hierarchy to the current preference hierarchy.
        /// </summary>
        void AddPreferencesFromResource(Action<int> baseAddPreferencesFromResource, int preferencesResId);

        /// <summary>
        ///     This hook is called whenever an item in your options menu is selected.
        /// </summary>
        Func<IMenuItem, bool> OptionsItemSelected { get; set; }

        /// <summary>
        ///     Called by the system when the device configuration changes while your activity is running.
        /// </summary>
        event EventHandler<Activity, ValueEventArgs<Configuration>> ConfigurationChanged;

        /// <summary>
        ///     Called when activity start-up is complete (after <c>OnStart</c> and <c>OnRestoreInstanceState</c> have been called).
        /// </summary>
        event EventHandler<Activity, ValueEventArgs<Bundle>> PostCreate;

        /// <summary>
        ///     Occurs when the activity has detected the user's press of the back key.
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