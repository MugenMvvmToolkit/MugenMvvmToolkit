#region Copyright
// ****************************************************************************
// <copyright file="AttachedMembersModuleMenu.cs">
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
using Android.Content;
using Android.Graphics.Drawables;
using Android.Support.V4.View;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using Android.Widget;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;
using Object = Java.Lang.Object;
#if API8SUPPORT
using IMenuItemOnActionExpandListener = Android.Support.V4.View.MenuItemCompat.IOnActionExpandListener;
#endif

// ReSharper disable once CheckNamespace
namespace MugenMvvmToolkit.Infrastructure
{
    public partial class AttachedMembersModule
    {
        #region Nested types

        internal sealed class MenuItemOnMenuItemClickListener : Object, IMenuItemOnMenuItemClickListener
        {
            #region Fields

            private const string Key = "#ClickListener";
#if API8SUPPORT
            private readonly IMenuItem _item;
#else
            public static readonly MenuItemOnMenuItemClickListener Instance;
#endif
            #endregion

            #region Constructors

#if API8SUPPORT
            public MenuItemOnMenuItemClickListener(IMenuItem menuItem)
            {
                _item = menuItem;
            }
#else
            static MenuItemOnMenuItemClickListener()
            {
                Instance = new MenuItemOnMenuItemClickListener();
            }

            private MenuItemOnMenuItemClickListener()
            {
            }
#endif
            #endregion

            #region Implementation of IMenuItemOnMenuItemClickListener

            public bool OnMenuItemClick(IMenuItem item)
            {
#if API8SUPPORT
                item = _item;
#endif
                if (item.IsCheckable)
                    IsCheckedMenuItemMember.SetValue(item, !item.IsChecked);
                var list = ServiceProvider.AttachedValueProvider.GetValue<EventListenerList>(item, Key, false);
                if (list != null)
                    list.Raise(item, EventArgs.Empty);
                return true;
            }

            #endregion

            #region Methods

            public static IDisposable AddClickListener(IMenuItem item, IEventListener listener)
            {
                return ServiceProvider.AttachedValueProvider.GetOrAdd(item, Key, (menuItem, o) => new EventListenerList(), null).AddWithUnsubscriber(listener);
            }

            #endregion
        }

#if !API8
        private sealed class ActionViewExpandedListener : Object, IMenuItemOnActionExpandListener
        {
            #region Fields

            private const string Key = "!~ActionViewExpandedListener";
#if API8SUPPORT
            private readonly IMenuItem _item;
#else
            public static readonly ActionViewExpandedListener Instance;
#endif
            #endregion

            #region Constructors

#if API8SUPPORT
            public ActionViewExpandedListener(IMenuItem menuItem)
            {
                _item = menuItem;
            }
#else
            static ActionViewExpandedListener()
            {
                Instance = new ActionViewExpandedListener();
            }

            private ActionViewExpandedListener()
            {
            }
#endif
            #endregion

            #region Methods

            public static IDisposable AddExpandListener(IMenuItem menuItem, IEventListener listener)
            {
                return ServiceProvider.AttachedValueProvider
                                      .GetOrAdd(menuItem, Key, (item, o) => new EventListenerList(), null)
                                      .AddWithUnsubscriber(listener);
            }

            private static void Raise(IMenuItem item)
            {
                EventListenerList list;
                if (ServiceProvider.AttachedValueProvider.TryGetValue(item, Key, out list))
                    list.Raise(item, EventArgs.Empty);
            }

            #endregion

            #region Implementation of IMenuItemOnActionExpandListener

            public bool OnMenuItemActionCollapse(IMenuItem item)
            {
#if API8SUPPORT
                item = _item;
#endif
                Raise(item);
                return true;
            }

            public bool OnMenuItemActionExpand(IMenuItem item)
            {
#if API8SUPPORT
                item = _item;
#endif
                Raise(item);
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

            public bool IsWeak
            {
                get { return false; }
            }

            public void Handle(object sender, object message)
            {
                var view = _view;
                if (!view.IsAlive())
                {
                    Update(null);
                    return;
                }
                var activity = _view.Context.GetActivity();
                if (activity == null)
                {
                    Update(null);
                    Tracer.Warn("(PopupMenu) The contex of view is not an activity.");
                    return;
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
#endif
        #endregion

        #region Fields

        internal static readonly IAttachedBindingMemberInfo<object, object> MenuParentMember;
        private static readonly IAttachedBindingMemberInfo<IMenuItem, bool> IsCheckedMenuItemMember;
#if !API8
        private static readonly IAttachedBindingMemberInfo<IMenuItem, object> MenuItemActionViewMember;
        private static readonly IAttachedBindingMemberInfo<IMenuItem, object> MenuItemActionProviderMember;
        private static readonly IAttachedBindingMemberInfo<IMenuItem, IDataTemplateSelector> MenuItemActionViewSelectorMember;
        private static readonly IAttachedBindingMemberInfo<IMenuItem, IDataTemplateSelector> MenuItemActionProviderSelectorMember;
#endif
        private static readonly IAttachedBindingMemberInfo<IMenu, IEnumerable> MenuItemsSourceMember;

        #endregion

        #region Methods

        private static void RegisterMenuMembers(IBindingMemberProvider memberProvider)
        {
            //IMenu
            memberProvider.Register(typeof(IMenu), MenuParentMember, true);
            memberProvider.Register(MenuItemsSourceMember);
            var menuEnabledMember = AttachedBindingMember.CreateAutoProperty<IMenu, bool?>(AttachedMemberConstants.Enabled, (menu, args) => menu.SetGroupEnabled(0, args.NewValue.GetValueOrDefault()));
            memberProvider.Register(menuEnabledMember);
            memberProvider.Register("IsEnabled", menuEnabledMember);

            var menuVisibleMember = AttachedBindingMember.CreateAutoProperty<IMenu, bool?>("Visible",
                (menu, args) => menu.SetGroupVisible(0, args.NewValue.GetValueOrDefault()));
            memberProvider.Register(menuVisibleMember);
            memberProvider.Register("IsVisible", menuVisibleMember);

            //IMenuItem
#if !API8
            memberProvider.Register(MenuItemActionViewMember);
            memberProvider.Register(MenuItemActionViewSelectorMember);

            memberProvider.Register(MenuItemActionProviderMember);
            memberProvider.Register(MenuItemActionProviderSelectorMember);
#endif
            memberProvider.Register(IsCheckedMenuItemMember);
            memberProvider.Register(typeof(IMenuItem), MenuParentMember, true);
            memberProvider.Register(AttachedBindingMember.CreateEvent<IMenuItem>("Click", SetClickEventValue));

            memberProvider.Register(AttachedBindingMember
                .CreateMember<IMenuItem, object>("AlphabeticShortcut",
                    (info, item, arg3) => item.AlphabeticShortcut,
                    (info, item, arg3) =>
                    {
                        if (arg3[0] is char)
                            item.SetAlphabeticShortcut((char)arg3[0]);
                        else
                            item.SetAlphabeticShortcut(arg3[0].ToStringSafe()[0]);
                        return null;
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateMember<IMenuItem, object>("Icon", (info, item, arg3) => item.Icon,
                    (info, item, arg3) =>
                    {
                        if (arg3[0] is int)
                            item.SetIcon((int)arg3[0]);
                        else
                            item.SetIcon((Drawable)arg3[0]);
                        return null;
                    }));

            memberProvider.Register(AttachedBindingMember
                .CreateMember<IMenuItem, bool>("IsCheckable",
                    (info, item, arg3) => item.IsCheckable,
                    (info, item, arg3) => item.SetCheckable((bool)arg3[0])));

            var menuItemEnabled = AttachedBindingMember.CreateMember<IMenuItem, bool>(AttachedMemberConstants.Enabled,
                (info, item, arg3) => item.IsEnabled,
                (info, item, arg3) => item.SetEnabled((bool)arg3[0]));
            memberProvider.Register(menuItemEnabled);
            memberProvider.Register("IsEnabled", menuItemEnabled);
            memberProvider.Register(AttachedBindingMember
                .CreateMember<IMenuItem, bool>("IsVisible", (info, item, arg3) => item.IsVisible,
                    (info, item, arg3) => item.SetVisible((bool)arg3[0])));
            memberProvider.Register(AttachedBindingMember
                .CreateMember<IMenuItem, object>("NumericShortcut",
                    (info, item, arg3) => item.NumericShortcut,
                    (info, item, arg3) =>
                    {
                        if (arg3[0] is char)
                            item.SetNumericShortcut((char)arg3[0]);
                        else
                            item.SetNumericShortcut(arg3[0].ToStringSafe()[0]);
                        return null;
                    }));
            memberProvider.Register(AttachedBindingMember.CreateMember<IMenuItem, string>("Title",
                (info, item, arg3) => item.TitleFormatted.ToString(),
                (info, item, arg3) => item.SetTitle(arg3[0].ToStringSafe())));
            memberProvider.Register(AttachedBindingMember.CreateMember<IMenuItem, string>("TitleCondensed",
                (info, item, arg3) => item.TitleCondensedFormatted.ToString(),
                (info, item, arg3) => item.SetTitleCondensed(arg3[0].ToStringSafe())));

#if !API8
            memberProvider.Register(AttachedBindingMember
                .CreateMember<IMenuItem, bool>("IsActionViewExpanded", (info, item, arg3) => item.GetIsActionViewExpanded(), SetIsActionViewExpanded,
                    ObserveIsActionViewExpanded, (item, args) =>
                    {
#if API8SUPPORT
                        item.SetOnActionExpandListener(new ActionViewExpandedListener(item));
#else
                        item.SetOnActionExpandListener(ActionViewExpandedListener.Instance);
#endif
                    }));
            memberProvider.Register(AttachedBindingMember
                .CreateMember<IMenuItem, ShowAsAction>("ShowAsAction", null,
                    (info, o, arg3) =>
                    {
                        o.SetShowAsActionFlags((ShowAsAction)arg3[0]);
                        return null;
                    }));
#endif
        }

        private static void MenuItemsSourceChanged(IMenu menu, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            var generator = MenuItemsSourceGenerator.Get(menu);
            if (generator != null)
                generator.Update(args.NewValue);
        }

        private static IDisposable SetClickEventValue(IBindingMemberInfo bindingMemberInfo, IMenuItem menuItem, IEventListener listener)
        {
            return MenuItemOnMenuItemClickListener.AddClickListener(menuItem, listener);
        }

#if !API8
        private static bool MenuItemUpdateActionView(IMenuItem menuItem, object content)
        {
            if (menuItem.GetActionView() != null)
                ViewAttachedParentMember.SetValue(menuItem.GetActionView(), BindingExtensions.NullValue);
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

            View view;
            int viewId;
            if (int.TryParse(content.ToString(), out viewId))
            {
                menuItem.SetActionView(viewId);
                view = menuItem.GetActionView();
            }
            else
            {
                view = content as View;
                if (view == null)
                {
                    Type viewType = TypeCache<View>.Instance.GetTypeByName(content.ToString(), false, true);
                    view = viewType.CreateView(GetContextFromMenuItem(menuItem));
                    view.ListenParentChange();
                }
                menuItem.SetActionView(view);
            }
            ViewAttachedParentMember.SetValue(view, menuItem);
            var bindings = MenuItemTemplate.GetActionViewBind(menuItem);
            if (!string.IsNullOrEmpty(bindings))
                BindingServiceProvider.BindingProvider.CreateBindingsFromString(view, bindings, null);
            return true;
        }

        private static bool MenuItemUpdateActionProvider(IMenuItem menuItem, object content)
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
            BindingServiceProvider
                .BindingProvider
                .CreateBindingFromString(actionProvider, AttachedMemberConstants.DataContext, AttachedMemberConstants.DataContext, menuItem);
            var bindings = MenuItemTemplate.GetActionProviderBind(menuItem);
            if (!string.IsNullOrEmpty(bindings))
                BindingServiceProvider.BindingProvider.CreateBindingsFromString(actionProvider, bindings, null);
            return true;
        }

        private static object SetIsActionViewExpanded(IBindingMemberInfo bindingMemberInfo, IMenuItem menuItem, object[] arg3)
        {
            if ((bool)arg3[0])
                menuItem.ExpandActionView();
            else
                menuItem.CollapseActionView();
            return null;
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
#endif
        #endregion
    }
}