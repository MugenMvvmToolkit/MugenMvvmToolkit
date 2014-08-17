#region Copyright
// ****************************************************************************
// <copyright file="View.cs">
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
using System.Collections.ObjectModel;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Utils;
#if NETFX_CORE || WINDOWSCOMMON
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
#else
using System.Reflection;
using System.Windows;
using System.Windows.Data;
#endif

namespace MugenMvvmToolkit.MarkupExtensions
{
    public static class View
    {
        #region Nested types

#if WINDOWS_PHONE
        public sealed class BindingEventClosure
        {
        #region Fields

            internal static readonly MethodInfo HandleMethod = typeof(BindingEventClosure).GetMethod("Handle",
                BindingFlags.Public | BindingFlags.Instance);
            private readonly DependencyProperty _property;

        #endregion

        #region Constructors

            /// <summary>
            /// Initializes a new instance of the <see cref="BindingEventClosure"/> class.
            /// </summary>
            public BindingEventClosure(DependencyProperty property)
            {
                Should.NotBeNull(property, "property");
                _property = property;
            }

        #endregion

        #region Methods

            public void Handle<TSender, TValue>(TSender sender, TValue value)
            {
                var frameworkElement = (FrameworkElement)(object)sender;
                var bindingExpression = frameworkElement.GetBindingExpression(_property);
                if (bindingExpression != null)
                    bindingExpression.UpdateSource();
            }

        #endregion
        }
#endif

        #endregion

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

#if NETFX_CORE || WINDOWSCOMMON
        public static readonly DependencyProperty ErrorsProperty = DependencyProperty.RegisterAttached(
                    "Errors", typeof(ReadOnlyObservableCollection<object>), typeof(View), new PropertyMetadata(default(ReadOnlyObservableCollection<object>)));

        public static void SetErrors(DependencyObject element, ReadOnlyObservableCollection<object> value)
        {
            element.SetValue(ErrorsProperty, value);
        }

        public static ReadOnlyObservableCollection<object> GetErrors(DependencyObject element)
        {
            return (ReadOnlyObservableCollection<object>)element.GetValue(ErrorsProperty);
        }

        public static readonly DependencyProperty HasErrorsProperty = DependencyProperty.RegisterAttached(
            "HasErrors", typeof(bool), typeof(View), new PropertyMetadata(default(bool)));

        public static void SetHasErrors(DependencyObject element, bool value)
        {
            element.SetValue(HasErrorsProperty, value);
        }

        public static bool GetHasErrors(DependencyObject element)
        {
            return (bool)element.GetValue(HasErrorsProperty);
        }
#endif
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

#if WINDOWS_PHONE
        public static readonly DependencyProperty PropertyChangedBindingsProperty = DependencyProperty.RegisterAttached(
            "PropertyChangedBindings", typeof(string), typeof(View), new PropertyMetadata(null, PropertyChangedBindingsChanged));

        public static void SetPropertyChangedBindings(DependencyObject element, string value)
        {
            element.SetValue(PropertyChangedBindingsProperty, value);
        }

        public static string GetPropertyChangedBindings(DependencyObject element)
        {
            return (string)element.GetValue(PropertyChangedBindingsProperty);
        }
#endif
        #endregion

        #region Properties

        public static Action<DependencyObject, string> OnBindChanged { get; set; }

        #endregion

        #region Methods

#if WINDOWS_PHONE
        private static void PropertyChangedBindingsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null)
                return;
            var newValue = (string)args.NewValue;
            if (string.IsNullOrEmpty(newValue))
                return;

            foreach (var name in newValue.Split(';'))
            {
                var propertyToEvent = name.Split(':');
                string propertyName = propertyToEvent[0];
                string eventName;
                if (propertyToEvent.Length > 1)
                    eventName = propertyToEvent[1];
                else
                    eventName = propertyName + "Changed";
                var dp = sender.GetDependencyPropertyByName(propertyName);
                var eventInfo = sender.GetEventByName(eventName);
                var closure = new BindingEventClosure(dp);
                var @delegate = ServiceProvider.ReflectionManager.TryCreateDelegate(eventInfo.EventHandlerType, closure,
                    BindingEventClosure.HandleMethod);
                if (@delegate != null)
                    eventInfo.AddEventHandler(sender, @delegate);
            }
        }

        private static DependencyProperty GetDependencyPropertyByName(this DependencyObject associatedObject, string name)
        {
            string propertyName = name + "Property";
            if (string.IsNullOrEmpty(name))
                throw new NullReferenceException("SourceProperty is null.");
            var field = associatedObject
                 .GetType()
                 .GetField(propertyName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            DependencyProperty result = null;
            if (field != null)
                result = (DependencyProperty)field.GetValue(null);
            if (result == null)
                throw new NullReferenceException("The property not found in associated object, property name: " +
                                                 propertyName);
            return result;
        }

        private static EventInfo GetEventByName(this DependencyObject associatedObject, string eventName)
        {
            if (string.IsNullOrEmpty(eventName))
                throw new NullReferenceException("SourceProperty is null.");
            var @event = associatedObject
                .GetType()
                .GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
            if (@event == null)
                throw new NullReferenceException("The event not found in associated object, property name: " + eventName);
            return @event;
        }
#endif

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
                BindingOperations.SetBinding(sender, VisibilityInternalProperty, new Binding
                {
                    Path = new PropertyPath("Visibility"),
                    Mode = BindingMode.OneWay,
#if !NETFX_CORE && !WINDOWSCOMMON
#if WINDOWS_PHONE
                    UpdateSourceTrigger = UpdateSourceTrigger.Default,
#else
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
#endif
                    ValidatesOnDataErrors = false,
                    ValidatesOnExceptions = false,
#endif
                    Source = sender
                });
        }

        private static void OnBindChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var bindChanged = OnBindChanged;
            Should.MethodBeSupported(DesignTimeManagerBase.IsDesignModeStatic || bindChanged != null, "OnBindChanged");
            if (bindChanged != null)
                bindChanged(sender, (string)args.NewValue);
        }

        private static void SetValueEx<T>(this DependencyObject dp, DependencyProperty property, T value)
        {
            if (!Equals(value, dp.GetValue(property)))
                dp.SetValue(property, value);
        }

        #endregion
    }
}