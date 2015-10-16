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
using System.Collections.Generic;
using System.Linq;
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
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using Object = Java.Lang.Object;

namespace MugenMvvmToolkit.Android
{
    // ReSharper disable once PartialTypeWithSinglePart
    public static partial class PlatformExtensions
    {
        #region Nested types

        private sealed class WeakReferenceChain : WeakReference
        {
            #region Fields

            public WeakReferenceChain Next;

            #endregion

            #region Constructors

            public WeakReferenceChain(object target)
                : base(target, true)
            {
            }

            #endregion

            #region Overrides of WeakReference

            public override object Target
            {
                get { return base.Target; }
                set { throw new NotSupportedException(); }
            }

            #endregion
        }

        private sealed class IntPtrComparer : IEqualityComparer<IntPtr>
        {
            #region Implementation of IEqualityComparer<in IntPtr>

            public bool Equals(IntPtr x, IntPtr y)
            {
                return x == y;
            }

            public int GetHashCode(IntPtr obj)
            {
                return obj.GetHashCode();
            }

            #endregion
        }

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
            ~WeakReferenceCollector()
            {
                try
                {
                    if (_weakReferenceChainHead != null)
                    {
                        lock (ReferenceChainLocker)
                        {
                            WeakReferenceChain head = null;
                            WeakReferenceChain tail = null;
                            var current = _weakReferenceChainHead;
                            while (current != null)
                            {
                                if (current.Target != null)
                                {
                                    if (head == null)
                                    {
                                        head = current;
                                        tail = current;
                                    }
                                    else
                                    {
                                        tail.Next = current;
                                        tail = current;
                                    }
                                }
                                current = current.Next;
                            }
                        }
                    }
                    if (NativeWeakReferences.Count != 0)
                    {
                        lock (NativeWeakReferences)
                        {
                            List<IntPtr> itemsToDelete = null;
                            foreach (var keyPair in NativeWeakReferences)
                            {
                                if (keyPair.Value.Target != null)
                                    continue;
                                if (itemsToDelete == null)
                                    itemsToDelete = new List<IntPtr>();
                                itemsToDelete.Add(keyPair.Key);
                            }
                            if (itemsToDelete != null)
                            {
                                for (int i = 0; i < itemsToDelete.Count; i++)
                                    NativeWeakReferences.Remove(itemsToDelete[i]);
                            }
                        }
                    }
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
        private static WeakReferenceChain _weakReferenceChainTail;
        private static WeakReferenceChain _weakReferenceChainHead;
        private static readonly Dictionary<IntPtr, JavaObjectWeakReference> NativeWeakReferences;
        private static readonly ContentViewManager ContentViewManagerField;
        private static readonly object CurrentActivityLocker;
        private static readonly object ReferenceChainLocker;
        private static Func<Activity, IDataContext, IMvvmActivityMediator> _mvvmActivityMediatorFactory;
        private static Func<Context, IDataContext, BindableMenuInflater> _menuInflaterFactory;
        private static Func<Context, IDataContext, IViewFactory, LayoutInflater, BindableLayoutInflater> _layoutInflaterFactory;
        private static Func<object, Context, object, int?, IDataTemplateSelector, object> _getContentViewDelegete;
        private static Action<object, object> _setContentViewDelegete;
        private static Func<object, bool> _isFragment;
        private static Func<object, bool> _isActionBar;
        private static WeakReference _activityRef;

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
                if (factory == null && !ServiceProvider.TryGet(out factory))
                    factory = new ViewFactory();
                if (inflater == null)
                    return new BindableLayoutInflater(factory, context);
                return new BindableLayoutInflater(factory, inflater, context);
            };
            ContentViewManagerField = new ContentViewManager();
            ContentViewManagerField.Add(new ViewContentViewManager());
            _mvvmActivityMediatorFactory = MvvmActivityMediatorFactoryMethod;
            _getContentViewDelegete = GetContentViewInternal;
            _setContentViewDelegete = ContentViewManagerField.SetContent;
            _isFragment = o => false;
            _isActionBar = _isFragment;
            _activityRef = Empty.WeakReference;
            NativeWeakReferences = new Dictionary<IntPtr, JavaObjectWeakReference>(109, new IntPtrComparer());
            CurrentActivityLocker = new object();
            ReferenceChainLocker = new object();
            FragmentViewMember = AttachedBindingMember.CreateAutoProperty<View, object>("!$fragment");
            _mvvmFragmentMediatorFactory = MvvmFragmentMediatorFactoryMethod;
            // ReSharper disable once ObjectCreationAsStatement
            new WeakReferenceCollector();
        }

        #endregion

        #region Properties

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
        public static Func<Context, IDataContext, IViewFactory, LayoutInflater, BindableLayoutInflater> LayoutInflaterFactory
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
        public static BindableLayoutInflater ToBindableLayoutInflater(this LayoutInflater inflater, Context context = null)
        {
            if (context == null)
            {
                Should.NotBeNull(inflater, "inflater");
                context = inflater.Context;
            }
            var bindableInflater = inflater as BindableLayoutInflater;
            if (bindableInflater != null)
                return bindableInflater;
            return LayoutInflaterFactory(context, DataContext.Empty, null, inflater);
        }

        [NotNull]
        public static BindableLayoutInflater GetBindableLayoutInflater([NotNull] this Context context)
        {
            Should.NotBeNull(context, "context");
            var activity = context.GetActivity();
            if (activity == null)
                return LayoutInflaterFactory(context, DataContext.Empty, null, null);
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

        [CanBeNull]
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
            var obj = item as Object;
            if (obj == null)
            {
                var reference = new WeakReferenceChain(item);
                lock (ReferenceChainLocker)
                {
                    if (_weakReferenceChainHead == null)
                    {
                        _weakReferenceChainHead = reference;
                        _weakReferenceChainTail = reference;
                    }
                    else
                    {
                        _weakReferenceChainTail.Next = reference;
                        _weakReferenceChainTail = reference;
                    }
                }
                return reference;
            }
            var handle = obj.Handle;
            if (handle == IntPtr.Zero)
                return Empty.WeakReference;

            lock (NativeWeakReferences)
            {
                JavaObjectWeakReference value;
                if (!NativeWeakReferences.TryGetValue(handle, out value) || !ReferenceEquals(value.Target, obj))
                {
                    value = new JavaObjectWeakReference(obj);
                    NativeWeakReferences[handle] = value;
                }
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

        internal static bool IsAlive([CanBeNull] this IJavaObject javaObj)
        {
            return javaObj != null && javaObj.Handle != IntPtr.Zero;
        }

        internal static View CreateView(this Type type, Context ctx)
        {
            return (View)Activator.CreateInstance(type, ctx);
        }

        internal static View CreateView(this Type type, Context ctx, IAttributeSet set)
        {
            return (View)Activator.CreateInstance(type, ctx, set);
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
                throw new ArgumentException(string.Format("To use a fragment {0}, you must specify the id for view {1}, for instance: @+id/placeholder", view, content),
                    "view");
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

        #endregion
    }
}
