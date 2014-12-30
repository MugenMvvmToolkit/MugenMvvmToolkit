#region Copyright

// ****************************************************************************
// <copyright file="View.cs">
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
#if NETFX_CORE || WINDOWSCOMMON
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using BindingEx = Windows.UI.Xaml.Data.Binding;
#else
using System.Reflection;
using System.Windows;
using System.Windows.Data;
using BindingEx = System.Windows.Data.Binding;
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

            internal static readonly MethodInfo HandleMethod;
            private readonly DependencyProperty _property;

            #endregion

            #region Constructors

            static BindingEventClosure()
            {
                HandleMethod = typeof(BindingEventClosure).GetMethod("Handle", BindingFlags.Public | BindingFlags.Instance);
            }

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

        public static readonly DependencyProperty DesignDataContextProperty = DependencyProperty.RegisterAttached(
            "DesignDataContext", typeof(object), typeof(View), new PropertyMetadata(null, OnDesignDataContextChanged));

        private static readonly DependencyProperty VisibilityInternalProperty = DependencyProperty.RegisterAttached(
            "VisibilityInternal", typeof(object), typeof(View),
            new PropertyMetadata(null, VisibilityInternalChanged));

        public static readonly DependencyProperty ErrorsProperty = DependencyProperty.RegisterAttached(
            "Errors", typeof(ICollection<object>), typeof(View), new PropertyMetadata(null, OnErrorsChanged));

        public static readonly DependencyProperty HasErrorsProperty = DependencyProperty.RegisterAttached(
            "HasErrors", typeof(bool), typeof(View), new PropertyMetadata(Empty.FalseObject));

        public static void SetErrors(DependencyObject element, ICollection<object> value)
        {
            element.SetValue(ErrorsProperty, value);
        }

        public static ICollection<object> GetErrors(DependencyObject element)
        {
            return (ICollection<object>)element.GetValue(ErrorsProperty);
        }

        public static bool GetHasErrors(DependencyObject element)
        {
            return (bool)element.GetValue(HasErrorsProperty);
        }

        public static void SetDesignDataContext(DependencyObject element, object value)
        {
            element.SetValue(DesignDataContextProperty, value);
        }

        public static object GetDesignDataContext(DependencyObject element)
        {
            return element.GetValue(DesignDataContextProperty);
        }

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

        public static Action<DependencyObject, string> BindChanged { get; set; }

        public static Action<DependencyObject, ICollection<object>> ErrorsChanged { get; set; }

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
        private static void OnErrorsChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var newValue = (ICollection<object>)args.NewValue;
            sender.SetValue(HasErrorsProperty, Empty.BooleanToObject(newValue != null && newValue.Count != 0));
            var errorsChanged = ErrorsChanged;
            if (errorsChanged != null)
                errorsChanged(sender, newValue);
        }

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

        private static void OnDesignDataContextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            if (!ServiceProvider.DesignTimeManager.IsDesignMode)
                return;
            var element = sender as FrameworkElement;
            if (element == null)
                return;
            var disposable = element.DataContext as IDisposable;
            if (disposable != null)
                disposable.Dispose();

            if (args.NewValue == null)
            {
                element.DataContext = null;
                return;
            }
            var type = args.NewValue as Type;
            if (type == null)
            {
                var typeName = args.NewValue as string;
                if (typeName == null)
                {
                    element.DataContext = args.NewValue;
                    return;
                }

                var fullName = typeName.IndexOf('.') >= 0;
                foreach (var assembly in ReflectionExtensions.GetDesignAssemblies())
                {
                    foreach (var t in assembly.SafeGetTypes(false))
                    {
                        string name = fullName ? t.FullName : t.Name;
                        if (name == typeName)
                        {
                            type = t;
                            break;
                        }
                    }
                }
                if (type == null)
                {
                    element.DataContext = null;
                    return;
                }
            }
            var iocContainer = ServiceProvider.DesignTimeManager.IocContainer;
            element.DataContext = iocContainer == null
                ? Activator.CreateInstance(type)
                : iocContainer.Get(type);
        }

        private static void OnBindChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs args)
        {
            var bindChanged = BindChanged;
            Should.MethodBeSupported(ServiceProvider.DesignTimeManager.IsDesignMode || bindChanged != null, "BindChanged");
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