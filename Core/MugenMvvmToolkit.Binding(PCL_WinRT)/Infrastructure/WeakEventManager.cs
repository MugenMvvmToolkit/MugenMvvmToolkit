#region Copyright

// ****************************************************************************
// <copyright file="WeakEventManager.cs">
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
using System.ComponentModel;
using System.Reflection;
using JetBrains.Annotations;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    /// <summary>
    ///     Represents the class that allows to subscribe to events using weak references.
    /// </summary>
    public class WeakEventManager : IWeakEventManager
    {
        #region Nested types

        /// <summary>
        ///     This is internal class, don't use it.
        /// </summary>
        public sealed class WeakListenerInternal : EventListenerList
        {
            #region Fields

            internal static readonly WeakListenerInternal EmptyListener;
            internal static readonly MethodInfo HandleMethod;
            private static readonly WeakEventListenerWrapper[] Empty;
            private static readonly Func<object, object, WeakListenerInternal> AddSourceEventDelegate;
            private static readonly UpdateValueDelegate<object, Func<object, object, WeakListenerInternal>, WeakListenerInternal, object> UpdateSourceEventDelegate;

            private readonly WeakReference _sourceRef;
            private readonly EventInfo _eventInfo;

            internal object Handler;
            //Only for WinRT
            internal object RegToken;

            #endregion

            #region Constructors

            static WeakListenerInternal()
            {
                Empty = new WeakEventListenerWrapper[0];
                AddSourceEventDelegate = AddSourceEvent;
                UpdateSourceEventDelegate = UpdateSourceEvent;
                HandleMethod = typeof(WeakListenerInternal).GetMethodEx("Raise", MemberFlags.Public | MemberFlags.Instance);
                EmptyListener = new WeakListenerInternal();
            }

            internal WeakListenerInternal(object source, EventInfo eventInfo)
            {
                _sourceRef = ToolkitExtensions.GetWeakReference(source);
                _eventInfo = eventInfo;
            }

            private WeakListenerInternal()
            {
                Listeners = null;
            }

            #endregion

            #region Properties

            internal bool IsEmpty
            {
                get { return Listeners == null; }
            }

            #endregion

            #region Overrides of EventListenerListBase

            /// <summary>
            /// Occurs on add a listener.
            /// </summary>
            protected override bool OnAdd(WeakEventListenerWrapper weakItem, bool withUnsubscriber, out IDisposable unsubscriber)
            {
                unsubscriber = null;
                if (!ReferenceEquals(Listeners, Empty))
                    return false;
                var source = _sourceRef.Target;
                if (source == null)
                    return false;
                var state = new object[] { this, weakItem, null };
                string member = _eventInfo == null ? BindingContextMember : EventPrefix + _eventInfo.Name;
                ServiceProvider.AttachedValueProvider.AddOrUpdate(source, member, AddSourceEventDelegate, UpdateSourceEventDelegate, state);
                unsubscriber = (IDisposable)state[2];
                return true;
            }

            /// <summary>
            /// Occurs on empty collection.
            /// </summary>
            protected override void OnEmpty()
            {
                Listeners = Empty;
                var source = _sourceRef.Target;
                if (source == null)
                    return;
                //Binding context
                if (_eventInfo == null)
                {
                    BindingServiceProvider.ContextManager.GetBindingContext(source).ValueChanged -= (EventHandler<ISourceValue, EventArgs>)Handler;
                    ServiceProvider.AttachedValueProvider.Clear(source, BindingContextMember);
                }
                else
                {
#if PCL_WINRT
                    var removeMethod = _eventInfo.RemoveMethod;
#else
                    var removeMethod = _eventInfo.GetRemoveMethod(true);
#endif
                    if (removeMethod != null)
                        removeMethod.InvokeEx(source, RegToken ?? Handler);
                    ServiceProvider.AttachedValueProvider.Clear(source, EventPrefix + _eventInfo.Name);
                }
            }

            #endregion

            #region Methods

            private static WeakListenerInternal AddSourceEvent(object source, object state)
            {
                var array = (object[])state;
                var @this = (WeakListenerInternal)array[0];
                var weakItem = (WeakEventListenerWrapper)array[1];
                @this.Listeners = new[] { weakItem };

                //Binding context
                if (@this._eventInfo == null)
                    BindingServiceProvider.ContextManager.GetBindingContext(source).ValueChanged += (EventHandler<ISourceValue, EventArgs>)@this.Handler;
                else
                {
#if PCL_WINRT
                    var addMethod = @this._eventInfo.AddMethod;
#else
                    var addMethod = @this._eventInfo.GetAddMethod(true);
#endif
                    @this.RegToken = addMethod.InvokeEx(source, @this.Handler);
                }

                array[2] = new Unsubscriber(@this, weakItem);
                return @this;
            }

            private static WeakListenerInternal UpdateSourceEvent(object source,
                Func<object, object, WeakListenerInternal> addValue, WeakListenerInternal currentValue, object state)
            {
                var array = (object[])state;
                var weakItem = (WeakEventListenerWrapper)array[1];
                array[2] = currentValue.AddInternal(weakItem, true);
                return currentValue;
            }

            #endregion
        }

        private sealed class WeakPropertyChangedListener
        {
            #region Nested types

            private sealed class Unsubscriber : IDisposable
            {
                #region Fields

                private WeakPropertyChangedListener _eventListener;
                private WeakEventListenerWrapper _weakItem;

                #endregion

                #region Constructors

                public Unsubscriber(WeakPropertyChangedListener eventListener, WeakEventListenerWrapper weakItem)
                {
                    _eventListener = eventListener;
                    _weakItem = weakItem;
                }

                #endregion

                #region Implementation of IDisposable

                public void Dispose()
                {
                    var listener = _eventListener;
                    var weakItem = _weakItem;
                    if (listener != null && !weakItem.IsEmpty)
                    {
                        _eventListener = null;
                        _weakItem = WeakEventListenerWrapper.Empty;
                        listener.Remove(weakItem);
                    }
                }

                #endregion
            }

            #endregion

            #region Fields

            private static readonly KeyValuePair<WeakEventListenerWrapper, string>[] Empty;

            private static readonly Func<INotifyPropertyChanged, object, WeakPropertyChangedListener> AddSourceEventDelegate;
            private static readonly UpdateValueDelegate<INotifyPropertyChanged, Func<INotifyPropertyChanged, object, WeakPropertyChangedListener>, WeakPropertyChangedListener, object> UpdateSourceEventDelegate;

            private readonly WeakReference _propertyChanged;

            //Use an array to reduce the cost of memory and do not lock during a call event.
            private KeyValuePair<WeakEventListenerWrapper, string>[] _listeners;

            #endregion

            #region Constructors

            static WeakPropertyChangedListener()
            {
                Empty = new KeyValuePair<WeakEventListenerWrapper, string>[0];
                UpdateSourceEventDelegate = UpdateSourceEvent;
                AddSourceEventDelegate = AddSourceEvent;
            }

            public WeakPropertyChangedListener(INotifyPropertyChanged propertyChanged)
            {
                _listeners = MugenMvvmToolkit.Empty.Array<KeyValuePair<WeakEventListenerWrapper, string>>();
                _propertyChanged = ToolkitExtensions.GetWeakReference(propertyChanged);
            }

            #endregion

            #region Methods

            public void Handle(object sender, PropertyChangedEventArgs args)
            {
                bool hasDeadRef = false;
                var listeners = _listeners;
                for (int i = 0; i < listeners.Length; i++)
                {
                    var pair = listeners[i];
                    if (ToolkitExtensions.PropertyNameEqual(args.PropertyName, pair.Value, true))
                    {
                        if (!pair.Key.EventListener.TryHandle(sender, args))
                            hasDeadRef = true;
                    }
                }
                if (hasDeadRef)
                {
                    lock (_propertyChanged)
                        Update(WeakEventListenerWrapper.Empty, null);
                }
            }

            internal IDisposable Add(IEventListener target, string path)
            {
                return AddInternal(target.ToWeakWrapper(), path);
            }

            private IDisposable AddInternal(WeakEventListenerWrapper weakItem, string path)
            {
                lock (_propertyChanged)
                {
                    //if value was removed from another thread
                    if (ReferenceEquals(_listeners, Empty))
                    {
                        var source = (INotifyPropertyChanged)_propertyChanged.Target;
                        if (source != null)
                        {
                            var state = new object[] { this, weakItem, path, null };
                            ServiceProvider.AttachedValueProvider.AddOrUpdate(source, PropertyChangedMember, AddSourceEventDelegate, UpdateSourceEventDelegate, state);
                            return (IDisposable)state[3];
                        }
                    }
                    else if (_listeners.Length == 0)
                        _listeners = new[] { new KeyValuePair<WeakEventListenerWrapper, string>(weakItem, path) };
                    else
                        Update(weakItem, path);
                }
                return new Unsubscriber(this, weakItem);
            }

            private void Remove(WeakEventListenerWrapper weakItem)
            {
                lock (_propertyChanged)
                {
                    for (int i = 0; i < _listeners.Length; i++)
                    {
                        if (ReferenceEquals(_listeners[i].Key.Source, weakItem.Source))
                        {
                            _listeners[i] = new KeyValuePair<WeakEventListenerWrapper, string>(WeakEventListenerWrapper.Empty, string.Empty);
                            Update(WeakEventListenerWrapper.Empty, null);
                            return;
                        }
                    }
                }
            }

            private void Update(WeakEventListenerWrapper newItem, string path)
            {
                var references = newItem.IsEmpty
                    ? new KeyValuePair<WeakEventListenerWrapper, string>[_listeners.Length]
                    : new KeyValuePair<WeakEventListenerWrapper, string>[_listeners.Length + 1];
                int index = 0;
                for (int i = 0; i < _listeners.Length; i++)
                {
                    var reference = _listeners[i];
                    if (reference.Key.EventListener.IsAlive)
                        references[index++] = reference;
                }
                if (!newItem.IsEmpty)
                {
                    references[index] = new KeyValuePair<WeakEventListenerWrapper, string>(newItem, path);
                    index++;
                }
                else if (index == 0)
                {
                    //Remove event from source, if no listeners.
                    _listeners = Empty;
                    var target = (INotifyPropertyChanged)_propertyChanged.Target;
                    if (target != null)
                    {
                        target.PropertyChanged -= Handle;
                        ServiceProvider.AttachedValueProvider.Clear(target, PropertyChangedMember);
                    }
                    return;
                }
                if (references.Length != index)
                    Array.Resize(ref references, index);
                _listeners = references;
            }

            private static WeakPropertyChangedListener AddSourceEvent(INotifyPropertyChanged source, object state)
            {
                var array = (object[])state;
                var @this = (WeakPropertyChangedListener)array[0];
                var weakItem = (WeakEventListenerWrapper)array[1];
                var path = (string)array[2];
                @this._listeners = new[] { new KeyValuePair<WeakEventListenerWrapper, string>(weakItem, path) };
                source.PropertyChanged += @this.Handle;
                array[3] = new Unsubscriber(@this, weakItem);
                return @this;
            }

            private static WeakPropertyChangedListener UpdateSourceEvent(INotifyPropertyChanged item,
                Func<INotifyPropertyChanged, object, WeakPropertyChangedListener> addValue, WeakPropertyChangedListener currentValue, object state)
            {
                var array = (object[])state;
                var weakItem = (WeakEventListenerWrapper)array[1];
                var path = (string)array[2];
                array[3] = currentValue.AddInternal(weakItem, path);
                return currentValue;
            }

            #endregion
        }

        #endregion

        #region Fields

        private static readonly Func<object, object, WeakListenerInternal> CreateContextListenerDelegate;
        private static readonly Func<object, object, WeakListenerInternal> CreateWeakListenerDelegate;
        private static readonly Func<INotifyPropertyChanged, object, WeakPropertyChangedListener> CreateWeakPropertyListenerDelegate;

        private const string EventPrefix = "#@!weak";
        private const string PropertyChangedMember = "#@!weakpropchang";
        private const string BindingContextMember = "@$ctxchanged";

        #endregion

        #region Constructors

        static WeakEventManager()
        {
            CreateWeakListenerDelegate = CreateWeakListener;
            CreateContextListenerDelegate = CreateContextListener;
            CreateWeakPropertyListenerDelegate = CreateWeakPropertyListener;
        }

        #endregion

        #region Implementation of IWeakEventManager

        /// <summary>
        ///     Attempts to subscribe to the event.
        /// </summary>
        public virtual IDisposable TrySubscribe(object target, EventInfo eventInfo, IEventListener listener, IDataContext context = null)
        {
            Should.NotBeNull(target, "target");
            Should.NotBeNull(eventInfo, "eventInfo");
            Should.NotBeNull(listener, "listener");
            var listenerInternal = ServiceProvider
                    .AttachedValueProvider
                    .GetOrAdd(target, EventPrefix + eventInfo.Name, CreateWeakListenerDelegate, eventInfo);
            if (listenerInternal.IsEmpty)
            {
                Tracer.Warn("The event '{0}' is not supported by weak event manager", eventInfo);
                return null;
            }
            return listenerInternal.AddWithUnsubscriber(listener);
        }

        /// <summary>
        ///     Subscribes to the property changed event.
        /// </summary>
        public virtual IDisposable Subscribe(INotifyPropertyChanged propertyChanged, string propertyName, IEventListener listener, IDataContext context = null)
        {
            Should.NotBeNull(propertyChanged, "propertyChanged");
            Should.NotBeNull(propertyName, "propertyName");
            Should.NotBeNull(listener, "listener");
            return ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(propertyChanged, PropertyChangedMember, CreateWeakPropertyListenerDelegate, null)
                .Add(listener, propertyName);
        }

        #endregion

        #region Methods

        internal static WeakListenerInternal GetBindingContextListener([NotNull] object source)
        {
            return ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(source, BindingContextMember, CreateContextListenerDelegate, null);
        }

        private static WeakListenerInternal CreateContextListener(object src, object state)
        {
            var listenerInternal = new WeakListenerInternal(src, null);
            var handler = new EventHandler<ISourceValue, EventArgs>(listenerInternal.Raise);
            listenerInternal.Handler = handler;
            BindingServiceProvider.ContextManager.GetBindingContext(src).ValueChanged += handler;
            return listenerInternal;
        }

        private static WeakPropertyChangedListener CreateWeakPropertyListener(INotifyPropertyChanged propertyChanged, object state)
        {
            var listener = new WeakPropertyChangedListener(propertyChanged);
            propertyChanged.PropertyChanged += listener.Handle;
            return listener;
        }

        private static WeakListenerInternal CreateWeakListener(object target, object state)
        {
            var eventInfo = (EventInfo)state;
            var listenerInternal = new WeakListenerInternal(target, eventInfo);
            listenerInternal.Handler = eventInfo.EventHandlerType == typeof(EventHandler)
                ? new EventHandler(listenerInternal.Raise)
                : ServiceProvider.ReflectionManager.TryCreateDelegate(eventInfo.EventHandlerType,
                    listenerInternal, WeakListenerInternal.HandleMethod);
            if (listenerInternal.Handler == null)
                return WeakListenerInternal.EmptyListener;
#if PCL_WINRT
            var addMethod = eventInfo.AddMethod;
#else
            var addMethod = eventInfo.GetAddMethod(true);
#endif
            if (addMethod == null)
                return WeakListenerInternal.EmptyListener;
            listenerInternal.RegToken = addMethod.InvokeEx(target, listenerInternal.Handler);
            return listenerInternal;
        }

        #endregion
    }
}