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
using System.Collections.Generic;
using System.Linq;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Modules;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;

#if WPF
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using MugenMvvmToolkit.WPF.MarkupExtensions;
using MugenMvvmToolkit.WPF.Binding.Converters;
using MugenMvvmToolkit.WPF.Binding.Infrastructure;
using MugenMvvmToolkit.WPF.Binding.Models;
using BooleanToVisibilityConverter = MugenMvvmToolkit.WPF.Binding.Converters.BooleanToVisibilityConverter;

namespace MugenMvvmToolkit.WPF.Binding.Modules
#elif SILVERLIGHT
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using MugenMvvmToolkit.Silverlight.MarkupExtensions;
using MugenMvvmToolkit.Silverlight.Binding.Converters;
using MugenMvvmToolkit.Silverlight.Binding.Infrastructure;
using MugenMvvmToolkit.Silverlight.Binding.Models;
using BooleanToVisibilityConverter = MugenMvvmToolkit.Silverlight.Binding.Converters.BooleanToVisibilityConverter;

namespace MugenMvvmToolkit.Silverlight.Binding.Modules
#elif WINDOWSCOMMON || NETFX_CORE
using System.Reflection;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using MugenMvvmToolkit.WinRT.MarkupExtensions;
using MugenMvvmToolkit.WinRT.Binding.Converters;
using MugenMvvmToolkit.WinRT.Binding.Infrastructure;
using MugenMvvmToolkit.WinRT.Binding.Models;
using BooleanToVisibilityConverter = MugenMvvmToolkit.WinRT.Binding.Converters.BooleanToVisibilityConverter;

namespace MugenMvvmToolkit.WinRT.Binding.Modules
#elif WINDOWS_PHONE
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Input;
using Microsoft.Phone.Controls;
using MugenMvvmToolkit.WinPhone.MarkupExtensions;
using MugenMvvmToolkit.WinPhone.Binding.Converters;
using MugenMvvmToolkit.WinPhone.Binding.Infrastructure;
using MugenMvvmToolkit.WinPhone.Binding.Models;
using BooleanToVisibilityConverter = MugenMvvmToolkit.WinPhone.Binding.Converters.BooleanToVisibilityConverter;

namespace MugenMvvmToolkit.WinPhone.Binding.Modules
#endif

{
    public class PlatformDataBindingModule : DataBindingModule
    {
        #region Constructors

        static PlatformDataBindingModule()
        {
            if (View.BindChanged == null)
                View.BindChanged = OnBindChanged;
            ViewManager.ViewCleared += OnViewCleared;
#if !WINDOWSCOMMON && !NETFX_CORE
            BindingServiceProvider.ValueConverter = BindingReflectionExtensions.Convert;
#endif
        }

        #endregion

        #region Methods

        private static void Register(IBindingMemberProvider memberProvider)
        {
            Should.NotBeNull(memberProvider, "memberProvider");
            BindingBuilderExtensions.RegisterDefaultBindingMember<TextBlock>(() => t => t.Text);
            BindingBuilderExtensions.RegisterDefaultBindingMember<TextBox>(() => t => t.Text);
            BindingBuilderExtensions.RegisterDefaultBindingMember<Button>("Click");
            BindingBuilderExtensions.RegisterDefaultBindingMember<ComboBox>(() => c => c.ItemsSource);
            BindingBuilderExtensions.RegisterDefaultBindingMember<ListBox>(() => c => c.ItemsSource);
            BindingBuilderExtensions.RegisterDefaultBindingMember<ProgressBar>(() => c => c.Value);

            //UIElement
            memberProvider.Register(AttachedBindingMember.CreateMember<UIElement, bool>("Visible",
                    (info, view) => view.Visibility == Visibility.Visible,
                    (info, view, value) => view.Visibility = value ? Visibility.Visible : Visibility.Collapsed, ObserveVisiblityMember));
            memberProvider.Register(AttachedBindingMember.CreateMember<UIElement, bool>("Hidden",
                    (info, view) => view.Visibility != Visibility.Visible,
                    (info, view, value) => view.Visibility = value ? Visibility.Collapsed : Visibility.Visible, ObserveVisiblityMember));

            //FrameworkElement            
            memberProvider.Register(AttachedBindingMember
                .CreateMember<FrameworkElement, object>(AttachedMemberConstants.ParentExplicit, GetParentValue, SetParentValue, ObserveParentMember));
            memberProvider.Register(AttachedBindingMember
                .CreateMember<FrameworkElement, object>(AttachedMemberConstants.FindByNameMethod, FindByNameMemberImpl));
#if SILVERLIGHT || NETFX_CORE || WINDOWSCOMMON || WINDOWS_PHONE
            memberProvider.Register(AttachedBindingMember
                .CreateMember<FrameworkElement, bool>(AttachedMemberConstants.Focused,
                    (info, control) => FocusManager.GetFocusedElement() == control, null, "LostFocus"));
            memberProvider.Register(AttachedBindingMember
                .CreateMember<Control, bool>(AttachedMemberConstants.Enabled,
                    (info, control) => control.IsEnabled,
                    (info, control, value) => control.IsEnabled = value));
#else
            memberProvider.Register(AttachedBindingMember
                .CreateMember<FrameworkElement, bool>(AttachedMemberConstants.Focused,
                    (info, control) => control.IsFocused, null, "LostFocus"));
            memberProvider.Register(AttachedBindingMember
                .CreateMember<FrameworkElement, bool>(AttachedMemberConstants.Enabled,
                    (info, control) => control.IsEnabled,
                    (info, control, value) => control.IsEnabled = value, "IsEnabledChanged"));
#endif

            //TextBox                        
#if WINDOWSCOMMON || NETFX_CORE
            memberProvider.Register(AttachedBindingMember.CreateMember<TextBox, string>("Text",
                (info, box) => box.Text,
                (info, box, value) => box.Text = value ?? string.Empty, "TextChanged"));

            //TextBlock
            memberProvider.Register(AttachedBindingMember.CreateMember<TextBlock, string>("Text",
                (info, box) => box.Text,
                (info, box, value) => box.Text = value ?? string.Empty, ObserveTextTextBlock));
#else
            //WebBrowser
            memberProvider.Register(AttachedBindingMember.CreateMember<WebBrowser, Uri>("Source",
                (info, browser) => browser.Source, (info, browser, arg3) => browser.Source = arg3, "Navigated"));
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

        private static object GetParentValue(IBindingMemberInfo bindingMemberInfo, FrameworkElement target)
        {
            return ParentObserver.GetOrAdd(target).Parent;
        }

        private static void SetParentValue(IBindingMemberInfo bindingMemberInfo, FrameworkElement frameworkElement, object arg3)
        {
            ParentObserver.GetOrAdd(frameworkElement).Parent = arg3;
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

        private static void OnViewCleared(IViewManager viewManager, IViewModel viewModel, object arg3, IDataContext arg4)
        {
            ClearBindingsRecursively(arg3 as DependencyObject);
        }

        private static void ClearBindingsRecursively(DependencyObject item)
        {
            if (item == null)
                return;
            var count = VisualTreeHelper.GetChildrenCount(item);
            for (int i = 0; i < count; i++)
                ClearBindingsRecursively(VisualTreeHelper.GetChild(item, i));
            item.ClearBindings(true, true);
        }

        #endregion

        #region Overrides of DataBindingModule

        /// <summary>
        ///    Occurs on load the current module.
        /// </summary>
        protected override void OnLoaded(IModuleContext context)
        {
            if (View.BindChanged == null)
                View.BindChanged = OnBindChanged;

            Register(BindingServiceProvider.MemberProvider);

            var resourceResolver = BindingServiceProvider.ResourceResolver;
            resourceResolver.AddObject("Visible", new BindingResourceObject(Visibility.Visible), true);
            resourceResolver.AddObject("Collapsed", new BindingResourceObject(Visibility.Collapsed), true);

            IValueConverter conv = new BooleanToVisibilityConverter(Visibility.Visible, Visibility.Collapsed, Visibility.Collapsed);
            resourceResolver
                .AddConverter("FalseToCollapsed", new ValueConverterWrapper(conv), true);
            conv = new BooleanToVisibilityConverter(Visibility.Collapsed, Visibility.Visible, Visibility.Collapsed);
            resourceResolver
                .AddConverter("TrueToCollapsed", new ValueConverterWrapper(conv), true);

            conv = new NullToVisibilityConverter(Visibility.Collapsed, Visibility.Visible);
            resourceResolver
                .AddConverter("NullToCollapsed", new ValueConverterWrapper(conv), true);
            conv = new NullToVisibilityConverter(Visibility.Visible, Visibility.Collapsed);
            resourceResolver
                .AddConverter("NotNullToCollapsed", new ValueConverterWrapper(conv), true);

#if WPF
            conv = new BooleanToVisibilityConverter(Visibility.Visible, Visibility.Hidden, Visibility.Hidden);
            resourceResolver
                .AddConverter("FalseToHidden", new ValueConverterWrapper(conv), true);
            conv = new BooleanToVisibilityConverter(Visibility.Hidden, Visibility.Visible, Visibility.Hidden);
            resourceResolver
                .AddConverter("TrueToHidden", new ValueConverterWrapper(conv), true);

            conv = new NullToVisibilityConverter(Visibility.Hidden, Visibility.Visible);
            resourceResolver
                .AddConverter("NullToHidden", new ValueConverterWrapper(conv), true);
            conv = new NullToVisibilityConverter(Visibility.Visible, Visibility.Hidden);
            resourceResolver
                .AddConverter("NotNullToHidden", new ValueConverterWrapper(conv), true);
#endif
            base.OnLoaded(context);
        }

        /// <summary>
        ///     Gets the <see cref="IBindingErrorProvider" /> that will be used by default.
        /// </summary>
        protected override IBindingErrorProvider GetBindingErrorProvider(IModuleContext context)
        {
#if WPF
            if (context.Platform.Platform != PlatformType.WPF)
                return null;
#endif
            return new BindingErrorProvider();
        }

        /// <summary>
        ///     Gets the <see cref="IBindingMemberProvider" /> that will be used by default.
        /// </summary>
        protected override IBindingMemberProvider GetBindingMemberProvider(IModuleContext context)
        {
            var provider = BindingServiceProvider.MemberProvider as BindingMemberProvider;
            return provider == null
                ? new BindingMemberProviderEx()
                : new BindingMemberProviderEx(provider);
        }

        /// <summary>
        ///     Gets the <see cref="IBindingContextManager" /> that will be used by default.
        /// </summary>
        protected override IBindingContextManager GetBindingContextManager(IModuleContext context)
        {
            return new BindingContextManagerEx();
        }

        /// <summary>
        ///     Gets the <see cref="IBindingResourceResolver" /> that will be used by default.
        /// </summary>
        protected override IBindingResourceResolver GetBindingResourceResolver(IModuleContext context)
        {
            var resolver = BindingServiceProvider.ResourceResolver as BindingResourceResolver;
            return resolver == null
                ? new BindingResourceResolverEx()
                : new BindingResourceResolverEx(resolver);
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
            BindingServiceProvider.ResourceResolver.AddConverter(new ValueConverterWrapper(converter), type, true);
            if (Tracer.TraceInformation)
                Tracer.Info("The {0} converter is registered.", type);
        }

        #endregion
    }
}