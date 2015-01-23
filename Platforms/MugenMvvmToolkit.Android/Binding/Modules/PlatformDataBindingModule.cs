#region Copyright

// ****************************************************************************
// <copyright file="PlatformDataBindingModule.cs">
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
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Java.Lang;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using Object = Java.Lang.Object;

namespace MugenMvvmToolkit.Binding.Modules
{
    public partial class PlatformDataBindingModule : DataBindingModule
    {
        #region Nested types

        private sealed class DateChangedListener : Object, DatePicker.IOnDateChangedListener
        {
            #region Fields

            private const string Key = "#DateChangedListener";
            public static readonly DateChangedListener Instance;

            #endregion

            #region Constructors

            static DateChangedListener()
            {
                Instance = new DateChangedListener();
            }

            private DateChangedListener()
            {
            }

            #endregion

            #region Implementation of IOnDateChangedListener

            public void OnDateChanged(DatePicker view, int year, int monthOfYear, int dayOfMonth)
            {
                Raise(view);
            }

            #endregion

            #region Methods

            public static IDisposable AddDateChangedListener(DatePicker datePicker, IEventListener listener)
            {
                return EventListenerList.GetOrAdd(datePicker, Key).AddWithUnsubscriber(listener);
            }

            private static void Raise(DatePicker picker)
            {
                EventListenerList.Raise(picker, Key, EventArgs.Empty);
            }

            #endregion
        }

        private sealed class ContentChangeListener : Object, ViewGroup.IOnHierarchyChangeListener
        {
            #region Fields

            public static readonly ContentChangeListener Instance;
            private static readonly EventHandler<ISourceValue, EventArgs> BindingContextChangedDelegate;

            #endregion

            #region Constructors

            static ContentChangeListener()
            {
                Instance = new ContentChangeListener();
                BindingContextChangedDelegate = BindingContextChanged;
            }

            private ContentChangeListener()
            {
            }

            #endregion

            #region Implementation of IOnHierarchyChangeListener

            public void OnChildViewAdded(View parent, View child)
            {
                var viewGroup = (ViewGroup)parent;
                if (viewGroup.IndexOfChild(child) == 0)
                {
                    var underlyingView = GetUnderlyingView(child);
                    if (underlyingView != null)
                    {
                        var dataContext = BindingServiceProvider.ContextManager.GetBindingContext(underlyingView);
                        dataContext.ValueChanged += BindingContextChangedDelegate;
                        UpdataContext(viewGroup, underlyingView, dataContext);
                    }
                }
                GlobalViewParentListener.Instance.OnChildViewAdded(parent, child);
            }

            public void OnChildViewRemoved(View parent, View child)
            {
                var viewGroup = (ViewGroup)parent;
                if (viewGroup.ChildCount == 0 || viewGroup.GetChildAt(0) == child)
                {
                    var underlyingView = GetUnderlyingView(child);
                    if (underlyingView != null)
                        BindingServiceProvider.ContextManager.GetBindingContext(underlyingView).ValueChanged -= BindingContextChangedDelegate;
                    ContentMember.SetValue(viewGroup, RemoveViewValue);
                }
                GlobalViewParentListener.Instance.OnChildViewRemoved(parent, child);
            }

            #endregion

            #region Methods

            [CanBeNull]
            private static View GetUnderlyingView(View child)
            {
                if (IsNoSaveStateFrameLayout(child))
                {
                    var layout = (FrameLayout)child;
                    if (layout.ChildCount == 0)
                        return null;
                    return layout.GetChildAt(0);
                }
                return child;
            }

            private static View GetParent(View view)
            {
                var parent = view.Parent as View;
                if (IsNoSaveStateFrameLayout(parent))
                    return parent.Parent as View;
                return parent;
            }

            private static bool IsNoSaveStateFrameLayout(View view)
            {
                return view != null && view.Class.CanonicalName.SafeContains("NoSaveStateFrameLayout", StringComparison.OrdinalIgnoreCase);
            }

            private static void BindingContextChanged(ISourceValue value, EventArgs args)
            {
                var context = (IBindingContext)value;
                UpdataContext(null, context.Source as View, context);
            }

            private static void UpdataContext(View parent, View view, IBindingContext context)
            {
                if (view == null)
                    return;
                if (parent == null)
                    parent = GetParent(view);
                if (parent != null && !Equals(BindingServiceProvider.ContextManager.GetBindingContext(parent).Value, context.Value))
                    ContentMember.SetValue(parent, new[] { context.Value, AddViewValue });
            }

            #endregion
        }

        #endregion

        #region Fields

        internal static readonly IAttachedBindingMemberInfo<object, bool?> AutoDisposeMember;
        internal static readonly IAttachedBindingMemberInfo<AdapterView, int> AdapterViewSelectedPositionMember;
        internal readonly static IAttachedBindingMemberInfo<Object, ICollectionViewManager> CollectionViewManagerMember;

        private readonly static IAttachedBindingMemberInfo<Object, IContentViewManager> ContentViewManagerMember;
        private static readonly IAttachedBindingMemberInfo<AdapterView, object> AdapterViewSelectedItemMember;
        private static readonly IAttachedBindingMemberInfo<AdapterView, bool> ScrollToSelectedItemMember;

        private static readonly IAttachedBindingMemberInfo<ViewGroup, object> ContentMember;
        private static readonly IAttachedBindingMemberInfo<ViewGroup, int?> ContentTemplateIdMember;
        private static readonly IAttachedBindingMemberInfo<ViewGroup, IDataTemplateSelector> ContentTemplateSelectorMember;

        private static IBindingMemberInfo _rawAdapterMember;
        private static readonly object AddViewValue;
        private static readonly object[] RemoveViewValue;

        #endregion

        #region Constructors

        static PlatformDataBindingModule()
        {
            AddViewValue = new object();
            RemoveViewValue = new object[] { null };
            //Object
            CollectionViewManagerMember = AttachedBindingMember.CreateAutoProperty<Object, ICollectionViewManager>("CollectionViewManager");
            ContentViewManagerMember = AttachedBindingMember.CreateAutoProperty<Object, IContentViewManager>("ContentViewManager");
            AutoDisposeMember = AttachedBindingMember.CreateAutoProperty<object, bool?>("AutoDispose");

            //Menu
            MenuItemsSourceMember = AttachedBindingMember.CreateAutoProperty<IMenu, IEnumerable>(AttachedMemberConstants.ItemsSource, MenuItemsSourceChanged);
            IsCheckedMenuItemMember = AttachedBindingMember.CreateNotifiableMember<IMenuItem, bool>("IsChecked",
                (info, item) => item.IsChecked, (info, item, value) =>
                {
                    if (value == item.IsChecked)
                        return false;
                    item.SetChecked(value);
                    return true;
                });

            //ViewGroup
            ContentMember = AttachedBindingMember
                .CreateAutoProperty<ViewGroup, object>(AttachedMemberConstants.Content, ContentMemberChanged, ContentMemberAttached);
            ContentTemplateIdMember = AttachedBindingMember
                .CreateAutoProperty<ViewGroup, int?>(AttachedMemberConstants.ContentTemplate, ContentTemplateIdChanged);
            ContentTemplateSelectorMember = AttachedBindingMember
                .CreateAutoProperty<ViewGroup, IDataTemplateSelector>(AttachedMemberConstants.ContentTemplateSelector, ContentTemplateSelectorChanged);

            //AdapterView
            AdapterViewSelectedPositionMember =
                AttachedBindingMember.CreateAutoProperty<AdapterView, int>("SelectedItemPosition",
                    AdapterViewSelectedItemPositionChanged, AdapterViewSelectedMemberAttached, (view, info) => view.SelectedItemPosition);
            AdapterViewSelectedItemMember = AttachedBindingMember.CreateAutoProperty<AdapterView, object>(
                AttachedMemberConstants.SelectedItem, AdapterViewSelectedItemChanged, AdapterViewSelectedMemberAttached);
            ScrollToSelectedItemMember = AttachedBindingMember.CreateAutoProperty<AdapterView, bool>("ScrollToSelectedItem");
        }

        #endregion

        #region Methods

        private static void Register([NotNull] IBindingMemberProvider memberProvider)
        {
            Should.NotBeNull(memberProvider, "memberProvider");
            RegisterMenuMembers(memberProvider);
            RegisterViewMembers(memberProvider);

            //Object
            memberProvider.Register(CollectionViewManagerMember);
            memberProvider.Register(ContentViewManagerMember);
            memberProvider.Register(AutoDisposeMember);

            //Dialog
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<Dialog, object>("Title",
                (dialog, args) => dialog.SetTitle(args.NewValue.ToStringSafe())));

            //Activity
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<Activity, string>("Title",
                (activity, args) => activity.Title = args.NewValue, getDefaultValue: (activity, info) => activity.Title));
            //to suppress message about parent property.
            memberProvider.Register(AttachedBindingMember.CreateMember<Activity, object>(AttachedMemberConstants.Parent, (info, activity) => null, null));

            //CompoundButton
            memberProvider.Register(AttachedBindingMember
                .CreateMember<CompoundButton, bool>("Checked", (info, btn) => btn.Checked,
                    (info, btn, value) => btn.Checked = value, "CheckedChange"));

            //RatingBar
            memberProvider.Register(AttachedBindingMember
                .CreateMember<RatingBar, float>("Rating", (info, btn) => btn.Rating,
                    (info, btn, value) => btn.Rating = value, "RatingBarChange"));

            //AdapterView
            _rawAdapterMember = memberProvider.GetBindingMember(typeof(AdapterView), "RawAdapter", false, true);
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty<AdapterView, int?>(AttachedMemberNames.DropDownItemTemplate,
                    ViewGroupTemplateChanged));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty<AdapterView, int?>(AttachedMemberNames.DropDownItemTemplateSelector,
                    ViewGroupTemplateChanged));
            memberProvider.Register(AdapterViewSelectedItemMember);
            memberProvider.Register(AdapterViewSelectedPositionMember);
            memberProvider.Register(typeof(AdapterView), "SelectedIndex", AdapterViewSelectedPositionMember, true);
            memberProvider.Register(ScrollToSelectedItemMember);

            //ViewGroup
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty<ViewGroup, IEnumerable>(AttachedMemberConstants.ItemsSource, ViewGroupItemsSourceChanged));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty<ViewGroup, int?>(AttachedMemberConstants.ItemTemplate, ViewGroupTemplateChanged));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty<ViewGroup, IDataTemplateSelector>(AttachedMemberConstants.ItemTemplateSelector, ViewGroupTemplateChanged));

            memberProvider.Register(ContentMember);
            memberProvider.Register(ContentTemplateIdMember);
            memberProvider.Register(ContentTemplateSelectorMember);

            //TabHost
            memberProvider.Register(
                AttachedBindingMember.CreateAutoProperty<TabHost, object>(AttachedMemberConstants.SelectedItem,
                    TabHostSelectedItemChanged));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty<TabHost, IEnumerable>(AttachedMemberConstants.ItemsSource, TabHostItemsSourceChanged));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty<TabHost, int?>(AttachedMemberConstants.ItemTemplate, TabHostTemplateChanged));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty<TabHost, IDataTemplateSelector>(AttachedMemberConstants.ItemTemplateSelector,
                    TabHostTemplateChanged));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty<TabHost, int?>(AttachedMemberConstants.ContentTemplate, TabHostTemplateChanged));
            memberProvider.Register(AttachedBindingMember
                .CreateAutoProperty<TabHost, IDataTemplateSelector>(AttachedMemberConstants.ContentTemplateSelector,
                    TabHostTemplateChanged));
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<TabHost.TabSpec, string>("Title",
                (spec, args) => spec.SetIndicator(args.NewValue)));


            //DatePicker
            var selectedDateMember = AttachedBindingMember.CreateMember<DatePicker, DateTime>("SelectedDate",
                (info, picker) => picker.DateTime, (info, picker, value) => picker.DateTime = value,
                ObserveSelectedDate, SelectedDateMemberAttached);
            memberProvider.Register(selectedDateMember);
            memberProvider.Register("DateTime", selectedDateMember);

            //TimePicker
            var selectedTimeMember = AttachedBindingMember.CreateMember<TimePicker, TimeSpan>("SelectedTime", GetTimePickerValue, SetTimePickerValue, ObserveTimePickerValue);
            memberProvider.Register(selectedTimeMember);
            memberProvider.Register("Value", selectedTimeMember);

            //ImageView
            memberProvider.Register(AttachedBindingMember.CreateAutoProperty<ImageView, object>("ImageSource",
                (view, args) =>
                {
                    if (args.NewValue == null)
                    {
                        view.SetImageBitmap(null);
                        return;
                    }
                    var bitmap = args.NewValue as Bitmap;
                    if (bitmap != null)
                    {
                        view.SetImageBitmap(bitmap);
                        return;
                    }
                    var drawable = args.NewValue as Drawable;
                    if (drawable != null)
                    {
                        view.SetImageDrawable(drawable);
                        return;
                    }
                    var uri = args.NewValue as Android.Net.Uri;
                    if (uri != null)
                    {
                        view.SetImageURI(uri);
                        return;
                    }
                    view.SetImageResource((int)args.NewValue);
                }));
        }

        private static object GetAdapter(AdapterView item)
        {
            return _rawAdapterMember.GetValue(item, null);
        }

        private static void SetAdapter(AdapterView item, IAdapter adapter)
        {
            _rawAdapterMember.SetValue(item, new object[] { adapter });
        }

        private static void MenuTemplateChanged(View view, AttachedMemberChangedEventArgs<int> args)
        {
            var type = view.GetType();
            if (type.FullName != "Android.Widget.Toolbar")
                return;
            var activity = view.Context.GetActivity();
            if (activity != null)
            {
                var menuMember = BindingServiceProvider
                    .MemberProvider
                    .GetBindingMember(type, "Menu", true, true);
                activity.MenuInflater.Inflate(args.NewValue, (IMenu)menuMember.GetValue(view, null), view);
            }
        }

        #region TabHost

        private static void TabHostSelectedItemChanged(TabHost tabHost, AttachedMemberChangedEventArgs<object> arg)
        {
            var generator = ItemsSourceGeneratorBase.Get(tabHost) as TabHostItemsSourceGenerator;
            if (generator != null)
                generator.SetSelectedItem(arg.NewValue);
        }

        private static void TabHostTemplateChanged<T>(TabHost tabHost, AttachedMemberChangedEventArgs<T> args)
        {
            var generator = ItemsSourceGeneratorBase.Get(tabHost);
            if (generator != null)
                generator.Reset();
        }

        private static void TabHostItemsSourceChanged(TabHost tabHost, AttachedMemberChangedEventArgs<IEnumerable> arg)
        {
            TabHostItemsSourceGenerator.GetOrAdd(tabHost).SetItemsSource(arg.NewValue);
        }

        #endregion

        #region AdapterView

        private static void AdapterViewSelectedItemPositionChanged(AdapterView sender,
            AttachedMemberChangedEventArgs<int> args)
        {
            if (!(sender is ListView) || ScrollToSelectedItemMember.GetValue(sender, null))
                sender.SetSelection(args.NewValue);

            var adapter = GetAdapter(sender) as ItemsSourceAdapter;
            if (adapter == null)
                return;
            object item = adapter.GetRawItem(args.NewValue);
            AdapterViewSelectedItemMember.SetValue(sender, item);
        }

        private static void AdapterViewSelectedItemChanged(AdapterView sender, AttachedMemberChangedEventArgs<object> args)
        {
            var adapter = GetAdapter(sender) as ItemsSourceAdapter;
            if (adapter == null)
                return;
            int position = adapter.GetPosition(args.NewValue);
            AdapterViewSelectedPositionMember.SetValue(sender, position);
        }

        private static void AdapterViewSelectedMemberAttached(AdapterView adapterView, MemberAttachedEventArgs arg)
        {
            if (adapterView is ListView)
                adapterView.ItemClick += (sender, args) => AdapterViewSelectedPositionMember.SetValue((AdapterView)sender, args.Position);
            else
            {
                adapterView.ItemSelected += (sender, args) => AdapterViewSelectedPositionMember.SetValue((AdapterView)sender, args.Position);
                adapterView.NothingSelected += (sender, args) => AdapterViewSelectedPositionMember.SetValue((AdapterView)sender, -1);
            }
        }

        #endregion

        #region DatePicker

        private static IDisposable ObserveSelectedDate(IBindingMemberInfo bindingMemberInfo, DatePicker datePicker,
            IEventListener arg3)
        {
            return DateChangedListener.AddDateChangedListener(datePicker, arg3);
        }

        private static void SelectedDateMemberAttached(DatePicker picker, MemberAttachedEventArgs memberAttachedEventArgs)
        {
            picker.Init(picker.Year, picker.Month, picker.DayOfMonth, DateChangedListener.Instance);
        }

        #endregion

        #region TimePicker

        private static void SetTimePickerValue(IBindingMemberInfo bindingMemberInfo, TimePicker timePicker, TimeSpan value)
        {
            timePicker.CurrentHour = new Integer(value.Hours);
            timePicker.CurrentMinute = new Integer(value.Minutes);
        }

        private static TimeSpan GetTimePickerValue(IBindingMemberInfo bindingMemberInfo, TimePicker timePicker)
        {
            int currentHour = timePicker.CurrentHour.IntValue();
            int currentMinute = timePicker.CurrentMinute.IntValue();
            return new TimeSpan(currentHour, currentMinute, 0);
        }

        private static IDisposable ObserveTimePickerValue(IBindingMemberInfo bindingMemberInfo, TimePicker timePicker,
            IEventListener arg3)
        {
            EventHandler<TimePicker.TimeChangedEventArgs> handler = arg3.ToWeakEventListener().Handle;
            timePicker.TimeChanged += handler;
            return WeakActionToken.Create(timePicker, handler, (picker, eventHandler) => picker.TimeChanged -= eventHandler);
        }

        #endregion

        #region ViewGroup

        private static void ViewGroupTemplateChanged<T>(ViewGroup sender, AttachedMemberChangedEventArgs<T> args)
        {
            var container = sender as AdapterView;
            if (container == null)
            {
                var sourceGenerator = ViewGroupItemsSourceGenerator.GetOrAdd(sender);
                if (sourceGenerator != null)
                    sourceGenerator.Reset();
                return;
            }
            var adapter = GetAdapter(container) as BaseAdapter;
            if (adapter != null)
                adapter.NotifyDataSetChanged();
        }

        private static void ViewGroupItemsSourceChanged(ViewGroup sender, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            var container = sender as AdapterView;
            if (container == null)
            {
                var sourceGenerator = ViewGroupItemsSourceGenerator.GetOrAdd(sender);
                if (sourceGenerator != null)
                    sourceGenerator.SetItemsSource(args.NewValue);
                return;
            }
            var adapter = GetAdapter(container) as IItemsSourceAdapter;
            if (adapter == null)
            {
                adapter = ItemsSourceAdapter.Factory(container, container.Context, DataContext.Empty);
                SetAdapter(container, adapter);
            }
            adapter.ItemsSource = args.NewValue;
        }

        private static void ContentMemberAttached(ViewGroup viewGroup, MemberAttachedEventArgs args)
        {
            viewGroup.ListenParentChange();
            viewGroup.SetOnHierarchyChangeListener(ContentChangeListener.Instance);
        }

        private static void ContentTemplateSelectorChanged(ViewGroup sender,
            AttachedMemberChangedEventArgs<IDataTemplateSelector> args)
        {
            UpdateContent(sender, ContentMember.GetValue(sender, null), args.Args);
        }

        private static void ContentMemberChanged(ViewGroup sender, AttachedMemberChangedEventArgs<object> args)
        {
            UpdateContent(sender, args.NewValue, args.Args);
        }

        private static void ContentTemplateIdChanged(ViewGroup sender, AttachedMemberChangedEventArgs<int?> args)
        {
            UpdateContent(sender, ContentMember.GetValue(sender, null), args.Args);
        }

        private static void UpdateContent(ViewGroup sender, object newContent, object[] args)
        {
            if (newContent == null && args == RemoveViewValue)
                return;

            //NOTE cheking if it's a view group listener.
            if (args != null && args.Length == 2 && args[1] == AddViewValue)
                return;
            var templateId = ContentTemplateIdMember.GetValue(sender, null);
            var templateSelector = ContentTemplateSelectorMember.GetValue(sender, null);
            newContent = PlatformExtensions.GetContentView(sender, sender.Context, newContent, templateId, templateSelector);
            var contentViewManager = ContentViewManagerMember.GetValue(sender, null);
            if (contentViewManager == null)
                PlatformExtensions.SetContentView(sender, newContent);
            else
                contentViewManager.SetContent(sender, newContent);
        }

        #endregion

        #endregion

        #region Overrides of DataBindingModule

        /// <summary>
        ///    Occurs on load the current module.
        /// </summary>
        protected override void OnLoaded(IModuleContext context)
        {
            Register(BindingServiceProvider.MemberProvider);
            base.OnLoaded(context);
        }

        /// <summary>
        ///     Gets the <see cref="IBindingErrorProvider" /> that will be used by default.
        /// </summary>
        protected override IBindingErrorProvider GetBindingErrorProvider()
        {
            return new BindingErrorProvider();
        }

        #endregion
    }
}