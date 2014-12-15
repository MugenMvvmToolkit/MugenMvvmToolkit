#region Copyright
// ****************************************************************************
// <copyright file="ActionBarModule.cs">
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
using System.ComponentModel;
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Modules;
using Object = Java.Lang.Object;
#if APPCOMPAT
using Android.Support.V4.App;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Support.V7.View;
using Android.Support.V7.Widget;
using MugenMvvmToolkit.AppCompat.Infrastructure;
using MugenMvvmToolkit.AppCompat.Models;
using ShowAsAction = MugenMvvmToolkit.AppCompat.Models.ShowAsAction;
using ActionBarDisplayOptions = MugenMvvmToolkit.AppCompat.Models.ActionBarDisplayOptions;
using ActionBarNavigationMode = MugenMvvmToolkit.AppCompat.Models.ActionBarNavigationMode;
using ActionProvider = Android.Support.V4.View.ActionProvider;
using Fragment = Android.Support.V4.App.Fragment;
using IMenuItemOnActionExpandListener = Android.Support.V4.View.MenuItemCompat.IOnActionExpandListener;
using ActionBar = Android.Support.V7.App.ActionBar;
using ActionMode = Android.Support.V7.View.ActionMode;
using PopupMenu = Android.Support.V7.Widget.PopupMenu;
using SearchView = Android.Support.V7.Widget.SearchView;

namespace MugenMvvmToolkit.AppCompat.Modules
#else
using MugenMvvmToolkit.ActionBarSupport.Infrastructure;

namespace MugenMvvmToolkit.ActionBarSupport.Modules
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
                return arg.ItemId == Android.Resource.Id.Home;
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
                var adapter = ItemsSourceAdapter.Get(_actionBar);
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
                var value = ActionBarContextActionBarVisibleMember.GetValue(_actionBar, null);
                if (value)
                    args.Cancel = true;
                ActionBarContextActionBarVisibleMember.SetValue(_actionBar, false);
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
#if APPCOMPAT
                Unsubscribe(_actionBar.ThemedContext.GetActivity());
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
            private readonly Type _viewType;
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
                _viewType = _view.GetType();
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

                var templateId = (int)BindingServiceProvider
                    .MemberProvider
                    .GetBindingMember(_viewType, AttachedMemberNames.PopupMenuTemplate, false, true)
                    .GetValue(_view, null);
                IBindingMemberInfo bindingMember = BindingServiceProvider
                    .MemberProvider
                    .GetBindingMember(_viewType, AttachedMemberNames.PlacementTargetPath, false, false);
                if (bindingMember != null)
                {
                    var path = (string)bindingMember.GetValue(view, null);
                    if (!string.IsNullOrEmpty(path))
                    {
                        var itemView = (View)BindingExtensions.GetValueFromPath(message, path);
                        if (itemView != null)
                            view = itemView;
                    }
                }

                var menu = new PopupMenu(activity, view);
                activity.MenuInflater.Inflate(templateId, menu.Menu, view);
                menu.SetOnDismissListener(DismissListener);
                menu.Show();
                return true;
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

        private const string ActionBarActionModeKey = "!#CurrentActionMode";
        private const string ActionViewBindKey = "@ActionViewBind";
        private const string ActionProviderBindKey = "@ActionProviderBind";

        internal static readonly IAttachedBindingMemberInfo<ActionBar.Tab, object> ActionBarTabContentMember;
        internal static readonly IAttachedBindingMemberInfo<ActionBar, object> ActionBarSelectedItemMember;

        private static readonly IAttachedBindingMemberInfo<ActionBar, IEnumerable> ActionBarItemsSourceMember;
        private static readonly IAttachedBindingMemberInfo<ActionBar, int?> ActionBarContextActionBarTemplateMember;
        private static readonly IAttachedBindingMemberInfo<ActionBar, bool> ActionBarContextActionBarVisibleMember;


        private static readonly IAttachedBindingMemberInfo<IMenuItem, object> MenuItemActionViewMember;
        private static readonly IAttachedBindingMemberInfo<IMenuItem, object> MenuItemActionProviderMember;
        private static readonly IAttachedBindingMemberInfo<IMenuItem, IDataTemplateSelector> MenuItemActionViewSelectorMember;
        private static readonly IAttachedBindingMemberInfo<IMenuItem, IDataTemplateSelector> MenuItemActionProviderSelectorMember;

        #endregion

        #region Constructors

        static ActionBarModule()
        {
            MenuItemActionViewMember = AttachedBindingMember
                .CreateNotifiableMember<IMenuItem, object>("ActionView", (info, item) => item.GetActionView(), MenuItemUpdateActionView);
            MenuItemActionViewSelectorMember = AttachedBindingMember
                .CreateAutoProperty<IMenuItem, IDataTemplateSelector>("ActionViewTemplateSelector", (o, args) => RefreshValue(o, MenuItemActionViewMember));

            MenuItemActionProviderMember = AttachedBindingMember
                .CreateNotifiableMember<IMenuItem, object>("ActionProvider", (info, item) => item.GetActionProvider(), MenuItemUpdateActionProvider);
            MenuItemActionProviderSelectorMember = AttachedBindingMember
                .CreateAutoProperty<IMenuItem, IDataTemplateSelector>("ActionProviderTemplateSelector", (o, args) => RefreshValue(o, MenuItemActionProviderMember));

            //Action bar
            ActionBarItemsSourceMember = AttachedBindingMember.CreateAutoProperty<ActionBar, IEnumerable>(AttachedMemberConstants.ItemsSource, (bar, args) => ActionBarUpdateItemsSource(bar));
            ActionBarSelectedItemMember = AttachedBindingMember.CreateAutoProperty<ActionBar, object>(AttachedMemberConstants.SelectedItem, ActionBarSelectedItemChanged);
            //Context action bar
            ActionBarContextActionBarTemplateMember = AttachedBindingMember.CreateAutoProperty<ActionBar, int?>("ContextActionBarTemplate");
            ActionBarContextActionBarVisibleMember = AttachedBindingMember.CreateAutoProperty<ActionBar, bool>("ContextActionBarVisible", ActionBarContextActionBarVisibleChanged);

            //ActioBar.Tab
            ActionBarTabContentMember = AttachedBindingMember.CreateAutoProperty<ActionBar.Tab, object>(AttachedMemberConstants.Content);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ActionBarModule" /> class.
        /// </summary>
        public ActionBarModule()
            : base(true, priority: BindingModulePriority - 1)
        {
        }

        #endregion

        #region Methods

        private static void RegisterActionBarMembers(IBindingMemberProvider memberProvider)
        {
            //PopupMenu
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<View, int>(AttachedMemberNames.PopupMenuTemplate));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<View, string>(AttachedMemberNames.PopupMenuEvent, PopupMenuEventChanged));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<View, string>(AttachedMemberNames.PlacementTargetPath));

            //Menu
            memberProvider.Register(MenuItemActionViewMember);
            memberProvider.Register(MenuItemActionViewSelectorMember);
            memberProvider.Register(MenuItemActionProviderMember);
            memberProvider.Register(MenuItemActionProviderSelectorMember);


            memberProvider.Register(AttachedBindingMember
                .CreateMember<IMenuItem, bool>("IsActionViewExpanded", (info, item) => item.GetIsActionViewExpanded(),
                    SetIsActionViewExpanded,
                    ObserveIsActionViewExpanded, (item, args) => item.SetOnActionExpandListener(new ActionViewExpandedListener(item))));
            memberProvider.Register(AttachedBindingMember
                .CreateMember<IMenuItem, ShowAsAction>("ShowAsAction", null,
                    (info, o, value) => o.SetShowAsActionFlags(value)));

            //ActionBar
            memberProvider.Register(ActionBarTabContentMember);
            memberProvider.Register(ActionBarSelectedItemMember);
            memberProvider.Register(ActionBarItemsSourceMember);
            memberProvider.Register(ActionBarContextActionBarTemplateMember);
            memberProvider.Register(ActionBarContextActionBarVisibleMember);


            memberProvider.Register(AttachedBindingMember
                .CreateMember<ActionBar, object>(AttachedMemberConstants.Parent, (info, bar) => bar.ThemedContext.GetActivity(), null));
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
                    (info, actionBar) => actionBar.CustomView,
                    (info, actionBar, value) =>
                    {
                        if (actionBar.CustomView != null)
                            ParentObserver.GetOrAdd(actionBar.CustomView).Parent = null;
                        if (value is int)
                            actionBar.SetCustomView((int)value);
                        else
                            actionBar.CustomView = (View)value;
                        if (actionBar.CustomView != null)
                            ParentObserver.GetOrAdd(actionBar.CustomView).Parent = actionBar;
                        return true;
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty<ActionBar, bool>("DisplayHomeAsUpEnabled",
                    (actionBar, args) => actionBar.SetDisplayHomeAsUpEnabled(args.NewValue)));

            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember<ActionBar, ActionBarDisplayOptions>("DisplayOptions",
                    (info, actionBar) => actionBar.GetActionBarDisplayOptions(),
                    (info, actionBar, value) =>
                    {
                        actionBar.SetActionBarDisplayOptions(value);
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
                    (info, actionBar) => actionBar.GetNavigationMode(), ActionBarSetNavigationMode));
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
                .CreateNotifiableMember<ActionBar, bool>("IsShowing", (info, actionBar) => actionBar.IsShowing, SetActionBarIsShowing));
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
                .CreateNotifiableMember<ActionBar, bool>("Visible", (info, actionBar) => actionBar.IsShowing, SetActionBarIsShowing));
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
                    (info, tab) => tab.ContentDescription,
                    (info, tab, value) =>
                    {
                        tab.SetContentDescription(value);
                        return true;
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember<ActionBar.Tab, object>("CustomView",
                    (info, tab) => tab.CustomView, (info, tab, value) =>
                    {
                        if (tab.CustomView != null)
                            ParentObserver.GetOrAdd(tab.CustomView).Parent = null;
                        if (value is int)
                            tab.SetCustomView((int)value);
                        else
                            tab.SetCustomView((View)value);
                        if (tab.CustomView != null)
                            ParentObserver.GetOrAdd(tab.CustomView).Parent = tab;
                        return true;
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember<ActionBar.Tab, object>("Icon",
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
                    var adapter = ItemsSourceAdapter.Get(actionBar);
                    if (adapter == null || adapter.ItemsSource == null)
                        return;
                    if (args.NewValue == null)
                        args.Member.SetValue(actionBar, new[] { adapter.GetRawItem(actionBar.SelectedNavigationIndex) });
                    else
                        actionBar.SetSelectedNavigationItem(adapter.GetPosition(args.NewValue));
                    break;
                case ActionBarNavigationMode.Tabs:
                    var tabGenerator = ItemsSourceGeneratorBase.Get(actionBar) as ActionBarTabItemsSourceGenerator;
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
                            tabGenerator.SetSelectedItem(args.NewValue);
                    }
                    break;
            }
        }

        private static void ActionBarUpdateItemsSource(ActionBar actionBar)
        {
            switch (actionBar.GetNavigationMode())
            {
                case ActionBarNavigationMode.List:
                    IItemsSourceAdapter sourceAdapter = ItemsSourceAdapter.Get(actionBar);
                    if (sourceAdapter == null)
                    {
                        sourceAdapter = ItemsSourceAdapter.Factory(actionBar, actionBar.ThemedContext, DataContext.Empty);
                        ItemsSourceAdapter.Set(actionBar, sourceAdapter);
                        actionBar.SetListNavigationCallbacks(sourceAdapter, new ActionBarNavigationListener(actionBar));
                    }
                    sourceAdapter.ItemsSource = ActionBarItemsSourceMember.GetValue(actionBar, null);
                    break;
                case ActionBarNavigationMode.Standard:
                    ActionBarSelectedItemMember.SetValue(actionBar, BindingExtensions.NullValue);
                    actionBar.SetListNavigationCallbacks(null, null);
                    var generator = ItemsSourceGeneratorBase.Get(actionBar);
                    if (generator != null)
                        generator.SetItemsSource(null);
                    var adapter = ItemsSourceAdapter.Get(actionBar);
                    if (adapter != null)
                        adapter.ItemsSource = null;
                    break;
                case ActionBarNavigationMode.Tabs:
                    var tabGenerator = ItemsSourceGeneratorBase.Get(actionBar);
                    if (tabGenerator != null)
                        tabGenerator.SetItemsSource(ActionBarItemsSourceMember.GetValue(actionBar, null));
                    break;
            }
        }

        private static bool MenuItemUpdateActionView(IBindingMemberInfo bindingMemberInfo, IMenuItem menuItem, object content)
        {
            var actionView = menuItem.GetActionView();
            if (actionView != null)
                ParentObserver.GetOrAdd(actionView).Parent = null;

            var selector = MenuItemActionViewSelectorMember.GetValue(menuItem, null);
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
            {
                menuItem.SetActionView(viewId);
                actionView = menuItem.GetActionView();
            }
            else
            {
                actionView = content as View;
                if (actionView == null)
                {
                    Type viewType = TypeCache<View>.Instance.GetTypeByName(content.ToString(), false, true);
                    actionView = viewType.CreateView(GetContextFromMenuItem(menuItem));
                }
                menuItem.SetActionView(actionView);
            }
            ParentObserver.GetOrAdd(actionView).Parent = menuItem;
            var bindings = GetActionViewBind(menuItem);
            if (!string.IsNullOrEmpty(bindings))
                BindingServiceProvider.BindingProvider.CreateBindingsFromString(actionView, bindings, null);
            return true;
        }

        private static bool MenuItemUpdateActionProvider(IBindingMemberInfo bindingMemberInfo, IMenuItem menuItem, object content)
        {
            var selector = MenuItemActionProviderSelectorMember.GetValue(menuItem, null);
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
                Type viewType = TypeCache<ActionProvider>.Instance.GetTypeByName(content.ToString(), false, true);
                actionProvider = (ActionProvider)Activator.CreateInstance(viewType, GetContextFromMenuItem(menuItem));
            }
            //TODO WRAPPER???
            menuItem.SetActionProvider(actionProvider);
            BindingExtensions.AttachedParentMember.SetValue(actionProvider, menuItem);
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

        private static Context GetContextFromMenuItem(IMenuItem menuItem)
        {
            var parent = BindingServiceProvider.VisualTreeManager.FindParent(menuItem);
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
            var unsubscriber = member.SetValue(view, new object[] { presenter }) as IDisposable;
            presenter.Update(unsubscriber);
        }

        private static void RefreshValue(object target, IBindingMemberInfo member)
        {
            member.SetValue(target, new[] { member.GetValue(target, null) });
        }

        private static void MenuItemTemplateInitialized(MenuItemTemplate menuItemTemplate, IMenuItem menuItem, XmlPropertySetter<MenuItemTemplate, IMenuItem> setter)
        {
            setter.SetEnumProperty<ShowAsAction>(template => template.ShowAsAction, menuItemTemplate.ShowAsAction);

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

        /// <summary>
        ///     Loads the current module.
        /// </summary>
        protected override bool LoadInternal()
        {
            var isActionBar = PlatformExtensions.IsActionBar;
            var isFragment = PlatformExtensions.IsFragment;
            PlatformExtensions.IsActionBar = o => isActionBar(o) || o is ActionBar;
            PlatformExtensions.IsFragment = o => isFragment(o) || o is Fragment;
            MenuItemTemplate.Initalized += MenuItemTemplateInitialized;
            RegisterActionBarMembers(BindingServiceProvider.MemberProvider);
            return true;
        }

        /// <summary>
        ///     Unloads the current module.
        /// </summary>
        protected override void UnloadInternal()
        {
        }

        #endregion
    }
}