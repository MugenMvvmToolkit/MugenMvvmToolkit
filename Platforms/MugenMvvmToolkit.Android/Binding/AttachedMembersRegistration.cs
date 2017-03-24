#region Copyright

// ****************************************************************************
// <copyright file="AttachedMembersRegistration.cs">
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
using System.Collections;
using System.Reflection;
using Android.OS;
using Android.Runtime;
using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Views;
using Android.Widget;
using Java.Lang;
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Android.Binding.Models;
using MugenMvvmToolkit.Android.Infrastructure;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Models;
using Object = Java.Lang.Object;

namespace MugenMvvmToolkit.Android.Binding
{
    public static partial class AttachedMembersRegistration
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

            public DateChangedListener(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
            {
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

            public ContentChangeListener(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
            {
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
                ParentObserver.Raise(child);
                var childViewGroup = child as ViewGroup;
                if (childViewGroup != null && !childViewGroup.IsDisableHierarchyListener())
                    childViewGroup.SetOnHierarchyChangeListener(GlobalViewParentListener.Instance);
            }

            public void OnChildViewRemoved(View parent, View child)
            {
                var viewGroup = (ViewGroup)parent;
                if (viewGroup.ChildCount == 0 || viewGroup.GetChildAt(0) == child)
                {
                    var underlyingView = GetUnderlyingView(child);
                    if (underlyingView != null)
                        BindingServiceProvider.ContextManager.GetBindingContext(underlyingView).ValueChanged -= BindingContextChangedDelegate;
                    viewGroup.SetBindingMemberValue(AttachedMembers.ViewGroup.Content, RemoveViewValue);
                }
                ParentObserver.Raise(child);
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
                if (parent != null && !Equals(parent.DataContext(), context.Value))
                    (parent as ViewGroup)?.SetBindingMemberValue(AttachedMembers.ViewGroup.Content, new[] { context.Value, AddViewValue });
            }

            #endregion
        }

        private sealed class MenuItemOnMenuItemClickListener : Object, IMenuItemOnMenuItemClickListener
        {
            #region Fields

            private const string Key = "#ClickListener";
            private readonly IMenuItem _item;

            #endregion

            #region Constructors

            public MenuItemOnMenuItemClickListener(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
            {
            }

            public MenuItemOnMenuItemClickListener(IMenuItem menuItem)
            {
                _item = menuItem;
            }

            #endregion

            #region Implementation of IMenuItemOnMenuItemClickListener

            public bool OnMenuItemClick(IMenuItem item)
            {
                if (_item == null)
                    return false;
                item = _item;
                if (item.IsCheckable)
                    item.SetBindingMemberValue(AttachedMembers.MenuItem.IsChecked, !item.IsChecked);
                EventListenerList.Raise(item, Key, EventArgs.Empty);
                return true;
            }

            #endregion

            #region Methods

            public static IDisposable AddClickListener(IMenuItem item, IEventListener listener)
            {
                return EventListenerList.GetOrAdd(item, Key).AddWithUnsubscriber(listener);
            }

            #endregion
        }

        private abstract class LayoutObserver : Java.Lang.Object, View.IOnLayoutChangeListener, ViewTreeObserver.IOnGlobalLayoutListener
        {
            #region Fields

            private WeakReference _viewReference;

            #endregion

            #region Constructors

            protected LayoutObserver(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
            {
            }

            protected LayoutObserver(View view, bool treeObserver)
            {
                _viewReference = ServiceProvider.WeakReferenceFactory(view);
                if (!treeObserver && Build.VERSION.SdkInt >= BuildVersionCodes.Honeycomb)
                    view.AddOnLayoutChangeListener(this);
                else
                {
                    var viewTreeObserver = view.ViewTreeObserver;
                    if (viewTreeObserver.IsAlive)
                        viewTreeObserver.AddOnGlobalLayoutListener(this);
                }
            }

            #endregion

            #region Methods

            protected View GetView()
            {
                return (View)_viewReference?.Target;
            }

            private void Raise()
            {
                if (_viewReference == null)
                    return;
                var view = GetView();
                if (view.IsAlive())
                    OnGlobalLayoutChangedInternal(view);
                else
                    Dispose();
            }

            protected abstract void OnGlobalLayoutChangedInternal(View view);

            #endregion

            #region Implementation of interfaces

            public void OnLayoutChange(View v, int left, int top, int right, int bottom, int oldLeft, int oldTop,
                int oldRight,
                int oldBottom)
            {
                Raise();
            }

            public void OnGlobalLayout()
            {
                Raise();
            }

            #endregion

            #region Overrides

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    try
                    {
                        var view = GetView();
                        if (view.IsAlive())
                        {
                            if (Build.VERSION.SdkInt >= BuildVersionCodes.Honeycomb)
                                view.RemoveOnLayoutChangeListener(this);
                            else
                            {
                                var viewTreeObserver = view.ViewTreeObserver;
                                if (viewTreeObserver.IsAlive)
                                    viewTreeObserver.RemoveOnGlobalLayoutListener(this);
                            }
                        }
                    }
                    catch (System.Exception e)
                    {
                        Tracer.Warn(e.Flatten());
                    }
                    finally
                    {
                        _viewReference = null;
                    }
                }
                base.Dispose(disposing);
            }

            #endregion

        }

        private sealed class SizeObserver : LayoutObserver
        {
            #region Fields

            private readonly WeakEventListenerWrapper _listenerRef;
            private int _width;
            private int _height;

            #endregion

            #region Constructors

            public SizeObserver(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
            {
            }

            public SizeObserver(View view, IEventListener handler)
                : base(view, false)
            {
                _listenerRef = handler.ToWeakWrapper();
                _height = view.Height;
                _width = view.Width;
            }

            #endregion

            #region Overrides of LayoutObserver

            protected override void OnGlobalLayoutChangedInternal(View view)
            {
                if (view.Width != _width || view.Height != _height)
                {
                    _width = view.Width;
                    _height = view.Height;
                    if (!_listenerRef.EventListener.TryHandle(view, EventArgs.Empty))
                        Dispose();
                }
            }

            #endregion
        }

        private sealed class VisiblityObserver : LayoutObserver
        {
            #region Fields

            private readonly WeakEventListenerWrapper _listenerRef;
            private ViewStates _oldValue;

            #endregion

            #region Constructors

            public VisiblityObserver(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
            {
            }

            public VisiblityObserver(View view, IEventListener handler)
                : base(view, true)
            {
                _listenerRef = handler.ToWeakWrapper();
                _oldValue = view.Visibility;
            }

            #endregion

            #region Overrides of LayoutObserver

            protected override void OnGlobalLayoutChangedInternal(View view)
            {
                ViewStates visibility = view.Visibility;
                if (_oldValue == visibility)
                    return;
                _oldValue = visibility;
                if (!_listenerRef.EventListener.TryHandle(view, EventArgs.Empty))
                    Dispose();
            }

            #endregion
        }

        #endregion

        #region Fields

        private static Func<object, object> _rawAdapterGetter;
        private static Action<object, object> _rawAdapterSetter;
        private static readonly object AddViewValue;
        private static readonly object[] RemoveViewValue;
        private static IntPtr _textViewSetTextMethodId;
        private static Java.Lang.String _emptyString;
        private static JValue[] _nullJValue;
        private static JValue[] _emptyStringJValue;

        #endregion

        #region Constructors

        static AttachedMembersRegistration()
        {
            AddViewValue = new object();
            RemoveViewValue = new object[] { null };
        }

        #endregion

        #region Methods

        public static void RegisterObjectMembers()
        {
            BindingServiceProvider.BindingMemberPriorities[AttachedMembers.Object.StableIdProvider] = BindingServiceProvider.TemplateMemberPriority - 1;
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.Object.StableIdProvider));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty<Object, ICollectionViewManager>(AttachedMembers.ViewGroup.CollectionViewManager.Path));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty<Object, IContentViewManager>(AttachedMembers.ViewGroup.ContentViewManager.Path));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(ItemsSourceGeneratorBase.MemberDescriptor,
                (o, args) =>
                {
                    IEnumerable itemsSource = null;
                    if (args.OldValue != null)
                    {
                        itemsSource = args.OldValue.ItemsSource;
                        args.OldValue.SetItemsSource(null);
                    }
                    args.NewValue?.SetItemsSource(itemsSource);
                }));
        }

        public static void RegisterDialogMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.Dialog.Title,
                (dialog, args) => dialog.SetTitle(args.NewValue.ToStringSafe())));
        }

        public static void RegisterActivityMembers()
        {
            //to suppress message about parent
            MemberProvider.Register(AttachedBindingMember.CreateMember<Activity, object>(AttachedMemberConstants.ParentExplicit, (info, activity) => null, null));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty<Activity, string>(nameof(Activity.Title),
                (activity, args) => activity.Title = args.NewValue, getDefaultValue: (activity, info) => activity.Title));
            MemberProvider.Register(AttachedBindingMember.CreateMember<Activity, object>(AttachedMemberConstants.FindByNameMethod, ActivityFindByNameMember));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.Activity.ToastTemplateSelector));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.Activity.StartActivityDelegate));
        }

        public static void RegisterRatingBarMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<RatingBar>(nameof(RatingBar.Rating));
            MemberProvider.Register(AttachedBindingMember
                .CreateMember<RatingBar, float>(nameof(RatingBar.Rating), (info, btn) => btn.Rating,
                    (info, btn, value) => btn.Rating = value, nameof(RatingBar.RatingBarChange)));
        }

        public static void RegisterAdapterViewBaseMembers()
        {
            MemberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.AdapterView.DropDownItemTemplate, ViewGroupTemplateChanged));
            MemberProvider.Register(AttachedBindingMember
                .CreateAutoProperty(AttachedMembers.AdapterView.DropDownItemTemplateSelector, ViewGroupTemplateChanged));
        }

        public static void RegisterAdapterViewMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.AdapterView.SelectedItem, AdapterViewSelectedItemChanged, AdapterViewSelectedItemMemberAttached));
            var selectedItemPosMember = AttachedBindingMember.CreateAutoProperty(AttachedMembers.AdapterView.SelectedItemPosition, AdapterViewSelectedItemPositionChanged,
                AdapterViewSelectedItemPositionMemberAttached, (view, info) => view.SelectedItemPosition);
            MemberProvider.Register(selectedItemPosMember);
            MemberProvider.Register(typeof(AdapterView), "SelectedIndex", selectedItemPosMember, true);
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.AdapterView.ScrollToSelectedItem));
        }

        public static void RegisterViewGroupMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.ViewGroup.ItemsSource);
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.ItemsSource, ViewGroupItemsSourceChanged));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.ItemTemplate, ViewGroupTemplateChanged));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.ItemTemplateSelector, ViewGroupTemplateChanged));

            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.ViewGroup.Content.Override<FrameLayout>());
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.Content, ContentMemberChanged, ContentMemberAttached));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.ContentTemplate, ContentTemplateIdChanged));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.ContentTemplateSelector, ContentTemplateSelectorChanged));

            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.DisableHierarchyListener, (view, args) =>
            {
                view.SetOnHierarchyChangeListener(args.NewValue ? null : GlobalViewParentListener.Instance);
            }));
        }

        public static void RegisterTabHostMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.TabHost.RestoreSelectedIndex));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.TabHost.SelectedItem, TabHostSelectedItemChanged));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.ItemsSource.Override<TabHost>(), TabHostItemsSourceChanged));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.ItemTemplate.Override<TabHost>(), TabHostTemplateChanged));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.ItemTemplateSelector.Override<TabHost>(), TabHostTemplateChanged));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.ContentTemplate.Override<TabHost>(), TabHostTemplateChanged));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.ContentTemplateSelector.Override<TabHost>(), TabHostTemplateChanged));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.TabSpec.Title, (spec, args) => spec.SetIndicator(args.NewValue)));
        }

        public static void RegisterTextViewMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<TextView>(nameof(TextView.Text));
            try
            {
                //we can get method from TextView because it is marked as final
                _textViewSetTextMethodId = JNIEnv.GetMethodID(Class.FromType(typeof(TextView)).Handle, "setText", "(Ljava/lang/CharSequence;)V");
            }
            catch
            {
                ;
            }

            if (_textViewSetTextMethodId != IntPtr.Zero)
            {
                _nullJValue = new[] { new JValue(IntPtr.Zero) };
                _emptyString = new Java.Lang.String("");
                _emptyStringJValue = new[] { new JValue(_emptyString.Handle) };
                var fastTextMember = AttachedBindingMember.CreateMember<TextView, string>("FastText", (info, view) => view.Text, (info, view, arg3) =>
                {
                    //Default Xamarin implementation creates and release new Java.Lang.String on every text change, can be replaced with direct method call
                    //                    Java.Lang.String @string = value != null ? new Java.Lang.String(value) : (Java.Lang.String)null;
                    //                    this.TextFormatted = (ICharSequence)@string;
                    //                    if (@string == null)
                    //                        return;
                    //                    @string.Dispose();

                    if (arg3 == null)
                        JNIEnv.CallVoidMethod(view.Handle, _textViewSetTextMethodId, _nullJValue);
                    else if (arg3 == "")
                        JNIEnv.CallVoidMethod(view.Handle, _textViewSetTextMethodId, _emptyStringJValue);
                    else
                    {
                        var stringPtr = JNIEnv.NewString(arg3);
                        try
                        {
                            unsafe
                            {
                                JValue* ptr = stackalloc JValue[1];
                                *ptr = new JValue(stringPtr);
                                JNIEnv.CallVoidMethod(view.Handle, _textViewSetTextMethodId, ptr);
                            }
                        }
                        finally
                        {
                            JNIEnv.DeleteLocalRef(stringPtr);
                        }
                    }
                }, nameof(TextView.TextChanged));
                MemberProvider.Register(fastTextMember);
                if (PlatformExtensions.EnableFastTextViewTextProperty)
                    MemberProvider.Register(nameof(TextView.Text), fastTextMember);
            }
        }

        public static void RegisterAutoCompleteTextViewMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.AutoCompleteTextView.FilterText));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.AutoCompleteTextView.ItemTemplate, (view, args) => AutoCompleteTextViewTemplateChanged(view)));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.AutoCompleteTextView.ItemTemplateSelector, (view, args) => AutoCompleteTextViewTemplateChanged(view)));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.AutoCompleteTextView.ItemsSource, AutoCompleteTextViewItemsSourceChanged));
        }

        public static void RegisterDatePickerMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.DatePicker.SelectedDate);
            var selectedDateMember = AttachedBindingMember.CreateMember(AttachedMembers.DatePicker.SelectedDate,
                (info, picker) => picker.DateTime, (info, picker, value) => picker.DateTime = value,
                ObserveSelectedDate, SelectedDateMemberAttached);
            MemberProvider.Register(selectedDateMember);
            MemberProvider.Register("DateTime", selectedDateMember);
        }

        public static void RegisterTimePickerMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.TimePicker.SelectedTime);
            var selectedTimeMember = AttachedBindingMember.CreateMember(AttachedMembers.TimePicker.SelectedTime, GetTimePickerValue, SetTimePickerValue, nameof(TimePicker.TimeChanged));
            MemberProvider.Register(selectedTimeMember);
            MemberProvider.Register("Value", selectedTimeMember);
        }

        public static void RegisterImageViewMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.ImageView.ImageSource);
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ImageView.ImageSource,
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
                    var uri = args.NewValue as global::Android.Net.Uri;
                    if (uri != null)
                    {
                        view.SetImageURI(uri);
                        return;
                    }
                    view.SetImageResource((int)args.NewValue);
                }));
        }

        public static void RegisterToolbarMembers()
        {
            if (PlatformExtensions.IsApiGreaterThanOrEqualTo21)
            {
                MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.Toolbar.IsActionBar, ToolbarIsActionBarChanged));
                MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.View.MenuTemplate.Override<Toolbar>(), ToolbarMenuTemplateChanged));
            }
        }

        public static void RegisterButtonMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<Button>(nameof(Button.Click));
        }

        public static void RegisterCompoundButtonMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<CompoundButton>(nameof(CompoundButton.Checked));
        }

        public static void RegisterSeekBarMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<SeekBar>(nameof(SeekBar.Progress));
        }

        public static void RegisterMenuMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.Menu.ItemsSource, MenuItemsSourceChanged));
            var menuEnabledMember = AttachedBindingMember.CreateAutoProperty(AttachedMembers.Menu.Enabled,
                    (menu, args) => menu.SetGroupEnabled(0, args.NewValue.GetValueOrDefault()));
            MemberProvider.Register(menuEnabledMember);
            MemberProvider.Register("IsEnabled", menuEnabledMember);

            var menuVisibleMember = AttachedBindingMember.CreateAutoProperty(AttachedMembers.Menu.Visible,
                (menu, args) => menu.SetGroupVisible(0, args.NewValue.GetValueOrDefault()));
            MemberProvider.Register(menuVisibleMember);
            MemberProvider.Register("IsVisible", menuVisibleMember);
        }

        public static void RegisterMenuItemBaseMembers()
        {
            PlatformExtensions.MenuItemTemplateInitalized += MenuItemTemplateInitialized;
            BindingBuilderExtensions.RegisterDefaultBindingMember(AttachedMembers.MenuItem.Click);
            MemberProvider.Register(AttachedBindingMember.CreateNotifiableMember(AttachedMembers.MenuItem.IsChecked,
                (info, item) => item.IsChecked, (info, item, value) =>
                {
                    if (value == item.IsChecked)
                        return false;
                    item.SetChecked(value);
                    return true;
                }));
            MemberProvider.Register(AttachedBindingMember.CreateEvent(AttachedMembers.MenuItem.Click, SetClickEventValue,
                (item, args) => item.SetOnMenuItemClickListener(new MenuItemOnMenuItemClickListener(item))));

            MemberProvider.Register(AttachedBindingMember
                .CreateMember<IMenuItem, bool>(nameof(IMenuItem.IsCheckable),
                    (info, item) => item.IsCheckable,
                    (info, item, value) => item.SetCheckable(value)));

            var menuItemEnabled = AttachedBindingMember.CreateMember<IMenuItem, bool>(AttachedMemberConstants.Enabled,
                (info, item) => item.IsEnabled,
                (info, item, value) => item.SetEnabled(value));
            MemberProvider.Register(menuItemEnabled);
            MemberProvider.Register(nameof(IMenuItem.IsEnabled), menuItemEnabled);
            MemberProvider.Register(AttachedBindingMember
                .CreateMember<IMenuItem, bool>(nameof(IMenuItem.IsVisible), (info, item) => item.IsVisible,
                    (info, item, value) => item.SetVisible(value)));
            MemberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.MenuItem.Title,
                (info, item) => item.TitleFormatted.ToStringSafe(),
                (info, item, value) => item.SetTitle(value)));
            MemberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.MenuItem.RenderView, (info, item) =>
            {
                var renderView = GetContextFromItem(item)?.GetActivity()?.FindViewById(item.ItemId);
                if (renderView == null)
                    Tracer.Error($"Cannot find render view for {item}");
                return renderView;
            }, null));
        }

        public static void RegisterMenuItemMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateMember<IMenuItem, string>(AttachedMembers.MenuItem.ShowAsAction.Path, null, (info, o, value) =>
            {
                if (!string.IsNullOrEmpty(value))
                    o.SetShowAsActionFlags((ShowAsAction)System.Enum.Parse(typeof(ShowAsAction), value.Replace("|", ","), true));
            }));
            MemberProvider.Register(AttachedBindingMember
                .CreateMember<IMenuItem, object>(nameof(IMenuItem.AlphabeticShortcut),
                    (info, item) => item.AlphabeticShortcut,
                    (info, item, value) =>
                    {
                        if (value is char)
                            item.SetAlphabeticShortcut((char)value);
                        else
                            item.SetAlphabeticShortcut(value.ToStringSafe()[0]);
                    }));
            MemberProvider.Register(AttachedBindingMember
                .CreateMember(AttachedMembers.MenuItem.Icon, (info, item) => item.Icon,
                    (info, item, value) =>
                    {
                        if (value is int)
                            item.SetIcon((int)value);
                        else
                            item.SetIcon((Drawable)value);
                    }));
            MemberProvider.Register(AttachedBindingMember
                .CreateMember<IMenuItem, object>(nameof(IMenuItem.NumericShortcut),
                    (info, item) => item.NumericShortcut,
                    (info, item, value) =>
                    {
                        if (value is char)
                            item.SetNumericShortcut((char)value);
                        else
                            item.SetNumericShortcut(value.ToStringSafe()[0]);
                    }));
            MemberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.MenuItem.TitleCondensed,
                (info, item) => item.TitleCondensedFormatted.ToStringSafe(),
                (info, item, value) => item.SetTitleCondensed(value)));
        }

        public static void RegisterViewBaseMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.View.PopupMenuTemplate));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.View.PopupMenuPlacementTargetPath));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.View.PopupMenuPresenter));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.ViewGroup.AddToBackStack));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.View.MenuTemplate));
            MemberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.View.Activity, (info, view, arg3) => view.Context.GetActivity()));
            MemberProvider.Register(AttachedBindingMember.CreateAutoProperty(AttachedMembers.View.Fragment));
            MemberProvider.Register(AttachedBindingMember.CreateMember<View, object>(AttachedMemberConstants.FindByNameMethod, ViewFindByNameMember));
            MemberProvider.Register(AttachedBindingMember.CreateMember<View, object>(AttachedMemberConstants.Parent, GetViewParentValue, SetViewParentValue, ObserveViewParent));
            MemberProvider.Register(AttachedBindingMember.CreateMember<View, bool>(AttachedMemberConstants.Enabled, (info, view) => view.Enabled, (info, view, value) => view.Enabled = value));
            MemberProvider.Register(AttachedBindingMember.CreateMember<View, ViewStates>(nameof(View.Visibility),
                (info, view) => view.Visibility, (info, view, value) => view.Visibility = value,
                ObserveViewVisibility));
            MemberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.View.Visible,
                (info, view) => view.Visibility == ViewStates.Visible,
                (info, view, value) => view.Visibility = value ? ViewStates.Visible : ViewStates.Gone,
                ObserveViewVisibility));
            MemberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.View.Hidden,
                (info, view) => view.Visibility != ViewStates.Visible,
                (info, view, value) => view.Visibility = value ? ViewStates.Gone : ViewStates.Visible,
                ObserveViewVisibility));
            MemberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.View.Invisible,
                (info, view) => view.Visibility == ViewStates.Invisible,
                (info, view, value) => view.Visibility = value ? ViewStates.Invisible : ViewStates.Visible,
                ObserveViewVisibility));
        }

        public static void RegisterViewMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateMember<View, bool>(AttachedMemberConstants.Focused,
                    (info, view) => view.IsFocused, (info, view, arg3) =>
                    {
                        if (arg3)
                            view.RequestFocus();
                        else
                            view.ClearFocus();
                    }, nameof(View.FocusChange)));
            MemberProvider.Register(AttachedBindingMember.CreateEvent<View>("WidthChanged", (info, o, arg3) => new SizeObserver(o, arg3)));
            MemberProvider.Register(AttachedBindingMember.CreateEvent<View>("HeightChanged", (info, o, arg3) => new SizeObserver(o, arg3)));
        }

        private static IDisposable ObserveViewVisibility(IBindingMemberInfo bindingMemberInfo, View view, IEventListener arg3)
        {
            return new VisiblityObserver(view, arg3);
        }

        private static IDisposable ObserveViewParent(IBindingMemberInfo bindingMemberInfo, View view, IEventListener arg3)
        {
            return ParentObserver.GetOrAdd(view).AddWithUnsubscriber(arg3);
        }

        private static object GetViewParentValue(IBindingMemberInfo arg1, View view)
        {
            return ParentObserver.GetOrAdd(view).Parent;
        }

        private static void SetViewParentValue(IBindingMemberInfo bindingMemberInfo, View view, object arg3)
        {
            ParentObserver.GetOrAdd(view).Parent = arg3;
        }

        private static object ViewFindByNameMember(IBindingMemberInfo bindingMemberInfo, View target, object[] arg3)
        {
            if (target == null)
                return null;
            var rootView = target.RootView;
            if (rootView != null)
                target = rootView;
            var name = arg3[0].ToStringSafe();
            var result = target.FindViewWithTag(name);
            if (result == null)
            {
                var id = target.Resources.GetIdentifier(name, "id", target.Context.PackageName);
                if (id != 0)
                    result = target.FindViewById(id);
            }
            return result;
        }

        private static void MenuItemTemplateInitialized(MenuItemTemplate menuItemTemplate, IMenuItem menuItem, XmlPropertySetter setter)
        {
            setter.SetStringProperty(nameof(ShowAsAction), menuItemTemplate.ShowAsAction);
            if (!string.IsNullOrEmpty(menuItemTemplate.ActionViewBind))
                ServiceProvider.AttachedValueProvider.SetValue(menuItem, ActionViewBindKey, menuItemTemplate.ActionViewBind);
            if (!string.IsNullOrEmpty(menuItemTemplate.ActionProviderBind))
                ServiceProvider.AttachedValueProvider.SetValue(menuItem, ActionProviderBindKey, menuItemTemplate.ActionProviderBind);
        }

        private static void MenuItemsSourceChanged(IMenu menu, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            menu.GetBindingMemberValue(AttachedMembers.Menu.ItemsSourceGenerator)?.SetItemsSource(args.NewValue);
        }

        private static IDisposable SetClickEventValue(IBindingMemberInfo bindingMemberInfo, IMenuItem menuItem, IEventListener listener)
        {
            return MenuItemOnMenuItemClickListener.AddClickListener(menuItem, listener);
        }

        private static void AutoCompleteTextViewItemsSourceChanged(AutoCompleteTextView sender, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            var listAdapter = sender.Adapter as IItemsSourceAdapter;
            if (listAdapter == null)
            {
                listAdapter = PlatformExtensions.ItemsSourceAdapterFactory(sender, sender.Context, DataContext.Empty);
                sender.Adapter = listAdapter;
            }
            listAdapter.ItemsSource = args.NewValue;
        }

        private static void AutoCompleteTextViewTemplateChanged(AutoCompleteTextView sender)
        {
            (sender.Adapter as BaseAdapter)?.NotifyDataSetChanged();
        }

        internal static object GetAdapter(AdapterView item)
        {
            if (_rawAdapterGetter == null)
            {
                var property = GetRawAdapterProperty();
                if (property == null)
                    _rawAdapterGetter = o => null;
                else
                    _rawAdapterGetter = ServiceProvider.ReflectionManager.GetMemberGetter<object>(property);
            }
            return _rawAdapterGetter(item);
        }

        internal static void SetAdapter(AdapterView item, IAdapter adapter)
        {
            if (_rawAdapterSetter == null)
            {
                var property = GetRawAdapterProperty();
                if (property == null)
                    _rawAdapterSetter = (o, v) => { };
                else
                    _rawAdapterSetter = ServiceProvider.ReflectionManager.GetMemberSetter<object>(property);
            }
            _rawAdapterSetter(item, adapter);
        }

        private static PropertyInfo GetRawAdapterProperty()
        {
            var rawAdapterProp = typeof(AdapterView).GetPropertyEx("RawAdapter", MemberFlags.Instance | MemberFlags.NonPublic | MemberFlags.Public);
            if (rawAdapterProp == null)
                Tracer.Error("The AdapterView does not contain RawAdapter property");
            return rawAdapterProp;
        }

        private static void ToolbarMenuTemplateChanged(Toolbar toolbar, AttachedMemberChangedEventArgs<object> args)
        {
            toolbar.Menu.ApplyMenuTemplate(args.NewValue, toolbar.Context, toolbar);
        }

        private static void ToolbarIsActionBarChanged(Toolbar view, AttachedMemberChangedEventArgs<bool> args)
        {
            if (args.NewValue)
                view.Context.GetActivity()?.SetActionBar(view);
        }

        private static object ActivityFindByNameMember(IBindingMemberInfo bindingMemberInfo, Activity target, object[] arg3)
        {
            return ViewFindByNameMember(bindingMemberInfo, target.FindViewById(global::Android.Resource.Id.Content), arg3);
        }

        #region TabHost

        private static void TabHostSelectedItemChanged(TabHost tabHost, AttachedMemberChangedEventArgs<object> arg)
        {
            var generator = tabHost.GetBindingMemberValue(AttachedMembers.ViewGroup.ItemsSourceGenerator) as IItemsSourceGeneratorEx;
            if (generator != null)
                generator.SelectedItem = arg.NewValue;
        }

        private static void TabHostTemplateChanged<T>(TabHost tabHost, AttachedMemberChangedEventArgs<T> args)
        {
            tabHost.GetBindingMemberValue(AttachedMembers.ViewGroup.ItemsSourceGenerator)?.Reset();
        }

        private static void TabHostItemsSourceChanged(TabHost tabHost, AttachedMemberChangedEventArgs<IEnumerable> arg)
        {
            var generator = tabHost.GetBindingMemberValue(AttachedMembers.ViewGroup.ItemsSourceGenerator);
            if (generator == null)
            {
                generator = new TabHostItemsSourceGenerator(tabHost);
                tabHost.SetBindingMemberValue(AttachedMembers.ViewGroup.ItemsSourceGenerator, generator);
            }
            generator.SetItemsSource(arg.NewValue);
        }

        #endregion

        #region AdapterView

        private static void AdapterViewSelectedItemPositionChanged(AdapterView sender, AttachedMemberChangedEventArgs<int> args)
        {
            if (sender.GetBindingMemberValue(AttachedMembers.AdapterView.ScrollToSelectedItem).GetValueOrDefault(true) || sender is Spinner)
                sender.SetSelection(args.NewValue);
            var adapter = GetAdapter(sender) as IItemsSourceAdapter;
            if (adapter != null)
                sender.SetBindingMemberValue(AttachedMembers.AdapterView.SelectedItem, adapter.GetRawItem(args.NewValue));
        }

        private static void AdapterViewSelectedItemChanged(AdapterView sender, AttachedMemberChangedEventArgs<object> args)
        {
            var adapter = GetAdapter(sender) as IItemsSourceAdapter;
            if (adapter != null)
                sender.SetBindingMemberValue(AttachedMembers.AdapterView.SelectedItemPosition, adapter.GetPosition(args.NewValue));
        }

        private static void AdapterViewSelectedItemMemberAttached(AdapterView adapterView, MemberAttachedEventArgs arg)
        {
            //to invoke the AdapterViewSelectedItemPositionMemberAttached method.
            int value;
            adapterView.TryGetBindingMemberValue(AttachedMembers.AdapterView.SelectedItemPosition, out value);
        }

        private static void AdapterViewSelectedItemPositionMemberAttached(AdapterView adapterView, MemberAttachedEventArgs arg)
        {
            bool isListView = false;
            var type = adapterView.GetType();
            while (type != null && type != typeof(object))
            {
                if (type.FullName == "Android.Widget.ListView")
                {
                    isListView = true;
                    break;
                }
                type = type.BaseType;
            }

            if (isListView)
                adapterView.ItemClick += (sender, args) => SetSelectedIndexAdapterView((AdapterView)sender, args.Position);
            else
            {
                adapterView.ItemSelected += (sender, args) => SetSelectedIndexAdapterView((AdapterView)sender, args.Position);
                adapterView.NothingSelected += (sender, args) => SetSelectedIndexAdapterView((AdapterView)sender, -1);
            }
        }

        private static void SetSelectedIndexAdapterView(AdapterView adapter, int index)
        {
            var oldValue = adapter.GetBindingMemberValue(AttachedMembers.AdapterView.ScrollToSelectedItem);
            if (oldValue != null && !oldValue.Value)
                adapter.SetBindingMemberValue(AttachedMembers.AdapterView.SelectedItemPosition, index);
            else
            {
                adapter.SetBindingMemberValue(AttachedMembers.AdapterView.ScrollToSelectedItem, false);
                adapter.SetBindingMemberValue(AttachedMembers.AdapterView.SelectedItemPosition, index);
                adapter.SetBindingMemberValue(AttachedMembers.AdapterView.ScrollToSelectedItem, oldValue);
            }
        }

        #endregion

        #region DatePicker

        private static IDisposable ObserveSelectedDate(IBindingMemberInfo bindingMemberInfo, DatePicker datePicker, IEventListener arg3)
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

        #endregion

        #region ViewGroup

        private static void ViewGroupTemplateChanged<T>(ViewGroup sender, AttachedMemberChangedEventArgs<T> args)
        {
            var container = sender as AdapterView;
            if (container == null)
                sender.GetBindingMemberValue(AttachedMembers.ViewGroup.ItemsSourceGenerator)?.Reset();
            else
                (GetAdapter(container) as BaseAdapter)?.NotifyDataSetChanged();
        }

        private static void ViewGroupItemsSourceChanged(ViewGroup sender, AttachedMemberChangedEventArgs<IEnumerable> args)
        {
            var container = sender as AdapterView;
            if (container == null)
            {
                var sourceGenerator = sender.GetBindingMemberValue(AttachedMembers.ViewGroup.ItemsSourceGenerator);
                if (sourceGenerator == null)
                {
                    sourceGenerator = new ViewGroupItemsSourceGenerator(sender);
                    sender.SetBindingMemberValue(AttachedMembers.ViewGroup.ItemsSourceGenerator, sourceGenerator);
                }
                sourceGenerator.SetItemsSource(args.NewValue);
                return;
            }
            var adapter = GetAdapter(container) as IItemsSourceAdapter;
            if (adapter == null)
            {
                adapter = PlatformExtensions.ItemsSourceAdapterFactory(container, container.Context, DataContext.Empty);
                SetAdapter(container, adapter);
            }
            adapter.ItemsSource = args.NewValue;
        }

        private static void ContentMemberAttached(ViewGroup viewGroup, MemberAttachedEventArgs args)
        {
            viewGroup.SetDisableHierarchyListener(true);
            viewGroup.SetOnHierarchyChangeListener(ContentChangeListener.Instance);
        }

        private static void ContentTemplateSelectorChanged(ViewGroup sender,
            AttachedMemberChangedEventArgs<IDataTemplateSelector> args)
        {
            UpdateContent(sender, sender.GetBindingMemberValue(AttachedMembers.ViewGroup.Content), args.Args);
        }

        private static void ContentMemberChanged(ViewGroup sender, AttachedMemberChangedEventArgs<object> args)
        {
            UpdateContent(sender, args.NewValue, args.Args);
        }

        private static void ContentTemplateIdChanged(ViewGroup sender, AttachedMemberChangedEventArgs<int?> args)
        {
            UpdateContent(sender, sender.GetBindingMemberValue(AttachedMembers.ViewGroup.Content), args.Args);
        }

        private static void UpdateContent(ViewGroup sender, object newContent, object[] args)
        {
            if (newContent == null && args == RemoveViewValue)
                return;

            //NOTE cheking if it's a view group listener.
            if (args != null && args.Length == 2 && args[1] == AddViewValue)
                return;
            var templateId = sender.GetBindingMemberValue(AttachedMembers.ViewGroup.ContentTemplate);
            var templateSelector = sender.GetBindingMemberValue(AttachedMembers.ViewGroup.ContentTemplateSelector);
            newContent = PlatformExtensions.GetContentView(sender, sender.Context, newContent, templateId, templateSelector);
            var contentViewManager = sender.GetBindingMemberValue(AttachedMembers.ViewGroup.ContentViewManager);
            if (contentViewManager == null)
                PlatformExtensions.SetContentView(sender, newContent);
            else
                contentViewManager.SetContent(sender, newContent);
        }

        #endregion

        #endregion

        #region Properties

        private static IBindingMemberProvider MemberProvider => BindingServiceProvider.MemberProvider;

        #endregion
    }
}