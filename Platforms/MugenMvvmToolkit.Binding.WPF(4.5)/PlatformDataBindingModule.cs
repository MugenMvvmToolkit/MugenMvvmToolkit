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
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.MarkupExtensions;
#if NETFX_CORE || WINDOWSCOMMON
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using MugenMvvmToolkit.MarkupExtensions;
using VisualStateManager = Windows.UI.Xaml.VisualStateManager;
#else
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Controls.Primitives;
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

#if WINDOWS_PHONE || WINDOWSCOMMON || NETFX_CORE
        private sealed class ParentListener
        {
            #region Fields

            private readonly FrameworkElement _view;
            private WeakReference _parentReference;
            private bool _isAttached;

            #endregion

            #region Constructors

            public ParentListener(FrameworkElement view)
            {
                _view = view;
                _parentReference = MvvmExtensions.GetWeakReference(GetParentValue(view));
                RoutedEventHandler handler = OnChanged;
                _view.Loaded += handler;
                _view.Unloaded += handler;
            }

            #endregion

            #region Properties

            public DependencyObject Parent
            {
                get { return _parentReference.Target as DependencyObject; }
                set
                {
                    _isAttached = true;
                    SetParent(value);
                }
            }

            #endregion

            #region Events

            public event EventHandler Changed;

            private void OnChanged(object sender, RoutedEventArgs routedEventArgs)
            {
                if (!_isAttached)
                    SetParent(GetParentValue(_view));
            }

            private void SetParent(DependencyObject value)
            {
                if (ReferenceEquals(value, _parentReference.Target))
                    return;
                _parentReference = MvvmExtensions.GetWeakReference(value);
                var handler = Changed;
                if (handler != null)
                    handler(_view, EventArgs.Empty);
            }

            private static DependencyObject GetParentValue(FrameworkElement target)
            {
                return VisualTreeHelper.GetParent(target) ?? target.Parent;
            }

            #endregion
        }
#endif
        #endregion

        #region Fields

#if WINDOWS_PHONE || WINDOWSCOMMON || NETFX_CORE
        private static readonly IAttachedBindingMemberInfo<FrameworkElement, ParentListener> ViewParentListenerMember;
#endif
        private readonly static IAttachedBindingMemberInfo<FrameworkElement, bool> DisableValidationMember;

        #endregion

        #region Constructors

        static PlatformDataBindingModule()
        {
            if (View.OnBindChanged == null)
                View.OnBindChanged = OnBindChanged;
            DisableValidationMember = AttachedBindingMember.CreateAutoProperty<FrameworkElement, bool>("DisableValidation");
#if WINDOWS_PHONE || WINDOWSCOMMON || NETFX_CORE
            ViewParentListenerMember = AttachedBindingMember.CreateAutoProperty<FrameworkElement, ParentListener>(
                "#ParentListener", defaultValue: (view, info) => new ParentListener(view));
#endif
        }

        #endregion

        #region Methods

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
            memberProvider.Register(AttachedBindingMember
                .CreateMember<FrameworkElement, object>(AttachedMemberConstants.SetErrorsMethod, null, SetErrorsValue));
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
            IList<IDataBinding> list = BindingProvider.Instance.CreateBindingsFromString(sender, bindings, null);
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

        private static object SetErrorsValue(IBindingMemberInfo bindingMemberInfo, FrameworkElement validatableControl, IList<object> arg3)
        {
            if (DisableValidationMember.GetValue(validatableControl, null))
                return null;
            var errors = (ICollection<object>)arg3[0];
#if NETFX_CORE || WINDOWSCOMMON
            var items = ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(validatableControl, "@$@errors_int", (element, o) =>
                {
                    var list = new ObservableCollection<object>();
                    View.SetErrors(element, new ReadOnlyObservableCollection<object>(list));
                    return list;
                }, null);
            var control = validatableControl as Control;
            if (errors == null || errors.Count == 0)
            {
                View.SetHasErrors(validatableControl, false);
                items.Clear();
                if (control != null)
                    VisualStateManager.GoToState(control, "Valid", true);
            }
            else
            {
                items.Clear();
                items.AddRange(errors);
                View.SetHasErrors(validatableControl, true);
                if (control != null)
                    VisualStateManager.GoToState(control, "Invalid", true);
            }
#else
            var binder = (ValidationBinder)ValidationBinder.GetErrorContainer(validatableControl);
            if (binder == null)
            {
                if (errors == null || errors.Count == 0)
                    return null;
                binder = new ValidationBinder();
                ValidationBinder.SetErrorContainer(validatableControl, binder);

                var binding = new System.Windows.Data.Binding(ValidationBinder.PropertyName)
                {
#if WPF && NET4
                    ValidatesOnDataErrors = true,                    
#else
                    ValidatesOnDataErrors = false,
                    ValidatesOnNotifyDataErrors = true,
#endif
                    Mode = System.Windows.Data.BindingMode.OneWay,
                    Source = binder,
                    ValidatesOnExceptions = false,
                    NotifyOnValidationError = false,
#if WPF
                    NotifyOnSourceUpdated = false,
                    NotifyOnTargetUpdated = false
#endif
                };
                validatableControl.SetBinding(ValidationBinder.ErrorContainerProperty, binding);
            }
            binder.SetErrors(errors);
#endif
            return null;
        }

        private static object FindByNameMemberImpl(IBindingMemberInfo bindingMemberInfo, FrameworkElement target, object[] arg3)
        {
            DependencyObject root = null;
            while (target != null)
            {
                root = target;
                target = GetParentValue(null, target, arg3) as FrameworkElement;
            }
            var frameworkElement = root as FrameworkElement;
            if (frameworkElement == null)
                return null;
            var name = (string)arg3[0];
            return frameworkElement.FindName(name) ?? FindChild(root, name);
        }

#if WINDOWS_PHONE || WINDOWSCOMMON || NETFX_CORE
        /// <summary>
        /// Tries to set attached parent.
        /// </summary>
        public static void SetAttachedParent([CanBeNull] FrameworkElement target, [CanBeNull] DependencyObject parent)
        {
            if (target != null)
                ViewParentListenerMember.GetValue(target, null).Parent = parent;
        }
#endif
        private static DependencyObject GetParentValue(IBindingMemberInfo bindingMemberInfo, FrameworkElement target, object[] arg3)
        {
#if WINDOWS_PHONE || WINDOWSCOMMON || NETFX_CORE
            var listener = ViewParentListenerMember.GetValue(target, arg3);
            return listener.Parent;
#else
            IBindingMemberInfo member = BindingProvider
                .Instance
                .MemberProvider
                .GetBindingMember(target.GetType(), "PlacementTarget", false, false);
            if (member != null)
            {
                object value = member.GetValue(target, arg3);
                if (value != null)
                    return (DependencyObject)value;
            }
#if WPF
            var parent = VisualTreeHelper.GetParent(target) ?? LogicalTreeHelper.GetParent(target);
#else
            var parent = VisualTreeHelper.GetParent(target);
#endif
            if (parent == null)
            {
                var frameworkElement = target as FrameworkElement;
                if (frameworkElement != null)
                    return frameworkElement.Parent;
            }
            return parent;
#endif
        }

        private static IDisposable ObserveParentMember(IBindingMemberInfo bindingMemberInfo, FrameworkElement o, IEventListener arg3)
        {
#if WINDOWS_PHONE || WINDOWSCOMMON || NETFX_CORE
            var parentListener = ViewParentListenerMember.GetValue(o, null);
            var handler = arg3.ToWeakEventHandler<EventArgs>();
            parentListener.Changed += handler.Handle;
            handler.Unsubscriber = WeakActionToken.Create(parentListener, handler, (listener, eventHandler) => listener.Changed -= eventHandler.Handle, false);
            return handler;
#else
            return new DependencyPropertyBindingMember.DependencyPropertyListener(o, AttachedMemberConstants.Parent, arg3);
#endif

        }

#if WINDOWSCOMMON || NETFX_CORE
        private static IDisposable ObserveTextTextBlock(IBindingMemberInfo bindingMemberInfo, TextBlock textBlock, IEventListener arg3)
        {
            return new DependencyPropertyBindingMember.DependencyPropertyListener(textBlock, "Text", arg3);
        }
#endif
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
            base.Load(context);
            var oldMember = BindingProvider.Instance.MemberProvider as BindingMemberProvider;
            BindingProvider.Instance.MemberProvider = oldMember == null
                ? new BindingMemberProviderEx()
                : new BindingMemberProviderEx(oldMember);
            BindingProvider.Instance.ContextManager = new BindingContextManagerEx();
            Register(BindingProvider.Instance.MemberProvider);
            var resourceResolver = BindingProvider.Instance.ResourceResolver;
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