#region Copyright
// ****************************************************************************
// <copyright file="PlatformDataBindingModule.cs">
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
using System.Collections;
using Android.App;
using Android.Graphics.Drawables;
using Android.Support.V7.App;
using Android.Support.V7.View;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;
using Object = Java.Lang.Object;

// ReSharper disable once CheckNamespace

namespace MugenMvvmToolkit
{
    public partial class PlatformDataBindingModule
    {
        #region Nested types

        private sealed class ActionBarNavigationListener : Object, ActionBar.IOnNavigationListener
        {
            #region Fields

            private readonly ActionBar _actionBar;

            #endregion

            #region Constructors

            public ActionBarNavigationListener(ActionBar actionBar)
            {
                _actionBar = actionBar;
            }

            #endregion

            #region Implementation of IOnNavigationListener

            public bool OnNavigationItemSelected(int itemPosition, long itemId)
            {
                ItemsSourceAdapter adapter = ItemsSourceAdapter.Get(_actionBar);
                if (adapter == null)
                    return false;
                ActionBarSelectedItemMember.SetValue(_actionBar, adapter.GetRawItem(itemPosition));
                return true;
            }

            #endregion
        }

        private sealed class BindableActionMode : Object, ActionMode.ICallback
        {
            #region Fields

            private readonly ActionBar _actionBar;

            #endregion

            #region Constructors

            public BindableActionMode(ActionBar actionBar)
            {
                _actionBar = actionBar;
            }

            #endregion

            #region Implementation of ICallback

            public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
            {
                return true;
            }

            public bool OnCreateActionMode(ActionMode mode, IMenu menu)
            {
                int? templateId = ActionBarContextActionBarTemplateMember.GetValue(_actionBar, null);
                if (templateId == null)
                    return false;
                var activity = _actionBar.ThemedContext.GetActivity();
                if (activity == null)
                {
                    Tracer.Warn("The activity is null action bar {0}", _actionBar);
                    return false;
                }
                activity.MenuInflater.Inflate(templateId.Value, menu, _actionBar);
                return true;
            }

            public void OnDestroyActionMode(ActionMode mode)
            {
                ActionBarContextActionBarVisibleMember.SetValue(_actionBar, false);
            }

            public bool OnPrepareActionMode(ActionMode mode, IMenu menu)
            {
                return true;
            }

            #endregion
        }

        #endregion

        #region Fields

        private const string ActionBarActionModeKey = "!#CurrentActionMode";

        internal static readonly IAttachedBindingMemberInfo<ActionBar.Tab, object> ActionBarTabContentMember;
        internal static readonly IAttachedBindingMemberInfo<ActionBar, object> ActionBarSelectedItemMember;

        private static readonly IAttachedBindingMemberInfo<ActionBar, IEnumerable> ActionBarItemsSourceMember;
        private static readonly IAttachedBindingMemberInfo<ActionBar, int?> ActionBarContextActionBarTemplateMember;
        private static readonly IAttachedBindingMemberInfo<ActionBar, bool> ActionBarContextActionBarVisibleMember;

        #endregion

        #region Methods

        private static void RegisterActionBarMembers(IBindingMemberProvider memberProvider)
        {
            memberProvider.Register(ActionBarTabContentMember);
            memberProvider.Register(ActionBarSelectedItemMember);
            memberProvider.Register(ActionBarItemsSourceMember);
            memberProvider.Register(ActionBarContextActionBarTemplateMember);
            memberProvider.Register(ActionBarContextActionBarVisibleMember);

            memberProvider.Register(AttachedBindingMember
                .CreateMember<ActionBar, object>(AttachedMemberConstants.Parent, (info, bar, arg3) => bar.ThemedContext.GetActivity(), null));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty<ActionBar, object>("BackgroundDrawable",
                    (actionBar, args) =>
                    {
                        if (args.NewValue is int)
                            actionBar.SetBackgroundDrawable(
                                actionBar.ThemedContext.Resources.GetDrawable((int)args.NewValue));
                        else
                            actionBar.SetBackgroundDrawable((Drawable)args.NewValue);
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember<ActionBar, object>("CustomView",
                    (info, actionBar, arg3) => actionBar.CustomView,
                    (info, actionBar, arg3) =>
                    {
                        if (actionBar.CustomView != null)
                            ParentObserver.GetOrAdd(actionBar.CustomView).Parent = null;
                        if (arg3[0] is int)
                            actionBar.SetCustomView((int)arg3[0]);
                        else
                            actionBar.CustomView = (View)arg3[0];
                        if (actionBar.CustomView != null)
                            ParentObserver.GetOrAdd(actionBar.CustomView).Parent = actionBar;
                        return true;
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty<ActionBar, bool>("DisplayHomeAsUpEnabled",
                    (actionBar, args) => actionBar.SetDisplayHomeAsUpEnabled(args.NewValue)));

            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember<ActionBar, ActionBarDisplayOptions>("DisplayOptions",
                    (info, actionBar, arg3) => actionBar.GetActionBarDisplayOptions(),
                    (info, actionBar, arg3) =>
                    {
                        actionBar.SetActionBarDisplayOptions((ActionBarDisplayOptions)arg3[0]);
                        return true;
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty<ActionBar, bool>("DisplayShowCustomEnabled",
                    (actionBar, args) => actionBar.SetDisplayShowCustomEnabled(args.NewValue)));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty<ActionBar, bool>("DisplayShowHomeEnabled",
                    (actionBar, args) => actionBar.SetDisplayShowHomeEnabled(args.NewValue)));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty<ActionBar, bool>("DisplayShowTitleEnabled",
                    (actionBar, args) => actionBar.SetDisplayShowTitleEnabled(args.NewValue)));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty<ActionBar, bool>("DisplayUseLogoEnabled",
                    (actionBar, args) => actionBar.SetDisplayUseLogoEnabled(args.NewValue)));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty<ActionBar, bool>("HomeButtonEnabled",
                    (actionBar, args) => actionBar.SetHomeButtonEnabled(args.NewValue)));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty<ActionBar, object>("Icon", (actionBar, args) =>
                {
                    if (args.NewValue is int)
                        actionBar.SetIcon((int)args.NewValue);
                    else
                        actionBar.SetIcon((Drawable)args.NewValue);
                }));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty<ActionBar, object>("Logo", (actionBar, args) =>
                {
                    if (args.NewValue is int)
                        actionBar.SetLogo((int)args.NewValue);
                    else
                        actionBar.SetLogo((Drawable)args.NewValue);
                }));
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember<ActionBar, ActionBarNavigationMode>("NavigationMode",
                    (info, actionBar, arg3) => actionBar.GetNavigationMode(), ActionBarSetNavigationMode));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty<ActionBar, object>("SplitBackgroundDrawable",
                    (actionBar, args) =>
                    {
                        if (args.NewValue is int)
                            actionBar.SetSplitBackgroundDrawable(
                                actionBar.ThemedContext.Resources.GetDrawable((int)args.NewValue));
                        else
                            actionBar.SetSplitBackgroundDrawable((Drawable)args.NewValue);
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty<ActionBar, object>("StackedBackgroundDrawable",
                    (actionBar, args) =>
                    {
                        if (args.NewValue is int)
                            actionBar.SetStackedBackgroundDrawable(
                                actionBar.ThemedContext.Resources.GetDrawable((int)args.NewValue));
                        else
                            actionBar.SetStackedBackgroundDrawable((Drawable)args.NewValue);
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember<ActionBar, bool>("IsShowing", (info, actionBar, arg3) => actionBar.IsShowing,
                    SetActionBarIsShowing));
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember<ActionBar, string>("Subtitle",
                    (info, actionBar, arg3) => actionBar.Subtitle, (info, actionBar, arg3) =>
                    {
                        actionBar.Subtitle = arg3[0].ToStringSafe();
                        return true;
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember<ActionBar, string>("Title",
                    (info, actionBar, arg3) => actionBar.Title, (info, actionBar, arg3) =>
                    {
                        actionBar.Title = arg3[0].ToStringSafe();
                        return true;
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember<ActionBar, bool>("Visible", (info, actionBar, arg3) => actionBar.IsShowing,
                    SetActionBarIsShowing));
            memberProvider.Register(AttachedBindingMember.CreateMember<ActionBar, View>("HomeButton", GetHomeButton, null));

            memberProvider.Register(
                AttachedBindingMember.CreateAutoProperty<ActionBar, int?>(AttachedMemberNames.DropDownItemTemplate));
            memberProvider.Register(
                AttachedBindingMember.CreateAutoProperty<ActionBar, IDataTemplateSelector>(
                    AttachedMemberNames.DropDownItemTemplateSelector));

            //ActionBar.Tab
            memberProvider.Register(
                AttachedBindingMember.CreateAutoProperty<ActionBar.Tab, IDataTemplateSelector>(
                    AttachedMemberConstants.ContentTemplateSelector));
            memberProvider.Register(
                AttachedBindingMember.CreateAutoProperty<ActionBar.Tab, int?>(
                    AttachedMemberConstants.ContentTemplate));
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember<ActionBar.Tab, string>("ContentDescription",
                    (info, tab, arg3) => tab.ContentDescription,
                    (info, tab, arg3) =>
                    {
                        tab.SetContentDescription(arg3[0].ToStringSafe());
                        return true;
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember<ActionBar.Tab, object>("CustomView",
                    (info, tab, arg3) => tab.CustomView, (info, tab, arg3) =>
                    {
                        if (tab.CustomView != null)
                            ParentObserver.GetOrAdd(tab.CustomView).Parent = null;
                        if (arg3[0] is int)
                            tab.SetCustomView((int)arg3[0]);
                        else
                            tab.SetCustomView((View)arg3[0]);
                        if (tab.CustomView != null)
                            ParentObserver.GetOrAdd(tab.CustomView).Parent = tab;
                        return true;
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember<ActionBar.Tab, object>("Icon",
                    (info, tab, arg3) => tab.Icon, (info, tab, arg3) =>
                    {
                        if (arg3[0] is int)
                            tab.SetIcon((int)arg3[0]);
                        else
                            tab.SetIcon((Drawable)arg3[0]);
                        return true;
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember<ActionBar.Tab, string>("Text",
                    (info, tab, arg3) => tab.Text,
                    (info, tab, arg3) =>
                    {
                        tab.SetText(arg3[0].ToStringSafe());
                        return true;
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember<ActionBar.Tab, Object>("Tag",
                    (info, tab, arg3) => tab.Tag,
                    (info, tab, arg3) =>
                    {
                        tab.SetTag((Object)arg3[0]);
                        return true;
                    }));

            //SearchView
            memberProvider.Register(AttachedBindingMember
                .CreateMember<SearchView, string>("Query",
                    (info, searchView, arg3) => searchView.Query,
                    (info, searchView, arg3) =>
                    {
                        searchView.SetQuery(arg3[0].ToStringSafe(), false);
                        return null;
                    }, memberChangeEventName: "QueryTextChange"));
            memberProvider.Register(AttachedBindingMember
                .CreateMember<SearchView, string>("Text",
                    (info, searchView, arg3) => searchView.Query,
                    (info, searchView, arg3) =>
                    {
                        searchView.SetQuery(arg3[0].ToStringSafe(), false);
                        return null;
                    }, memberChangeEventName: "QueryTextChange"));
        }

        private static View GetHomeButton(IBindingMemberInfo bindingMemberInfo, ActionBar actionBar, object[] arg3)
        {
            var activity = actionBar.ThemedContext.GetActivity();
            if (activity == null)
            {
                Tracer.Warn("The home button cannot be found");
                return null;
            }
            var finder = PlatformExtensions.HomeButtonFinder;
            if (finder != null)
                return finder(activity);

            var homeView = FindHomeView(activity);
            if (homeView != null)
            {
                if (homeView.Parent is LinearLayout)
                    return homeView.Parent as View;
                return homeView;
            }
            Tracer.Warn("The home button cannot be found.");
            return null;
        }

        private static View FindHomeView(Activity activity)
        {
#if API17
            var homeButton = activity.FindViewById(Android.Resource.Id.Home);
            if (homeButton == null)
                return null;
            return homeButton.Parent as View;
#else
            var content = activity.FindViewById(Android.Resource.Id.Content);
            if (content == null)
                return null;
            return FindHomeView(content.RootView);
#endif
        }

#if API8SUPPORT
        private static View FindHomeView(View view)
        {
            if (view == null)
                return null;
            if (view.Class.Name.SafeContains("homeview", StringComparison.OrdinalIgnoreCase))
                return view;
            var viewGroup = view as ViewGroup;
            if (viewGroup == null)
                return null;
            for (int i = 0; i < viewGroup.ChildCount; i++)
            {
                var homeView = FindHomeView(viewGroup.GetChildAt(i));
                if (homeView != null)
                    return homeView;
            }
            return null;
        }
#endif
        private static void ActionBarContextActionBarVisibleChanged(ActionBar actionBar,
            AttachedMemberChangedEventArgs<bool> args)
        {
            var attachedValueProvider = ServiceProvider.AttachedValueProvider;

            var actionMode = attachedValueProvider.GetValue<ActionMode>(actionBar, ActionBarActionModeKey, false);
            if (actionMode != null)
                actionMode.Finish();
            if (args.NewValue)
            {
                actionMode = actionBar.StartActionMode(new BindableActionMode(actionBar));
                attachedValueProvider.SetValue(actionBar, ActionBarActionModeKey, actionMode);
            }
            else
                attachedValueProvider.Clear(actionBar, ActionBarActionModeKey);
        }

        private static bool ActionBarSetNavigationMode(IBindingMemberInfo bindingMemberInfo, ActionBar actionBar,
            object[] arg3)
        {
            actionBar.SetNavigationMode((ActionBarNavigationMode)arg3[0]);
            return true;
        }

        private static bool SetActionBarIsShowing(IBindingMemberInfo bindingMemberInfo, ActionBar actionBar,
            object[] arg3)
        {
            if ((bool)arg3[0])
                actionBar.Show();
            else
                actionBar.Hide();
            ActionBarUpdateItemsSource(actionBar);
            return true;
        }

        private static void ActionBarSelectedItemChanged(ActionBar actionBar, AttachedMemberChangedEventArgs<object> args)
        {
            switch (actionBar.GetNavigationMode())
            {
                case ActionBarNavigationMode.List:
                    ItemsSourceAdapter adapter = ItemsSourceAdapter.Get(actionBar);
                    if (adapter == null || adapter.ItemsSource == null)
                        return;
                    if (args.NewValue == null)
                        args.Member.SetValue(actionBar, new[] { adapter.GetRawItem(actionBar.SelectedNavigationIndex) });
                    else
                        actionBar.SetSelectedNavigationItem(adapter.GetPosition(args.NewValue));
                    break;
                case ActionBarNavigationMode.Tabs:
                    var tabGenerator = ActionBarTabItemsSourceGenerator.Get(actionBar);
                    if (tabGenerator == null)
                    {
                        var tabValue = args.NewValue as ActionBar.Tab;
                        if (tabValue != null && tabValue.Position != actionBar.SelectedNavigationIndex)
                            tabValue.Select();
                    }
                    else
                    {
                        if (args.NewValue == null)
                        {
                            object ctx = actionBar.SelectedNavigationIndex < 0 ? null : actionBar.SelectedTab;
                            if (ctx != null)
                                ctx = BindingServiceProvider.ContextManager.GetBindingContext(ctx).Value;
                            args.Member.SetValue(actionBar, new[] { ctx });
                        }
                        else
                            tabGenerator.Select(args.NewValue);
                    }
                    break;
            }
        }

        private static void ActionBarUpdateItemsSource(ActionBar actionBar)
        {
            switch (actionBar.GetNavigationMode())
            {
                case ActionBarNavigationMode.List:
                    ItemsSourceAdapter sourceAdapter = ItemsSourceAdapter.Get(actionBar);
                    if (sourceAdapter == null)
                    {
                        sourceAdapter = new ItemsSourceAdapter(actionBar, actionBar.ThemedContext, true);
                        ItemsSourceAdapter.Set(actionBar, sourceAdapter);
                        actionBar.SetListNavigationCallbacks(sourceAdapter, new ActionBarNavigationListener(actionBar));
                    }
                    sourceAdapter.ItemsSource = ActionBarItemsSourceMember.GetValue(actionBar, null);
                    break;
                case ActionBarNavigationMode.Standard:
                    ActionBarSelectedItemMember.SetValue(actionBar, BindingExtensions.NullValue);
                    actionBar.SetListNavigationCallbacks(null, null);
                    ActionBarTabItemsSourceGenerator generator = ActionBarTabItemsSourceGenerator.Get(actionBar);
                    if (generator != null)
                        generator.Update(null);
                    ItemsSourceAdapter adapter = ItemsSourceAdapter.Get(actionBar);
                    if (adapter != null)
                        adapter.ItemsSource = null;
                    break;
                case ActionBarNavigationMode.Tabs:
                    ActionBarTabItemsSourceGenerator tabGenerator = ActionBarTabItemsSourceGenerator.Get(actionBar);
                    if (tabGenerator != null)
                        tabGenerator.Update(ActionBarItemsSourceMember.GetValue(actionBar, null));
                    break;
            }
        }

        #endregion
    }
}