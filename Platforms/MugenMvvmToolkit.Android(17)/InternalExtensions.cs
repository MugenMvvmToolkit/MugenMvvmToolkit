#region Copyright
// ****************************************************************************
// <copyright file="InternalExtensions.cs">
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
using Android.App;
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Support.V7.View;
using Android.Views;
using JetBrains.Annotations;
using MugenMvvmToolkit.Models;
#if API8SUPPORT
using IMenuItemOnActionExpandListener = Android.Support.V4.View.MenuItemCompat.IOnActionExpandListener;
#else
namespace Android.Support.V4.App { }
namespace Android.Support.V4.View { }
namespace Android.Support.V7.App { }
namespace Android.Support.V7.View { }
namespace Android.Support.V7.Widget { }
#endif

namespace MugenMvvmToolkit
{
    internal static class InternalExtensions
    {
        #region Methods
#if !API8
        public static View GetActionView(this IMenuItem menuItem)
        {
#if API8SUPPORT
            return MenuItemCompat.GetActionView(menuItem);
#else
            return menuItem.ActionView;
#endif
        }

        public static void SetActionView(this IMenuItem menuItem, View view)
        {
#if API8SUPPORT
            MenuItemCompat.SetActionView(menuItem, view);
#else
            menuItem.SetActionView(view);
#endif
        }

        public static void SetActionView(this IMenuItem menuItem, int resId)
        {
#if API8SUPPORT
            MenuItemCompat.SetActionView(menuItem, resId);
#else
            menuItem.SetActionView(resId);
#endif
        }

        public static ActionProvider GetActionProvider(this IMenuItem menuItem)
        {
#if API8SUPPORT
            return MenuItemCompat.GetActionProvider(menuItem);
#else
            return menuItem.ActionProvider;
#endif
        }

        public static void SetActionProvider(this IMenuItem menuItem, ActionProvider actionProvider)
        {
#if API8SUPPORT
            MenuItemCompat.SetActionProvider(menuItem, actionProvider);
#else
            menuItem.SetActionProvider(actionProvider);
#endif
        }

        public static ActionBarDisplayOptions GetActionBarDisplayOptions(this ActionBar actionBar)
        {
#if API8SUPPORT
            return (ActionBarDisplayOptions)actionBar.DisplayOptions;
#else
            return actionBar.DisplayOptions;
#endif
        }

        public static bool GetIsActionViewExpanded(this IMenuItem menuItem)
        {
#if API8SUPPORT
            return MenuItemCompat.IsActionViewExpanded(menuItem);
#else
            return menuItem.IsActionViewExpanded;
#endif
        }

        public static void ExpandActionView(this IMenuItem menuItem)
        {
#if API8SUPPORT
            MenuItemCompat.ExpandActionView(menuItem);
#else
            menuItem.ExpandActionView();
#endif
        }

        public static void CollapseActionView(this IMenuItem menuItem)
        {
#if API8SUPPORT
            MenuItemCompat.CollapseActionView(menuItem);
#else
            menuItem.CollapseActionView();
#endif
        }

        public static void SetOnActionExpandListener(this IMenuItem menuItem, IMenuItemOnActionExpandListener listener)
        {
#if API8SUPPORT
            MenuItemCompat.SetOnActionExpandListener(menuItem, listener);
#else
            menuItem.SetOnActionExpandListener(listener);
#endif
        }

        public static void SetShowAsActionFlags(this IMenuItem menuItem, ShowAsAction showAsAction)
        {
#if API8SUPPORT
            MenuItemCompat.SetShowAsAction(menuItem, (int)showAsAction);
#else
            menuItem.SetShowAsActionFlags(showAsAction);
#endif
        }

        public static void SetActionBarDisplayOptions(this ActionBar actionBar, ActionBarDisplayOptions options)
        {
#if API8SUPPORT
            actionBar.SetDisplayUseLogoEnabled(options.HasFlag(ActionBarDisplayOptions.UseLogo));
            actionBar.SetDisplayShowHomeEnabled(options.HasFlag(ActionBarDisplayOptions.ShowHome));
            actionBar.SetDisplayHomeAsUpEnabled(options.HasFlag(ActionBarDisplayOptions.HomeAsUp));
            actionBar.SetDisplayShowTitleEnabled(options.HasFlag(ActionBarDisplayOptions.ShowTitle));
            actionBar.SetDisplayShowCustomEnabled(options.HasFlag(ActionBarDisplayOptions.ShowCustom));
#else
            actionBar.DisplayOptions = options;
#endif
        }

        public static ActionBarNavigationMode GetNavigationMode(this ActionBar actionBar)
        {
#if API8SUPPORT
            if (actionBar.NavigationMode == ActionBar.NavigationModeList)
                return ActionBarNavigationMode.List;
            if (actionBar.NavigationMode == ActionBar.NavigationModeTabs)
                return ActionBarNavigationMode.Tabs;
            return ActionBarNavigationMode.Standard;
#else
            return actionBar.NavigationMode;
#endif
        }

        public static void SetNavigationMode(this ActionBar actionBar, ActionBarNavigationMode mode)
        {
#if API8SUPPORT
            switch (mode)
            {
                case ActionBarNavigationMode.List:
                    actionBar.NavigationMode = ActionBar.NavigationModeList;
                    break;
                case ActionBarNavigationMode.Tabs:
                    actionBar.NavigationMode = ActionBar.NavigationModeTabs;
                    break;
                default:
                    actionBar.NavigationMode = ActionBar.NavigationModeStandard;
                    break;
            }
#else
            actionBar.NavigationMode = mode;
#endif
        }

        public static ActionMode StartActionMode(this ActionBar actionBar, ActionMode.ICallback mode)
        {
#if API8SUPPORT
            Should.BeOfType<ActionBarActivity>(actionBar.ThemedContext, "Activity");
            var activity = (ActionBarActivity)actionBar.ThemedContext;
            return activity.StartSupportActionMode(mode);
#else
            Should.BeOfType<Activity>(actionBar.ThemedContext, "Activity");
            var activity = (Activity)actionBar.ThemedContext;
            return activity.StartActionMode(mode);
#endif
        }

        public static ActionBar GetActionBar(this Activity activity, bool throwOnError = true)
        {
            Should.NotBeNull(activity, "activity");
#if API8SUPPORT
            if (throwOnError)
            {
                Should.BeOfType<ActionBarActivity>(activity, "activity");
                return ((ActionBarActivity)activity).SupportActionBar;
            }
            var actionBarActivity = activity as ActionBarActivity;
            if (actionBarActivity == null)
                return null;
            return actionBarActivity.SupportActionBar;
#else
            return activity.ActionBar;
#endif
        }

        public static FragmentManager GetFragmentManager(this Activity activity)
        {
            Should.NotBeNull(activity, "activity");
#if API8SUPPORT
            Should.BeOfType<FragmentActivity>(activity, "activity");
            return ((FragmentActivity)activity).SupportFragmentManager;
#else
            return activity.FragmentManager;
#endif
        }

        [CanBeNull]
        public static FragmentManager TryGetFragmentManager(this Activity activity)
        {
            if (activity == null)
                return null;
#if API8SUPPORT
            var fragmentActivity = activity as FragmentActivity;
            if (fragmentActivity == null)
                return null;
            return fragmentActivity.SupportFragmentManager;
#else
            return activity.FragmentManager;
#endif
        }
#endif
        #endregion
    }
}