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
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using Windows.UI.Xaml;
using MugenMvvmToolkit.UWP.Binding.Models;
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

        private sealed class FrameworkElementBindingContext : IBindingContext
        {
            #region Fields

            private readonly WeakReference _sourceReference;

            #endregion

            #region Constructors

            public FrameworkElementBindingContext(FrameworkElement element)
            {
                _sourceReference = ServiceProvider.WeakReferenceFactory(element);
                element.DataContextChanged += RaiseDataContextChanged;
                if (ListenUnloadEvent)
                    element.Unloaded += ElementOnUnloaded;
            }

            #endregion

            #region Implementation of IBindingContext

            public object Source => _sourceReference.Target;

            public bool IsAlive => true;

            public object Value
            {
                get
                {
                    object context = ((FrameworkElement)Source)?.DataContext;
                    if (context == null)
                        return null;
                    if (DependencyPropertyBindingMember.IsNamedObjectFunc(context))
                        return BindingConstants.UnsetValue;
                    return context;
                }
                set
                {
                    var target = (FrameworkElement)Source;
                    if (target == null)
                        return;
                    if (ReferenceEquals(value, BindingConstants.UnsetValue))
                        target.DataContext = DependencyProperty.UnsetValue;
                    else
                        target.DataContext = value;
                }
            }

            public event EventHandler<ISourceValue, EventArgs> ValueChanged;

            #endregion

            #region Methods

            private void RaiseDataContextChanged(object sender, EventType args)
            {
                ValueChanged?.Invoke(this, EventArgs.Empty);
            }

            private void ElementOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
            {
                if (Value == null)
                    RaiseDataContextChanged(sender, default(DependencyPropertyChangedEventArgs));
            }

            #endregion
        }

        #endregion

        #region Constructors

#if WPF
        static WpfBindingContextManager()
        {

            ListenUnloadEvent = true;
        }
#endif

        #endregion

        #region Properties

        public static bool ListenUnloadEvent { get; set; }

        #endregion

        #region Overrides of BindingContextManager

        protected override IBindingContext CreateBindingContext(object item)
        {
            var member = item as FrameworkElement;
            if (member == null)
                return base.CreateBindingContext(item);
            return new FrameworkElementBindingContext(member);
        }

        #endregion
    }
}
