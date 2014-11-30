#region Copyright
// ****************************************************************************
// <copyright file="BindingContextManagerEx.cs">
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
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Binding.Models.EventArg;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;
#if NETFX_CORE || WINDOWSCOMMON
using Windows.UI.Xaml;
using EventType = System.Object;
#else
using System.Windows;
using EventType = System.Windows.DependencyPropertyChangedEventArgs;
#endif
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public class BindingContextManagerEx : BindingContextManager
    {
        #region Nested types

        private sealed class BindingContextSource : IBindingContext
#if WINDOWS_PHONE || NETFX_CORE
, IHandler<ValueChangedEventArgs>
#endif
        {
            #region Fields

#if WINDOWS_PHONE || NETFX_CORE
            private readonly IObserver _observer;
#else
            private readonly WeakReference _sourceReference;
#endif
            private static Type _namedObjectType;

            #endregion

            #region Constructors

            public BindingContextSource(FrameworkElement element)
            {
#if WINDOWS_PHONE || NETFX_CORE
                _observer = BindingServiceProvider
                    .ObserverProvider
                    .Observe(element, BindingPath.DataContext, true);
                _observer.Listener = this;
#else
                _sourceReference = ServiceProvider.WeakReferenceFactory(element, true);
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
#if WINDOWS_PHONE || NETFX_CORE
                    return _observer.Source;
#else
                    return _sourceReference.Target;
#endif
                }
            }

            public bool IsAlive
            {
                get { return true; }
            }

            public object Value
            {
                get
                {
                    var target = (FrameworkElement)Source;
                    if (target == null)
                        return null;
                    object context = target.DataContext;
                    if (context == null)
                        return null;
                    if (_namedObjectType == null)
                    {
                        if (context.GetType().FullName.Equals("MS.Internal.NamedObject", StringComparison.Ordinal))
                        {
                            _namedObjectType = context.GetType();
                            return null;
                        }
                    }
                    else if (_namedObjectType == context.GetType())
                        return null;
                    return context;
                }
                set
                {
                    var target = (FrameworkElement)Source;
                    if (target == null)
                        return;
                    if (ReferenceEquals(value, BindingConstants.UnsetValue))
                        value = DependencyProperty.UnsetValue;
                    target.DataContext = value;
                }
            }

            /// <summary>
            ///     Occurs when the <see cref="Value"/>  property changed.
            /// </summary>
            public event EventHandler<ISourceValue, EventArgs> ValueChanged;

            #endregion

            #region Methods

#if WINDOWS_PHONE || NETFX_CORE
            void IHandler<ValueChangedEventArgs>.Handle(object sender, ValueChangedEventArgs message)
            {
                var handler = ValueChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }
#else
            private void RaiseDataContextChanged(object sender, EventType args)
            {
                var handler = ValueChanged;
                if (handler != null)
                    handler(this, EventArgs.Empty);
            }

            private void ElementOnUnloaded(object sender, RoutedEventArgs routedEventArgs)
            {
                if (Value == null)
                    RaiseDataContextChanged(sender, default(EventType));
            }
#endif
            #endregion
        }

        #endregion

        #region Properties

#if !WINDOWS_PHONE && !NETFX_CORE
        static BindingContextManagerEx()
        {
#if WPF
            ListenUnloadEvent = true;
#endif
        }

        /// <summary>
        /// Gets or sets the value that indicates that context should listen the Unload event.
        /// </summary>
        public static bool ListenUnloadEvent { get; set; }
#endif

        #endregion

        #region Overrides of BindingContextManager

        /// <summary>
        ///     Creates an instance of <see cref="IBindingContext" /> for the specified item.
        /// </summary>
        /// <returns>An instnace of <see cref="IBindingContext" />.</returns>
        protected override IBindingContext CreateBindingContext(object item)
        {
            var member = item as FrameworkElement;
            if (member == null)
                return base.CreateBindingContext(item);
            return new BindingContextSource(member);
        }

        #endregion
    }
}