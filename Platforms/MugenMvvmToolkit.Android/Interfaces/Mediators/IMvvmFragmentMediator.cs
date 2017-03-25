#region Copyright

// ****************************************************************************
// <copyright file="IMvvmFragmentMediator.cs">
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
using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;
#if APPCOMPAT
using MugenMvvmToolkit.Android.AppCompat.Interfaces.Views;
using Fragment = Android.Support.V4.App.Fragment;

namespace MugenMvvmToolkit.Android.AppCompat.Interfaces.Mediators
#else
using MugenMvvmToolkit.Android.Interfaces.Views;

namespace MugenMvvmToolkit.Android.Interfaces.Mediators
#endif
{
    public interface IMvvmFragmentMediator
    {
        Fragment Fragment { get; }

        bool IsDestroyed { get; }

        object DataContext { get; set; }

        void OnAttach([NotNull]Activity activity, [NotNull] Action<Activity> baseOnAttach);

        void OnCreate(Bundle savedInstanceState, [NotNull] Action<Bundle> baseOnCreate);

        void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater, Action<IMenu, MenuInflater> baseOnCreateOptionsMenu);

        View OnCreateView(int? viewId, LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState,
            Func<LayoutInflater, ViewGroup, Bundle, View> baseOnCreateView);

        void OnViewCreated([NotNull]View view, Bundle savedInstanceState, [NotNull] Action<View, Bundle> baseOnViewCreated);

        void OnDestroyView([NotNull]Action baseOnDestroyView);

        void OnDestroy([NotNull]Action baseOnDestroy);

        void OnDetach([NotNull]Action baseOnDetach);

        void OnInflate([NotNull]Activity activity, [NotNull] IAttributeSet attrs, Bundle savedInstanceState,
            [NotNull]Action<Activity, IAttributeSet, Bundle> baseOnInflate);

        void OnPause([NotNull]Action baseOnPause);

        void OnResume([NotNull]Action baseOnResume);

        void OnSaveInstanceState(Bundle outState, [NotNull] Action<Bundle> baseOnSaveInstanceState);

        void OnStart([NotNull]Action baseOnStart);

        void OnStop([NotNull]Action baseOnStop);

        void OnCancel([NotNull]IDialogInterface dialog, [NotNull]Action<IDialogInterface> baseOnCancel);

        void Dismiss(Action baseDismiss);

        void DismissAllowingStateLoss(Action baseDismissAllowingStateLoss);

        void AddPreferencesFromResource(Action<int> baseAddPreferencesFromResource, int preferencesResId);

        event EventHandler<Fragment, EventArgs> DataContextChanged;

        event EventHandler<Fragment, EventArgs> Destroyed;

        event EventHandler<IWindowView, CancelEventArgs> Closing;

        event EventHandler<IWindowView, EventArgs> Canceled;
    }
}
