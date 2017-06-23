#region Copyright

// ****************************************************************************
// <copyright file="IMvvmActivityMediator.cs">
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
using System.Collections.Generic;
using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Android.Interfaces.Mediators
{
    public interface IMvvmActivityMediator
    {
        IDictionary<string, object> Metadata { get; }

        [NotNull]
        Bundle State { get; }

        Activity Activity { get; }

        bool IsDestroyed { get; }

        [CanBeNull]
        object DataContext { get; set; }

        [CanBeNull]
        Bundle SavedInstanceState { get; }

        IDataContext NavigationContext { get; set; }

        void OnBackPressed(Action baseOnBackPressed);

        void OnCreate(int? viewId, Bundle savedInstanceState, [NotNull] Action<Bundle> baseOnCreate);

        void OnNewIntent(Intent intent, [NotNull]Action<Intent> baseOnNewIntent);

        bool OnCreateOptionsMenu(IMenu menu, Func<IMenu, bool> baseOnCreateOptionsMenu);

        void OnDestroy([NotNull] Action baseOnDestroy);

        void OnPause([NotNull] Action baseOnPause);

        void OnRestart([NotNull] Action baseOnRestart);

        void OnResume([NotNull] Action baseOnResume);

        void OnSaveInstanceState(Bundle outState, [NotNull] Action<Bundle> baseOnSaveInstanceState);

        void OnStart([NotNull] Action baseOnStart);

        void OnStop([NotNull] Action baseOnStop);

        void SetContentView(int layoutResId);

        MenuInflater GetMenuInflater(MenuInflater baseMenuInflater);

        LayoutInflater GetLayoutInflater(LayoutInflater baseLayoutInflater);

        void Finish(Action baseFinish);

        void FinishAfterTransition(Action baseFinishAfterTransition);

        void OnConfigurationChanged(Configuration newConfig, Action<Configuration> baseOnConfigurationChanged);

        void OnPostCreate(Bundle savedInstanceState, Action<Bundle> baseOnPostCreate);

        bool OnOptionsItemSelected(IMenuItem item, Func<IMenuItem, bool> baseOnOptionsItemSelected);

        void OnActivityResult(Action<int, Result, Intent> baseOnActivityResult, int requestCode, Result resultCode, Intent data);

        void AddPreferencesFromResource(Action<int> baseAddPreferencesFromResource, int preferencesResId);

        Func<IMenuItem, bool> OptionsItemSelected { get; set; }

        event EventHandler<Activity, ValueEventArgs<Configuration>> ConfigurationChanged;

        event EventHandler<Activity, ValueEventArgs<Bundle>> PostCreate;

        event EventHandler<Activity, CancelEventArgs> BackPressing;

        event EventHandler<Activity, ValueEventArgs<Bundle>> Created;

        event EventHandler<Activity, EventArgs> Started;

        event EventHandler<Activity, EventArgs> Paused;

        event EventHandler<Activity, ValueEventArgs<Bundle>> SaveInstanceState;

        event EventHandler<Activity, EventArgs> Stoped;

        event EventHandler<Activity, EventArgs> Restarted;

        event EventHandler<Activity, EventArgs> Resume;

        event EventHandler<Activity, EventArgs> Destroyed;

        event EventHandler<Activity, EventArgs> DataContextChanged;

        event EventHandler<Activity, ActivityResultEventArgs> ActivityResult;
    }
}
