#region Copyright

// ****************************************************************************
// <copyright file="PlatformDataBindingModuleCompat.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MugenMvvmToolkit.Android.Binding.Models;
using MugenMvvmToolkit.Android.Infrastructure;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using Object = Java.Lang.Object;
#if APPCOMPAT
using Android.Support.V4.View;
using MugenMvvmToolkit.Android.Binding;
using ShowAsAction = MugenMvvmToolkit.Android.AppCompat.Models.ShowAsAction;
using ActionProvider = Android.Support.V4.View.ActionProvider;
using IMenuItemOnActionExpandListener = Android.Support.V4.View.MenuItemCompat.IOnActionExpandListener;
using PopupMenu = Android.Support.V7.Widget.PopupMenu;
using SearchView = Android.Support.V7.Widget.SearchView;

namespace MugenMvvmToolkit.Android.AppCompat
#else

namespace MugenMvvmToolkit.Android.Binding
#endif
{
    partial class AttachedMembersRegistration
    {
        #region Nested types

        private sealed class ActionViewExpandedListener : Object, IMenuItemOnActionExpandListener
        {
            #region Fields

            private const string Key = "!~ActionViewExpandedListener";
            private readonly IMenuItem _item;

            #endregion

            #region Constructors

            public ActionViewExpandedListener(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
            {
            }

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
                if (_item == null)
                    return false;
                item = _item;
                Raise(item);
                return true;
            }

            public bool OnMenuItemActionExpand(IMenuItem item)
            {
                if (_item == null)
                    return false;
                item = _item;
                Raise(item);
                return true;
            }

            #endregion
        }

        private sealed class PopupMenuDismissListener : Object, PopupMenu.IOnDismissListener
        {
            #region Constructors

            public PopupMenuDismissListener(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
            {
            }

            public PopupMenuDismissListener()
            {
            }

            #endregion

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

            public bool IsAlive => true;

            public bool IsWeak => false;

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

                var template = _view.GetBindingMemberValue(AttachedMembers.View.PopupMenuTemplate);
                var path = _view.GetBindingMemberValue(AttachedMembers.View.PopupMenuPlacementTargetPath);
                View itemView = null;
                if (!string.IsNullOrEmpty(path))
                    itemView = (View)BindingExtensions.GetValueFromPath(message, path);

                var menuPresenter = _view.GetBindingMemberValue(AttachedMembers.View.PopupMenuPresenter);
                if (menuPresenter == null)
                {
                    var menu = new PopupMenu(activity, itemView ?? view);
                    menu.Menu.ApplyMenuTemplate(template, activity, itemView ?? view);
                    menu.SetOnDismissListener(DismissListener);
                    menu.Show();
                    return true;
                }
                return menuPresenter.Show(view, itemView ?? view, template, message, (s, menu) => MenuTemplate.Clear(menu));
            }

            #endregion

            #region Methods

            public void Update(IDisposable unsubscriber)
            {
                _unsubscriber?.Dispose();
                _unsubscriber = unsubscriber;
            }

            #endregion
        }

        #endregion

        #region Fields

        private const string ActionViewBindKey = "@ActionViewBind";
        private const string ActionProviderBindKey = "@ActionProviderBind";

        #endregion

        #region Methods

        public static void RegisterPopupMenuMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.View.PopupMenuEvent, PopupMenuEventChanged));
        }

#if APPCOMPAT
        public static void RegisterMenuItemMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateMember<IMenuItem, string>(AttachedMembers.MenuItem.ShowAsAction.Path, null, (info, o, value) =>
            {
                if (!string.IsNullOrEmpty(value))
                    o.SetShowAsActionFlags((ShowAsAction)Enum.Parse(typeof(ShowAsAction), value.Replace("|", ","), true));
            }));
        }
#endif
        public static void RegisterMenuItemActionViewMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateNotifiableMember(AttachedMembers.MenuItem.ActionView, (info, item) => item.GetActionView(), MenuItemUpdateActionView));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.MenuItem.ActionViewTemplateSelector, (o, args) => RefreshValue(o, AttachedMembers.MenuItem.ActionView)));
            MemberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.MenuItem.IsActionViewExpanded, (info, item) => item.GetIsActionViewExpanded(), SetIsActionViewExpanded, ObserveIsActionViewExpanded, (item, args) => item.SetOnActionExpandListener(new ActionViewExpandedListener(item))));
        }

        public static void RegisterMenuItemActionProviderMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateNotifiableMember(AttachedMembers.MenuItem.ActionProvider, (info, item) => item.GetActionProvider(), MenuItemUpdateActionProvider));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.MenuItem.ActionProviderTemplateSelector, (o, args) => RefreshValue(o, AttachedMembers.MenuItem.ActionProvider)));
        }

        public static void RegisterSearchViewMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<SearchView>(nameof(SearchView.Query));
            var queryMember = AttachedBindingMember.CreateMember<SearchView, string>(nameof(SearchView.Query),
                (info, searchView) => searchView.Query,
                (info, searchView, value) => searchView.SetQuery(value, false), nameof(SearchView.QueryTextChange));
            MemberProvider.Register(queryMember);
            MemberProvider.Register(nameof(TextView.Text), queryMember);
        }

        private static bool MenuItemUpdateActionView(IBindingMemberInfo bindingMemberInfo, IMenuItem menuItem, object content)
        {
            var actionView = menuItem.GetActionView();
            if (actionView != null)
                ParentObserver.GetOrAdd(actionView).Parent = null;

            object template = menuItem.GetBindingMemberValue(AttachedMembers.MenuItem.ActionViewTemplateSelector)?.SelectTemplate(content, menuItem);
            if (template != null)
                content = template;
            if (content == null)
            {
#if APPCOMPAT
                MenuItemCompat.SetActionView(menuItem, null);
#else
                menuItem.SetActionView(null);
#endif
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
#if APPCOMPAT
            MenuItemCompat.SetActionView(menuItem, actionView);
#else
            menuItem.SetActionView(actionView);
#endif

            ParentObserver.GetOrAdd(actionView).Parent = menuItem;
            var bindings = GetActionViewBind(menuItem);
            if (!string.IsNullOrEmpty(bindings))
                BindingServiceProvider.BindingProvider.CreateBindingsFromString(actionView, bindings, null);
            return true;
        }

        private static bool MenuItemUpdateActionProvider(IBindingMemberInfo bindingMemberInfo, IMenuItem menuItem, object content)
        {
            object template = menuItem.GetBindingMemberValue(AttachedMembers.MenuItem.ActionProviderTemplateSelector)?.SelectTemplate(content, menuItem);
            if (template != null)
                content = template;
            if (content == null)
            {
#if APPCOMPAT
                MenuItemCompat.SetActionProvider(menuItem, null);
#else
                menuItem.SetActionProvider(null);
#endif
                return true;
            }

            var actionProvider = content as ActionProvider;
            if (actionProvider == null)
            {
                Type viewType = TypeCache<ActionProvider>.Instance.GetTypeByName(content.ToString(), true, true);
                actionProvider = (ActionProvider)Activator.CreateInstance(viewType, GetContextFromItem(menuItem));
            }

#if APPCOMPAT
            MenuItemCompat.SetActionProvider(menuItem, actionProvider);
#else
            menuItem.SetActionProvider(actionProvider);
#endif
            actionProvider.SetBindingMemberValue(AttachedMembers.Object.Parent, menuItem);
            var bindings = GetActionProviderBind(menuItem);
            if (!string.IsNullOrEmpty(bindings))
                BindingServiceProvider.BindingProvider.CreateBindingsFromString(actionProvider, bindings, null);
            return true;
        }

        private static void SetIsActionViewExpanded(IBindingMemberInfo bindingMemberInfo, IMenuItem menuItem, bool value)
        {
#if APPCOMPAT
            if (value)
                MenuItemCompat.ExpandActionView(menuItem);
            else
                MenuItemCompat.CollapseActionView(menuItem);
#else
            if (value)
                menuItem.ExpandActionView();
            else
                menuItem.CollapseActionView();
#endif
        }

        private static IDisposable ObserveIsActionViewExpanded(IBindingMemberInfo bindingMemberInfo, IMenuItem menuItem, IEventListener arg3)
        {
            return ActionViewExpandedListener.AddExpandListener(menuItem, arg3);
        }

        private static void RefreshValue<TTarget, TValue>(TTarget target, BindingMemberDescriptor<TTarget, TValue> member)
            where TTarget : class
        {
            target.SetBindingMemberValue(member, target.GetBindingMemberValue(member));
        }

        private static string GetActionViewBind(IMenuItem menuItem)
        {
            return ServiceProvider.AttachedValueProvider.GetValue<string>(menuItem, ActionViewBindKey, false);
        }

        private static string GetActionProviderBind(IMenuItem menuItem)
        {
            return ServiceProvider.AttachedValueProvider.GetValue<string>(menuItem, ActionProviderBindKey, false);
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

        #endregion
    }
}