#region Copyright

// ****************************************************************************
// <copyright file="AndroidToolkitExtensions.cs">
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Xml;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using JetBrains.Annotations;
using MugenMvvmToolkit.Android.Binding;
using MugenMvvmToolkit.Android.Binding.Infrastructure;
using MugenMvvmToolkit.Android.Binding.Interfaces;
using MugenMvvmToolkit.Android.Binding.Models;
using MugenMvvmToolkit.Android.Infrastructure;
using MugenMvvmToolkit.Android.Infrastructure.Mediators;
using MugenMvvmToolkit.Android.Interfaces;
using MugenMvvmToolkit.Android.Interfaces.Mediators;
using MugenMvvmToolkit.Android.Interfaces.Views;
using MugenMvvmToolkit.Android.Models;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Android
{
    // ReSharper disable once PartialTypeWithSinglePart
    public static partial class AndroidToolkitExtensions
    {
        #region Nested types

        private sealed class ContentViewManager
        {
            #region Fields

            private readonly List<IContentViewManager> _contentViewManagers;

            #endregion

            #region Constructors

            public ContentViewManager()
            {
                _contentViewManagers = new List<IContentViewManager>();
            }

            #endregion

            #region Methods

            public void SetContent(object view, object content)
            {
                lock (_contentViewManagers)
                {
                    for (var i = 0; i < _contentViewManagers.Count; i++)
                    {
                        if (_contentViewManagers[i].SetContent(view, content))
                            return;
                    }
                }
            }

            public void Add(IContentViewManager contentViewManager)
            {
                Should.NotBeNull(contentViewManager, nameof(contentViewManager));
                lock (_contentViewManagers)
                    _contentViewManagers.Insert(0, contentViewManager);
            }

            public void Remove<TType>()
                where TType : IContentViewManager
            {
                lock (_contentViewManagers)
                {
                    for (var i = 0; i < _contentViewManagers.Count; i++)
                    {
                        if (_contentViewManagers[i].GetType() == typeof(TType))
                        {
                            _contentViewManagers.RemoveAt(i);
                            return;
                        }
                    }
                }
            }

            public void Remove(IContentViewManager contentViewManager)
            {
                Should.NotBeNull(contentViewManager, nameof(contentViewManager));
                lock (_contentViewManagers)
                    _contentViewManagers.Remove(contentViewManager);
            }

            #endregion
        }

        private sealed class WeakReferenceCollector
        {
            #region Fields

            private const int GCInterval = 4;

            private static readonly TimeSpan MinInterval = TimeSpan.FromSeconds(4);
            private static DateTime _lastTime;
            private static int _gcCount;

            #endregion

            #region Methods

            #region Finalizers

            ~WeakReferenceCollector()
            {
                try
                {
                    if (Interlocked.Increment(ref _gcCount) >= GCInterval)
                        ThreadPool.QueueUserWorkItem(state => Collect(true));
                }
                catch (Exception e)
                {
                    Tracer.Error(e.Flatten(true));
                }
                finally
                {
                    GC.ReRegisterForFinalize(this);
                }
            }

            #endregion

            public static void Collect(bool fullCleanup)
            {
                Interlocked.Exchange(ref _gcCount, 0);
                var now = DateTime.UtcNow;
                if (now - _lastTime < MinInterval)
                    return;
                _lastTime = now;
                int oldCount;
                lock (WeakReferencesHolder)
                {
                    oldCount = WeakReferencesHolder.Count;
                    for (var i = 0; i < WeakReferencesHolder.Count; i++)
                    {
                        var target = ((WeakReference)WeakReferencesHolder[i]).Target;
                        if (target == null)
                        {
                            WeakReferencesHolder.RemoveAt(i);
                            --i;
                        }
                        else if (IsTargetAlive != null && !IsTargetAlive(target))
                        {
                            ((WeakReference)WeakReferencesHolder[i]).Target = null;
                            WeakReferencesHolder.RemoveAt(i);
                            --i;
                        }
                    }
                    if (fullCleanup)
                    {
                        var capacity = (int)(WeakReferencesHolder.Count * 1.25);
                        if (WeakReferencesHolder.Capacity > capacity)
                            WeakReferencesHolder.Capacity = capacity;
                    }
                }
                if (Tracer.TraceInformation)
                {
                    var count = WeakReferencesHolder.Count;
                    Tracer.Info("Collected " + (oldCount - count) + " weak references, total " + count);
                }
            }

            #endregion
        }

        private sealed class WeakReferenceKeyComparer : IComparer<object>
        {
            #region Fields

            public static readonly WeakReferenceKeyComparer Instance = new WeakReferenceKeyComparer();

            #endregion

            #region Methods

            private static int GetHashCode(object obj)
            {
                if (obj is WeakReference)
                    return obj.GetHashCode();
                return RuntimeHelpers.GetHashCode(obj);
            }

            #endregion

            #region Implementation of interfaces

            public int Compare(object x, object y)
            {
                return GetHashCode(x).CompareTo(GetHashCode(y));
            }

            #endregion
        }

        #endregion

        #region Fields

        public static readonly DataConstant<object> FragmentConstant;

        public static readonly bool IsApiGreaterThan10;
        public static readonly bool IsApiLessThanOrEqualTo10;
        public static readonly bool IsApiGreaterThanOrEqualTo14;
        public static readonly bool IsApiGreaterThanOrEqualTo17;
        public static readonly bool IsApiGreaterThanOrEqualTo19;
        public static readonly bool IsApiGreaterThanOrEqualTo21;
        //NOTE ConditionalWeakTable invokes finalizer for value, even if the key object is still alive https://bugzilla.xamarin.com/show_bug.cgi?id=21620
        private static readonly List<object> WeakReferencesHolder;

        private static readonly ContentViewManager ContentViewManagerField;
        private static readonly object CurrentActivityLocker;
        private static readonly ConcurrentDictionary<Type, bool> ObjectToDefaultJavaConstructor;
        private static readonly ConcurrentDictionary<Type, Func<object[], object>> ViewToContextConstructor;
        private static readonly ConcurrentDictionary<Type, Func<object[], object>> ViewToContextWithAttrsConstructor;
        private static readonly Type[] ViewContextArgs;
        private static readonly Type[] ViewContextWithAttrsArgs;
        private static readonly Type[] DefaultJavaConstructorArgs;

        private static Func<Context, MenuInflater, IDataContext, MenuInflater> _menuInflaterFactory;
        private static Func<Context, IDataContext, IViewFactory, LayoutInflater, LayoutInflater> _layoutInflaterFactory;
        private static Func<object, Context, object, int?, IDataTemplateSelector, object> _getContentViewDelegete;
        private static Action<object, object> _setContentViewDelegete;
        private static Func<object, bool> _isFragment;
        private static Func<object, bool> _isActionBar;
        private static WeakReference _activityRef;
        private static Func<object, IDataContext, Type, object> _mediatorFactory;
        private static Func<object, Context, IDataContext, IItemsSourceAdapter> _itemsSourceAdapterFactory;

        #endregion

        #region Constructors

        static AndroidToolkitExtensions()
        {
            IsApiGreaterThan10 = Build.VERSION.SdkInt > BuildVersionCodes.GingerbreadMr1;
            IsApiLessThanOrEqualTo10 = !IsApiGreaterThan10;
            IsApiGreaterThanOrEqualTo14 = Build.VERSION.SdkInt >= BuildVersionCodes.IceCreamSandwich;
            IsApiGreaterThanOrEqualTo17 = Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr1;
            IsApiGreaterThanOrEqualTo19 = Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat;
            IsApiGreaterThanOrEqualTo21 = Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop;
            FragmentConstant = DataConstant.Create<object>(typeof(AndroidToolkitExtensions), nameof(FragmentConstant), false);
            ContentViewManagerField = new ContentViewManager();

            _setContentViewDelegete = ContentViewManagerField.SetContent;
            _isFragment = o => false;
            _isActionBar = _isFragment;
            _activityRef = Empty.WeakReference;
            ViewToContextConstructor = new ConcurrentDictionary<Type, Func<object[], object>>();
            ViewToContextWithAttrsConstructor = new ConcurrentDictionary<Type, Func<object[], object>>();
            ObjectToDefaultJavaConstructor = new ConcurrentDictionary<Type, bool>();
            ViewContextArgs = new[] { typeof(Context) };
            ViewContextWithAttrsArgs = new[] { typeof(Context), typeof(IAttributeSet) };
            DefaultJavaConstructorArgs = new[] { typeof(IntPtr), typeof(JniHandleOwnership) };
            WeakReferencesHolder = new List<object>(1000);
            CurrentActivityLocker = new object();
            EnableFastTextViewTextProperty = true;
            BackgroundNotificationDelay = 200;

            // ReSharper disable once ObjectCreationAsStatement
            new WeakReferenceCollector();
        }

        #endregion

        #region Properties

        [CanBeNull]
        public static Func<View, string, Context, IAttributeSet, View> ViewCreated { get; set; }

        [CanBeNull]
        public static IDataTemplateSelector DefaultDataTemplateSelector { get; set; }

        public static Func<object, IDataContext, Type, object> MediatorFactory
        {
            get
            {
                AndroidBootstrapperBase.EnsureInitialized();
                return _mediatorFactory;
            }
            set { _mediatorFactory = value; }
        }

        [NotNull]
        public static Func<Context, MenuInflater, IDataContext, MenuInflater> MenuInflaterFactory
        {
            get
            {
                AndroidBootstrapperBase.EnsureInitialized();
                return _menuInflaterFactory;
            }
            set
            {
                Should.PropertyNotBeNull(value);
                _menuInflaterFactory = value;
            }
        }

        [NotNull]
        public static Func<Context, IDataContext, IViewFactory, LayoutInflater, LayoutInflater> LayoutInflaterFactory
        {
            get
            {
                AndroidBootstrapperBase.EnsureInitialized();
                return _layoutInflaterFactory;
            }
            set
            {
                Should.PropertyNotBeNull(value);
                _layoutInflaterFactory = value;
            }
        }

        [NotNull]
        public static Func<object, Context, IDataContext, IItemsSourceAdapter> ItemsSourceAdapterFactory
        {
            get { return _itemsSourceAdapterFactory; }
            set
            {
                Should.PropertyNotBeNull(value);
                _itemsSourceAdapterFactory = value;
            }
        }

        [NotNull]
        public static Func<object, Context, object, int?, IDataTemplateSelector, object> GetContentView
        {
            get
            {
                AndroidBootstrapperBase.EnsureInitialized();
                return _getContentViewDelegete;
            }
            set
            {
                Should.PropertyNotBeNull(value);
                _getContentViewDelegete = value;
            }
        }

        [NotNull]
        public static Action<object, object> SetContentView
        {
            get
            {
                AndroidBootstrapperBase.EnsureInitialized();
                return _setContentViewDelegete;
            }
            set
            {
                Should.PropertyNotBeNull(value);
                _setContentViewDelegete = value;
            }
        }

        [NotNull]
        public static Func<object, bool> IsFragment
        {
            get { return _isFragment; }
            set
            {
                Should.PropertyNotBeNull(value);
                _isFragment = value;
            }
        }

        public static Func<object, bool> IsActionBar
        {
            get { return _isActionBar; }
            set
            {
                Should.PropertyNotBeNull(value);
                _isActionBar = value;
            }
        }

        [CanBeNull]
        public static Action<MenuItemTemplate, IMenuItem, XmlPropertySetter> MenuItemTemplateInitalized;

        public static Activity CurrentActivity => (Activity)_activityRef.Target;

        public static bool AggressiveViewCleanup { get; set; }

        public static bool EnableFastTextViewTextProperty { get; set; }

        public static Func<object, bool> IsTargetAlive { get; set; }

        public static bool TypeCacheOnlyUsedTypeToBootstrapCodeBuilder { get; set; }

        public static int BackgroundNotificationDelay { get; set; }

        public static event EventHandler CurrentActivityChanged;

        #endregion

        #region Methods

        public static void Inflate(this MenuInflater menuInflater, int menuRes, IMenu menu, object parent)
        {
            Should.NotBeNull(menuInflater, nameof(menuInflater));
            var bindableMenuInflater = menuInflater as IBindableMenuInflater;
            if (bindableMenuInflater == null)
                menuInflater.Inflate(menuRes, menu);
            else
                bindableMenuInflater.Inflate(menuRes, menu, parent);
        }

        public static void ApplyMenuTemplate(this IMenu menu, object template, Context context, object parent)
        {
            var menuTemplate = template as IMenuTemplate;
            if (menuTemplate == null)
            {
                if (template != null)
                    context.GetActivity()?.MenuInflater.Inflate((int)template, menu, parent);
            }
            else
                menuTemplate.Apply(menu, context, parent);
        }

        [NotNull]
        public static LayoutInflater ToBindableLayoutInflater(this LayoutInflater inflater, Context context = null)
        {
            if (context == null)
            {
                Should.NotBeNull(inflater, nameof(inflater));
                context = inflater.Context;
            }
            return LayoutInflaterFactory(context, null, null, inflater);
        }

        [NotNull]
        public static LayoutInflater GetBindableLayoutInflater([NotNull] this Context context)
        {
            Should.NotBeNull(context, nameof(context));
            var activity = context.GetActivity();
            if (activity == null)
                return LayoutInflaterFactory(context, null, null, null);
            return activity.LayoutInflater.ToBindableLayoutInflater(context);
        }

        public static void ClearBindingsRecursively([CanBeNull] this View view, bool clearDataContext, bool clearAttachedValues, bool tryDispose)
        {
            if (view == null)
                return;
            var viewGroup = view as ViewGroup;
            if (viewGroup.IsAlive())
            {
                for (var i = 0; i < viewGroup.ChildCount; i++)
                    viewGroup.GetChildAt(i).ClearBindingsRecursively(clearDataContext, clearAttachedValues, tryDispose);
            }
            view.ClearBindings(clearDataContext, clearAttachedValues);
            if (tryDispose)
                view.TryDispose();
        }

        public static void NotifyActivityAttached([CanBeNull] Activity activity, [CanBeNull] View view)
        {
            if (!view.IsAlive() || !activity.IsAlive())
                return;
            var viewGroup = view as ViewGroup;
            if (viewGroup != null)
            {
                for (var i = 0; i < viewGroup.ChildCount; i++)
                    NotifyActivityAttached(activity, viewGroup.GetChildAt(i));
            }
            (view as IHasActivityDependency)?.OnAttached(activity);
        }

        public static void AddContentViewManager([NotNull] IContentViewManager contentViewManager)
        {
            ContentViewManagerField.Add(contentViewManager);
        }

        public static void RemoveContentViewManager<TType>()
            where TType : IContentViewManager
        {
            ContentViewManagerField.Remove<TType>();
        }

        public static void RemoveContentViewManager([NotNull] IContentViewManager contentViewManager)
        {
            ContentViewManagerField.Remove(contentViewManager);
        }

        [CanBeNull]
        public static Activity GetActivity([CanBeNull] this View view)
        {
            if (view.IsAlive())
                return GetActivity(view.Context);
            return null;
        }

        public static Activity GetActivity([CanBeNull] this Context context)
        {
            while (true)
            {
                var activity = context as Activity;
                if (activity == null)
                {
                    var wrapper = context as ContextWrapper;
                    if (wrapper == null)
                        return null;
                    context = wrapper.BaseContext;
                    continue;
                }
                return activity;
            }
        }

        public static void SetCurrentActivity(Activity activity, bool clear)
        {
            var changed = false;
            lock (CurrentActivityLocker)
            {
                var currentActivity = CurrentActivity;
                if (clear)
                {
                    if (ReferenceEquals(currentActivity, activity))
                    {
                        _activityRef = Empty.WeakReference;
                        changed = true;
                    }
                }
                else if (!ReferenceEquals(currentActivity, activity))
                {
                    _activityRef = ServiceProvider.WeakReferenceFactory(activity);
                    changed = true;
                }
            }
            if (changed)
                CurrentActivityChanged?.Invoke(activity, EventArgs.Empty);
        }

        public static void CleanupWeakReferences(bool fullCleanup)
        {
            WeakReferenceCollector.Collect(fullCleanup);
        }

        public static T GetOrCreateMediator<T>(this IJavaObject item, ref T mediator)
            where T : class
        {
            if (mediator == null)
                Interlocked.CompareExchange(ref mediator, (T)MediatorFactory(item, DataContext.Empty, typeof(T)), null);
            return mediator;
        }

        public static object MvvmActivityMediatorDefaultFactory(object activity, IDataContext dataContext, Type mediatorType)
        {
            if (activity is Activity && typeof(IMvvmActivityMediator).IsAssignableFrom(mediatorType))
                return new MvvmActivityMediator((Activity)activity);
            return null;
        }

        public static object GetContentViewDefault(object container, Context ctx, object content, int? templateId, IDataTemplateSelector templateSelector)
        {
            Should.NotBeNull(container, nameof(container));
            object result;
            if (templateSelector != null)
            {
                result = GetContentInternal(container, ctx, content, templateSelector);
                if (result is View || IsFragment(result))
                    return result;
                content = result;
            }
            if (templateId != null)
            {
                result = GetContentInternal(ctx, content, templateId);
                return result;
            }
            var vm = content as IViewModel;
            if (vm != null)
                content = GetOrCreateView(vm, null);

            if (content is View || IsFragment(content))
                return content;

            var selector = DefaultDataTemplateSelector;
            if (selector != null)
            {
                result = GetContentInternal(container, ctx, content, selector);
                if (result is View || IsFragment(result))
                    return result;
                content = result;
            }
            if (content == null)
                return null;
            Tracer.Warn("The content value {0} is not a View or Fragment.", content);
            result = new TextView(ctx) { Text = content.ToString() };
            return result;
        }

        public static void RemoveFragmentsState([CanBeNull] this Bundle bundle)
        {
            if (bundle == null)
                return;
            //https://github.com/android/platform_frameworks_support/blob/master/v4/java/android/support/v4/app/Fragment.java#L1945
            bundle.Remove("android:support:fragments");
            bundle.Remove("android:fragments");
        }

        internal static void RemoveFromParent([CanBeNull] this View view)
        {
            if (view.IsAlive())
                (view.Parent as ViewGroup)?.RemoveView(view);
        }

        internal static PlatformInfo GetPlatformInfo()
        {
            return new PlatformInfo(PlatformType.Android, Build.VERSION.Release, GetIdiom);
        }

        private static PlatformIdiom GetIdiom()
        {
            var context = CurrentActivity ?? Application.Context;
            int minWidthDp = context.Resources.Configuration.SmallestScreenWidthDp;
            return minWidthDp >= 600 ? PlatformIdiom.Tablet : PlatformIdiom.Phone;
        }

        internal static WeakReference CreateWeakReference(object item)
        {
            if (item == null)
                return Empty.WeakReference;
            var obj = item as IJavaObject;
            if (obj != null && obj.Handle == IntPtr.Zero)
                return Empty.WeakReference;

            var hasWeakReference = item as IHasWeakReference;
            if (hasWeakReference != null && !(item is IHasWeakReferenceInternal))
                return CreateWeakReference(item, obj, false);

            lock (WeakReferencesHolder)
            {
                WeakReference value;
                var index = WeakReferencesHolder.BinarySearch(item, WeakReferenceKeyComparer.Instance);
                if (index < 0)
                {
                    value = CreateWeakReference(item, obj, true);
                    WeakReferencesHolder.Insert(~index, value);
                    return value;
                }

                value = (WeakReference)WeakReferencesHolder[index];
                if (ReferenceEquals(value.Target, item))
                    return value;

                var leftIndex = index - 1;
                var rightIndex = index + 1;
                do
                {
                    if (rightIndex < WeakReferencesHolder.Count)
                    {
                        value = (WeakReference)WeakReferencesHolder[rightIndex];
                        if (WeakReferenceKeyComparer.Instance.Compare(item, value) != 0)
                            rightIndex = int.MaxValue;
                        else if (ReferenceEquals(value.Target, item))
                            return value;
                        else
                            ++rightIndex;
                    }
                    if (leftIndex >= 0)
                    {
                        value = (WeakReference)WeakReferencesHolder[leftIndex];
                        if (WeakReferenceKeyComparer.Instance.Compare(item, value) != 0)
                            leftIndex = int.MinValue;
                        else if (ReferenceEquals(value.Target, item))
                            return value;
                        else
                            --leftIndex;
                    }
                } while (rightIndex < WeakReferencesHolder.Count || leftIndex >= 0);

                value = CreateWeakReference(item, obj, true);
                WeakReferencesHolder.Insert(index, value);
                return value;
            }
        }

        internal static object GetOrCreateView(IViewModel vm, bool? alwaysCreateNewView, IDataContext dataContext = null)
        {
            if (vm == null)
                return null;
            //NOTE: trying to use current fragment, if any.
            var fragment = vm.Settings.Metadata.GetData(FragmentConstant, false);
            if (fragment == null)
                return ServiceProvider.ViewManager.GetOrCreateView(vm, alwaysCreateNewView, dataContext);
            return fragment;
        }

        internal static TItem Deserialize<TItem>(this XmlReader xmlReader)
        {
            return (TItem)Deserialize(typeof(TItem), xmlReader, true);
        }

        internal static bool IsDisableHierarchyListener(this ViewGroup viewGroup)
        {
            if (!viewGroup.IsAlive())
                return false;
            bool value;
            if (viewGroup.TryGetBindingMemberValue(AttachedMembers.ViewGroup.DisableHierarchyListener, out value))
                return value;
            return false;
        }

        internal static void SetDisableHierarchyListener(this ViewGroup viewGroup, bool value)
        {
            if (!viewGroup.IsAlive())
                return;
            var member = BindingServiceProvider.MemberProvider.GetBindingMember(viewGroup.GetType(), AttachedMembers.ViewGroup.DisableHierarchyListener, false, false);
            if (member != null && member.CanWrite)
                member.SetSingleValue(viewGroup, Empty.BooleanToObject(value));
        }

        private static object Deserialize(Type type, XmlReader reader, bool needMoveReader)
        {
            object result = null;
            while (true)
            {
                if (needMoveReader)
                {
                    if (!reader.Read())
                        break;
                }
                needMoveReader = true;
                switch (reader.NodeType)
                {
                    case XmlNodeType.Element:
                        if (result == null)
                        {
                            result = type.GetConstructor(Empty.Array<Type>())?.InvokeEx() ?? Activator.CreateInstance(type);
                            for (int attInd = 0; attInd < reader.AttributeCount; attInd++)
                            {
                                reader.MoveToAttribute(attInd);
                                var property = type.GetProperty(reader.Name, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);
                                if (property != null)
                                    property.SetValueEx(result, reader.Value);
                            }
                            reader.MoveToElement();
                            if (reader.IsEmptyElement)
                                return result;
                        }
                        else
                        {
                            Type elementType = null;
                            bool isList = false;
                            var propertyInfo = type.GetProperty(reader.Name, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);
                            if (propertyInfo != null)
                                elementType = propertyInfo.PropertyType;
                            else
                            {
                                propertyInfo = type.GetProperty("Items", BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public);
                                if (propertyInfo != null)
                                {
                                    elementType = propertyInfo.PropertyType.GenericTypeArguments[0];
                                    isList = true;
                                }
                            }
                            if (elementType != null)
                            {
                                var item = Deserialize(elementType, reader, false);
                                if (isList)
                                {
                                    var items = propertyInfo.GetValueEx<IList>(result);
                                    if (items == null)
                                    {
                                        items = (IList)(propertyInfo.PropertyType.GetConstructor(Empty.Array<Type>())?.InvokeEx() ?? Activator.CreateInstance(propertyInfo.PropertyType));
                                        propertyInfo.SetValueEx(result, items);
                                    }
                                    items.Add(item);
                                }
                                else
                                    propertyInfo.SetValueEx(result, item);
                            }
                        }
                        break;
                    case XmlNodeType.EndElement:
                        return result;
                }
            }
            return result;
        }

        internal static void ValidateTemplate(string itemsSource, ICollection items)
        {
            if (!string.IsNullOrEmpty(itemsSource) && items != null && items.Count > 0)
                throw new InvalidOperationException("Operation is not valid while ItemsSource is in use.");
        }

        [AssertionMethod]
        internal static bool IsAlive([AssertionCondition(AssertionConditionType.IS_NOT_NULL)] this IJavaObject javaObj)
        {
            return javaObj != null && javaObj.Handle != IntPtr.Zero;
        }

        internal static View CreateView(this Type type, Context ctx)
        {
            var func = ViewToContextConstructor.GetOrAdd(type, t =>
            {
                var c = t.GetConstructor(ViewContextArgs);
                if (c == null)
                    return null;
                return ServiceProvider.ReflectionManager.GetActivatorDelegate(c);
            });
            if (func == null)
                return (View)Activator.CreateInstance(type, ctx);
            return (View)func(new object[] { ctx });
        }

        internal static View CreateView(this Type type, Context ctx, IAttributeSet attrs)
        {
            var func = ViewToContextWithAttrsConstructor.GetOrAdd(type, t =>
            {
                var c = t.GetConstructor(ViewContextWithAttrsArgs);
                if (c == null)
                    return null;
                return ServiceProvider.ReflectionManager.GetActivatorDelegate(c);
            });
            if (func == null)
                return type.CreateView(ctx);
            return (View)func(new object[] { ctx, attrs });
        }

        internal static string ToStringSafe<T>(this T item, string defaultValue = null)
            where T : class
        {
            if (item == null)
                return defaultValue;
            return item.ToString();
        }

        internal static void ValidateViewIdFragment(View view, object content)
        {
            if (view.Id == View.NoId)
                throw new ArgumentException($"To use a fragment {view}, you must specify the id for view {content}, for instance: @+id/placeholder", nameof(view));
        }

        private static View GetContentInternal(Context ctx, object content, int? templateId)
        {
            if (templateId == null)
                return null;
            var newView = ctx.GetBindableLayoutInflater().Inflate(templateId.Value, null);
            if (content != null)
                newView.SetDataContext(content);
            return newView;
        }

        private static object GetContentInternal(object container, Context ctx, object content, IDataTemplateSelector templateSelector)
        {
            var template = templateSelector.SelectTemplate(content, container);
            if (template is int)
                return GetContentInternal(ctx, content, (int)template);
            if (content != null && (template is View || IsFragment(template)))
                template.SetDataContext(content);
            return template;
        }

        private static WeakReference CreateWeakReference(object item, IJavaObject javaItem, bool isWeakTable)
        {
            if (javaItem == null)
            {
                if (isWeakTable)
                    return new WeakReferenceWeakTable(item);
                return new WeakReference(item, true);
            }
            if (isWeakTable)
                return new JavaObjectWeakReferenceWeakTable(javaItem);
            return new JavaObjectWeakReference(javaItem);
        }

        private static void TryDispose(this IJavaObject javaObject)
        {
            if (!javaObject.IsAlive())
                return;
            var hasDefaultConstructor = ObjectToDefaultJavaConstructor
                .GetOrAdd(javaObject.GetType(), type =>
                {
                    var constructor = type.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, DefaultJavaConstructorArgs, null);
                    if (constructor == null)
                    {
                        if (Tracer.TraceWarning)
                            Tracer.Warn($"The type {type} cannot be disposed");
                        return false;
                    }
                    return true;
                });
            if (hasDefaultConstructor)
                javaObject.Dispose();
        }

        #endregion
    }
}