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
using Android.OS;
using Android.Support.V4.App;
using Android.Util;
using Android.Views;
using Android.Widget;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Builders;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Modules;
using MugenMvvmToolkit.DataConstants;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Infrastructure.Mediators;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Mediators;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
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
                return view;
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
                if (WeakReferences.Count == 0)
                    return;
                try
                {
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

        //NOTE ConditionalWeakTable invokes finalizer for value, even if the key object is still alive https://bugzilla.xamarin.com/show_bug.cgi?id=21620
        private static readonly List<WeakReference> WeakReferences;
        private const string VisitedParentPath = "$``!Visited~";
        private const string AddedToBackStackKey = "@$backstack";
        private static Func<Activity, IDataContext, IMvvmActivityMediator> _mvvmActivityMediatorFactory;
#if !API8
        private static Func<Fragment, IDataContext, IMvvmFragmentMediator> _mvvmFragmentMediatorFactory;
#endif
        #endregion

        #region Constructors

        static PlatformExtensions()
        {
#if !API8
            _mvvmFragmentMediatorFactory = MvvmFragmentMediatorFactoryMethod;
#endif
            _mvvmActivityMediatorFactory = MvvmActivityMediatorFactoryMethod;
            WeakReferences = new List<WeakReference>(256);
            // ReSharper disable once ObjectCreationAsStatement
            new WeakReferenceCollector();
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets or sets the default <see cref="IDataTemplateSelector"/>.
        /// </summary>
        [CanBeNull]
        public static IDataTemplateSelector DefaultDataTemplateSelector { get; set; }

#if !API8
        /// <summary>
        /// Gets or sets the custom home button finder.
        /// </summary>
        [CanBeNull]
        public static Func<Activity, View> HomeButtonFinder { get; set; }
#endif
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

#if !API8
        /// <summary>
        ///     Gets or sets the factory that creates an instance of <see cref="IMvvmFragmentMediator" />.
        /// </summary>
        [NotNull]
        public static Func<Fragment, IDataContext, IMvvmFragmentMediator> MvvmFragmentMediatorFactory
        {
            get { return _mvvmFragmentMediatorFactory; }
            set
            {
                Should.PropertyBeNotNull(value);
                _mvvmFragmentMediatorFactory = value;
            }
        }
#endif
        #endregion

        #region Methods

        /// <summary>
        /// Gets the content view for the specified content.
        /// </summary>
        [CanBeNull]
        public static object GetContentView(object container, Context ctx, object content, int? templateId, IDataTemplateSelector templateSelector)
        {
            Should.NotBeNull(container, "container");
            object result;
            if (templateSelector != null)
            {
                result = GetContentInternal(container, ctx, content, templateSelector);
#if API8
                if (result is View)
#else
                if (result is View || result is Fragment)
#endif
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
#if API8
            if (content is View)
#else
            if (content is View || content is Fragment)
#endif
                return content;

            var selector = DefaultDataTemplateSelector;
            if (selector != null)
            {
                result = GetContentInternal(container, ctx, content, templateSelector);
#if API8
                if (result is View)
#else
                if (result is View || result is Fragment)
#endif
                    return result;
                content = result;
            }
            if (content == null)
                return null;
            Tracer.Warn("The content value {0} is not a View or Fragment.", content);
            result = new TextView(ctx) { Text = content.ToString() };
            return result;
        }

#if API8
        /// <summary>
        ///     Sets the content.
        /// </summary>
        public static object SetContentView([NotNull] this ViewGroup frameLayout, object content, int? templateId,
            IDataTemplateSelector templateSelector)
        {
            content = GetContentView(frameLayout, frameLayout.Context, content, templateId, templateSelector);
            frameLayout.SetContentView(content);
            return content;
        }

        public static void SetContentView([NotNull]this ViewGroup frameLayout, object content)
        {
            Should.NotBeNull(frameLayout, "frameLayout");
            if (content == null)
            {
                frameLayout.RemoveAllViews();
                return;
            }
            Should.BeOfType<View>(content, "content");
            if (frameLayout.ChildCount == 1 && frameLayout.GetChildAt(0) == content)
                return;
            frameLayout.RemoveAllViews();
            frameLayout.AddView((View)content);
        }
#else
        /// <summary>
        ///     Sets the content.
        /// </summary>
        public static object SetContentView([NotNull] this ViewGroup frameLayout, object content, int? templateId,
            IDataTemplateSelector templateSelector, FragmentTransaction transaction = null, Action<ViewGroup, Fragment, FragmentTransaction> updateAction = null)
        {
            content = GetContentView(frameLayout, frameLayout.Context, content, templateId, templateSelector);
            frameLayout.SetContentView(content, transaction, updateAction);
            return content;
        }

        public static void SetContentView([NotNull]this ViewGroup frameLayout, object content,
            FragmentTransaction transaction = null, Action<ViewGroup, Fragment, FragmentTransaction> updateAction = null)
        {
            Should.NotBeNull(frameLayout, "frameLayout");
            if (content == null)
            {
                bool hasFragment = false;
                var fragmentManager = frameLayout.GetFragmentManager();
                if (fragmentManager != null)
                {
                    var fragment = fragmentManager.FindFragmentById(frameLayout.Id);
                    hasFragment = fragment != null;
                    if (hasFragment && !fragmentManager.IsDestroyed)
                    {
                        fragmentManager.BeginTransaction().Remove(fragment).CommitAllowingStateLoss();
                        fragmentManager.ExecutePendingTransactions();
                    }
                }
                if (!hasFragment)
                    frameLayout.RemoveAllViews();
                return;
            }

            var view = content as View;
            if (view == null)
            {
                var fragment = (Fragment)content;
                ValidateViewIdFragment(frameLayout, fragment);
                var addToBackStack = PlatformDataBindingModule.AddToBackStackMember.GetValue(frameLayout, null);
                FragmentManager manager = null;
                if (transaction == null)
                {
                    manager = frameLayout.GetFragmentManager();
                    if (manager == null)
                        return;
                    transaction = manager.BeginTransaction();
                }
                if (addToBackStack && fragment.Arguments != null)
                    addToBackStack = !fragment.Arguments.GetBoolean(AddedToBackStackKey);

                if (updateAction == null)
                {
                    if (fragment.IsDetached)
                        transaction.Attach(fragment);
                    else
                    {
                        if (addToBackStack)
                        {
                            if (fragment.Arguments == null)
                                fragment.Arguments = new Bundle();
                            fragment.Arguments.PutBoolean(AddedToBackStackKey, true);
                        }
                        transaction.Replace(frameLayout.Id, fragment);
                    }
                }
                else
                    updateAction(frameLayout, fragment, transaction);
                if (addToBackStack)
                    transaction.AddToBackStack(null);


                if (manager != null)
                {
                    transaction.Commit();
                    manager.ExecutePendingTransactions();
                }
            }
            else
            {
                if (frameLayout.ChildCount == 1 && frameLayout.GetChildAt(0) == view)
                    return;
                frameLayout.RemoveAllViews();
                frameLayout.AddView(view);
            }
        }
#endif

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

        public static IList<IDataBinding> SetBindings(this Object item, string bindingExpression,
            IList<object> sources = null)
        {
            return BindingServiceProvider.BindingProvider.CreateBindingsFromString(item, bindingExpression, sources);
        }

        public static T SetBindings<T, TBindingSet>([NotNull] this T item, [NotNull] TBindingSet bindingSet,
            [NotNull] string bindings)
            where T : Object
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
            where T : Object
            where TBindingSet : BindingSet
        {
            Should.NotBeNull(item, "item");
            Should.NotBeNull(bindingSet, "bindingSet");
            Should.NotBeNull(setBinding, "setBinding");
            setBinding(bindingSet, item);
            return item;
        }

        public static void ClearBindingsHierarchically([CanBeNull]this View view, bool clearDataContext, bool clearAttachedValues, bool disposeView)
        {
            if (view == null)
                return;
            var viewGroup = view as ViewGroup;
            if (viewGroup != null)
            {
                for (int i = 0; i < viewGroup.ChildCount; i++)
                    viewGroup.GetChildAt(i).ClearBindingsHierarchically(clearDataContext, clearAttachedValues, disposeView);
            }
            view.ClearBindings(clearDataContext, clearAttachedValues, disposeView);
        }

        public static void ClearBindings([CanBeNull]this Object item, bool clearDataContext, bool clearAttachedValues, bool disposeObj)
        {
            BindingExtensions.ClearBindings(item, clearDataContext, clearAttachedValues, disposeObj);
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
#if API8            
            return ViewManager.GetOrCreateView(vm, alwaysCreateNewView, dataContext);
#else
            //NOTE: trying to use current fragment, if any.
            var fragment = vm.Settings.Metadata.GetData(MvvmFragmentMediator.CurrentFragment, false);
            if (fragment == null)
                return ViewManager.GetOrCreateView(vm, alwaysCreateNewView, dataContext);
            return fragment;
#endif
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

        internal static bool IsAlive(this Object javaObj)
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

#if !API8
        internal static FragmentManager GetFragmentManager(this View view)
        {
            var treeView = view;
            while (treeView != null)
            {
                var fragment = MvvmFragmentMediator.FragmentViewMember.GetValue(treeView, null);
                if (fragment != null)
                    return fragment.ChildFragmentManager;
                treeView = treeView.Parent as View;
            }
            var activity = view.Context.GetActivity();
            if (activity == null)
            {
                Tracer.Warn("The activity is null {0}", view);
                return null;
            }
            return activity.GetFragmentManager();
        }

        private static void ValidateViewIdFragment(View view, Fragment content)
        {
            if (view.Id == View.NoId)
                throw new ArgumentException(string.Format("To use a fragment {0}, you must specify the id for view {1}, for instance: @+id/placeholder", view, content),
                    "view");
        }

        private static IMvvmFragmentMediator MvvmFragmentMediatorFactoryMethod(Fragment fragment, IDataContext dataContext)
        {
            return new MvvmFragmentMediator(fragment);
        }
#endif

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

        private static object GetContentInternal(object container, Context ctx, object content, IDataTemplateSelector templateSelector)
        {
            object template = templateSelector.SelectTemplate(content, container);
            var newView = template as View;
            if (newView != null)
            {
                BindingServiceProvider.ContextManager.GetBindingContext(newView).Value = content;
                return newView;
            }
            if (template is int)
                return GetContentInternal(ctx, content, (int)template);
#if !API8
            var fragment = template as Fragment;
            if (fragment != null)
            {
                BindingServiceProvider.ContextManager.GetBindingContext(fragment).Value = content;
                return fragment;
            }
#endif
            return template;
        }

        private static IMvvmActivityMediator MvvmActivityMediatorFactoryMethod(Activity activity, IDataContext dataContext)
        {
            return new MvvmActivityMediator(activity);
        }

        #endregion
    }
}