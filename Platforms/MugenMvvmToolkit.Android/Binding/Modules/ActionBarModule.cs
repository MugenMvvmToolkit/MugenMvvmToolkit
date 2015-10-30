#region Copyright

// ****************************************************************************
// <copyright file="ActionBarModule.cs">
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
using System.Collections;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Android.Binding.Models;
using MugenMvvmToolkit.Android.Infrastructure;
using MugenMvvmToolkit.Android.Interfaces;
using MugenMvvmToolkit.Android.Interfaces.Views;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Modules;
using Object = Java.Lang.Object;
#if APPCOMPAT
using System.ComponentModel;
using ShowAsAction = MugenMvvmToolkit.Android.AppCompat.Models.ShowAsAction;
using ActionBarDisplayOptions = MugenMvvmToolkit.Android.AppCompat.Models.ActionBarDisplayOptions;
using ActionBarNavigationMode = MugenMvvmToolkit.Android.AppCompat.Models.ActionBarNavigationMode;
using ActionProvider = Android.Support.V4.View.ActionProvider;
using Fragment = Android.Support.V4.App.Fragment;
using IMenuItemOnActionExpandListener = Android.Support.V4.View.MenuItemCompat.IOnActionExpandListener;
using ActionBar = Android.Support.V7.App.ActionBar;
using ActionMode = Android.Support.V7.View.ActionMode;
using PopupMenu = Android.Support.V7.Widget.PopupMenu;
using SearchView = Android.Support.V7.Widget.SearchView;
using ActionBarTabItemsSourceGenerator = MugenMvvmToolkit.Android.AppCompat.Infrastructure.ActionBarTabItemsSourceGenerator;

namespace MugenMvvmToolkit.Android.AppCompat.Modules
#else
using Android.Widget;

namespace MugenMvvmToolkit.Android.Binding.Modules
#endif
{
    public class ActionBarModule : ModuleBase
    {
        #region Nested types

        private sealed class HomeButtonImpl : EventListenerList
        {
            #region Fields

            private readonly ActionBar _actionBar;
            private bool _enabled;

            #endregion

            #region Constructors

            public HomeButtonImpl(ActionBar actionBar)
            {
                Should.NotBeNull(actionBar, "actionBar");
                _actionBar = actionBar;
            }

            #endregion

            #region Properties

            public bool Enabled
            {
                get { return _enabled; }
                set
                {
                    _enabled = value;
                    _actionBar.SetHomeButtonEnabled(value);
                }
            }

            #endregion

            #region Methods

            public static HomeButtonImpl GetOrAdd(ActionBar actionBar)
            {
                return ServiceProvider.AttachedValueProvider.GetOrAdd(actionBar, "~2`homelist", (bar, o) =>
                {
                    var listener = new HomeButtonImpl(actionBar);
                    var activity = actionBar.ThemedContext.GetActivity() as IActivityView;
                    if (activity != null)
                        activity.Mediator.OptionsItemSelected += listener.OptionsItemSelected;
                    return listener;
                }, null);
            }

            private bool OptionsItemSelected(IMenuItem arg)
            {
                Raise(arg, EventArgs.Empty);
                return arg.ItemId == global::Android.Resource.Id.Home;
            }

            #endregion
        }

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
#if APPCOMPAT
                var adapter = _actionBar.GetBindingMemberValue(AttachedMembersCompat.ActionBar.ItemsSourceAdapter);
#else
                var adapter = _actionBar.GetBindingMemberValue(AttachedMembers.ActionBar.ItemsSourceAdapter);
#endif

                if (adapter == null)
                    return false;
#if APPCOMPAT
                _actionBar.SetBindingMemberValue(AttachedMembersCompat.ActionBar.SelectedItem, adapter.GetRawItem(itemPosition));
#else
                _actionBar.SetBindingMemberValue(AttachedMembers.ActionBar.SelectedItem, adapter.GetRawItem(itemPosition));
#endif
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
#if APPCOMPAT
                var activity = _actionBar.ThemedContext.GetActivity() as IActivityView;
                if (activity != null)
                    activity.Mediator.BackPressing += OnBackPressing;
#endif
            }

            #endregion

            #region Methods

#if APPCOMPAT
            private void OnBackPressing(Activity sender, CancelEventArgs args)
            {
                var value = _actionBar.GetBindingMemberValue(AttachedMembersCompat.ActionBar.ContextActionBarVisible);
                if (value)
                    args.Cancel = true;
                _actionBar.SetBindingMemberValue(AttachedMembersCompat.ActionBar.ContextActionBarVisible, false);
            }

            private void Unsubscribe(object item)
            {
                if (item != null)
                    ((IActivityView)item).Mediator.BackPressing -= OnBackPressing;
            }
#endif
            #endregion

            #region Implementation of ICallback

            public bool OnActionItemClicked(ActionMode mode, IMenuItem item)
            {
                return true;
            }

            public bool OnCreateActionMode(ActionMode mode, IMenu menu)
            {
#if APPCOMPAT
                var templateId = _actionBar.GetBindingMemberValue(AttachedMembersCompat.ActionBar.ContextActionBarTemplate);
#else
                var templateId = _actionBar.GetBindingMemberValue(AttachedMembers.ActionBar.ContextActionBarTemplate);
#endif
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
#if APPCOMPAT
                _actionBar.SetBindingMemberValue(AttachedMembersCompat.ActionBar.ContextActionBarVisible, false);
                Unsubscribe(_actionBar.ThemedContext.GetActivity());
#else
                _actionBar.SetBindingMemberValue(AttachedMembers.ActionBar.ContextActionBarVisible, false);
#endif
            }

            public bool OnPrepareActionMode(ActionMode mode, IMenu menu)
            {
                return true;
            }

            #endregion
        }

        private sealed class PopupMenuDismissListener : Object, PopupMenu.IOnDismissListener
        {
            #region Implementation of IOnDismissListener

            public void OnDismiss(PopupMenu menu)
            {
                MenuTemplate.Clear(menu.Menu);
            }

            #endregion
        }

        private sealed class PopupMenuPresenter : IEventListener
        {
            #region Fields

            private static readonly PopupMenuDismissListener DismissListener;
            private readonly View _view;
            private IDisposable _unsubscriber;

            #endregion

            #region Constructors

            static PopupMenuPresenter()
            {
                DismissListener = new PopupMenuDismissListener();
            }

            public PopupMenuPresenter(View view)
            {
                _view = view;
            }

            #endregion

            #region Implementation of IEventListener

            public bool IsAlive
            {
                get { return true; }
            }

            public bool IsWeak
            {
                get { return false; }
            }

            public bool TryHandle(object sender, object message)
            {
                var view = _view;
                if (!view.IsAlive())
                {
                    Update(null);
                    return false;
                }

                var activity = _view.Context.GetActivity();
                if (activity == null)
                {
                    Update(null);
                    Tracer.Warn("(PopupMenu) The contex of view is not an activity.");
                    return false;
                }

                var templateId = _view.GetBindingMemberValue(AttachedMembers.View.PopupMenuTemplate);
                var path = _view.GetBindingMemberValue(AttachedMembers.View.PopupMenuPlacementTargetPath);
                View itemView = null;
                if (!string.IsNullOrEmpty(path))
                    itemView = (View)BindingExtensions.GetValueFromPath(message, path);

                var menuPresenter = _view.GetBindingMemberValue(AttachedMembers.View.PopupMenuPresenter);
                if (menuPresenter == null)
                {
                    var menu = new PopupMenu(activity, itemView ?? view);
                    activity.MenuInflater.Inflate(templateId, menu.Menu, itemView ?? view);
                    menu.SetOnDismissListener(DismissListener);
                    menu.Show();
                    return true;
                }
                return menuPresenter.Show(view, itemView ?? view, templateId, message, (s, menu) => MenuTemplate.Clear(menu));
            }

            #endregion

            #region Methods

            public void Update(IDisposable unsubscriber)
            {
                IDisposable oldValue = _unsubscriber;
                if (oldValue != null)
                    oldValue.Dispose();
                _unsubscriber = unsubscriber;
            }

            #endregion
        }


        private sealed class ActionViewExpandedListener : Object, IMenuItemOnActionExpandListener
        {
            #region Fields

            private const string Key = "!~ActionViewExpandedListener";
            private readonly IMenuItem _item;

            #endregion

            #region Constructors

            public ActionViewExpandedListener(IMenuItem menuItem)
            {
                _item = menuItem;
            }

            #endregion

            #region Methods

            public static IDisposable AddExpandListener(IMenuItem menuItem, IEventListener listener)
            {
                return EventListenerList.GetOrAdd(menuItem, Key).AddWithUnsubscriber(listener);
            }

            private static void Raise(IMenuItem item)
            {
                EventListenerList.Raise(item, Key, EventArgs.Empty);
            }

            #endregion

            #region Implementation of IMenuItemOnActionExpandListener

            public bool OnMenuItemActionCollapse(IMenuItem item)
            {
                item = _item;
                Raise(item);
                return true;
            }

            public bool OnMenuItemActionExpand(IMenuItem item)
            {
                item = _item;
                Raise(item);
                return true;
            }

            #endregion
        }

        #endregion

        #region Fields

#if APPCOMPAT
        private const int P = BindingModulePriority - 2;
#else
        private const int P = BindingModulePriority - 1;
#endif
        private const string ActionBarActionModeKey = "!#CurrentActionMode";
        private const string ActionViewBindKey = "@ActionViewBind";
        private const string ActionProviderBindKey = "@ActionProviderBind";

        #endregion

        #region Constructors

        public ActionBarModule()
            : base(true, priority: P)
        {
        }

        #endregion

        #region Methods

        private static void RegisterActionBarMembers(IBindingMemberProvider memberProvider)
        {
            //PopupMenu
#if !APPCOMPAT
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.View.PopupMenuTemplate));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.View.PopupMenuPlacementTargetPath));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.View.PopupMenuPresenter));
#endif
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.View.PopupMenuEvent, PopupMenuEventChanged));

            //Menu
            memberProvider.Register(AttachedBindingMember.CreateNotifiableMember(AttachedMembers.MenuItem.ActionView, (info, item) => item.GetActionView(), MenuItemUpdateActionView));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.MenuItem.ActionViewTemplateSelector, (o, args) => RefreshValue(o, AttachedMembers.MenuItem.ActionView)));
            memberProvider.Register(AttachedBindingMember.CreateNotifiableMember(AttachedMembers.MenuItem.ActionProvider, (info, item) => item.GetActionProvider(), MenuItemUpdateActionProvider));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.MenuItem.ActionProviderTemplateSelector, (o, args) => RefreshValue(o, AttachedMembers.MenuItem.ActionProvider)));


            memberProvider.Register(AttachedBindingMember
                .CreateMember(AttachedMembers.MenuItem.IsActionViewExpanded, (info, item) => item.GetIsActionViewExpanded(),
                    SetIsActionViewExpanded, ObserveIsActionViewExpanded, (item, args) => item.SetOnActionExpandListener(new ActionViewExpandedListener(item))));
            memberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.MenuItem.ShowAsAction, null, (info, o, value) => o.SetShowAsActionFlags(value)));

            //ActionBar
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ActionBarTab.Content.Override<ActionBar.Tab>()));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ActionBar.ItemsSource.Override<ActionBar>(), (bar, args) => ActionBarUpdateItemsSource(bar)));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ActionBar.SelectedItem.Override<ActionBar>(), ActionBarSelectedItemChanged));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ActionBar.ContextActionBarVisible.Override<ActionBar>(), ActionBarContextActionBarVisibleChanged));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ActionBar.ContextActionBarTemplate.Override<ActionBar>()));
            memberProvider.Register(AttachedBindingMember.CreateMember<ActionBar, object>(AttachedMemberConstants.ParentExplicit, (info, bar) => bar.ThemedContext.GetActivity(), null));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.ActionBar.BackgroundDrawable.Override<ActionBar>(),
                    (actionBar, args) =>
                    {
                        if (args.NewValue is int)
                            actionBar.SetBackgroundDrawable(
                                actionBar.ThemedContext.Resources.GetDrawable((int)args.NewValue));
                        else
                            actionBar.SetBackgroundDrawable((Drawable)args.NewValue);
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember(AttachedMembers.ActionBar.CustomView.Override<ActionBar>(),
                    (info, actionBar) => actionBar.CustomView,
                    (info, actionBar, value) =>
                    {
                        if (actionBar.CustomView != null)
                            ParentObserver.GetOrAdd(actionBar.CustomView).Parent = null;
                        if (value is int)
                            value = actionBar.ThemedContext.GetBindableLayoutInflater().Inflate((int)value, null);
                        actionBar.CustomView = (View)value;
                        if (actionBar.CustomView != null)
                            ParentObserver.GetOrAdd(actionBar.CustomView).Parent = actionBar;
                        return true;
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.ActionBar.DisplayHomeAsUpEnabled.Override<ActionBar>(),
                    (actionBar, args) => actionBar.SetDisplayHomeAsUpEnabled(args.NewValue)));

            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember(AttachedMembers.ActionBar.DisplayOptions.Override<ActionBar>().Cast<ActionBarDisplayOptions>(),
                    (info, actionBar) => actionBar.GetActionBarDisplayOptions(),
                    (info, actionBar, value) =>
                    {
                        actionBar.SetActionBarDisplayOptions(value);
                        return true;
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.ActionBar.DisplayShowCustomEnabled.Override<ActionBar>(),
                    (actionBar, args) => actionBar.SetDisplayShowCustomEnabled(args.NewValue)));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.ActionBar.DisplayShowHomeEnabled.Override<ActionBar>(),
                    (actionBar, args) => actionBar.SetDisplayShowHomeEnabled(args.NewValue)));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.ActionBar.DisplayShowTitleEnabled.Override<ActionBar>(),
                    (actionBar, args) => actionBar.SetDisplayShowTitleEnabled(args.NewValue)));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.ActionBar.DisplayUseLogoEnabled.Override<ActionBar>(),
                    (actionBar, args) => actionBar.SetDisplayUseLogoEnabled(args.NewValue)));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.ActionBar.HomeButtonEnabled.Override<ActionBar>(),
                    (actionBar, args) => actionBar.SetHomeButtonEnabled(args.NewValue)));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.ActionBar.Icon.Override<ActionBar>(), (actionBar, args) =>
                {
                    if (args.NewValue is int)
                        actionBar.SetIcon((int)args.NewValue);
                    else
                        actionBar.SetIcon((Drawable)args.NewValue);
                }));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.ActionBar.Logo.Override<ActionBar>(), (actionBar, args) =>
                {
                    if (args.NewValue is int)
                        actionBar.SetLogo((int)args.NewValue);
                    else
                        actionBar.SetLogo((Drawable)args.NewValue);
                }));
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember(AttachedMembers.ActionBar.NavigationMode.Override<ActionBar>().Cast<ActionBarNavigationMode>(),
                    (info, actionBar) => actionBar.GetNavigationMode(), ActionBarSetNavigationMode));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.ActionBar.SplitBackgroundDrawable.Override<ActionBar>(),
                    (actionBar, args) =>
                    {
                        if (args.NewValue is int)
                            actionBar.SetSplitBackgroundDrawable(
                                actionBar.ThemedContext.Resources.GetDrawable((int)args.NewValue));
                        else
                            actionBar.SetSplitBackgroundDrawable((Drawable)args.NewValue);
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.ActionBar.StackedBackgroundDrawable.Override<ActionBar>(),
                    (actionBar, args) =>
                    {
                        if (args.NewValue is int)
                            actionBar.SetStackedBackgroundDrawable(
                                actionBar.ThemedContext.Resources.GetDrawable((int)args.NewValue));
                        else
                            actionBar.SetStackedBackgroundDrawable((Drawable)args.NewValue);
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember(AttachedMembers.ActionBar.IsShowing.Override<ActionBar>(), (info, actionBar) => actionBar.IsShowing, SetActionBarIsShowing));
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember<ActionBar, string>("Subtitle",
                    (info, actionBar) => actionBar.Subtitle, (info, actionBar, value) =>
                    {
                        actionBar.Subtitle = value;
                        return true;
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember<ActionBar, string>("Title",
                    (info, actionBar) => actionBar.Title, (info, actionBar, value) =>
                    {
                        actionBar.Title = value;
                        return true;
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember(AttachedMembers.ActionBar.Visible.Override<ActionBar>(), (info, actionBar) => actionBar.IsShowing, SetActionBarIsShowing));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ActionBar.DropDownItemTemplate.Override<ActionBar>()));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ActionBar.DropDownItemTemplateSelector.Override<ActionBar>()));

            //ActionBar.Tab
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ActionBarTab.ContentTemplateSelector.Override<ActionBar.Tab>()));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ActionBarTab.ContentTemplate.Override<ActionBar.Tab>()));
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember<ActionBar.Tab, string>("ContentDescription",
                    (info, tab) => tab.ContentDescription,
                    (info, tab, value) =>
                    {
                        tab.SetContentDescription(value);
                        return true;
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember(AttachedMembers.ActionBarTab.CustomView.Override<ActionBar.Tab>(),
                    (info, tab) => tab.CustomView, (info, tab, value) =>
                    {
                        if (tab.CustomView != null)
                            ParentObserver.GetOrAdd(tab.CustomView).Parent = null;
                        if (value is int)
                            value = GetContextFromItem(tab).GetBindableLayoutInflater().Inflate((int)value, null);
                        tab.SetCustomView((View)value);
                        if (tab.CustomView != null)
                            ParentObserver.GetOrAdd(tab.CustomView).Parent = tab;
                        return true;
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember(AttachedMembers.ActionBarTab.Icon.Override<ActionBar.Tab>(),
                    (info, tab) => tab.Icon, (info, tab, value) =>
                    {
                        if (value is int)
                            tab.SetIcon((int)value);
                        else
                            tab.SetIcon((Drawable)value);
                        return true;
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember<ActionBar.Tab, string>("Text",
                    (info, tab) => tab.Text,
                    (info, tab, value) =>
                    {
                        tab.SetText(value);
                        return true;
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember<ActionBar.Tab, Object>("Tag",
                    (info, tab) => tab.Tag,
                    (info, tab, value) =>
                    {
                        tab.SetTag(value);
                        return true;
                    }));
            memberProvider.Register(AttachedBindingMember.CreateMember<ActionBar, HomeButtonImpl>("HomeButton",
                (info, bar) => HomeButtonImpl.GetOrAdd(bar), null));
            memberProvider.Register(AttachedBindingMember.CreateEvent<HomeButtonImpl>("Click",
                (info, homeButton, arg3) => homeButton.AddWithUnsubscriber(arg3)));

            //SearchView
            BindingBuilderExtensions.RegisterDefaultBindingMember<SearchView>(() => v => v.Query);
            var queryMember = AttachedBindingMember.CreateMember<SearchView, string>("Query",
                (info, searchView) => searchView.Query,
                (info, searchView, value) => searchView.SetQuery(value, false), "QueryTextChange");
            memberProvider.Register(queryMember);
            memberProvider.Register("Text", queryMember);
        }

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

        private static bool ActionBarSetNavigationMode(IBindingMemberInfo bindingMemberInfo, ActionBar actionBar, ActionBarNavigationMode value)
        {
            actionBar.SetNavigationMode(value);
            return true;
        }

        private static bool SetActionBarIsShowing(IBindingMemberInfo bindingMemberInfo, ActionBar actionBar, bool value)
        {
            if (value)
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
#if APPCOMPAT
                    var adapter = actionBar.GetBindingMemberValue(AttachedMembersCompat.ActionBar.ItemsSourceAdapter);
#else
                    var adapter = actionBar.GetBindingMemberValue(AttachedMembers.ActionBar.ItemsSourceAdapter);
#endif

                    if (adapter == null || adapter.ItemsSource == null)
                        return;
                    if (args.NewValue == null)
                        args.Member.SetSingleValue(actionBar, adapter.GetRawItem(actionBar.SelectedNavigationIndex));
                    else
                        actionBar.SetSelectedNavigationItem(adapter.GetPosition(args.NewValue));
                    break;
                case ActionBarNavigationMode.Tabs:
#if APPCOMPAT
                    var tabGenerator = actionBar.GetBindingMemberValue(AttachedMembersCompat.ActionBar.ItemsSourceGenerator) as IItemsSourceGeneratorEx;
#else
                    var tabGenerator = actionBar.GetBindingMemberValue(AttachedMembers.ActionBar.ItemsSourceGenerator) as IItemsSourceGeneratorEx;
#endif
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
                                ctx = ctx.DataContext();
                            args.Member.SetSingleValue(actionBar, ctx);
                        }
                        else
                            tabGenerator.SelectedItem = args.NewValue;
                    }
                    break;
            }
        }

        private static void ActionBarUpdateItemsSource(ActionBar actionBar)
        {
            switch (actionBar.GetNavigationMode())
            {
                case ActionBarNavigationMode.List:
#if APPCOMPAT
                    IItemsSourceAdapter sourceAdapter = actionBar.GetBindingMemberValue(AttachedMembersCompat.ActionBar.ItemsSourceAdapter);
#else
                    IItemsSourceAdapter sourceAdapter = actionBar.GetBindingMemberValue(AttachedMembers.ActionBar.ItemsSourceAdapter);
#endif
                    if (sourceAdapter == null)
                    {
                        sourceAdapter = ItemsSourceAdapter.Factory(actionBar, actionBar.ThemedContext, DataContext.Empty);
#if APPCOMPAT
                        actionBar.SetBindingMemberValue(AttachedMembersCompat.ActionBar.ItemsSourceAdapter, sourceAdapter);
#else
                        actionBar.SetBindingMemberValue(AttachedMembers.ActionBar.ItemsSourceAdapter, sourceAdapter);
#endif
                        actionBar.SetListNavigationCallbacks(sourceAdapter, new ActionBarNavigationListener(actionBar));
                    }
#if APPCOMPAT
                    sourceAdapter.ItemsSource = actionBar.GetBindingMemberValue(AttachedMembersCompat.ActionBar.ItemsSource);
#else
                    sourceAdapter.ItemsSource = actionBar.GetBindingMemberValue(AttachedMembers.ActionBar.ItemsSource);
#endif
                    break;
                case ActionBarNavigationMode.Standard:
#if APPCOMPAT
                    actionBar.SetBindingMemberValue(AttachedMembersCompat.ActionBar.SelectedItem, BindingExtensions.NullValue);
#else
                    actionBar.SetBindingMemberValue(AttachedMembers.ActionBar.SelectedItem, BindingExtensions.NullValue);
#endif

                    actionBar.SetListNavigationCallbacks(null, null);
#if APPCOMPAT
                    var generator = actionBar.GetBindingMemberValue(AttachedMembersCompat.ActionBar.ItemsSourceGenerator);
#else
                    var generator = actionBar.GetBindingMemberValue(AttachedMembers.ActionBar.ItemsSourceGenerator);
#endif

                    if (generator != null)
                        generator.SetItemsSource(null);
#if APPCOMPAT
                    var adapter = actionBar.GetBindingMemberValue(AttachedMembersCompat.ActionBar.ItemsSourceAdapter);
#else
                    var adapter = actionBar.GetBindingMemberValue(AttachedMembers.ActionBar.ItemsSourceAdapter);
#endif

                    if (adapter != null)
                        adapter.ItemsSource = null;
                    break;
                case ActionBarNavigationMode.Tabs:
#if APPCOMPAT
                    var tabGenerator = actionBar.GetBindingMemberValue(AttachedMembersCompat.ActionBar.ItemsSourceGenerator);
                    if (tabGenerator != null)
                        tabGenerator.SetItemsSource(actionBar.GetBindingMemberValue(AttachedMembersCompat.ActionBar.ItemsSource));
#else
                    var tabGenerator = actionBar.GetBindingMemberValue(AttachedMembers.ActionBar.ItemsSourceGenerator);
                    if (tabGenerator != null)
                        tabGenerator.SetItemsSource(actionBar.GetBindingMemberValue(AttachedMembers.ActionBar.ItemsSource));
#endif

                    break;
            }
        }

        private static bool MenuItemUpdateActionView(IBindingMemberInfo bindingMemberInfo, IMenuItem menuItem, object content)
        {
            var actionView = menuItem.GetActionView();
            if (actionView != null)
                ParentObserver.GetOrAdd(actionView).Parent = null;

            var selector = menuItem.GetBindingMemberValue(AttachedMembers.MenuItem.ActionViewTemplateSelector);
            if (selector != null)
            {
                object template = selector.SelectTemplate(content, menuItem);
                if (template != null)
                    content = template;
            }
            if (content == null)
            {
                menuItem.SetActionView(null);
                return true;
            }

            int viewId;
            if (int.TryParse(content.ToString(), out viewId))
                content = GetContextFromItem(menuItem).GetBindableLayoutInflater().Inflate(viewId, null);

            actionView = content as View;
            if (actionView == null)
            {
                Type viewType = TypeCache<View>.Instance.GetTypeByName(content.ToString(), true, true);
                actionView = viewType.CreateView(GetContextFromItem(menuItem));
            }
            menuItem.SetActionView(actionView);

            ParentObserver.GetOrAdd(actionView).Parent = menuItem;
            var bindings = GetActionViewBind(menuItem);
            if (!string.IsNullOrEmpty(bindings))
                BindingServiceProvider.BindingProvider.CreateBindingsFromString(actionView, bindings, null);
            return true;
        }

        private static bool MenuItemUpdateActionProvider(IBindingMemberInfo bindingMemberInfo, IMenuItem menuItem, object content)
        {
            var selector = menuItem.GetBindingMemberValue(AttachedMembers.MenuItem.ActionProviderTemplateSelector);
            if (selector != null)
            {
                object template = selector.SelectTemplate(content, menuItem);
                if (template != null)
                    content = template;
            }
            if (content == null)
            {
                menuItem.SetActionProvider(null);
                return true;
            }

            var actionProvider = content as ActionProvider;
            if (actionProvider == null)
            {
                Type viewType = TypeCache<ActionProvider>.Instance.GetTypeByName(content.ToString(), true, true);
                actionProvider = (ActionProvider)Activator.CreateInstance(viewType, GetContextFromItem(menuItem));
            }

            menuItem.SetActionProvider(actionProvider);
            actionProvider.SetBindingMemberValue(AttachedMembers.Object.Parent, menuItem);
            var bindings = GetActionProviderBind(menuItem);
            if (!string.IsNullOrEmpty(bindings))
                BindingServiceProvider.BindingProvider.CreateBindingsFromString(actionProvider, bindings, null);
            return true;
        }

        private static void SetIsActionViewExpanded(IBindingMemberInfo bindingMemberInfo, IMenuItem menuItem, bool value)
        {
            if (value)
                menuItem.ExpandActionView();
            else
                menuItem.CollapseActionView();
        }

        private static IDisposable ObserveIsActionViewExpanded(IBindingMemberInfo bindingMemberInfo, IMenuItem menuItem, IEventListener arg3)
        {
            return ActionViewExpandedListener.AddExpandListener(menuItem, arg3);
        }

        private static Context GetContextFromItem(object item)
        {
            var parent = BindingServiceProvider.VisualTreeManager.FindParent(item);
            while (parent != null)
            {
                var actionBar = parent as ActionBar;
                if (actionBar != null)
                    return actionBar.ThemedContext;
                var view = parent as View;
                if (view != null)
                    return view.Context;
                var ctx = parent as Context;
                if (ctx != null)
                    return ctx;
                parent = BindingServiceProvider.VisualTreeManager.FindParent(parent);
            }
            return Application.Context;
        }

        private static void PopupMenuEventChanged(View view, AttachedMemberChangedEventArgs<string> args)
        {
            if (string.IsNullOrEmpty(args.NewValue))
                return;
            IBindingMemberInfo member = BindingServiceProvider
                .MemberProvider
                .GetBindingMember(view.GetType(), args.NewValue, false, true);
            var presenter = ServiceProvider.AttachedValueProvider.GetOrAdd(view, "!@popup", (view1, o) => new PopupMenuPresenter(view1), null);
            var unsubscriber = member.SetSingleValue(view, presenter) as IDisposable;
            presenter.Update(unsubscriber);
        }

        private static void RefreshValue<TTarget, TValue>(TTarget target, BindingMemberDescriptor<TTarget, TValue> member)
            where TTarget : class
        {
            target.SetBindingMemberValue(member, target.GetBindingMemberValue(member));
        }

        private static void MenuItemTemplateInitialized(MenuItemTemplate menuItemTemplate, IMenuItem menuItem, XmlPropertySetter<MenuItemTemplate, IMenuItem> setter)
        {
            setter.SetEnumProperty<ShowAsAction>(() => template => template.ShowAsAction, menuItemTemplate.ShowAsAction);

            if (!string.IsNullOrEmpty(menuItemTemplate.ActionViewBind))
                ServiceProvider.AttachedValueProvider.SetValue(menuItem, ActionViewBindKey, menuItemTemplate.ActionViewBind);
            if (!string.IsNullOrEmpty(menuItemTemplate.ActionProviderBind))
                ServiceProvider.AttachedValueProvider.SetValue(menuItem, ActionProviderBindKey, menuItemTemplate.ActionProviderBind);
        }

        private static string GetActionViewBind(IMenuItem menuItem)
        {
            return ServiceProvider.AttachedValueProvider.GetValue<string>(menuItem, ActionViewBindKey, false);
        }

        private static string GetActionProviderBind(IMenuItem menuItem)
        {
            return ServiceProvider.AttachedValueProvider.GetValue<string>(menuItem, ActionProviderBindKey, false);
        }

        #endregion

        #region Overrides of ModuleBase

        protected override bool LoadInternal()
        {
#if !APPCOMPAT
            if (!PlatformExtensions.IsApiGreaterThanOrEqualTo14)
                return false;
#endif
            var isActionBar = PlatformExtensions.IsActionBar;
            var isFragment = PlatformExtensions.IsFragment;
            PlatformExtensions.IsActionBar = o => isActionBar(o) || o is ActionBar;
            PlatformExtensions.IsFragment = o => isFragment(o) || o is Fragment;
            MenuItemTemplate.Initalized += MenuItemTemplateInitialized;
            RegisterActionBarMembers(BindingServiceProvider.MemberProvider);
            return true;
        }

        protected override void UnloadInternal()
        {
        }

        #endregion
    }
}
