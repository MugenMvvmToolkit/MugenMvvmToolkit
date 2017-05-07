#region Copyright

// ****************************************************************************
// <copyright file="BindingContextManagerEx.cs">
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
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Infrastructure;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Models;

#if WPF
using System.Windows;
using MugenMvvmToolkit.WPF.Binding.Models;
using EventType = System.Windows.DependencyPropertyChangedEventArgs;

namespace MugenMvvmToolkit.WPF.Binding.Infrastructure
#elif WINDOWS_UWP
using Windows.UI.Xaml;
using EventType = System.Object;

namespace MugenMvvmToolkit.UWP.Binding.Infrastructure
#endif

{
#if WPF
    public class WpfBindingContextManager : BindingContextManager
#elif WINDOWS_UWP
    public class UwpBindingContextManager : BindingContextManager
#endif
    {
        #region Nested types

        public sealed class FrameworkElementBindingContext : IBindingContext
        {
            #region Fields

            private readonly WeakReference _sourceReference;
            private bool _isUnset;

            #endregion

            #region Constructors

            public FrameworkElementBindingContext(FrameworkElement element)
            {
                _sourceReference = ServiceProvider.WeakReferenceFactory(element);
                element.DataContextChanged += RaiseDataContextChanged;
                if (ListenUnloadEvent)
                    element.Unloaded += ElementOnUnloaded;
                _isUnset = IsUnset(element);
            }

            #endregion

            #region Implementation of IBindingContext

            public object Source => _sourceReference.Target;

            public bool IsAlive => true;

            public object Value
            {
                get
                {
                    if (_isUnset)
                        return BindingConstants.UnsetValue;
#if WPF
                    object context = ((FrameworkElement)Source)?.DataContext;
                    if (context == null)
                        return null;
                    if (DependencyPropertyBindingMember.IsNamedObjectFunc(context))
                        return BindingConstants.UnsetValue;
                    return context;
#else
                    return ((FrameworkElement)Source)?.DataContext;
#endif
                }
                set
                {
                    var target = (FrameworkElement)Source;
                    if (target != null)
                        target.DataContext = ReferenceEquals(value, BindingConstants.UnsetValue) ? DependencyProperty.UnsetValue : value;
                }
            }

            public event EventHandler<ISourceValue, EventArgs> ValueChanged;

            #endregion

            #region Methods

            private void RaiseDataContextChanged(object sender, EventType args)
            {
                _isUnset = false;
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }

            private void ElementOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
            {
                if (Value == null)
                    RaiseDataContextChanged(sender, default(DependencyPropertyChangedEventArgs));
            }

            private static bool IsUnset(FrameworkElement element)
            {
                if (element.DataContext != null)
                    return false;
                while (element != null)
                {
                    if (element.ReadLocalValue(FrameworkElement.DataContextProperty) != DependencyProperty.UnsetValue)
                        return false;
                    element = BindingServiceProvider.VisualTreeManager.GetParent(element) as FrameworkElement;
                }
                return true;
            }

            #endregion
        }

        #endregion

        #region Properties

        public static bool ListenUnloadEvent { get; set; }

        #endregion

        #region Overrides of BindingContextManager

        protected override IBindingContext CreateBindingContext(object item)
        {
            var element = item as FrameworkElement;
            if (element == null)
                return base.CreateBindingContext(item);
            return new FrameworkElementBindingContext(element);
        }

        #endregion
    }
}
