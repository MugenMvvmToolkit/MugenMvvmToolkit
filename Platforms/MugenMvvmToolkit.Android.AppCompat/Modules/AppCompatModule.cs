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
using Android.Support.V7.App;
using Android.Views;
using Android.Support.V4.Widget;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using System.Collections;
using MugenMvvmToolkit.AppCompat.Infrastructure;
using Android.Support.V4.View;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models.EventArg;
using MugenMvvmToolkit.Modules;
#if APPCOMPAT
using Toolbar = Android.Support.V7.Widget.Toolbar;
#endif

namespace MugenMvvmToolkit.AppCompat.Modules
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

            public bool IsAlive
            {
                get { return true; }
            }

            public bool IsWeak
            {
                get { return true; }
            }

            public bool TryHandle(object sender, object message)
            {
                var drawer = FindDrawer(sender as View);
                if (drawer == null)
                    return true;
                DrawerListenerImpl.GetOrAdd(drawer);
                return false;
            }

            #endregion
        }

        internal sealed class DrawerListenerImpl : Java.Lang.Object, DrawerLayout.IDrawerListener
        {
            #region Fields

            private DrawerLayout.IDrawerListener _listener = null;

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
                ViewDrawerIsOpenedMember.SetValue(drawerView, false);
                if (_listener != null)
                    _listener.OnDrawerClosed(drawerView);
            }

            public void OnDrawerOpened(View drawerView)
            {
                ViewDrawerIsOpenedMember.SetValue(drawerView, true);
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

        #region Fields

        private static readonly IAttachedBindingMemberInfo<ViewPager, object> ViewPagerSelectedItemMember;
        private static readonly IAttachedBindingMemberInfo<ViewPager, int> ViewPagerCurrentItemMember;
        private static readonly IAttachedBindingMemberInfo<View, bool> ViewDrawerIsOpenedMember;

        #endregion

        #region Constructors

        static AppCompatModule()
        {
            ViewPagerSelectedItemMember = AttachedBindingMember.CreateAutoProperty<ViewPager, object>(AttachedMemberConstants.SelectedItem, ViewPagerSelectedItemChanged);
            ViewPagerCurrentItemMember = AttachedBindingMember.CreateAutoProperty<ViewPager, int>("CurrentItem",
                ViewPagerCurrentItemChanged, AdapterViewCurrentItemAttached, (pager, info) => pager.CurrentItem);
            ViewDrawerIsOpenedMember = AttachedBindingMember.CreateAutoProperty<View, bool>("Drawer.IsOpened", ViewDrawerIsOpenedChanged, getDefaultValue: ViewDrawerIsOpenedGetDefaultValue);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AppCompatModule" /> class.
        /// </summary>
        public AppCompatModule()
            : base(true, priority: BindingModulePriority - 1)
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
            ViewPagerSelectedItemMember.SetValue(sender, item);
        }

        private static void ViewPagerSelectedItemChanged(ViewPager sender, AttachedMemberChangedEventArgs<object> args)
        {
            var adapter = sender.Adapter as ItemsSourcePagerAdapter;
            if (adapter == null)
                return;
            int position = adapter.GetPosition(args.NewValue);
            ViewPagerCurrentItemMember.SetValue(sender, position);
        }

        private static void AdapterViewCurrentItemAttached(ViewPager adapterView, MemberAttachedEventArgs memberAttached)
        {
            adapterView.PageSelected += (sender, args) => ViewPagerCurrentItemMember.SetValue(adapterView, args.Position);
        }

        private static void ToolbarMenuTemplateChanged(Toolbar toolbar, AttachedMemberChangedEventArgs<int> args)
        {
            var activity = toolbar.Context.GetActivity();
            if (activity != null)
                activity.MenuInflater.Inflate(args.NewValue, toolbar.Menu, toolbar);
        }

        private static void ViewDrawerIsOpenedChanged(View view, AttachedMemberChangedEventArgs<bool> args)
        {
            var drawer = FindDrawer(view);
            if (drawer == null)
                return;
            if (args.NewValue)
                drawer.OpenDrawer(view);
            else
                drawer.CloseDrawer(view);
        }

        private static bool ViewDrawerIsOpenedGetDefaultValue(View view, IBindingMemberInfo bindingMemberInfo)
        {
            var drawer = FindDrawer(view);
            if (drawer == null)
            {
                var rootMember = BindingServiceProvider.VisualTreeManager.GetRootMember(view.GetType());
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

        /// <summary>
        ///     Loads the current module.
        /// </summary>
        protected override bool LoadInternal()
        {
            var memberProvider = BindingServiceProvider.MemberProvider;

            //View
            memberProvider.Register(ViewDrawerIsOpenedMember);

            //Toolbar
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<Toolbar, int>(AttachedMemberNames.MenuTemplate, ToolbarMenuTemplateChanged));

            //DrawerLayout
            var actionBarDrawerToggleEnabledMember = AttachedBindingMember.CreateAutoProperty<DrawerLayout, bool>("ActionBarDrawerToggleEnabled",
                (layout, args) =>
                {
                    if (!args.NewValue)
                        return;
                    var activity = layout.Context.GetActivity();
                    if (activity == null)
                        return;
                    DrawerListenerImpl
                        .GetOrAdd(layout)
                        .SetListener(activity, new ActionBarDrawerToggle(activity, layout, Resource.String.Empty, Resource.String.Empty));
                });
            memberProvider.Register(actionBarDrawerToggleEnabledMember);
            memberProvider.Register(typeof(DrawerLayout), "ActionBarDrawerEnabled", actionBarDrawerToggleEnabledMember, true);
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<DrawerLayout, object>("DrawerListener",
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
            memberProvider.Register(ViewPagerSelectedItemMember);
            memberProvider.Register(ViewPagerCurrentItemMember);
            memberProvider.Register(typeof(ViewPager), "SelectedIndex", ViewPagerCurrentItemMember, true);
            memberProvider.Register(
                AttachedBindingMember.CreateAutoProperty<ViewPager, IEnumerable>(AttachedMemberConstants.ItemsSource,
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

        /// <summary>
        ///     Unloads the current module.
        /// </summary>
        protected override void UnloadInternal()
        {
        }

        #endregion
    }
}