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
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
#if WPF
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MugenMvvmToolkit.WPF.Binding.Models;

namespace MugenMvvmToolkit.WPF.Binding
#elif WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using MugenMvvmToolkit.UWP.Binding.Models;

namespace MugenMvvmToolkit.UWP.Binding
#endif
{
    public static class AttachedMembersRegistration
    {
        #region Properties

        private static IBindingMemberProvider MemberProvider => BindingServiceProvider.MemberProvider;

        #endregion

        #region Methods

        public static void RegisterUIElementMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.UIElement.Visible,
                (info, view) => view.Visibility == Visibility.Visible,
                (info, view, value) => view.Visibility = value ? Visibility.Visible : Visibility.Collapsed, ObserveVisiblityMember));
            MemberProvider.Register(AttachedBindingMember.CreateMember(AttachedMembers.UIElement.Hidden,
                (info, view) => view.Visibility != Visibility.Visible,
                (info, view, value) => view.Visibility = value ? Visibility.Collapsed : Visibility.Visible, ObserveVisiblityMember));
        }

        public static void RegisterFrameworkElementMembers()
        {
            MemberProvider.Register(AttachedBindingMember
                .CreateMember<FrameworkElement, object>(AttachedMemberConstants.Parent, GetParentValue, SetParentValue, ObserveParentMember));
            MemberProvider.Register(AttachedBindingMember
                .CreateMember<FrameworkElement, object>(AttachedMemberConstants.FindByNameMethod, FindByNameMemberImpl));
#if WINDOWS_UWP
            MemberProvider.Register(AttachedBindingMember
                .CreateMember<FrameworkElement, bool>(AttachedMemberConstants.Focused,
                    (info, control) => FocusManager.GetFocusedElement() == control, null, nameof(FrameworkElement.LostFocus)));
            MemberProvider.Register(AttachedBindingMember
                .CreateMember<Control, bool>(AttachedMemberConstants.Enabled,
                    (info, control) => control.IsEnabled,
                    (info, control, value) => control.IsEnabled = value));
#else
            MemberProvider.Register(AttachedBindingMember
                .CreateMember<FrameworkElement, bool>(AttachedMemberConstants.Focused,
                    (info, control) => control.IsFocused, null, nameof(FrameworkElement.LostFocus)));
            MemberProvider.Register(AttachedBindingMember
                .CreateMember<FrameworkElement, bool>(AttachedMemberConstants.Enabled,
                    (info, control) => control.IsEnabled,
                    (info, control, value) => control.IsEnabled = value, nameof(FrameworkElement.IsEnabledChanged)));
#endif
        }

        public static void RegisterTextBoxMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<TextBox>(nameof(TextBox.Text));
#if WINDOWS_UWP
            MemberProvider.Register(AttachedBindingMember.CreateMember<TextBox, string>(nameof(TextBox.Text),
                (info, box) => box.Text,
                (info, box, value) => box.Text = value ?? string.Empty, nameof(TextBox.TextChanged)));
#endif
        }

        public static void RegisterTextBlockMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<TextBlock>(nameof(TextBlock.Text));
#if WINDOWS_UWP
            MemberProvider.Register(AttachedBindingMember.CreateMember<TextBlock, string>(nameof(TextBlock.Text),
                (info, box) => box.Text,
                (info, box, value) => box.Text = value ?? string.Empty, ObserveTextTextBlock));
#endif
        }

        public static void RegisterButtonMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<Button>(nameof(Button.Click));
        }

        public static void RegisterComboBoxMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<ComboBox>(nameof(ComboBox.ItemsSource));
        }

        public static void RegisterListBoxMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<ListBox>(nameof(ListBox.ItemsSource));
        }

        public static void RegisterProgressBarMembers()
        {
            BindingBuilderExtensions.RegisterDefaultBindingMember<ProgressBar>(nameof(ProgressBar.Value));
        }

#if !WINDOWS_UWP
        public static void RegisterWebBrowserMembers()
        {
            MemberProvider.Register(AttachedBindingMember.CreateMember<WebBrowser, Uri>(nameof(WebBrowser),
                (info, browser) => browser.Source, (info, browser, arg3) => browser.Source = arg3, nameof(WebBrowser.Navigated)));
        }
#endif

        private static IDisposable ObserveVisiblityMember(IBindingMemberInfo bindingMemberInfo, UIElement uiElement, IEventListener arg3)
        {
#if WINDOWS_UWP
            return DependencyPropertyBindingMember.ObserveProperty(uiElement, UIElement.VisibilityProperty, arg3);
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
            return ParentObserver.Get(target);
        }

        private static void SetParentValue(IBindingMemberInfo bindingMemberInfo, FrameworkElement frameworkElement, object arg3)
        {
            ParentObserver.Set(frameworkElement, arg3);
        }

        private static IDisposable ObserveParentMember(IBindingMemberInfo bindingMemberInfo, FrameworkElement o, IEventListener arg3)
        {
            return ParentObserver.AddListener(o, arg3);
        }

#if WINDOWS_UWP
        private static IDisposable ObserveTextTextBlock(IBindingMemberInfo bindingMemberInfo, TextBlock textBlock, IEventListener arg3)
        {
            return DependencyPropertyBindingMember.ObserveProperty(textBlock, TextBlock.TextProperty, arg3);
        }
#endif

        private static DependencyObject FindChild(DependencyObject parent, string childName)
        {
            if (parent == null)
                return null;

            var childrenCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childrenCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                var element = child as FrameworkElement;
                if ((element != null) && (element.Name == childName))
                    return element;

                child = FindChild(child, childName);
                if (child != null)
                    return child;
            }
            return null;
        }

        #endregion
    }
}