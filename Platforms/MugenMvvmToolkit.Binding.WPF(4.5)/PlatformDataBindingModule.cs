#region Copyright
// ****************************************************************************
// <copyright file="PlatformDataBindingModule.cs">
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
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using MugenMvvmToolkit.MarkupExtensions;
#if NETFX_CORE || WINDOWSCOMMON
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
#endif
using MugenMvvmToolkit.Binding.Converters;
using MugenMvvmToolkit.Binding.Core;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
using BooleanToVisibilityConverter = MugenMvvmToolkit.Binding.Converters.BooleanToVisibilityConverter;

namespace MugenMvvmToolkit.Binding
{
    public class PlatformDataBindingModule : DataBindingModule
    {
        #region Nested types

        private sealed class ParentListener : EventListenerList
        {
            #region Fields

            private readonly WeakReference _view;
            private WeakReference _parent;
            private bool _isAttached;

            #endregion

            #region Constructors

            private ParentListener(FrameworkElement view)
            {
                _view = ServiceProvider.WeakReferenceFactory(view, true);
                _parent = ServiceProvider.WeakReferenceFactory(FindParent(view), true);
                RoutedEventHandler handler = OnChanged;
                view.Loaded += handler;
                view.Unloaded += handler;
            }

            #endregion

            #region Properties

            public DependencyObject Parent
            {
                get { return _parent.Target as DependencyObject; }
                set
                {
                    _isAttached = true;
                    SetParent(value);
                }
            }

            #endregion

            #region Methods

            public static ParentListener GetOrAdd(FrameworkElement element)
            {
                return ServiceProvider.AttachedValueProvider.GetOrAdd(element, "#ParentListener",
                    (frameworkElement, o) => new ParentListener(frameworkElement), null);
            }

            private void OnChanged(object sender, RoutedEventArgs routedEventArgs)
            {
                var view = (FrameworkElement)_view.Target;
                if (view == null)
                {
                    Clear();
                    return;
                }
                if (!_isAttached)
                    SetParent(FindParent(view));
            }

            private void SetParent(DependencyObject value)
            {
                if (ReferenceEquals(value, _parent.Target))
                    return;
                _parent = ServiceProvider.WeakReferenceFactory(value, true);
                Raise(_view.Target, EventArgs.Empty);
            }

            #endregion
        }

        #endregion

        #region Fields

        internal readonly static IAttachedBindingMemberInfo<FrameworkElement, bool> DisableValidationMember;

        #endregion

        #region Constructors

        static PlatformDataBindingModule()
        {
            if (View.OnBindChanged == null)
                View.OnBindChanged = OnBindChanged;
            DisableValidationMember = AttachedBindingMember.CreateAutoProperty<FrameworkElement, bool>("DisableValidation");
        }

        #endregion

        #region Methods

        /// <summary>
        /// Tries to set attached parent.
        /// </summary>
        public static void SetAttachedParent([CanBeNull] FrameworkElement target, [CanBeNull] DependencyObject parent)
        {
            if (target != null)
                ParentListener.GetOrAdd(target).Parent = parent;
        }

        private static void Register(IBindingMemberProvider memberProvider)
        {
            Should.NotBeNull(memberProvider, "memberProvider");
            //UIElement
            memberProvider.Register(AttachedBindingMember.CreateMember<UIElement, bool>("Visible",
                    (info, view, arg3) => view.Visibility == Visibility.Visible,
                    (info, view, arg3) => view.Visibility = ((bool)arg3[0]) ? Visibility.Visible : Visibility.Collapsed, ObserveVisiblityMember));
            memberProvider.Register(AttachedBindingMember.CreateMember<UIElement, bool>("Hidden",
                    (info, view, arg3) => view.Visibility != Visibility.Visible,
                    (info, view, arg3) => view.Visibility = ((bool)arg3[0]) ? Visibility.Collapsed : Visibility.Visible, ObserveVisiblityMember));


            //FrameworkElement      
            memberProvider.Register(DisableValidationMember);
            memberProvider.Register(AttachedBindingMember
                .CreateMember<FrameworkElement, object>(AttachedMemberConstants.Parent, GetParentValue, null, ObserveParentMember));
            memberProvider.Register(AttachedBindingMember
                .CreateMember<FrameworkElement, object>(AttachedMemberConstants.FindByNameMethod, FindByNameMemberImpl, null));
#if SILVERLIGHT || NETFX_CORE || WINDOWSCOMMON
            memberProvider.Register(AttachedBindingMember
                .CreateMember<FrameworkElement, bool>(AttachedMemberConstants.Focused,
                    (info, control, arg3) => FocusManager.GetFocusedElement() == control, null,
                    memberChangeEventName: "LostFocus"));
            memberProvider.Register(AttachedBindingMember
                .CreateMember<Control, bool>(AttachedMemberConstants.Enabled,
                    (info, control, arg3) => control.IsEnabled,
                    (info, control, arg3) => control.IsEnabled = (bool)arg3[0]));
#else
            memberProvider.Register(AttachedBindingMember
                .CreateMember<FrameworkElement, bool>(AttachedMemberConstants.Focused,
                    (info, control, arg3) => control.IsFocused, null,
                    memberChangeEventName: "LostFocus"));
            memberProvider.Register(AttachedBindingMember
                .CreateMember<FrameworkElement, bool>(AttachedMemberConstants.Enabled,
                    (info, control, arg3) => control.IsEnabled,
                    (info, control, arg3) => control.IsEnabled = (bool)arg3[0],
                    memberChangeEventName: "IsEnabledChanged"));
#endif

            //TextBox, TextBlock            
#if WINDOWSCOMMON || NETFX_CORE
            memberProvider.Register(AttachedBindingMember.CreateMember<TextBox, string>("Text",
                (info, box, arg3) => box.Text,
                (info, box, arg3) => box.Text = arg3[0] as string ?? string.Empty, memberChangeEventName: "TextChanged"));

            memberProvider.Register(AttachedBindingMember.CreateMember<TextBlock, string>("Text",
                (info, box, arg3) => box.Text,
                (info, box, arg3) => box.Text = arg3[0] as string ?? string.Empty, ObserveTextTextBlock));
#endif
        }

        private static void OnBindChanged(DependencyObject sender, string bindings)
        {
            if (string.IsNullOrWhiteSpace(bindings))
                return;
            IList<IDataBinding> list = BindingServiceProvider.BindingProvider.CreateBindingsFromString(sender, bindings, null);
            if (!ApplicationSettings.IsDesignMode)
                return;
            foreach (InvalidDataBinding binding in list.OfType<InvalidDataBinding>())
                throw binding.Exception;
        }

        private static IDisposable ObserveVisiblityMember(IBindingMemberInfo bindingMemberInfo, UIElement uiElement, IEventListener arg3)
        {
#if NETFX_CORE || WINDOWSCOMMON || WINDOWS_PHONE
            return new DependencyPropertyBindingMember.DependencyPropertyListener(uiElement, "Visibility", arg3);
#else
            return new DependencyPropertyBindingMember.DependencyPropertyListener(uiElement, UIElement.VisibilityProperty, arg3);
#endif
        }

        private static object FindByNameMemberImpl(IBindingMemberInfo bindingMemberInfo, FrameworkElement target, object[] arg3)
        {
            DependencyObject root = null;
            while (target != null)
            {
                root = target;
                target = FindParent(target) as FrameworkElement;
            }
            var frameworkElement = root as FrameworkElement;
            if (frameworkElement == null)
                return null;
            var name = (string)arg3[0];
            return frameworkElement.FindName(name) ?? FindChild(root, name);
        }

        private static DependencyObject GetParentValue(IBindingMemberInfo bindingMemberInfo, FrameworkElement target, object[] arg3)
        {
            return ParentListener.GetOrAdd(target).Parent;
        }

        private static IDisposable ObserveParentMember(IBindingMemberInfo bindingMemberInfo, FrameworkElement o, IEventListener arg3)
        {
            return ParentListener.GetOrAdd(o).AddWithUnsubscriber(arg3);

        }

#if WINDOWSCOMMON || NETFX_CORE
        private static IDisposable ObserveTextTextBlock(IBindingMemberInfo bindingMemberInfo, TextBlock textBlock, IEventListener arg3)
        {
            return new DependencyPropertyBindingMember.DependencyPropertyListener(textBlock, "Text", arg3);
        }
#endif

        private static DependencyObject FindParent(FrameworkElement target)
        {
            IBindingMemberInfo member = BindingServiceProvider
                    .MemberProvider
                    .GetBindingMember(target.GetType(), "PlacementTarget", false, false);
            if (member != null)
            {
                object value = member.GetValue(target, null);
                if (value != null)
                    return (DependencyObject)value;
            }
            return VisualTreeHelper.GetParent(target) ?? target.Parent;
        }

        private static DependencyObject FindChild(DependencyObject parent, string childName)
        {
            if (parent == null)
                return null;

            int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var element = child as FrameworkElement;
                if (element != null && element.Name == childName)
                    return element;

                child = FindChild(child, childName);
                if (child != null)
                    return child;
            }

            return null;
        }

        #endregion

        #region Overrides of DataBindingModule

        /// <summary>
        ///     Loads the current module.
        /// </summary>
        public override bool Load(IModuleContext context)
        {
            if (View.OnBindChanged == null)
                View.OnBindChanged = OnBindChanged;
            ViewManager.GetDataContext = o => BindingServiceProvider.ContextManager.GetBindingContext(o).Value;
            ViewManager.SetDataContext = (o, o1) => BindingServiceProvider.ContextManager.GetBindingContext(o).Value = o1;
            base.Load(context);
            var oldMember = BindingServiceProvider.MemberProvider as BindingMemberProvider;
            BindingServiceProvider.MemberProvider = oldMember == null
                ? new BindingMemberProviderEx()
                : new BindingMemberProviderEx(oldMember);
            BindingServiceProvider.ContextManager = new BindingContextManagerEx();
            var resolver = BindingServiceProvider.ResourceResolver as BindingResourceResolver;
            BindingServiceProvider.ResourceResolver = resolver == null
                ? new BindingResourceResolverEx()
                : new BindingResourceResolverEx(resolver);
            BindingServiceProvider.ErrorProvider = new BindingErrorProvider();
            Register(BindingServiceProvider.MemberProvider);
            var resourceResolver = BindingServiceProvider.ResourceResolver;
            resourceResolver.AddObject("Visible", new BindingResourceObject(Visibility.Visible), true);
            resourceResolver.AddObject("Collapsed", new BindingResourceObject(Visibility.Collapsed), true);

            IValueConverter conv = new BooleanToVisibilityConverter(Visibility.Visible, Visibility.Collapsed, Visibility.Collapsed);
            resourceResolver
                .AddConverter("FalseToCollapsed", new ValueConverterWrapper(conv.Convert, conv.ConvertBack), true);
            conv = new BooleanToVisibilityConverter(Visibility.Collapsed, Visibility.Visible, Visibility.Collapsed);
            resourceResolver
                .AddConverter("TrueToCollapsed", new ValueConverterWrapper(conv.Convert, conv.ConvertBack), true);

            conv = new NullToVisibilityConverter(Visibility.Collapsed, Visibility.Visible);
            resourceResolver
                .AddConverter("NullToCollapsed", new ValueConverterWrapper(conv.Convert, conv.ConvertBack), true);
            conv = new NullToVisibilityConverter(Visibility.Visible, Visibility.Collapsed);
            resourceResolver
                .AddConverter("NotNullToCollapsed", new ValueConverterWrapper(conv.Convert, conv.ConvertBack), true);

#if WPF
            conv = new BooleanToVisibilityConverter(Visibility.Visible, Visibility.Hidden, Visibility.Hidden);
            resourceResolver
                .AddConverter("FalseToHidden", new ValueConverterWrapper(conv.Convert, conv.ConvertBack), true);
            conv = new BooleanToVisibilityConverter(Visibility.Hidden, Visibility.Visible, Visibility.Hidden);
            resourceResolver
                .AddConverter("TrueToHidden", new ValueConverterWrapper(conv.Convert, conv.ConvertBack), true);

            conv = new NullToVisibilityConverter(Visibility.Hidden, Visibility.Visible);
            resourceResolver
                .AddConverter("NullToHidden", new ValueConverterWrapper(conv.Convert, conv.ConvertBack), true);
            conv = new NullToVisibilityConverter(Visibility.Visible, Visibility.Hidden);
            resourceResolver
                .AddConverter("NotNullToHidden", new ValueConverterWrapper(conv.Convert, conv.ConvertBack), true);
#endif
            foreach (var assembly in context.Assemblies)
            {
                foreach (var type in assembly.SafeGetTypes(context.Mode != LoadMode.Design))
                {
#if NETFX_CORE || WINDOWSCOMMON
                    if (!typeof(IValueConverter).IsAssignableFrom(type) || type.GetTypeInfo().IsAbstract || !type.GetTypeInfo().IsClass)
                        continue;
                    var constructor = type.GetTypeInfo().DeclaredConstructors.FirstOrDefault(info => info.GetParameters().Length == 0);
#else
                    if (!typeof(IValueConverter).IsAssignableFrom(type) || type.IsAbstract || !type.IsClass)
                        continue;
                    var constructor = type.GetConstructor(EmptyValue<Type>.ArrayInstance);
#endif

                    if (constructor == null || !constructor.IsPublic)
                        continue;
                    var converter = (IValueConverter)constructor.InvokeEx();
                    string name = RemoveTail(RemoveTail(type.Name, "ValueConverter"), "Converter");
                    if (resourceResolver.TryAddConverter(name,
                        new ValueConverterWrapper(converter.Convert, converter.ConvertBack)))
                        Tracer.Info("The {0} converter is registered.", type);
                }
            }
            return true;
        }

        #endregion
    }
}