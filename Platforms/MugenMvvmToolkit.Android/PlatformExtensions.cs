#region Copyright
// ****************************************************************************
// <copyright file="PlatformExtensions.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using System.Xml.Linq;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Builders;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Mediators;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Mediators;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Interfaces.Views;
using MugenMvvmToolkit.Models;
using ViewManager = MugenMvvmToolkit.Infrastructure.ViewManager;
using Object = Java.Lang.Object;

namespace MugenMvvmToolkit
{
    public static class PlatformExtensions
    {
        #region Nested types

        private sealed class BindingFactory : Object, LayoutInflater.IFactory
        {
            #region Fields

            private readonly IBindingProvider _bindingProvider;
            private readonly List<IDataBinding> _bindings;
            private readonly IViewFactory _viewFactory;

            #endregion

            #region Constructors

            public BindingFactory(IBindingProvider bindingProvider, IViewFactory viewFactory)
            {
                _bindingProvider = bindingProvider;
                _viewFactory = viewFactory;
                _bindings = new List<IDataBinding>();
            }

            #endregion

            #region Properties

            public IList<IDataBinding> Bindings
            {
                get { return _bindings; }
            }

            #endregion

            #region Implementation of IFactory

            public View OnCreateView(string name, Context context, IAttributeSet attrs)
            {
                if (name == "fragment")
                    return null;
                ViewResult viewResult = _viewFactory.Create(name, context, attrs);
                View view = viewResult.View;
                IList<string> bindings = viewResult.DataContext.GetData(ViewFactoryConstants.Bindings);
                if (bindings != null)
                {
                    var manualBindings = view as IManualBindings;
                    if (manualBindings == null)
                    {
                        foreach (string binding in bindings)
                            SetBinding(view, binding);
                    }
                    else
                        _bindings.AddRange(manualBindings.SetBindings(bindings));
                }
                var viewCreated = ViewCreated;
                if (viewCreated == null)
                    return view;
                return viewCreated(view, name, context, attrs);
            }

            #endregion

            #region Methods

            private void SetBinding(object source, string bindingExpression)
            {
                _bindings.AddRange(_bindingProvider.CreateBindingsFromString(source, bindingExpression, null));
            }

            #endregion
        }

        private sealed class WeakReferenceCollector
        {
            ~WeakReferenceCollector()
            {
                try
                {
                    if (WeakReferences.Count == 0)
                        return;
                    lock (WeakReferences)
                    {
                        for (int i = 0; i < WeakReferences.Count; i++)
                        {
                            if (WeakReferences[i].Target == null)
                            {
                                WeakReferences.RemoveAt(i);
                                i--;
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

        /// <summary>
        ///     Gets the constant that returns current fragment.
        /// </summary>
        public static readonly DataConstant<object> CurrentFragment;

        //NOTE ConditionalWeakTable invokes finalizer for value, even if the key object is still alive https://bugzilla.xamarin.com/show_bug.cgi?id=21620
        private static readonly List<WeakReference> WeakReferences;
        private const string VisitedParentPath = "$``!Visited~";
        private static Func<Activity, IDataContext, IMvvmActivityMediator> _mvvmActivityMediatorFactory;
        private static Func<Context, IDataContext, MenuInflater> _menuInflaterFactory;
        private static Func<object, Context, object, int?, IDataTemplateSelector, object> _getContentViewDelegete;
        private static Func<ViewGroup, object, int?, IDataTemplateSelector, object> _setContentViewDelegete;
        private static Func<object, bool> _isFragment;
        private static Func<object, bool> _isActionBar;

        #endregion

        #region Constructors

        static PlatformExtensions()
        {
            CurrentFragment = DataConstant.Create(() => CurrentFragment, true);
            _menuInflaterFactory = (context, dataContext) => new BindableMenuInflater(context);
            _mvvmActivityMediatorFactory = MvvmActivityMediatorFactoryMethod;
            _getContentViewDelegete = GetContentViewInternal;
            _setContentViewDelegete = SetContentViewInternal;
            _isFragment = o => false;
            _isActionBar = _isFragment;
            WeakReferences = new List<WeakReference>(256);
            // ReSharper disable once ObjectCreationAsStatement
            new WeakReferenceCollector();
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets the delegate that allows to handle view creation.
        /// </summary>
        [CanBeNull]
        public static Func<View, string, Context, IAttributeSet, View> ViewCreated { get; set; }

        /// <summary>
        ///     Gets or sets the default <see cref="IDataTemplateSelector" />.
        /// </summary>
        [CanBeNull]
        public static IDataTemplateSelector DefaultDataTemplateSelector { get; set; }


        /// <summary>
        ///     Gets or sets the factory that creates an instance of <see cref="IMvvmActivityMediator" />.
        /// </summary>
        [NotNull]
        public static Func<Activity, IDataContext, IMvvmActivityMediator> MvvmActivityMediatorFactory
        {
            get { return _mvvmActivityMediatorFactory; }
            set
            {
                Should.PropertyBeNotNull(value);
                _mvvmActivityMediatorFactory = value;
            }
        }

        /// <summary>
        ///     Gets or sets the factory that creates an instance of <see cref="MenuInflater" />.
        /// </summary>
        [NotNull]
        public static Func<Context, IDataContext, MenuInflater> MenuInflaterFactory
        {
            get { return _menuInflaterFactory; }
            set
            {
                Should.PropertyBeNotNull(value);
                _menuInflaterFactory = value;
            }
        }

        /// <summary>
        ///     Gets or sets the delegate that initializes a content view.
        /// </summary>
        [NotNull]
        public static Func<object, Context, object, int?, IDataTemplateSelector, object> GetContentView
        {
            get { return _getContentViewDelegete; }
            set
            {
                Should.PropertyBeNotNull(value);
                _getContentViewDelegete = value;
            }
        }


        /// <summary>
        ///     Gets or sets the delegate that initializes a content view.
        /// </summary>
        [NotNull]
        public static Func<ViewGroup, object, int?, IDataTemplateSelector, object> SetContentView
        {
            get { return _setContentViewDelegete; }
            set
            {
                Should.PropertyBeNotNull(value);
                _setContentViewDelegete = value;
            }
        }

        /// <summary>
        ///     Gets or sets the delegate that determines that an object is fragment.
        /// </summary>
        [NotNull]
        public static Func<object, bool> IsFragment
        {
            get { return _isFragment; }
            set
            {
                Should.PropertyBeNotNull(value);
                _isFragment = value;
            }
        }

        /// <summary>
        ///     Gets or sets the delegate that determines that an object is action bar.
        /// </summary>
        public static Func<object, bool> IsActionBar
        {
            get { return _isActionBar; }
            set
            {
                Should.PropertyBeNotNull(value);
                _isActionBar = value;
            }
        }

        #endregion

        #region Methods

        public static void ListenParentChange([NotNull] this View view)
        {
            Should.NotBeNull(view, "view");
            if (view.Context == null)
                return;
            ParentObserver.Raise(view);
            if (ServiceProvider.AttachedValueProvider.GetValue<object>(view, VisitedParentPath, false) != null)
                return;
            ServiceProvider.AttachedValueProvider.SetValue(view, VisitedParentPath, view);
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

        /// <summary>
        ///     Inflate a menu hierarchy from the specified XML resource.
        /// </summary>
        public static void Inflate(this MenuInflater menuInflater, int menuRes, IMenu menu, object parent)
        {
            Should.NotBeNull(menuInflater, "menuInflater");
            var bindableMenuInflater = menuInflater as IBindableMenuInflater;
            if (bindableMenuInflater == null)
                menuInflater.Inflate(menuRes, menu);
            else
                bindableMenuInflater.Inflate(menuRes, menu, parent);
        }

        public static Tuple<View, IList<IDataBinding>> CreateBindableView([NotNull] this Activity activity,
            int layoutResId, IViewFactory viewFactory = null)
        {
            Should.NotBeNull(activity, "activity");
            return CreateBindableView(activity.LayoutInflater, layoutResId, null, false, viewFactory);
        }

        public static Tuple<View, IList<IDataBinding>> CreateBindableView([NotNull] this Activity activity,
            int layoutResId, ViewGroup viewGroup, bool attachToRoot, IViewFactory viewFactory = null)
        {
            Should.NotBeNull(activity, "activity");
            return CreateBindableView(activity.LayoutInflater, layoutResId, viewGroup, attachToRoot,
                viewFactory);
        }

        public static Tuple<View, IList<IDataBinding>> CreateBindableView([NotNull] this LayoutInflater layoutInflater,
            int layoutResId, IViewFactory viewFactory = null)
        {
            return CreateBindableView(layoutInflater, layoutResId, null, false, viewFactory);
        }

        public static Tuple<View, IList<IDataBinding>> CreateBindableView(
            [NotNull] this LayoutInflater layoutInflater, int layoutResId, ViewGroup viewGroup, bool attachToRoot,
            IViewFactory viewFactory = null)
        {
            Should.NotBeNull(layoutInflater, "layoutInflater");
            if (viewFactory == null)
                viewFactory = ServiceProvider.IocContainer.Get<IViewFactory>();
            using (LayoutInflater inflater = layoutInflater.CloneInContext(layoutInflater.Context))
            using (var bindingFactory = new BindingFactory(BindingServiceProvider.BindingProvider, viewFactory))
            {
                inflater.Factory = bindingFactory;
                View view = inflater.Inflate(layoutResId, viewGroup, attachToRoot);
                return Tuple.Create(view, bindingFactory.Bindings);
            }
        }

        public static IList<IDataBinding> SetBindings(this IJavaObject item, string bindingExpression,
            IList<object> sources = null)
        {
            return BindingServiceProvider.BindingProvider.CreateBindingsFromString(item, bindingExpression, sources);
        }

        public static T SetBindings<T, TBindingSet>([NotNull] this T item, [NotNull] TBindingSet bindingSet,
            [NotNull] string bindings)
            where T : IJavaObject
            where TBindingSet : BindingSet
        {
            Should.NotBeNull(item, "item");
            Should.NotBeNull(bindingSet, "bindingSet");
            Should.NotBeNull(bindings, "bindings");
            bindingSet.BindFromExpression(item, bindings);
            return item;
        }


        public static T SetBindings<T, TBindingSet>([NotNull] this T item, [NotNull] TBindingSet bindingSet,
            [NotNull] Action<TBindingSet, T> setBinding)
            where T : IJavaObject
            where TBindingSet : BindingSet
        {
            Should.NotBeNull(item, "item");
            Should.NotBeNull(bindingSet, "bindingSet");
            Should.NotBeNull(setBinding, "setBinding");
            setBinding(bindingSet, item);
            return item;
        }

        public static void ClearBindingsHierarchically([CanBeNull]this View view, bool clearDataContext, bool clearAttachedValues)
        {
            if (view == null || !view.IsAlive())
                return;
            var viewGroup = view as ViewGroup;
            if (viewGroup != null)
            {
                for (int i = 0; i < viewGroup.ChildCount; i++)
                    viewGroup.GetChildAt(i).ClearBindingsHierarchically(clearDataContext, clearAttachedValues);
            }
            view.ClearBindings(clearDataContext, clearAttachedValues);
        }

        public static void ClearBindings([CanBeNull]this IJavaObject item, bool clearDataContext, bool clearAttachedValues)
        {
            BindingExtensions.ClearBindings(item, clearDataContext, clearAttachedValues);
        }

        public static void NotifyActivityAttached([CanBeNull] Activity activity, [CanBeNull] View view)
        {
            if (view == null || activity == null)
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

        internal static PlatformInfo GetPlatformInfo()
        {
            Version result;
            Version.TryParse(Android.OS.Build.VERSION.Release, out result);
            return new PlatformInfo(PlatformType.Android, result);
        }

        internal static bool IsSerializable(this Type type)
        {
            return type.IsDefined(typeof(DataContractAttribute), false) || type.IsPrimitive;
        }

        internal static WeakReference CreateWeakReference(object item, bool trackResurrection)
        {
            var obj = item as Object;
            var reference = obj == null
                ? new WeakReference(item, trackResurrection)
                : new JavaObjectWeakReference(obj, trackResurrection);
            lock (WeakReferences)
                WeakReferences.Add(reference);
            return reference;
        }

        internal static object GetOrCreateView(IViewModel vm, bool? alwaysCreateNewView, IDataContext dataContext = null)
        {
            //NOTE: trying to use current fragment, if any.
            var fragment = vm.Settings.Metadata.GetData(CurrentFragment, false);
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

        internal static bool IsAlive(this IJavaObject javaObj)
        {
            return javaObj.Handle != IntPtr.Zero;
        }

        internal static View CreateView(this Type type, Context ctx)
        {
            return (View)Activator.CreateInstance(type, ctx);
        }

        internal static View CreateView(this Type type, Context ctx, IAttributeSet set)
        {
            return (View)Activator.CreateInstance(type, ctx, set);
        }

        internal static Activity GetActivity(this Context context)
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

        internal static string ToStringSafe(this object item, string defaultValue = null)
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
            var newView = LayoutInflater
                    .From(ctx)
                    .CreateBindableView(templateId.Value).Item1;
            BindingServiceProvider
                .ContextManager
                .GetBindingContext(newView)
                .Value = content;
            return newView;
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

        private static object SetContentViewInternal(ViewGroup frameLayout, object content, int? templateId, IDataTemplateSelector templateSelector)
        {
            content = GetContentView(frameLayout, frameLayout.Context, content, templateId, templateSelector);
            if (content == null)
            {
                frameLayout.RemoveAllViews();
                return null;
            }
            Should.BeOfType<View>(content, "content");
            if (frameLayout.ChildCount == 1 && frameLayout.GetChildAt(0) == content)
                return content;
            frameLayout.RemoveAllViews();
            frameLayout.AddView((View)content);
            return content;
        }

        private static object GetContentInternal(object container, Context ctx, object content, IDataTemplateSelector templateSelector)
        {
            object template = templateSelector.SelectTemplate(content, container);
            if (template is int)
                return GetContentInternal(ctx, content, (int)template);
            if (template is View || IsFragment(template))
                BindingServiceProvider.ContextManager.GetBindingContext(template).Value = content;
            return template;
        }

        private static IMvvmActivityMediator MvvmActivityMediatorFactoryMethod(Activity activity, IDataContext dataContext)
        {
            return new MvvmActivityMediator(activity);
        }

        #endregion
    }
}