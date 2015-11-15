#region Copyright

// ****************************************************************************
// <copyright file="PlatformExtensions.cs">
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml.Linq;
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
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using Object = Java.Lang.Object;
using WeakReference = System.WeakReference;

namespace MugenMvvmToolkit.Android
{
    // ReSharper disable once PartialTypeWithSinglePart
    public static partial class PlatformExtensions
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
                    for (int i = 0; i < _contentViewManagers.Count; i++)
                    {
                        if (_contentViewManagers[i].SetContent(view, content))
                            return;
                    }
                }
            }

            public void Add(IContentViewManager contentViewManager)
            {
                Should.NotBeNull(contentViewManager, "contentViewManager");
                lock (_contentViewManagers)
                    _contentViewManagers.Insert(0, contentViewManager);
            }

            public void Remove<TType>()
                where TType : IContentViewManager
            {
                lock (_contentViewManagers)
                {
                    for (int i = 0; i < _contentViewManagers.Count; i++)
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
                Should.NotBeNull(contentViewManager, "contentViewManager");
                lock (_contentViewManagers)
                    _contentViewManagers.Remove(contentViewManager);
            }

            #endregion
        }

        private sealed class WeakReferenceCollector
        {
            #region Fields

            private const int MaxGC = 4;
            private static int _gcCount;

            #endregion

            #region Finalizers

            ~WeakReferenceCollector()
            {
                try
                {
                    if (Interlocked.Increment(ref _gcCount) < MaxGC)
                        return;
                    ThreadPool.QueueUserWorkItem(state => Collect());
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

            #region Methods

            public static void Collect()
            {
                Interlocked.Exchange(ref _gcCount, 0);
                int collected = 0;
                int total = 0;
                foreach (var keyPair in WeakReferences)
                {
                    if (keyPair.Value.Target == null)
                    {
                        WeakReference value;
                        WeakReferences.TryRemove(keyPair.Key, out value);
                        ++collected;
                    }
                    else
                        ++total;
                }
                if (Tracer.TraceWarning)
                    Tracer.Warn("Collected " + collected + " weak references, total " + total);
            }

            #endregion
        }

        private sealed class WeakKeyEqualityComparer : IEqualityComparer<WeakKey>
        {
            #region Implementation of IEqualityComparer<WeakKey>

            public bool Equals(WeakKey x, WeakKey y)
            {
                return ReferenceEquals(x.GetItem(), y.GetItem());
            }

            public int GetHashCode(WeakKey obj)
            {
                return obj.Hash;
            }

            #endregion
        }

        private struct WeakKey
        {
            #region Fields

            public int Hash;
            private object _item;

            #endregion

            #region Constructors

            public WeakKey(object item)
            {
                _item = item;
                Hash = RuntimeHelpers.GetHashCode(item) * 397;
            }

            private WeakKey(WeakReference reference, int hash)
            {
                _item = reference;
                Hash = hash;
            }

            #endregion

            #region Methods

            public object GetItem()
            {
                var weakReference = _item as WeakReference;
                if (weakReference == null)
                    return _item;
                return weakReference.Target;
            }

            public WeakKey ToWeak(WeakReference reference)
            {
                return new WeakKey(reference, Hash);
            }

            public override string ToString()
            {
                var item = GetItem();
                if (item == null)
                    return Hash.ToString();
                return item.ToString();
            }

            #endregion
        }

        #endregion

        #region Fields

        public static readonly DataConstant<object> FragmentConstant;

        internal static readonly bool IsApiGreaterThan10;
        internal static readonly bool IsApiLessThanOrEqualTo10;
        internal static readonly bool IsApiGreaterThanOrEqualTo21;
        internal static readonly bool IsApiGreaterThanOrEqualTo14;
        internal static readonly bool IsApiGreaterThanOrEqualTo17;

        //NOTE ConditionalWeakTable invokes finalizer for value, even if the key object is still alive https://bugzilla.xamarin.com/show_bug.cgi?id=21620
        private static readonly ConcurrentDictionary<WeakKey, WeakReference> WeakReferences;
        private static readonly ContentViewManager ContentViewManagerField;
        private static readonly object CurrentActivityLocker;
        private static readonly ConcurrentDictionary<Type, Func<object[], object>> ViewToContextConstructor;
        private static readonly ConcurrentDictionary<Type, Func<object[], object>> ViewToContextWithAttrsConstructor;
        private static readonly Type[] ViewContextArgs;
        private static readonly Type[] ViewContextWithAttrsArgs;
        private static readonly Func<object, object> GetJniNativeInvoker;
        private static readonly Func<object, object> GetNewWeakGlobalRefDelegate;
        private static readonly Func<object, IntPtr, IntPtr, IntPtr> InvokeNewWeakGlobalRef;

        private static Func<Activity, IDataContext, IMvvmActivityMediator> _mvvmActivityMediatorFactory;
        private static Func<Context, IDataContext, BindableMenuInflater> _menuInflaterFactory;
        private static Func<Context, IDataContext, IViewFactory, LayoutInflater, LayoutInflater> _layoutInflaterFactory;
        private static Func<object, Context, object, int?, IDataTemplateSelector, object> _getContentViewDelegete;
        private static Action<object, object> _setContentViewDelegete;
        private static Func<object, bool> _isFragment;
        private static Func<object, bool> _isActionBar;
        private static WeakReference _activityRef;
        private static bool _useJniWeakReference;

        //If any exception using default JavaObjectWeakReference impl.
        private static bool _hasJNIException;

        #endregion

        #region Constructors

        static PlatformExtensions()
        {
            IsApiGreaterThan10 = Build.VERSION.SdkInt > BuildVersionCodes.GingerbreadMr1;
            IsApiLessThanOrEqualTo10 = !IsApiGreaterThan10;
            IsApiGreaterThanOrEqualTo21 = Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop;
            IsApiGreaterThanOrEqualTo14 = Build.VERSION.SdkInt >= BuildVersionCodes.IceCreamSandwich;
            IsApiGreaterThanOrEqualTo17 = Build.VERSION.SdkInt >= BuildVersionCodes.JellyBeanMr1;
            FragmentConstant = DataConstant.Create(() => FragmentConstant, false);
            _menuInflaterFactory = (context, dataContext) => new BindableMenuInflater(context);
            _layoutInflaterFactory = (context, dataContext, factory, inflater) =>
            {
                if (inflater == null)
                {
                    Tracer.Error("The bindable inflater cannot be created without the original inflater");
                    return null;
                }
                LayoutInflaterFactoryWrapper.SetFactory(inflater, factory);
                return inflater;
            };
            ContentViewManagerField = new ContentViewManager();
            ContentViewManagerField.Add(new ViewContentViewManager());
            _mvvmActivityMediatorFactory = MvvmActivityMediatorFactoryMethod;
            _getContentViewDelegete = GetContentViewInternal;
            _setContentViewDelegete = ContentViewManagerField.SetContent;
            _isFragment = o => false;
            _isActionBar = _isFragment;
            _activityRef = Empty.WeakReference;
            WeakReferences = new ConcurrentDictionary<WeakKey, WeakReference>(2, Empty.Array<KeyValuePair<WeakKey, WeakReference>>(), new WeakKeyEqualityComparer());
            ViewToContextConstructor = new ConcurrentDictionary<Type, Func<object[], object>>();
            ViewToContextWithAttrsConstructor = new ConcurrentDictionary<Type, Func<object[], object>>();
            ViewContextArgs = new[] { typeof(Context) };
            ViewContextWithAttrsArgs = new[] { typeof(Context), typeof(IAttributeSet) };
            CurrentActivityLocker = new object();
            _mvvmFragmentMediatorFactory = MvvmFragmentMediatorFactoryMethod;

            _useJniWeakReference = true;
            //Until Android 2.2 (Froyo), weak global references were not implemented.
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Froyo)
            {
                try
                {
                    var property = typeof(JNIEnv).GetProperty("Env", BindingFlags.Static | BindingFlags.NonPublic);
                    if (property != null)
                    {
                        GetJniNativeInvoker = ServiceProvider.ReflectionManager.GetMemberGetter<object>(property);
                        property = property.PropertyType.GetProperty("NewWeakGlobalRef", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                        if (property != null)
                        {
                            GetNewWeakGlobalRefDelegate = ServiceProvider.ReflectionManager.GetMemberGetter<object>(property);
                            var method = property.PropertyType.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance);
                            if (method != null)
                                InvokeNewWeakGlobalRef = (Func<object, IntPtr, IntPtr, IntPtr>)ServiceProvider.ReflectionManager.GetMethodDelegate(typeof(Func<object, IntPtr, IntPtr, IntPtr>), method);
                        }
                    }
                }
                catch
                {
                    ;
                }
            }

            _hasJNIException = InvokeNewWeakGlobalRef == null;
            // ReSharper disable once ObjectCreationAsStatement
            new WeakReferenceCollector();
        }

        #endregion

        #region Properties

        [CanBeNull]
        public static Func<IJavaObject, WeakReference> JavaWeakReferenceFactory { get; set; }

        [CanBeNull]
        public static Func<View, string, Context, IAttributeSet, View> ViewCreated { get; set; }

        [CanBeNull]
        public static IDataTemplateSelector DefaultDataTemplateSelector { get; set; }

        [NotNull]
        public static Func<Activity, IDataContext, IMvvmActivityMediator> MvvmActivityMediatorFactory
        {
            get { return _mvvmActivityMediatorFactory; }
            set
            {
                Should.PropertyNotBeNull(value);
                _mvvmActivityMediatorFactory = value;
            }
        }

        [NotNull]
        public static Func<Context, IDataContext, BindableMenuInflater> MenuInflaterFactory
        {
            get { return _menuInflaterFactory; }
            set
            {
                Should.PropertyNotBeNull(value);
                _menuInflaterFactory = value;
            }
        }

        [NotNull]
        public static Func<Context, IDataContext, IViewFactory, LayoutInflater, LayoutInflater> LayoutInflaterFactory
        {
            get { return _layoutInflaterFactory; }
            set
            {
                Should.PropertyNotBeNull(value);
                _layoutInflaterFactory = value;
            }
        }

        [NotNull]
        public static Func<object, Context, object, int?, IDataTemplateSelector, object> GetContentView
        {
            get { return _getContentViewDelegete; }
            set
            {
                Should.PropertyNotBeNull(value);
                _getContentViewDelegete = value;
            }
        }


        [NotNull]
        public static Action<object, object> SetContentView
        {
            get { return _setContentViewDelegete; }
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

        public static Activity CurrentActivity
        {
            get { return (Activity)_activityRef.Target; }
        }

        public static bool UseJNIWeakReference
        {
            get { return _useJniWeakReference && !_hasJNIException; }
            set { _useJniWeakReference = value; }
        }

        public static event EventHandler CurrentActivityChanged;

        #endregion

        #region Methods

        public static void ListenParentChange([CanBeNull] this View view)
        {
            if (!view.IsAlive())
                return;

            ParentObserver.Raise(view);
            if (view.GetTag(Resource.Id.ListenParentChange) != null)
                return;
            view.SetTag(Resource.Id.ListenParentChange, GlobalViewParentListener.Instance);
            var parent = BindingServiceProvider.VisualTreeManager.FindParent(view) as View;
            if (parent != null)
                parent.ListenParentChange();

            var viewGroup = view as ViewGroup;
            if (viewGroup != null)
            {
                if (!viewGroup.GetBindingMemberValue(AttachedMembers.ViewGroup.DisableHierarchyListener))
                    viewGroup.SetOnHierarchyChangeListener(GlobalViewParentListener.Instance);
                for (int i = 0; i < viewGroup.ChildCount; i++)
                    viewGroup.GetChildAt(i).ListenParentChange();
            }
        }

        public static void Inflate(this MenuInflater menuInflater, int menuRes, IMenu menu, object parent)
        {
            Should.NotBeNull(menuInflater, "menuInflater");
            var bindableMenuInflater = menuInflater as BindableMenuInflater;
            if (bindableMenuInflater == null)
                menuInflater.Inflate(menuRes, menu);
            else
                bindableMenuInflater.Inflate(menuRes, menu, parent);
        }

        [NotNull]
        public static LayoutInflater ToBindableLayoutInflater(this LayoutInflater inflater, Context context = null)
        {
            if (context == null)
            {
                Should.NotBeNull(inflater, "inflater");
                context = inflater.Context;
            }
            return LayoutInflaterFactory(context, null, null, inflater);
        }

        [NotNull]
        public static LayoutInflater GetBindableLayoutInflater([NotNull] this Context context)
        {
            Should.NotBeNull(context, "context");
            var activity = context.GetActivity();
            if (activity == null)
                return LayoutInflaterFactory(context, null, null, null);
            return activity.LayoutInflater.ToBindableLayoutInflater(context);
        }

        public static void ClearBindingsRecursively([CanBeNull]this View view, bool clearDataContext, bool clearAttachedValues)
        {
            if (view == null)
                return;
            var viewGroup = view as ViewGroup;
            if (viewGroup.IsAlive())
            {
                for (int i = 0; i < viewGroup.ChildCount; i++)
                    viewGroup.GetChildAt(i).ClearBindingsRecursively(clearDataContext, clearAttachedValues);
            }
            view.ClearBindings(clearDataContext, clearAttachedValues);
        }

        public static void NotifyActivityAttached([CanBeNull] Activity activity, [CanBeNull] View view)
        {
            if (!view.IsAlive() || !activity.IsAlive())
                return;
            var viewGroup = view as ViewGroup;
            if (viewGroup != null)
            {
                for (int i = 0; i < viewGroup.ChildCount; i++)
                    NotifyActivityAttached(activity, viewGroup.GetChildAt(i));
            }
            var dependency = view as IHasActivityDependency;
            if (dependency != null)
                dependency.OnAttached(activity);
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
            bool changed = false;
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
            {
                var handler = CurrentActivityChanged;
                if (handler != null)
                    handler(activity, EventArgs.Empty);
            }
        }

        public static void CleanupWeakReferences()
        {
            WeakReferenceCollector.Collect();
        }

        internal static void RemoveFromParent([CanBeNull] this View view)
        {
            if (!view.IsAlive())
                return;
            var viewGroup = view.Parent as ViewGroup;
            if (viewGroup != null)
                viewGroup.RemoveView(view);
        }

        internal static IMvvmActivityMediator GetOrCreateMediator(this Activity activity, ref IMvvmActivityMediator mediator)
        {
            if (mediator == null)
                Interlocked.CompareExchange(ref mediator, MvvmActivityMediatorFactory(activity, DataContext.Empty), null);
            return mediator;
        }

        internal static PlatformInfo GetPlatformInfo()
        {
            Version result;
            Version.TryParse(Build.VERSION.Release, out result);
            return new PlatformInfo(PlatformType.Android, result);
        }

        internal static bool IsSerializable(this Type type)
        {
            return type.IsDefined(typeof(DataContractAttribute), false) || type.IsPrimitive;
        }

        internal static WeakReference CreateWeakReference(object item)
        {
            if (item == null)
                return Empty.WeakReference;
            var obj = item as Object;
            if (obj != null && obj.Handle == IntPtr.Zero)
                return Empty.WeakReference;

            var hasWeakReference = item as IHasWeakReference;
            if (hasWeakReference != null && !(item is IHasWeakReferenceInternal))
                return CreateWeakReference(item, obj);


            var key = new WeakKey(item);
            WeakReference value;
            if (!WeakReferences.TryGetValue(key, out value))
            {
                value = CreateWeakReference(item, obj);
                WeakReferences[key.ToWeak(value)] = value;
                if (obj != null)
                {
                    var view = item as View;
                    if (view != null)
                    {
                        var activityView = view.Context.GetActivity() as IActivityView;
                        if (activityView != null)
                            activityView.Mediator.OnDependencyItemAttached(value);
                    }
                }
            }
            return value;
        }

        internal static object GetOrCreateView(IViewModel vm, bool? alwaysCreateNewView, IDataContext dataContext = null)
        {
            if (vm == null)
                return null;
            //NOTE: trying to use current fragment, if any.
            var fragment = vm.Settings.Metadata.GetData(FragmentConstant, false);
            if (fragment == null)
                return ViewManager.GetOrCreateView(vm, alwaysCreateNewView, dataContext);
            return fragment;
        }

        internal static string XmlTagsToUpper(string xml)
        {
            XDocument xDocument = XDocument.Parse(xml);
            foreach (XElement descendant in xDocument.Descendants())
            {
                foreach (XAttribute attribute in descendant.Attributes().ToArray())
                {
                    if (attribute.IsNamespaceDeclaration)
                        continue;
                    attribute.Remove();
                    var newAttribute = new XAttribute(attribute.Name.ToString().ToUpper(), attribute.Value);
                    descendant.Add(newAttribute);
                }
                descendant.Name = descendant.Name.ToString().ToUpper();
            }
            return xDocument.ToString();
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
                throw new ArgumentException(string.Format("To use a fragment {0}, you must specify the id for view {1}, for instance: @+id/placeholder", view, content), "view");
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
            object template = templateSelector.SelectTemplate(content, container);
            if (template is int)
                return GetContentInternal(ctx, content, (int)template);
            if (content != null && (template is View || IsFragment(template)))
                template.SetDataContext(content);
            return template;
        }

        private static object GetContentViewInternal(object container, Context ctx, object content, int? templateId, IDataTemplateSelector templateSelector)
        {
            Should.NotBeNull(container, "container");
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
                result = GetContentInternal(container, ctx, content, templateSelector);
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

        private static IMvvmActivityMediator MvvmActivityMediatorFactoryMethod(Activity activity, IDataContext dataContext)
        {
            return new MvvmActivityMediator(activity);
        }

        private static WeakReference CreateJavaWeakReferenceDefault(IJavaObject javaItem)
        {
            var weakReferenceFactory = JavaWeakReferenceFactory;
            if (weakReferenceFactory != null)
            {
                var weak = weakReferenceFactory(javaItem);
                if (weak != null)
                    return weak;
            }
            if (UseJNIWeakReference)
            {
                try
                {
                    var envHandle = JNIEnv.Handle;
                    var invoker = GetJniNativeInvoker(null);
                    var del = GetNewWeakGlobalRefDelegate(invoker);

                    var weakGlobalRef = InvokeNewWeakGlobalRef(del, envHandle, javaItem.Handle);
                    return new JNIWeakReference(javaItem, weakGlobalRef);
                }
                catch
                {
                    _hasJNIException = true;
                }
            }
            var o = javaItem as Object;
            if (o == null)
                return new WeakReference(javaItem, true);
            return new JavaObjectWeakReference(o);
        }

        private static WeakReference CreateWeakReference(object item, Object javaItem)
        {
            if (javaItem == null)
                return new WeakReference(item, true);
            return CreateJavaWeakReferenceDefault(javaItem);
        }

        #endregion
    }
}
