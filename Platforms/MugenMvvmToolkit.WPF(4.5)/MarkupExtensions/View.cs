#region Copyright

// ****************************************************************************
// <copyright file="View.cs">
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
using System.Collections.Generic;
#if WINDOWS_UWP
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using BindingEx = Windows.UI.Xaml.Data.Binding;
#else
using System.Windows;
using System.Windows.Data;
using BindingEx = System.Windows.Data.Binding;
#endif

#if WPF
namespace MugenMvvmToolkit.WPF.MarkupExtensions
#elif WINDOWS_UWP
namespace MugenMvvmToolkit.UWP.MarkupExtensions
#endif
{
    public static class View
    {
        #region Attached properties

        public static readonly DependencyProperty BindProperty = DependencyProperty.RegisterAttached(
            "Bind", typeof(string), typeof(View), new PropertyMetadata(default(string), OnBindChangedCallback));

        public static readonly DependencyProperty VisibleProperty = DependencyProperty.RegisterAttached(
            "Visible", typeof(object), typeof(View), new PropertyMetadata(null, VisibleChanged));

        public static readonly DependencyProperty CollapsedProperty = DependencyProperty.RegisterAttached(
            "Collapsed", typeof(object), typeof(View), new PropertyMetadata(null, CollapsedChanged));

        private static readonly DependencyProperty VisibilityInternalProperty = DependencyProperty.RegisterAttached(
            "VisibilityInternal", typeof(object), typeof(View),
            new PropertyMetadata(null, VisibilityInternalChanged));

        private static Visibility? GetVisibilityInternal(DependencyObject element)
        {
            return (Visibility?)element.GetValue(VisibilityInternalProperty);
        }

        public static void SetVisible(DependencyObject element, bool value)
        {
            element.SetValueEx(VisibleProperty, value);
        }

        public static bool GetVisible(DependencyObject element)
        {
            return (bool)element.GetValue(VisibleProperty);
        }

        public static void SetCollapsed(DependencyObject element, bool value)
        {
            element.SetValueEx(CollapsedProperty, value);
        }

        public static bool GetCollapsed(DependencyObject element)
        {
            return (bool)element.GetValue(CollapsedProperty);
        }

        public static void SetBind(DependencyObject element, string value)
        {
            element.SetValue(BindProperty, value);
        }

        public static string GetBind(DependencyObject element)
        {
            return (string)element.GetValue(BindProperty);
        }

        #endregion

        #region Properties

        public static Action<DependencyObject, string> BindChanged { get; set; }

        #endregion

        #region Methods

        private static void VisibilityInternalChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null)
                return;
            var newValue = (Visibility)args.NewValue;
            switch (newValue)
            {
                case Visibility.Visible:
                    SetCollapsed(sender, false);
                    SetVisible(sender, true);
                    break;
                default:
                    SetCollapsed(sender, true);
                    SetVisible(sender, false);
                    break;
            }
        }

        private static void VisibleChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null)
                return;
            if ((bool)args.NewValue)
                sender.SetValue(UIElement.VisibilityProperty, Visibility.Visible);
            else
                sender.SetValue(UIElement.VisibilityProperty, Visibility.Collapsed);
            TrySetVisibilityBinding(sender);
        }

        private static void CollapsedChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (args.NewValue == null)
                return;
            if ((bool)args.NewValue)
                sender.SetValue(UIElement.VisibilityProperty, Visibility.Collapsed);
            else
                sender.SetValue(UIElement.VisibilityProperty, Visibility.Visible);
            TrySetVisibilityBinding(sender);
        }

        private static void TrySetVisibilityBinding(DependencyObject sender)
        {
            if (!GetVisibilityInternal(sender).HasValue)
                BindingOperations.SetBinding(sender, VisibilityInternalProperty, new BindingEx
                {
                    Path = new PropertyPath("Visibility"),
                    Mode = BindingMode.OneWay,
#if !WINDOWS_UWP
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    ValidatesOnDataErrors = false,
                    ValidatesOnExceptions = false,
#endif
                    Source = sender
                });
        }

        private static void OnBindChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var bindChanged = BindChanged;
            Should.MethodBeSupported(ServiceProvider.IsDesignMode || bindChanged != null, "BindChanged");
            bindChanged?.Invoke(sender, (string)args.NewValue);
        }

        private static void SetValueEx<T>(this DependencyObject dp, DependencyProperty property, T value)
        {
            if (!Equals(value, dp.GetValue(property)))
                dp.SetValue(property, value);
        }

        #endregion
    }
}
