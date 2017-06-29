#region Copyright

// ****************************************************************************
// <copyright file="ActionBarMembersRegistration.cs">
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
using Android.App;
using Android.Content;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Views;
using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Android.Binding.Models;
using MugenMvvmToolkit.Android.Interfaces.Views;
using MugenMvvmToolkit.Android.Models;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Models;
using Object = Java.Lang.Object;
#if APPCOMPAT
using MugenMvvmToolkit.Android.Binding;
using System.ComponentModel;
using ActionBarDisplayOptions = MugenMvvmToolkit.Android.AppCompat.Models.ActionBarDisplayOptions;
using ActionBarNavigationMode = MugenMvvmToolkit.Android.AppCompat.Models.ActionBarNavigationMode;
using ActionBar = Android.Support.V7.App.ActionBar;
using ActionMode = Android.Support.V7.View.ActionMode;

namespace MugenMvvmToolkit.Android.AppCompat
#else

namespace MugenMvvmToolkit.Android.Binding
#endif
{
    partial class AttachedMembersRegistration
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
                Should.NotBeNull(actionBar, nameof(actionBar));
                _actionBar = actionBar;
            }

            #endregion

            #region Properties

            [Preserve(Conditional = true)]
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
                if (arg.ItemId == global::Android.Resource.Id.Home)
                {
                    Raise(arg, EventArgs.Empty);
                    return true;
                }
                return false;
            }

            #endregion
        }

        private sealed class ActionBarNavigationListener : Object, ActionBar.IOnNavigationListener
        {
            #region Fields

            private readonly ActionBar _actionBar;

            #endregion

            #region Constructors

            public ActionBarNavigationListener(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
            {
            }

            public ActionBarNavigationListener(ActionBar actionBar)
            {
                _actionBar = actionBar;
            }

            #endregion

            #region Implementation of IOnNavigationListener

            public bool OnNavigationItemSelected(int itemPosition, long itemId)
            {
#if APPCOMPAT
                var adapter = _actionBar?.GetBindingMemberValue(AttachedMembersCompat.ActionBar.ItemsSourceAdapter);
#else
                var adapter = _actionBar?.GetBindingMemberValue(AttachedMembers.ActionBar.ItemsSourceAdapter);
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

            public BindableActionMode(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
            {
            }

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
                var templateId = _actionBar?.GetBindingMemberValue(AttachedMembersCompat.ActionBar.ContextActionBarTemplate);
#else
                var templateId = _actionBar?.GetBindingMemberValue(AttachedMembers.ActionBar.ContextActionBarTemplate);
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
                if (_actionBar == null)
                    return;
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

        #endregion

        #region Fields

        private const string ActionBarActionModeKey = "!#CurrentActionMode";

        #endregion

        #region Methods

        public static void RegisterActionBarBaseMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateMember<ActionBar, object>(AttachedMemberConstants.ParentExplicit, (info, bar) => bar.ThemedContext.GetActivity(), null));
            MemberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember<ActionBar, string>(nameof(ActionBar.Subtitle),
                    (info, actionBar) => actionBar.Subtitle, (info, actionBar, value) =>
                    {
                        actionBar.Subtitle = value;
                        return true;
                    }));
            MemberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember<ActionBar, string>(nameof(ActionBar.Title),
                    (info, actionBar) => actionBar.Title, (info, actionBar, value) =>
                    {
                        actionBar.Title = value;
                        return true;
                    }));
            MemberProvider.Register(AttachedBindingMember.CreateMember<ActionBar, HomeButtonImpl>("HomeButton", (info, bar) => HomeButtonImpl.GetOrAdd(bar), null));
            MemberProvider.Register(AttachedBindingMember.CreateEvent<HomeButtonImpl>("Click", (info, homeButton, arg3) => homeButton.AddWithUnsubscriber(arg3)));
        }

        public static void RegisterActionBarMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ActionBar.ItemsSource.Override<ActionBar>(), (bar, args) => ActionBarUpdateItemsSource(bar)));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ActionBar.SelectedItem.Override<ActionBar>(), ActionBarSelectedItemChanged));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ActionBar.ContextActionBarVisible.Override<ActionBar>(), ActionBarContextActionBarVisibleChanged));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ActionBar.ContextActionBarTemplate.Override<ActionBar>()));

            MemberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.ActionBar.BackgroundDrawable.Override<ActionBar>(),
                    (actionBar, args) =>
                    {
                        if (args.NewValue is int)
                            actionBar.SetBackgroundDrawable(
                                actionBar.ThemedContext.Resources.GetDrawable((int)args.NewValue));
                        else
                            actionBar.SetBackgroundDrawable((Drawable)args.NewValue);
                    }));
            MemberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember(AttachedMembers.ActionBar.CustomView.Override<ActionBar>(),
                    (info, actionBar) => actionBar.CustomView,
                    (info, actionBar, value) =>
                    {
                        LayoutInflaterResult result = null;
                        if (actionBar.CustomView != null)
                            ParentObserver.GetOrAdd(actionBar.CustomView).Parent = null;
                        if (value is int)
                        {
                            result = actionBar.ThemedContext.GetBindableLayoutInflater().InflateEx((int)value, null, false);
                            value = result.View;
                        }
                        actionBar.CustomView = (View)value;
                        if (actionBar.CustomView != null)
                            ParentObserver.GetOrAdd(actionBar.CustomView).Parent = actionBar;
                        result?.ApplyBindings();
                        return true;
                    }));
            MemberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.ActionBar.DisplayHomeAsUpEnabled.Override<ActionBar>(),
                    (actionBar, args) => actionBar.SetDisplayHomeAsUpEnabled(args.NewValue)));

            MemberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember(AttachedMembers.ActionBar.DisplayOptions.Override<ActionBar>().Cast<ActionBarDisplayOptions>(),
                    (info, actionBar) => actionBar.GetActionBarDisplayOptions(),
                    (info, actionBar, value) =>
                    {
                        actionBar.SetActionBarDisplayOptions(value);
                        return true;
                    }));
            MemberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.ActionBar.DisplayShowCustomEnabled.Override<ActionBar>(),
                    (actionBar, args) => actionBar.SetDisplayShowCustomEnabled(args.NewValue)));
            MemberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.ActionBar.DisplayShowHomeEnabled.Override<ActionBar>(),
                    (actionBar, args) => actionBar.SetDisplayShowHomeEnabled(args.NewValue)));
            MemberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.ActionBar.DisplayShowTitleEnabled.Override<ActionBar>(),
                    (actionBar, args) => actionBar.SetDisplayShowTitleEnabled(args.NewValue)));
            MemberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.ActionBar.DisplayUseLogoEnabled.Override<ActionBar>(),
                    (actionBar, args) => actionBar.SetDisplayUseLogoEnabled(args.NewValue)));
            MemberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.ActionBar.HomeButtonEnabled.Override<ActionBar>(),
                    (actionBar, args) => actionBar.SetHomeButtonEnabled(args.NewValue)));
            MemberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.ActionBar.Icon.Override<ActionBar>(), (actionBar, args) =>
                {
                    if (args.NewValue is int)
                        actionBar.SetIcon((int)args.NewValue);
                    else
                        actionBar.SetIcon((Drawable)args.NewValue);
                }));
            MemberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.ActionBar.Logo.Override<ActionBar>(), (actionBar, args) =>
                {
                    if (args.NewValue is int)
                        actionBar.SetLogo((int)args.NewValue);
                    else
                        actionBar.SetLogo((Drawable)args.NewValue);
                }));
            MemberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember(AttachedMembers.ActionBar.NavigationMode.Override<ActionBar>().Cast<ActionBarNavigationMode>(),
                    (info, actionBar) => actionBar.GetNavigationMode(), ActionBarSetNavigationMode));
            MemberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.ActionBar.SplitBackgroundDrawable.Override<ActionBar>(),
                    (actionBar, args) =>
                    {
                        if (args.NewValue is int)
                            actionBar.SetSplitBackgroundDrawable(
                                actionBar.ThemedContext.Resources.GetDrawable((int)args.NewValue));
                        else
                            actionBar.SetSplitBackgroundDrawable((Drawable)args.NewValue);
                    }));
            MemberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.ActionBar.StackedBackgroundDrawable.Override<ActionBar>(),
                    (actionBar, args) =>
                    {
                        if (args.NewValue is int)
                            actionBar.SetStackedBackgroundDrawable(
                                actionBar.ThemedContext.Resources.GetDrawable((int)args.NewValue));
                        else
                            actionBar.SetStackedBackgroundDrawable((Drawable)args.NewValue);
                    }));
            MemberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember(AttachedMembers.ActionBar.IsShowing.Override<ActionBar>(), (info, actionBar) => actionBar.IsShowing, SetActionBarIsShowing));


            MemberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember(AttachedMembers.ActionBar.Visible.Override<ActionBar>(), (info, actionBar) => actionBar.IsShowing, SetActionBarIsShowing));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ActionBar.DropDownItemTemplate.Override<ActionBar>()));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ActionBar.DropDownItemTemplateSelector.Override<ActionBar>()));
        }

        public static void RegisterActionBarTabMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ActionBarTab.Content.Override<ActionBar.Tab>()));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ActionBarTab.ContentTemplateSelector.Override<ActionBar.Tab>()));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ActionBarTab.ContentTemplate.Override<ActionBar.Tab>()));
            MemberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember<ActionBar.Tab, string>(nameof(ActionBar.Tab.ContentDescription),
                    (info, tab) => tab.ContentDescription,
                    (info, tab, value) =>
                    {
                        tab.SetContentDescription(value);
                        return true;
                    }));
            MemberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember(AttachedMembers.ActionBarTab.CustomView.Override<ActionBar.Tab>(),
                    (info, tab) => tab.CustomView, (info, tab, value) =>
                    {
                        LayoutInflaterResult result = null;
                        if (tab.CustomView != null)
                            ParentObserver.GetOrAdd(tab.CustomView).Parent = null;
                        if (value is int)
                        {
                            result = GetContextFromItem(tab).GetBindableLayoutInflater().InflateEx((int)value, null, false);
                            value = result.View;
                        }
                        tab.SetCustomView((View)value);
                        if (tab.CustomView != null)
                            ParentObserver.GetOrAdd(tab.CustomView).Parent = tab;
                        result?.ApplyBindings();
                        return true;
                    }));
            MemberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember(AttachedMembers.ActionBarTab.Icon.Override<ActionBar.Tab>(),
                    (info, tab) => tab.Icon, (info, tab, value) =>
                    {
                        if (value is int)
                            tab.SetIcon((int)value);
                        else
                            tab.SetIcon((Drawable)value);
                        return true;
                    }));
            MemberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember<ActionBar.Tab, string>(nameof(ActionBar.Tab.Text),
                    (info, tab) => tab.Text,
                    (info, tab, value) =>
                    {
                        tab.SetText(value);
                        return true;
                    }));
            MemberProvider.Register(AttachedBindingMember
                .CreateNotifiableMember<ActionBar.Tab, Object>(nameof(ActionBar.Tab.Tag),
                    (info, tab) => tab.Tag,
                    (info, tab, value) =>
                    {
                        tab.SetTag(value);
                        return true;
                    }));
        }

        private static void ActionBarContextActionBarVisibleChanged(ActionBar actionBar,
            AttachedMemberChangedEventArgs<bool> args)
        {
            var attachedValueProvider = ServiceProvider.AttachedValueProvider;

            var actionMode = attachedValueProvider.GetValue<ActionMode>(actionBar, ActionBarActionModeKey, false);
            actionMode?.Finish();
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
                            object ctx = (actionBar.SelectedNavigationIndex < 0 ? null : actionBar.SelectedTab)?.DataContext();
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
                        sourceAdapter = AndroidToolkitExtensions.ItemsSourceAdapterFactory(actionBar, actionBar.ThemedContext, DataContext.Empty);
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
                    actionBar.GetBindingMemberValue(AttachedMembersCompat.ActionBar.ItemsSourceGenerator)?.SetItemsSource(null);
                    var adapter = actionBar.GetBindingMemberValue(AttachedMembersCompat.ActionBar.ItemsSourceAdapter);
#else
                    actionBar.GetBindingMemberValue(AttachedMembers.ActionBar.ItemsSourceGenerator)?.SetItemsSource(null);
                    var adapter = actionBar.GetBindingMemberValue(AttachedMembers.ActionBar.ItemsSourceAdapter);
#endif
                    if (adapter != null)
                        adapter.ItemsSource = null;
                    break;
                case ActionBarNavigationMode.Tabs:
#if APPCOMPAT                    
                    actionBar.GetBindingMemberValue(AttachedMembersCompat.ActionBar.ItemsSourceGenerator)?.SetItemsSource(actionBar.GetBindingMemberValue(AttachedMembersCompat.ActionBar.ItemsSource));
#else                    
                    actionBar.GetBindingMemberValue(AttachedMembers.ActionBar.ItemsSourceGenerator)?.SetItemsSource(actionBar.GetBindingMemberValue(AttachedMembers.ActionBar.ItemsSource));
#endif

                    break;
            }
        }

        internal static Context GetContextFromItem(object item)
        {
            while (item != null)
            {
                var view = item as View;
                if (view != null)
                    return view.Context;
                var ctx = item as Context;
                if (ctx != null)
                    return ctx;
                item = BindingServiceProvider.VisualTreeManager.GetParent(item);
            }
            return Application.Context;
        }

        #endregion
    }
}
