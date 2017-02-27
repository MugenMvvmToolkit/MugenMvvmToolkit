#region Copyright

// ****************************************************************************
// <copyright file="DependencyPropertyBindingMember.cs">
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

#if WINDOWS_UWP
using Windows.UI.Xaml;
#else
using System.Windows;
using System.Windows.Data;
#endif
using System;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding;
using MugenMvvmToolkit.Binding.DataConstants;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Models;

#if WINDOWS_UWP

namespace MugenMvvmToolkit.UWP.Binding.Models
#elif WPF
using BindingEx = System.Windows.Data.Binding;

namespace MugenMvvmToolkit.WPF.Binding.Models
#endif
{
    public sealed class DependencyPropertyBindingMember : IBindingMemberInfo
    {
        #region Nested types

#if !WINDOWS_UWP
        public sealed class DependencyPropertyListener : DependencyObject, IDisposable
        {
            #region Fields

            public static readonly DependencyProperty ValueProperty = DependencyProperty
                .Register("Value", typeof(object), typeof(DependencyPropertyListener), new PropertyMetadata(null, OnValueChanged));


            private static readonly Action<DependencyPropertyListener> DisposeDelegate = DisposeInternal;
            private WeakEventListenerWrapper _listener;

            #endregion

            #region Constructors

            public DependencyPropertyListener(DependencyObject source, string propertyToBind, IEventListener listener)
            {
                _listener = listener.ToWeakWrapper();
                BindingOperations.SetBinding(this, ValueProperty,
                    new BindingEx
                    {
                        Path = new PropertyPath(propertyToBind),
                        Source = source,
                        Mode = BindingMode.OneWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
#if !WPF || !NET4
                        ValidatesOnNotifyDataErrors = false,
#endif
                        ValidatesOnDataErrors = false,
                        ValidatesOnExceptions = false,
                        NotifyOnValidationError = false,
#if WPF
                        NotifyOnSourceUpdated = false,
                        NotifyOnTargetUpdated = false
#endif
                    });
            }

            public DependencyPropertyListener(DependencyObject source, DependencyProperty propertyToBind, IEventListener listener)
            {
                _listener = listener.ToWeakWrapper();
                BindingOperations.SetBinding(this, ValueProperty,
                    new System.Windows.Data.Binding
                    {
                        Path = new PropertyPath(propertyToBind),
                        Source = source,
                        Mode = BindingMode.OneWay,
                        UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
#if !WPF || !NET4
                        ValidatesOnNotifyDataErrors = false,
#endif
                        ValidatesOnDataErrors = false,
                        ValidatesOnExceptions = false,
                        NotifyOnValidationError = false,
#if WPF
                        NotifyOnSourceUpdated = false,
                        NotifyOnTargetUpdated = false
#endif
                    });
            }

            #endregion

            #region Properties

            public object Value
            {
                get { return GetValue(ValueProperty); }
                set { SetValue(ValueProperty, value); }
            }

            #endregion

            #region Methods

            private static void OnValueChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
            {
                var listener = (DependencyPropertyListener)sender;
                if (!listener._listener.EventListener.TryHandle(sender, EventArgs.Empty))
                    DisposeInternal(listener);
            }

            private static void DisposeInternal(DependencyPropertyListener listener)
            {
                try
                {
                    if (listener._listener.IsEmpty)
                        return;
                    listener.ClearValue(ValueProperty);
                    listener._listener = WeakEventListenerWrapper.Empty;
                }
                catch (InvalidOperationException e)
                {
                    Tracer.Error(e.Flatten(true));
                }
            }

            #endregion

            #region Implementation of IDisposable

            public void Dispose()
            {
                if (Dispatcher.CheckAccess())
                    DisposeInternal(this);
                else
                    Dispatcher.BeginInvoke(DisposeDelegate, this);
            }

            #endregion
        }
#endif


        #endregion

        #region Fields

        internal static readonly Func<object, bool> IsNamedObjectFunc;

        private readonly bool _canWrite;
        private readonly IBindingMemberInfo _changePropertyMember;
        private readonly DependencyProperty _dependencyProperty;
        private readonly object _member;
        private readonly string _path;
        private readonly Type _type;

        #endregion

        #region Constructors

        static DependencyPropertyBindingMember()
        {
            Type type = null;
            bool isNamedObject = false;
            try
            {
                type = DependencyProperty.UnsetValue.GetType();
                isNamedObject = type.FullName.Equals("MS.Internal.NamedObject", StringComparison.Ordinal);
                var methodInfo = typeof(DependencyPropertyBindingMember).GetMethodEx(nameof(Is), MemberFlags.Static | MemberFlags.Public);
                if (methodInfo == null || !isNamedObject)
                    IsNamedObjectFunc = o => false;
                else
                    IsNamedObjectFunc = (Func<object, bool>)ServiceProvider.ReflectionManager.TryCreateDelegate(typeof(Func<object, bool>), null, methodInfo.MakeGenericMethod(type));
            }
            catch
            {
                if (isNamedObject)
                    IsNamedObjectFunc = o => o != null && o.GetType() == type;
                else
                    IsNamedObjectFunc = o => false;
            }
        }

        public DependencyPropertyBindingMember([NotNull] DependencyProperty dependencyProperty, [NotNull] string path,
            [NotNull] Type type, bool readOnly, [CanBeNull] object member, [CanBeNull] IBindingMemberInfo changePropertyMember)
        {
            Should.NotBeNull(dependencyProperty, nameof(dependencyProperty));
            Should.NotBeNullOrEmpty(path, nameof(path));
            Should.NotBeNull(type, nameof(type));
            _dependencyProperty = dependencyProperty;
            _path = path;
#if WPF
            _type = dependencyProperty.PropertyType;
            _canWrite = !dependencyProperty.ReadOnly;
#else
            _type = type;
            _canWrite = !readOnly;
#endif
            _member = member;
            _changePropertyMember = changePropertyMember;
        }

        #endregion

        #region Methods

        public static bool Is<T>(object item)
        {
            return item is T;
        }

#if WINDOWS_UWP
        public static IDisposable ObserveProperty(DependencyObject src, DependencyProperty property, IEventListener listener)
        {
            listener = listener.ToWeakEventListener();
            var t = src.RegisterPropertyChangedCallback(property, listener.Handle);
            return WeakActionToken.Create(src, property, t, (dp, p, token) => dp.UnregisterPropertyChangedCallback(p, token));
        }
#endif
        #endregion

        #region Implementation of IBindingMemberInfo

        public string Path => _path;

        public Type Type => _type;

        public object Member => _member;

        public BindingMemberType MemberType => BindingMemberType.DependencyProperty;

        public bool CanRead => true;

        public bool CanWrite => _canWrite;

        public bool CanObserve => true;

        public object GetValue(object source, object[] args)
        {
            object value = ((DependencyObject)source).GetValue(_dependencyProperty);
            if (ReferenceEquals(value, DependencyProperty.UnsetValue) || IsNamedObjectFunc(value))
                return BindingConstants.UnsetValue;
            return value;
        }

        public object SetValue(object source, object[] args)
        {
            return SetSingleValue(source, args[0]);
        }

        public object SetSingleValue(object source, object value)
        {
            if (ReferenceEquals(value, BindingConstants.UnsetValue))
                value = DependencyProperty.UnsetValue;
            ((DependencyObject)source).SetValue(_dependencyProperty, value);
            return null;
        }

        public IDisposable TryObserve(object source, IEventListener listener)
        {
            if (_changePropertyMember == null)
#if WINDOWS_UWP
                return ObserveProperty((DependencyObject)source, _dependencyProperty, listener);
#else
                return new DependencyPropertyListener((DependencyObject)source, _dependencyProperty, listener);
#endif
            return _changePropertyMember.SetSingleValue(source, listener) as IDisposable;
        }

        #endregion
    }
}
