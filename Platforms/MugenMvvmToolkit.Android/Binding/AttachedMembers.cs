#region Copyright

// ****************************************************************************
// <copyright file="AttachedMembers.cs">
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
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Android.Interfaces;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Interfaces.Models;
#if APPCOMPAT
using Java.Lang;
using MugenMvvmToolkit.Android.Binding;
using ActionBarEx = Android.Support.V7.App.ActionBar;
using TolbarEx = Android.Support.V7.Widget.Toolbar;
using Object = System.Object;
namespace MugenMvvmToolkit.Android.AppCompat
{
    public static class AttachedMembersCompat
#else
using Android.Views;
using System.Collections.Generic;
using ActionBarEx = Android.App.ActionBar;
using TolbarEx = Android.Widget.Toolbar;
namespace MugenMvvmToolkit.Android.Binding
{
    public static class AttachedMembers
#endif
    {
        #region Nested types

#if !APPCOMPAT
        public abstract class Object
        {
            #region Fields

            public static readonly BindingMemberDescriptor<object, object> DataContext;
            public static BindingMemberDescriptor<object, object> Parent;
            public static readonly BindingMemberDescriptor<object, object> CommandParameter;
            public static readonly BindingMemberDescriptor<object, IEnumerable<object>> Errors;
            public static readonly BindingMemberDescriptor<object, IStableIdProvider> StableIdProvider;

            #endregion

            #region Constructors

            static Object()
            {
                DataContext = new BindingMemberDescriptor<object, object>(AttachedMemberConstants.DataContext);
                Parent = new BindingMemberDescriptor<object, object>(AttachedMemberConstants.Parent);
                CommandParameter = new BindingMemberDescriptor<object, object>(AttachedMemberConstants.CommandParameter);
                Errors = new BindingMemberDescriptor<object, IEnumerable<object>>(AttachedMemberConstants.ErrorsPropertyMember);
                StableIdProvider = new BindingMemberDescriptor<object, IStableIdProvider>("StableIdProvider");
            }

            #endregion
        }

        public abstract class Activity : AttachedMembers.Object
        {
            #region Fields

            public static readonly BindingMemberDescriptor<global::Android.App.Activity, IDataTemplateSelector> ToastTemplateSelector;
            public static readonly BindingMemberDescriptor<global::Android.App.Activity, Action<Context, Intent, IDataContext>> StartActivityDelegate;
            #endregion

            #region Constructors

            static Activity()
            {
                ToastTemplateSelector = new BindingMemberDescriptor<global::Android.App.Activity, IDataTemplateSelector>("ToastTemplateSelector");
                StartActivityDelegate = new BindingMemberDescriptor<global::Android.App.Activity, Action<Context, Intent, IDataContext>>("StartActivityDelegate");
            }

            #endregion
        }

        public abstract class View : Object
        {
            #region Fields

            public static readonly BindingMemberDescriptor<global::Android.Views.View, bool> Visible;
            public static readonly BindingMemberDescriptor<global::Android.Views.View, bool> Hidden;
            public static readonly BindingMemberDescriptor<global::Android.Views.View, int> PopupMenuTemplate;
            public static readonly BindingMemberDescriptor<global::Android.Views.View, IPopupMenuPresenter> PopupMenuPresenter;
            public static readonly BindingMemberDescriptor<global::Android.Views.View, string> PopupMenuEvent;
            public static readonly BindingMemberDescriptor<global::Android.Views.View, string> PopupMenuPlacementTargetPath;
            public static readonly BindingMemberDescriptor<global::Android.Views.View, object> Fragment;

            #endregion

            #region Constructors

            static View()
            {
                Visible = new BindingMemberDescriptor<global::Android.Views.View, bool>("Visible");
                Hidden = new BindingMemberDescriptor<global::Android.Views.View, bool>("Hidden");
                PopupMenuTemplate = new BindingMemberDescriptor<global::Android.Views.View, int>("PopupMenuTemplate");
                PopupMenuEvent = new BindingMemberDescriptor<global::Android.Views.View, string>("PopupMenuEvent");
                PopupMenuPlacementTargetPath = new BindingMemberDescriptor<global::Android.Views.View, string>("PlacementTargetPath");
                PopupMenuPresenter = new BindingMemberDescriptor<global::Android.Views.View, IPopupMenuPresenter>("PopupMenuPresenter");
                Fragment = new BindingMemberDescriptor<global::Android.Views.View, object>("Fragment");
            }

            #endregion
        }

        public abstract class Dialog : Object
        {
            #region Fields

            public static readonly BindingMemberDescriptor<global::Android.App.Dialog, string> Title;

            #endregion

            #region Constructors

            static Dialog()
            {
                Title = new BindingMemberDescriptor<global::Android.App.Dialog, string>("Title");
            }

            #endregion
        }

        public abstract class ViewGroup : View
        {
            #region Fields

            public static readonly BindingMemberDescriptor<global::Android.Views.ViewGroup, object> Content;
            public static readonly BindingMemberDescriptor<global::Android.Views.ViewGroup, IContentViewManager> ContentViewManager;
            public static readonly BindingMemberDescriptor<global::Android.Views.ViewGroup, int?> ContentTemplate;
            public static readonly BindingMemberDescriptor<global::Android.Views.ViewGroup, IDataTemplateSelector> ContentTemplateSelector;
            public static readonly BindingMemberDescriptor<global::Android.Views.ViewGroup, bool> AddToBackStack;

            public static readonly BindingMemberDescriptor<global::Android.Views.ViewGroup, IEnumerable> ItemsSource;
            public static readonly BindingMemberDescriptor<global::Android.Views.ViewGroup, IItemsSourceGenerator> ItemsSourceGenerator;
            public static readonly BindingMemberDescriptor<global::Android.Views.ViewGroup, int?> ItemTemplate;
            public static readonly BindingMemberDescriptor<global::Android.Views.ViewGroup, IDataTemplateSelector> ItemTemplateSelector;
            public static readonly BindingMemberDescriptor<global::Android.Views.ViewGroup, ICollectionViewManager> CollectionViewManager;
            public static readonly BindingMemberDescriptor<global::Android.Views.ViewGroup, bool> DisableHierarchyListener;

            #endregion

            #region Constructors

            static ViewGroup()
            {
                Content = new BindingMemberDescriptor<global::Android.Views.ViewGroup, object>(AttachedMemberConstants.Content);
                ContentTemplate = new BindingMemberDescriptor<global::Android.Views.ViewGroup, int?>(AttachedMemberConstants.ContentTemplate);
                ContentTemplateSelector = new BindingMemberDescriptor<global::Android.Views.ViewGroup, IDataTemplateSelector>(AttachedMemberConstants.ContentTemplateSelector);
                ContentViewManager = new BindingMemberDescriptor<global::Android.Views.ViewGroup, IContentViewManager>("ContentViewManager");
                AddToBackStack = new BindingMemberDescriptor<global::Android.Views.ViewGroup, bool>("AddToBackStack");
                DisableHierarchyListener = new BindingMemberDescriptor<global::Android.Views.ViewGroup, bool>("DisableHierarchyListener");

                ItemsSource = new BindingMemberDescriptor<global::Android.Views.ViewGroup, IEnumerable>(AttachedMemberConstants.ItemsSource);
                ItemsSourceGenerator = new BindingMemberDescriptor<global::Android.Views.ViewGroup, IItemsSourceGenerator>(ItemsSourceGeneratorBase.MemberDescriptor);
                ItemTemplate = new BindingMemberDescriptor<global::Android.Views.ViewGroup, int?>(AttachedMemberConstants.ItemTemplate);
                ItemTemplateSelector = new BindingMemberDescriptor<global::Android.Views.ViewGroup, IDataTemplateSelector>(AttachedMemberConstants.ItemTemplateSelector);
                CollectionViewManager = new BindingMemberDescriptor<global::Android.Views.ViewGroup, ICollectionViewManager>("CollectionViewManager");
            }

            #endregion
        }

        public abstract class AdapterView : ViewGroup
        {
            #region Fields

            public static readonly BindingMemberDescriptor<global::Android.Widget.AdapterView, int?> DropDownItemTemplate;
            public static readonly BindingMemberDescriptor<global::Android.Widget.AdapterView, IDataTemplateSelector> DropDownItemTemplateSelector;
            public static readonly BindingMemberDescriptor<global::Android.Widget.AdapterView, int> SelectedItemPosition;
            public static readonly BindingMemberDescriptor<global::Android.Widget.AdapterView, object> SelectedItem;
            public static readonly BindingMemberDescriptor<global::Android.Widget.AdapterView, bool?> ScrollToSelectedItem;

            #endregion

            #region Constructors

            static AdapterView()
            {
                DropDownItemTemplate = new BindingMemberDescriptor<global::Android.Widget.AdapterView, int?>("DropDownItemTemplate");
                DropDownItemTemplateSelector = new BindingMemberDescriptor<global::Android.Widget.AdapterView, IDataTemplateSelector>("DropDownItemTemplateSelector");
                SelectedItemPosition = new BindingMemberDescriptor<global::Android.Widget.AdapterView, int>("SelectedItemPosition");
                SelectedItem = new BindingMemberDescriptor<global::Android.Widget.AdapterView, object>(AttachedMemberConstants.SelectedItem);
                ScrollToSelectedItem = new BindingMemberDescriptor<global::Android.Widget.AdapterView, bool?>("ScrollToSelectedItem");
            }

            #endregion
        }

        public abstract class TabHost : ViewGroup
        {
            #region Fields

            public static readonly BindingMemberDescriptor<global::Android.Widget.TabHost, object> SelectedItem;
            public static readonly BindingMemberDescriptor<global::Android.Widget.TabHost, bool?> RestoreSelectedIndex;

            #endregion

            #region Constructors

            static TabHost()
            {
                SelectedItem = new BindingMemberDescriptor<global::Android.Widget.TabHost, object>(AttachedMemberConstants.SelectedItem);
                RestoreSelectedIndex = new BindingMemberDescriptor<global::Android.Widget.TabHost, bool?>("RestoreSelectedIndex");
            }

            #endregion
        }

        public abstract class TabSpec : Object
        {
            #region Fields

            public static readonly BindingMemberDescriptor<global::Android.Widget.TabHost.TabSpec, string> Title;

            #endregion

            #region Constructors

            static TabSpec()
            {
                Title = new BindingMemberDescriptor<global::Android.Widget.TabHost.TabSpec, string>("Title");
            }

            #endregion
        }

        public abstract class AutoCompleteTextView : View
        {
            #region Fields

            public static readonly BindingMemberDescriptor<global::Android.Widget.AutoCompleteTextView, int?> ItemTemplate;
            public static readonly BindingMemberDescriptor<global::Android.Widget.AutoCompleteTextView, IDataTemplateSelector> ItemTemplateSelector;
            public static readonly BindingMemberDescriptor<global::Android.Widget.AutoCompleteTextView, IEnumerable> ItemsSource;

            #endregion

            #region Constructors

            static AutoCompleteTextView()
            {
                ItemTemplate = ViewGroup.ItemTemplate.Override<global::Android.Widget.AutoCompleteTextView>();
                ItemTemplateSelector = ViewGroup.ItemTemplateSelector.Override<global::Android.Widget.AutoCompleteTextView>();
                ItemsSource = ViewGroup.ItemsSource.Override<global::Android.Widget.AutoCompleteTextView>();
            }

            #endregion
        }

        public abstract class DatePicker : View
        {
            #region Fields

            public static readonly BindingMemberDescriptor<global::Android.Widget.DatePicker, DateTime> SelectedDate;

            #endregion

            #region Constructors

            static DatePicker()
            {
                SelectedDate = new BindingMemberDescriptor<global::Android.Widget.DatePicker, DateTime>("SelectedDate");
            }

            #endregion
        }

        public abstract class TimePicker : View
        {
            #region Fields

            public static readonly BindingMemberDescriptor<global::Android.Widget.TimePicker, TimeSpan> SelectedTime;

            #endregion

            #region Constructors

            static TimePicker()
            {
                SelectedTime = new BindingMemberDescriptor<global::Android.Widget.TimePicker, TimeSpan>("SelectedTime");
            }

            #endregion
        }

        public abstract class ImageView : View
        {
            #region Fields

            public static readonly BindingMemberDescriptor<global::Android.Widget.ImageView, object> ImageSource;

            #endregion

            #region Constructors

            static ImageView()
            {
                ImageSource = new BindingMemberDescriptor<global::Android.Widget.ImageView, object>("ImageSource");
            }

            #endregion
        }

        public abstract class Menu : Object
        {
            #region Fields

            public static readonly BindingMemberDescriptor<IMenu, IItemsSourceGenerator> ItemsSourceGenerator;
            public static readonly BindingMemberDescriptor<IMenu, IEnumerable> ItemsSource;
            public static readonly BindingMemberDescriptor<IMenu, bool?> Enabled;
            public static readonly BindingMemberDescriptor<IMenu, bool?> Visible;

            #endregion

            #region Constructors

            static Menu()
            {
                ItemsSource = new BindingMemberDescriptor<IMenu, IEnumerable>(AttachedMemberConstants.ItemsSource);
                ItemsSourceGenerator = ItemsSourceGeneratorBase.MemberDescriptor.Override<IMenu>();
                Enabled = new BindingMemberDescriptor<IMenu, bool?>(AttachedMemberConstants.Enabled);
                Visible = new BindingMemberDescriptor<IMenu, bool?>("Visible");
            }

            #endregion
        }

        public abstract class MenuItem : Object
        {
            #region Fields

            public static readonly BindingMemberDescriptor<IMenuItem, object> ActionView;
            public static readonly BindingMemberDescriptor<IMenuItem, IDataTemplateSelector> ActionViewTemplateSelector;
            public static readonly BindingMemberDescriptor<IMenuItem, object> ActionProvider;
            public static readonly BindingMemberDescriptor<IMenuItem, IDataTemplateSelector> ActionProviderTemplateSelector;
            public static readonly BindingMemberDescriptor<IMenuItem, bool> IsActionViewExpanded;
            public static readonly BindingMemberDescriptor<IMenuItem, ShowAsAction> ShowAsAction;
            public static readonly BindingMemberDescriptor<IMenuItem, bool> IsChecked;
            public static readonly BindingMemberDescriptor<IMenuItem, IEventListener> Click;
            public static readonly BindingMemberDescriptor<IMenuItem, object> Icon;
            public static readonly BindingMemberDescriptor<IMenuItem, string> Title;
            public static readonly BindingMemberDescriptor<IMenuItem, string> TitleCondensed;

            #endregion

            #region Constructors

            static MenuItem()
            {
                Icon = new BindingMemberDescriptor<IMenuItem, object>("Icon");
                Title = new BindingMemberDescriptor<IMenuItem, string>("Title");
                TitleCondensed = new BindingMemberDescriptor<IMenuItem, string>("TitleCondensed");
                ActionView = new BindingMemberDescriptor<IMenuItem, object>("ActionView");
                ActionViewTemplateSelector = new BindingMemberDescriptor<IMenuItem, IDataTemplateSelector>("ActionViewTemplateSelector");
                ActionProvider = new BindingMemberDescriptor<IMenuItem, object>("ActionProvider");
                ActionProviderTemplateSelector = new BindingMemberDescriptor<IMenuItem, IDataTemplateSelector>("ActionProviderTemplateSelector");
                IsActionViewExpanded = new BindingMemberDescriptor<IMenuItem, bool>("IsActionViewExpanded");
                ShowAsAction = new BindingMemberDescriptor<IMenuItem, ShowAsAction>("ShowAsAction");
                IsChecked = new BindingMemberDescriptor<IMenuItem, bool>("IsChecked");
                Click = new BindingMemberDescriptor<IMenuItem, IEventListener>("Click");
            }

            #endregion
        }

        public abstract class Preference : Object
        {
            #region Fields

            public static readonly BindingMemberDescriptor<global::Android.Preferences.Preference, bool> Click;
            public static readonly BindingMemberDescriptor<global::Android.Preferences.Preference, IEventListener> ValueChangedEvent;

            #endregion

            #region Constructors

            static Preference()
            {
                Click = new BindingMemberDescriptor<global::Android.Preferences.Preference, bool>("PreferenceClick");
                ValueChangedEvent = new BindingMemberDescriptor<global::Android.Preferences.Preference, IEventListener>("ValueChanged");
            }

            #endregion
        }

        public abstract class PreferenceGroup : Preference
        {
            #region Fields

            public static readonly BindingMemberDescriptor<global::Android.Preferences.PreferenceGroup, IEnumerable> ItemsSource;
            public static readonly BindingMemberDescriptor<global::Android.Preferences.PreferenceGroup, IItemsSourceGenerator> ItemsSourceGenerator;
            public static readonly BindingMemberDescriptor<global::Android.Preferences.PreferenceGroup, IDataTemplateSelector> ItemTemplateSelector;
            public static readonly BindingMemberDescriptor<global::Android.Preferences.PreferenceGroup, ICollectionViewManager> CollectionViewManager;

            #endregion

            #region Constructors

            static PreferenceGroup()
            {
                ItemsSource = new BindingMemberDescriptor<global::Android.Preferences.PreferenceGroup, IEnumerable>(AttachedMemberConstants.ItemsSource);
                ItemsSourceGenerator = ItemsSourceGeneratorBase.MemberDescriptor.Override<global::Android.Preferences.PreferenceGroup>();
                ItemTemplateSelector = new BindingMemberDescriptor<global::Android.Preferences.PreferenceGroup, IDataTemplateSelector>(AttachedMemberConstants.ItemTemplateSelector);
                CollectionViewManager = ViewGroup.CollectionViewManager.Override<global::Android.Preferences.PreferenceGroup>();
            }

            #endregion
        }
#else
        public abstract class View : AttachedMembers.View
        {
        #region Fields

            public static readonly BindingMemberDescriptor<global::Android.Views.View, bool> DrawerIsOpened;

        #endregion

        #region Constructors

            static View()
            {
                DrawerIsOpened = new BindingMemberDescriptor<global::Android.Views.View, bool>("Drawer.IsOpened");
            }

        #endregion
        }

        public abstract class ViewPager : AttachedMembers.ViewGroup
        {
        #region Fields

            public static readonly BindingMemberDescriptor<global::Android.Support.V4.View.ViewPager, object> SelectedItem;
            public static readonly BindingMemberDescriptor<global::Android.Support.V4.View.ViewPager, int> CurrentItem;
            public static readonly BindingMemberDescriptor<global::Android.Support.V4.View.ViewPager, bool?> RestoreSelectedIndex;
            public static readonly BindingMemberDescriptor<global::Android.Support.V4.View.ViewPager, Func<object, ICharSequence>> GetPageTitleDelegate;

        #endregion

        #region Constructors

            static ViewPager()
            {
                SelectedItem = new BindingMemberDescriptor<global::Android.Support.V4.View.ViewPager, object>(AttachedMemberConstants.SelectedItem);
                CurrentItem = new BindingMemberDescriptor<global::Android.Support.V4.View.ViewPager, int>("CurrentItem");
                GetPageTitleDelegate = new BindingMemberDescriptor<global::Android.Support.V4.View.ViewPager, Func<object, ICharSequence>>("GetPageTitle");
                RestoreSelectedIndex = AttachedMembers.TabHost.RestoreSelectedIndex.Override<global::Android.Support.V4.View.ViewPager>();
            }

        #endregion
        }

        public abstract class DrawerLayout : AttachedMembers.ViewGroup
        {
        #region Fields

            public static readonly BindingMemberDescriptor<global::Android.Support.V4.Widget.DrawerLayout, bool> ActionBarDrawerToggleEnabled;
            public static readonly BindingMemberDescriptor<global::Android.Support.V4.Widget.DrawerLayout, object> DrawerListener;

        #endregion

        #region Constructors

            static DrawerLayout()
            {
                ActionBarDrawerToggleEnabled = new BindingMemberDescriptor<global::Android.Support.V4.Widget.DrawerLayout, bool>("ActionBarDrawerToggleEnabled");
                DrawerListener = new BindingMemberDescriptor<global::Android.Support.V4.Widget.DrawerLayout, object>("DrawerListener");
            }

        #endregion
        }
#endif
        public abstract class ActionBar : Object
        {
            #region Fields

            public static readonly BindingMemberDescriptor<ActionBarEx, IEnumerable> ItemsSource;
            public static readonly BindingMemberDescriptor<ActionBarEx, IItemsSourceAdapter> ItemsSourceAdapter;
            public static readonly BindingMemberDescriptor<ActionBarEx, object> SelectedItem;
            public static readonly BindingMemberDescriptor<ActionBarEx, IItemsSourceGenerator> ItemsSourceGenerator;
            public static readonly BindingMemberDescriptor<ActionBarEx, int?> ItemTemplate;
            public static readonly BindingMemberDescriptor<ActionBarEx, IDataTemplateSelector> ItemTemplateSelector;
            public static readonly BindingMemberDescriptor<ActionBarEx, int?> DropDownItemTemplate;
            public static readonly BindingMemberDescriptor<ActionBarEx, IDataTemplateSelector> DropDownItemTemplateSelector;
            public static readonly BindingMemberDescriptor<ActionBarEx, ICollectionViewManager> CollectionViewManager;
            public static readonly BindingMemberDescriptor<ActionBarEx, int?> ContextActionBarTemplate;
            public static readonly BindingMemberDescriptor<ActionBarEx, bool> ContextActionBarVisible;
            public static readonly BindingMemberDescriptor<ActionBarEx, object> BackgroundDrawable;
            public static readonly BindingMemberDescriptor<ActionBarEx, object> CustomView;
            public static readonly BindingMemberDescriptor<ActionBarEx, bool> DisplayHomeAsUpEnabled;
            public static readonly BindingMemberDescriptor<ActionBarEx, ActionBarDisplayOptions> DisplayOptions;
            public static readonly BindingMemberDescriptor<ActionBarEx, bool> DisplayShowCustomEnabled;
            public static readonly BindingMemberDescriptor<ActionBarEx, bool> DisplayShowHomeEnabled;
            public static readonly BindingMemberDescriptor<ActionBarEx, bool> DisplayShowTitleEnabled;
            public static readonly BindingMemberDescriptor<ActionBarEx, bool> DisplayUseLogoEnabled;
            public static readonly BindingMemberDescriptor<ActionBarEx, bool> HomeButtonEnabled;
            public static readonly BindingMemberDescriptor<ActionBarEx, IEventListener> HomeButtonClick;
            public static readonly BindingMemberDescriptor<ActionBarEx, object> Icon;
            public static readonly BindingMemberDescriptor<ActionBarEx, object> Logo;
            public static readonly BindingMemberDescriptor<ActionBarEx, ActionBarNavigationMode> NavigationMode;
            public static readonly BindingMemberDescriptor<ActionBarEx, object> SplitBackgroundDrawable;
            public static readonly BindingMemberDescriptor<ActionBarEx, object> StackedBackgroundDrawable;
            public static readonly BindingMemberDescriptor<ActionBarEx, bool> IsShowing;
            public static readonly BindingMemberDescriptor<ActionBarEx, bool> Visible;

            #endregion

            #region Constructors

            static ActionBar()
            {
                BackgroundDrawable = new BindingMemberDescriptor<ActionBarEx, object>("BackgroundDrawable");
                CustomView = new BindingMemberDescriptor<ActionBarEx, object>("CustomView");
                ContextActionBarTemplate = new BindingMemberDescriptor<ActionBarEx, int?>("ContextActionBarTemplate");
                ContextActionBarVisible = new BindingMemberDescriptor<ActionBarEx, bool>("ContextActionBarVisible");
                DisplayHomeAsUpEnabled = new BindingMemberDescriptor<ActionBarEx, bool>("DisplayHomeAsUpEnabled");
                DisplayOptions = new BindingMemberDescriptor<ActionBarEx, ActionBarDisplayOptions>("DisplayOptions");
                DisplayShowCustomEnabled = new BindingMemberDescriptor<ActionBarEx, bool>("DisplayShowCustomEnabled");
                DisplayShowHomeEnabled = new BindingMemberDescriptor<ActionBarEx, bool>("DisplayShowHomeEnabled");
                DisplayShowTitleEnabled = new BindingMemberDescriptor<ActionBarEx, bool>("DisplayShowTitleEnabled");
                DisplayUseLogoEnabled = new BindingMemberDescriptor<ActionBarEx, bool>("DisplayUseLogoEnabled");
                HomeButtonEnabled = new BindingMemberDescriptor<ActionBarEx, bool>("HomeButtonEnabled");
                HomeButtonClick = new BindingMemberDescriptor<ActionBarEx, IEventListener>("HomeButton.Click");
                Icon = new BindingMemberDescriptor<ActionBarEx, object>("Icon");
                Logo = new BindingMemberDescriptor<ActionBarEx, object>("Logo");
                NavigationMode = new BindingMemberDescriptor<ActionBarEx, ActionBarNavigationMode>("NavigationMode");
                SplitBackgroundDrawable = new BindingMemberDescriptor<ActionBarEx, object>("SplitBackgroundDrawable");
                StackedBackgroundDrawable = new BindingMemberDescriptor<ActionBarEx, object>("StackedBackgroundDrawable");
                IsShowing = new BindingMemberDescriptor<ActionBarEx, bool>("IsShowing");
                Visible = new BindingMemberDescriptor<ActionBarEx, bool>("Visible");
                ItemsSource = new BindingMemberDescriptor<ActionBarEx, IEnumerable>(AttachedMemberConstants.ItemsSource);
                ItemsSourceAdapter = new BindingMemberDescriptor<ActionBarEx, IItemsSourceAdapter>("ItemsSourceAdapter");
                SelectedItem = new BindingMemberDescriptor<ActionBarEx, object>(AttachedMemberConstants.SelectedItem);
                ItemsSourceGenerator = new BindingMemberDescriptor<ActionBarEx, IItemsSourceGenerator>(ItemsSourceGeneratorBase.MemberDescriptor);
                ItemTemplate = new BindingMemberDescriptor<ActionBarEx, int?>(AttachedMemberConstants.ItemTemplate);
                ItemTemplateSelector = new BindingMemberDescriptor<ActionBarEx, IDataTemplateSelector>(AttachedMemberConstants.ItemTemplateSelector);
                DropDownItemTemplate = new BindingMemberDescriptor<ActionBarEx, int?>(AttachedMembers.AdapterView.DropDownItemTemplate);
                DropDownItemTemplateSelector = new BindingMemberDescriptor<ActionBarEx, IDataTemplateSelector>(AttachedMembers.AdapterView.DropDownItemTemplateSelector);
                CollectionViewManager = new BindingMemberDescriptor<ActionBarEx, ICollectionViewManager>(AttachedMembers.ViewGroup.CollectionViewManager);
            }

            #endregion
        }

        public abstract class ActionBarTab : Object
        {
            #region Fields

            public static readonly BindingMemberDescriptor<ActionBarEx.Tab, object> Content;
            public static readonly BindingMemberDescriptor<ActionBarEx.Tab, int?> ContentTemplate;
            public static readonly BindingMemberDescriptor<ActionBarEx.Tab, IDataTemplateSelector> ContentTemplateSelector;
            public static readonly BindingMemberDescriptor<ActionBarEx.Tab, object> CustomView;
            public static readonly BindingMemberDescriptor<ActionBarEx.Tab, object> Icon;

            #endregion

            #region Constructors

            static ActionBarTab()
            {
                Content = new BindingMemberDescriptor<ActionBarEx.Tab, object>(AttachedMemberConstants.Content);
                ContentTemplate = new BindingMemberDescriptor<ActionBarEx.Tab, int?>(AttachedMemberConstants.ContentTemplate);
                ContentTemplateSelector = new BindingMemberDescriptor<ActionBarEx.Tab, IDataTemplateSelector>(AttachedMemberConstants.ContentTemplateSelector);
                CustomView = new BindingMemberDescriptor<ActionBarEx.Tab, object>("CustomView");
                Icon = new BindingMemberDescriptor<ActionBarEx.Tab, object>("Icon");
            }

            #endregion
        }

        public abstract class Toolbar : AttachedMembers.View
        {
            #region Fields

            public static readonly BindingMemberDescriptor<TolbarEx, bool> IsActionBar;
            public static readonly BindingMemberDescriptor<TolbarEx, int> MenuTemplate;

            #endregion

            #region Constructors

            static Toolbar()
            {
                IsActionBar = new BindingMemberDescriptor<TolbarEx, bool>("IsActionBar");
                MenuTemplate = new BindingMemberDescriptor<TolbarEx, int>("MenuTemplate");
            }

            #endregion
        }

        #endregion
    }
}
