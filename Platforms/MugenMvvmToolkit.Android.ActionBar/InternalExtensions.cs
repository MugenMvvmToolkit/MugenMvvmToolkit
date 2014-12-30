#region Copyright

// ****************************************************************************
// <copyright file="InternalExtensions.cs">
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

using Android.App;
using Android.Views;
#if APPCOMPAT
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Support.V7.View;
using MugenMvvmToolkit.AppCompat.Models;
using ShowAsAction = MugenMvvmToolkit.AppCompat.Models.ShowAsAction;
using ActionBarDisplayOptions = MugenMvvmToolkit.AppCompat.Models.ActionBarDisplayOptions;
using ActionBarNavigationMode = MugenMvvmToolkit.AppCompat.Models.ActionBarNavigationMode;
using ActionProvider = Android.Support.V4.View.ActionProvider;
using IMenuItemOnActionExpandListener = Android.Support.V4.View.MenuItemCompat.IOnActionExpandListener;
using ActionBar = Android.Support.V7.App.ActionBar;
using ActionMode = Android.Support.V7.View.ActionMode;

namespace MugenMvvmToolkit.AppCompat
#else
namespace MugenMvvmToolkit.ActionBarSupport
#endif
{
    // ReSharper disable once PartialTypeWithSinglePart
    internal static partial class InternalExtensions
    {
        #region Methods

#if APPCOMPAT
        public static void SetShowAsActionFlags(this IMenuItem menuItem, ShowAsAction showAsAction)
        {
            MenuItemCompat.SetShowAsAction(menuItem, (int)showAsAction);
        }

        public static void SetOnActionExpandListener(this IMenuItem menuItem, IMenuItemOnActionExpandListener listener)
        {
            MenuItemCompat.SetOnActionExpandListener(menuItem, listener);
        }

        public static void ExpandActionView(this IMenuItem menuItem)
        {
            MenuItemCompat.ExpandActionView(menuItem);
        }

        public static void CollapseActionView(this IMenuItem menuItem)
        {
            MenuItemCompat.CollapseActionView(menuItem);
        }

        public static void SetActionView(this IMenuItem menuItem, View view)
        {
            MenuItemCompat.SetActionView(menuItem, view);
        }

        public static void SetActionView(this IMenuItem menuItem, int resId)
        {
            MenuItemCompat.SetActionView(menuItem, resId);
        }

        public static void SetActionProvider(this IMenuItem menuItem, ActionProvider actionProvider)
        {
            MenuItemCompat.SetActionProvider(menuItem, actionProvider);
        }
#endif

        public static bool GetIsActionViewExpanded(this IMenuItem menuItem)
        {
#if APPCOMPAT
            return MenuItemCompat.IsActionViewExpanded(menuItem);
#else
            return menuItem.IsActionViewExpanded;
#endif
        }

        public static View GetActionView(this IMenuItem menuItem)
        {
#if APPCOMPAT
            return MenuItemCompat.GetActionView(menuItem);
#else
            return menuItem.ActionView;
#endif
        }

        public static ActionProvider GetActionProvider(this IMenuItem menuItem)
        {
#if APPCOMPAT
            return MenuItemCompat.GetActionProvider(menuItem);
#else
            return menuItem.ActionProvider;
#endif
        }

        public static ActionBarDisplayOptions GetActionBarDisplayOptions(this ActionBar actionBar)
        {
#if APPCOMPAT
            return (ActionBarDisplayOptions)actionBar.DisplayOptions;
#else
            return actionBar.DisplayOptions;
#endif
        }

        public static void SetActionBarDisplayOptions(this ActionBar actionBar, ActionBarDisplayOptions options)
        {
#if APPCOMPAT
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
#if APPCOMPAT
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
#if APPCOMPAT
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
            var activity = actionBar.ThemedContext.GetActivity();
#if APPCOMPAT
            Should.BeOfType<ActionBarActivity>(activity, "Activity");
            return ((ActionBarActivity)activity).StartSupportActionMode(mode);
#else
            Should.NotBeNull(activity, "activity");
            return activity.StartActionMode(mode);
#endif
        }

        public static ActionBar GetActionBar(this Activity activity, bool throwOnError = true)
        {
            Should.NotBeNull(activity, "activity");
#if APPCOMPAT
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

        #endregion
    }
}