#region Copyright

// ****************************************************************************
// <copyright file="BindingContextManagerEx.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
#elif SILVERLIGHT
using System.Windows;
using MugenMvvmToolkit.Silverlight.Binding.Models;
using EventType = System.Windows.DependencyPropertyChangedEventArgs;

namespace MugenMvvmToolkit.Silverlight.Binding.Infrastructure
#elif WINDOWSCOMMON
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using Windows.UI.Xaml;
using MugenMvvmToolkit.WinRT.Binding.Models;
using EventType = System.Object;

namespace MugenMvvmToolkit.WinRT.Binding.Infrastructure
#elif WINDOWS_PHONE
using System.Windows;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.WinPhone.Binding.Models;
#if WINDOWS_PHONE
using EventType = MugenMvvmToolkit.Binding.Models.EventArg.ValueChangedEventArgs;
#else
using EventType = System.Windows.DependencyPropertyChangedEventArgs;
#endif

namespace MugenMvvmToolkit.WinPhone.Binding.Infrastructure
#endif

{
    public class BindingContextManagerEx : BindingContextManager
    {
        #region Nested types

        private sealed class FrameworkElementBindingContext : IBindingContext
        {
            #region Fields

#if WINDOWS_PHONE
            private readonly IObserver _observer;
#else
            private readonly WeakReference _sourceReference;
#endif
            #endregion

            #region Constructors

            public FrameworkElementBindingContext(FrameworkElement element)
            {
#if WINDOWS_PHONE
                _observer = BindingServiceProvider
                    .ObserverProvider
                    .Observe(element, BindingPath.DataContext, true, DataContext.Empty);
                _observer.ValueChanged += RaiseDataContextChanged;
#else
                _sourceReference = ServiceProvider.WeakReferenceFactory(element);
                element.DataContextChanged += RaiseDataContextChanged;
                if (ListenUnloadEvent)
                    element.Unloaded += ElementOnUnloaded;
#endif
            }

            #endregion

            #region Implementation of IBindingContext

            public object Source
            {
                get
                {
#if WINDOWS_PHONE
                    return _observer.Source;
#else
                    return _sourceReference.Target;
#endif
                }
            }

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

#if !WINDOWS_PHONE
            private void ElementOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
            {
                if (Value == null)
                    RaiseDataContextChanged(sender, default(DependencyPropertyChangedEventArgs));
            }
#endif
            #endregion
        }

        #endregion

        #region Properties

#if !WINDOWS_PHONE
        static BindingContextManagerEx()
        {
#if WPF
            ListenUnloadEvent = true;
#endif
        }

        public static bool ListenUnloadEvent { get; set; }
#endif

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
