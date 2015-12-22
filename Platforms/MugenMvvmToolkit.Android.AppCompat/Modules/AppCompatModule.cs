#region Copyright

// ****************************************************************************
// <copyright file="AppCompatModule.cs">
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
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Support.V7.App;
using Android.Support.V7.Widget;
using Android.Views;
using MugenMvvmToolkit.Android.AppCompat.Infrastructure;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Android.Interfaces.Views;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Modules;
using Object = Java.Lang.Object;

namespace MugenMvvmToolkit.Android.AppCompat.Modules
{
    public class AppCompatModule : ModuleBase
    {
        #region Nested types

        private sealed class DrawerInitializer : IEventListener
        {
            #region Fields

            public static readonly DrawerInitializer Instance;

            #endregion

            #region Constructors

            static DrawerInitializer()
            {
                Instance = new DrawerInitializer();
            }

            private DrawerInitializer()
            {
            }

            #endregion

            #region Implementation of IEventListener

            public bool IsAlive => true;

            public bool IsWeak => true;

            public bool TryHandle(object sender, object message)
            {
                DrawerLayout drawer = FindDrawer(sender as View);
                if (drawer == null)
                    return true;
                DrawerListenerImpl.GetOrAdd(drawer);
                return false;
            }

            #endregion
        }

        internal sealed class DrawerListenerImpl : Object, DrawerLayout.IDrawerListener
        {
            #region Fields

            private DrawerLayout.IDrawerListener _listener;

            #endregion

            #region Constructors

            private DrawerListenerImpl()
            {
            }

            private DrawerListenerImpl(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
            {
            }

            #endregion

            #region Methods

            public static DrawerListenerImpl GetOrAdd(DrawerLayout drawer)
            {
                return ServiceProvider.AttachedValueProvider.GetOrAdd(drawer, "!@!@dralist", (layout, o) =>
                {
                    var listenerImpl = new DrawerListenerImpl();
                    layout.SetDrawerListener(listenerImpl);
                    return listenerImpl;
                }, null);
            }

            public void SetListener(Context context, DrawerLayout.IDrawerListener listener)
            {
                _listener = listener;
                if (!(listener is ActionBarDrawerToggle))
                    return;
                var activity = context.GetActivity() as IActivityView;
                if (activity == null)
                    return;
                activity.Mediator.OptionsItemSelected += OptionsItemSelected;
                activity.Mediator.ConfigurationChanged += OnConfigurationChanged;
                activity.Mediator.PostCreate += OnPostCreate;
            }

            private void OnPostCreate(Activity sender, ValueEventArgs<Bundle> args)
            {
                var drawerToggle = _listener as ActionBarDrawerToggle;
                if (drawerToggle != null)
                    drawerToggle.SyncState();
            }

            private void OnConfigurationChanged(Activity sender, ValueEventArgs<Configuration> args)
            {
                var drawerToggle = _listener as ActionBarDrawerToggle;
                if (drawerToggle != null)
                    drawerToggle.OnConfigurationChanged(args.Value);
            }

            private bool OptionsItemSelected(IMenuItem menuItem)
            {
                var drawerToggle = _listener as ActionBarDrawerToggle;
                if (drawerToggle != null)
                    return drawerToggle.OnOptionsItemSelected(menuItem);
                return false;
            }

            #endregion

            #region Implementation of IDrawerListener

            public void OnDrawerClosed(View drawerView)
            {
                drawerView.SetBindingMemberValue(AttachedMembersCompat.View.DrawerIsOpened, false);
                if (_listener != null)
                    _listener.OnDrawerClosed(drawerView);
            }

            public void OnDrawerOpened(View drawerView)
            {
                drawerView.SetBindingMemberValue(AttachedMembersCompat.View.DrawerIsOpened, true);
                if (_listener != null)
                    _listener.OnDrawerOpened(drawerView);
            }

            public void OnDrawerSlide(View drawerView, float slideOffset)
            {
                if (_listener != null)
                    _listener.OnDrawerSlide(drawerView, slideOffset);
            }

            public void OnDrawerStateChanged(int newState)
            {
                if (_listener != null)
                    _listener.OnDrawerStateChanged(newState);
            }

            #endregion
        }

        #endregion

        #region Constructors

        public AppCompatModule()
            : base(true, priority: BindingModulePriority - 2)
        {
        }

        #endregion

        #region Methods

        private static void ViewPagerCurrentItemChanged(ViewPager sender, AttachedMemberChangedEventArgs<int> args)
        {
            sender.CurrentItem = args.NewValue;
            var adapter = sender.Adapter as ItemsSourcePagerAdapter;
            if (adapter == null)
                return;
            object item = adapter.GetRawItem(sender.CurrentItem);
            sender.SetBindingMemberValue(AttachedMembersCompat.ViewPager.SelectedItem, item);
        }

        private static void ViewPagerSelectedItemChanged(ViewPager sender, AttachedMemberChangedEventArgs<object> args)
        {
            var adapter = sender.Adapter as ItemsSourcePagerAdapter;
            if (adapter == null)
                return;
            int position = adapter.GetPosition(args.NewValue);
            sender.SetBindingMemberValue(AttachedMembersCompat.ViewPager.CurrentItem, position);
        }

        private static void AdapterViewCurrentItemAttached(ViewPager adapterView, MemberAttachedEventArgs memberAttached)
        {
            adapterView.PageSelected += (sender, args) => ((ViewPager)sender).SetBindingMemberValue(AttachedMembersCompat.ViewPager.CurrentItem, args.Position);
        }

        private static void ToolbarMenuTemplateChanged(Toolbar toolbar, AttachedMemberChangedEventArgs<int> args)
        {
            Activity activity = toolbar.Context.GetActivity();
            if (activity != null)
                activity.MenuInflater.Inflate(args.NewValue, toolbar.Menu, toolbar);
        }

        private static void ToolbarIsActionBarChanged(Toolbar toolbar, AttachedMemberChangedEventArgs<bool> args)
        {
            if (!args.NewValue)
                return;
            var activity = toolbar.Context.GetActivity() as AppCompatActivity;
            if (activity != null)
                activity.SetSupportActionBar(toolbar);
        }

        private static void ViewDrawerIsOpenedChanged(View view, AttachedMemberChangedEventArgs<bool> args)
        {
            DrawerLayout drawer = FindDrawer(view);
            if (drawer == null)
                return;
            if (args.NewValue)
                drawer.OpenDrawer(view);
            else
                drawer.CloseDrawer(view);
        }

        private static bool ViewDrawerIsOpenedGetDefaultValue(View view, IBindingMemberInfo bindingMemberInfo)
        {
            DrawerLayout drawer = FindDrawer(view);
            if (drawer == null)
            {
                IBindingMemberInfo rootMember = BindingServiceProvider.VisualTreeManager.GetRootMember(view.GetType());
                if (rootMember != null)
                    rootMember.TryObserve(view, DrawerInitializer.Instance);
                return false;
            }
            DrawerListenerImpl.GetOrAdd(drawer);
            return drawer.IsDrawerOpen(view);
        }

        private static DrawerLayout FindDrawer(View view)
        {
            while (view != null)
            {
                var drawer = view.Parent as DrawerLayout;
                if (drawer != null)
                    return drawer;
                view = view.Parent as View;
            }
            return null;
        }

        #endregion

        #region Overrides of ModuleBase

        protected override bool LoadInternal()
        {
            IBindingMemberProvider memberProvider = BindingServiceProvider.MemberProvider;

            //View
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembersCompat.View.DrawerIsOpened,
                ViewDrawerIsOpenedChanged, getDefaultValue: ViewDrawerIsOpenedGetDefaultValue));

            //Toolbar
            memberProvider.Register(
                AttachedBindingMember.CreateAutoProperty(AttachedMembersCompat.Toolbar.MenuTemplate,
                    ToolbarMenuTemplateChanged));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembersCompat.Toolbar.IsActionBar,
                ToolbarIsActionBarChanged));

            //DrawerLayout
            INotifiableAttachedBindingMemberInfo<DrawerLayout, bool> actionBarDrawerToggleEnabledMember =
                AttachedBindingMember.CreateAutoProperty(AttachedMembersCompat.DrawerLayout.ActionBarDrawerToggleEnabled,
                    (layout, args) =>
                    {
                        if (!args.NewValue)
                            return;
                        Activity activity = layout.Context.GetActivity();
                        if (activity != null)
                            DrawerListenerImpl
                                .GetOrAdd(layout)
                                .SetListener(activity, new ActionBarDrawerToggle(activity, layout, Resource.String.Empty, Resource.String.Empty));
                    });
            memberProvider.Register(actionBarDrawerToggleEnabledMember);
            memberProvider.Register(typeof(DrawerLayout), "ActionBarDrawerEnabled", actionBarDrawerToggleEnabledMember, true);
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembersCompat.DrawerLayout.DrawerListener,
                (layout, args) =>
                {
                    var listener = args.NewValue as DrawerLayout.IDrawerListener;
                    if (listener == null)
                    {
                        var func = args.NewValue as Func<DrawerLayout, DrawerLayout.IDrawerListener>;
                        if (func != null)
                            listener = func(layout);
                    }
                    if (listener == null)
                        Tracer.Warn("The DrawerListener '{0}' is not supported", args.NewValue);
                    DrawerListenerImpl.GetOrAdd(layout).SetListener(layout.Context, listener);
                }));


            //ViewPager
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembersCompat.ViewPager.SelectedItem, ViewPagerSelectedItemChanged));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembersCompat.ViewPager.GetPageTitleDelegate));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembersCompat.ViewPager.RestoreSelectedIndex));
            var itemMember = AttachedBindingMember.CreateAutoProperty(AttachedMembersCompat.ViewPager.CurrentItem,
                ViewPagerCurrentItemChanged, AdapterViewCurrentItemAttached, (pager, info) => pager.CurrentItem);
            memberProvider.Register(itemMember);
            memberProvider.Register(typeof(ViewPager), "SelectedIndex", itemMember, true);
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.ItemsSource.Override<ViewPager>(),
                    (pager, args) =>
                    {
                        var pagerAdapter = pager.Adapter as ItemsSourcePagerAdapter;
                        if (pagerAdapter == null)
                        {
                            pagerAdapter = new ItemsSourcePagerAdapter(pager);
                            pager.Adapter = pagerAdapter;
                        }
                        pagerAdapter.ItemsSource = args.NewValue;
                    }));
            return true;
        }

        protected override void UnloadInternal()
        {
        }

        #endregion
    }
}
