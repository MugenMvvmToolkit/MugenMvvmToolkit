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
using BooleanToVisibilityConverter = MugenMvvmToolkit.Binding.Converters.BooleanToVisibilityConverter;

namespace MugenMvvmToolkit.Binding
{
    public class PlatformDataBindingModule : DataBindingModule
    {
        #region Fields

        private const string ErrorsObserverKey = "~~@$errob";

        #endregion

        #region Constructors

        static PlatformDataBindingModule()
        {
            if (View.BindChanged == null)
                View.BindChanged = OnBindChanged;
        }

        #endregion

        #region Methods

        private static void Register(IBindingMemberProvider memberProvider)
        {
            Should.NotBeNull(memberProvider, "memberProvider");
            //DependencyObject
            memberProvider.Register(AttachedBindingMember.CreateMember<DependencyObject, ICollection<object>>(
                    AttachedMemberConstants.ErrorsPropertyMember, GetErrors, SetErrors, ObserveErrors));

            //UIElement
            memberProvider.Register(AttachedBindingMember.CreateMember<UIElement, bool>("Visible",
                    (info, view, arg3) => view.Visibility == Visibility.Visible,
                    (info, view, arg3) => view.Visibility = ((bool)arg3[0]) ? Visibility.Visible : Visibility.Collapsed, ObserveVisiblityMember));
            memberProvider.Register(AttachedBindingMember.CreateMember<UIElement, bool>("Hidden",
                    (info, view, arg3) => view.Visibility != Visibility.Visible,
                    (info, view, arg3) => view.Visibility = ((bool)arg3[0]) ? Visibility.Collapsed : Visibility.Visible, ObserveVisiblityMember));


            //FrameworkElement            
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
            if (!ServiceProvider.DesignTimeManager.IsDesignMode)
                return;
            foreach (InvalidDataBinding binding in list.OfType<InvalidDataBinding>())
                throw binding.Exception;
        }

        private static void OnErrorsChanged(DependencyObject sender, ICollection<object> errors)
        {
            var list = ServiceProvider.AttachedValueProvider.GetValue<EventListenerList>(sender, ErrorsObserverKey, false);
            if (list != null)
                list.Raise(sender, EventArgs.Empty);
        }

        private static IDisposable ObserveErrors(IBindingMemberInfo bindingMemberInfo, DependencyObject dependencyObject, IEventListener arg3)
        {
            return ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(dependencyObject, ErrorsObserverKey, (o, o1) => new EventListenerList(), null)
                .AddWithUnsubscriber(arg3);
        }

        private static object SetErrors(IBindingMemberInfo bindingMemberInfo, DependencyObject dependencyObject, object[] arg3)
        {
            View.SetErrors(dependencyObject, (ICollection<object>)arg3[0]);
            return null;
        }

        private static ICollection<object> GetErrors(IBindingMemberInfo bindingMemberInfo, DependencyObject dependencyObject, object[] arg3)
        {
            return View.GetErrors(dependencyObject);
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
                target = ParentObserver.FindParent(target) as FrameworkElement;
            }
            var frameworkElement = root as FrameworkElement;
            if (frameworkElement == null)
                return null;
            var name = (string)arg3[0];
            return frameworkElement.FindName(name) ?? FindChild(root, name);
        }

        private static DependencyObject GetParentValue(IBindingMemberInfo bindingMemberInfo, FrameworkElement target, object[] arg3)
        {
            return ParentObserver.GetOrAdd(target).Parent;
        }

        private static IDisposable ObserveParentMember(IBindingMemberInfo bindingMemberInfo, FrameworkElement o, IEventListener arg3)
        {
            return ParentObserver.GetOrAdd(o).AddWithUnsubscriber(arg3);

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
            if (View.BindChanged == null)
                View.BindChanged = OnBindChanged;
            View.ErrorsChanged = OnErrorsChanged;
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
            return true;
        }

        /// <summary>
        ///     Gets the <see cref="IBindingErrorProvider" /> that will be used by default.
        /// </summary>
        protected override IBindingErrorProvider GetBindingErrorProvider()
        {
            return new BindingErrorProvider();
        }

        /// <summary>
        /// Tries to register type.
        /// </summary>
        protected override void RegisterType(Type type)
        {
            base.RegisterType(type);

            if (!typeof(IValueConverter).IsAssignableFrom(type) || !type.IsPublicNonAbstractClass())
                return;
#if NETFX_CORE || WINDOWSCOMMON
            var constructor = type.GetTypeInfo().DeclaredConstructors.FirstOrDefault(info => !info.IsStatic && info.GetParameters().Length == 0);
#else
            var constructor = type.GetConstructor(Empty.Array<Type>());
#endif
            if (constructor == null || !constructor.IsPublic)
                return;
            var converter = (IValueConverter)constructor.InvokeEx();
            string name = RemoveTail(RemoveTail(type.Name, "ValueConverter"), "Converter");
            if (BindingServiceProvider.ResourceResolver.TryAddConverter(name,
                new ValueConverterWrapper(converter.Convert, converter.ConvertBack)))
                Tracer.Info("The {0} converter is registered.", type);
        }

        #endregion
    }
}