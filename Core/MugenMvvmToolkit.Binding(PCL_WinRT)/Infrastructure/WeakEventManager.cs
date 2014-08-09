#region Copyright
// ****************************************************************************
// <copyright file="WeakEventManager.cs">
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Infrastructure;
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
        public sealed class WeakListenerInternal
        {
            #region Fields

            private WeakReference[] _listeners;

            private readonly WeakReference _selfReference;
            internal static readonly WeakListenerInternal EmptyListener;
            internal static readonly MethodInfo HandleMethod;

            #endregion

            #region Constructors

            static WeakListenerInternal()
            {
                HandleMethod = typeof(WeakListenerInternal)
                    .GetMethodEx("Handle", MemberFlags.Public | MemberFlags.Instance);
                EmptyListener = new WeakListenerInternal(true);
            }

            internal WeakListenerInternal()
                : this(false)
            {
            }

            private WeakListenerInternal(bool empty)
            {
                if (!empty)
                {
                    _listeners = EmptyValue<WeakReference>.ArrayInstance;
                    _selfReference = ServiceProvider.WeakReferenceFactory(this, true);
                }
            }

            #endregion

            #region Properties

            internal bool IsEmpty
            {
                get { return _listeners == null; }
            }

            #endregion

            #region Methods

            /// <summary>
            ///     This is internal method, don't use it.
            /// </summary>
            public void Handle<TSender, TArgs>(TSender sender, TArgs args)
            {
                bool hasDeadRef = false;
                var listeners = _listeners;
                for (int i = 0; i < listeners.Length; i++)
                {
                    var listener = (IEventListener)listeners[i].Target;
                    if (listener == null)
                        hasDeadRef = true;
                    else
                        listener.Handle(sender, args);
                }
                if (hasDeadRef)
                {
                    lock (this)
                        Update(null);
                }
            }

            internal IDisposable Add(IEventListener target)
            {
                var reference = GetWeakReference(target);
                //NOTE it's normal here.
                lock (this)
                {
                    if (_listeners.Length == 0)
                        _listeners = new[] { reference };
                    else
                        Update(reference);
                }
                return WeakActionToken.Create(_selfReference, reference, RemoveStatic, false);
            }

            private void Update(WeakReference newItem)
            {
                var references = newItem == null
                    ? new WeakReference[_listeners.Length]
                    : new WeakReference[_listeners.Length + 1];
                int index = 0;
                for (int i = 0; i < _listeners.Length; i++)
                {
                    var reference = _listeners[i];
                    if (reference.Target != null)
                        references[index++] = reference;
                }
                if (newItem != null)
                {
                    references[index] = newItem;
                    index++;
                }
                else if (index == 0)
                {
                    _listeners = EmptyValue<WeakReference>.ArrayInstance;
                    return;
                }
                if (references.Length != index)
                    Array.Resize(ref references, index);
                _listeners = references;
            }

            private void Remove(WeakReference weakReference)
            {
                lock (this)
                {
                    for (int i = 0; i < _listeners.Length; i++)
                    {
                        if (ReferenceEquals(_listeners[i], weakReference))
                        {
                            _listeners[i] = MvvmExtensions.GetWeakReference(null);
                            Update(null);
                            return;
                        }
                    }
                }
            }

            private static void RemoveStatic(WeakReference selfReference, WeakReference weakReference)
            {
                var listener = (WeakListenerInternal)selfReference.Target;
                if (listener != null)
                    listener.Remove(weakReference);
            }

            #endregion
        }

        private sealed class WeakPropertyChangedListener
        {
            #region Fields

            private readonly WeakReference _selfReference;
            private KeyValuePair<WeakReference, string>[] _listeners;

            #endregion

            #region Constructors

            public WeakPropertyChangedListener()
            {
                _listeners = EmptyValue<KeyValuePair<WeakReference, string>>.ArrayInstance;
                _selfReference = ServiceProvider.WeakReferenceFactory(this, true);
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
                    var listener = (IEventListener)pair.Key.Target;
                    if (listener == null)
                        hasDeadRef = true;
                    else
                    {
                        if (BindingExtensions.PropertyNameEqual(args.PropertyName, pair.Value))
                            listener.Handle(sender, args);
                    }
                }
                if (hasDeadRef)
                {
                    lock (this)
                        Update(null, null);
                }
            }

            internal IDisposable Add(IEventListener target, string path)
            {
                var reference = GetWeakReference(target);
                //NOTE it's normal here.
                lock (this)
                {
                    if (_listeners.Length == 0)
                        _listeners = new[] { new KeyValuePair<WeakReference, string>(reference, path) };
                    else
                        Update(reference, path);
                }
                return WeakActionToken.Create(_selfReference, reference, RemoveStatic, false);
            }

            private void Update(WeakReference newItem, string path)
            {
                var references = newItem == null
                    ? new KeyValuePair<WeakReference, string>[_listeners.Length]
                    : new KeyValuePair<WeakReference, string>[_listeners.Length + 1];
                int index = 0;
                for (int i = 0; i < _listeners.Length; i++)
                {
                    var reference = _listeners[i];
                    if (reference.Key.Target != null)
                        references[index++] = reference;
                }
                if (newItem != null)
                {
                    references[index] = new KeyValuePair<WeakReference, string>(newItem, path);
                    index++;
                }
                else if (index == 0)
                {
                    _listeners = EmptyValue<KeyValuePair<WeakReference, string>>.ArrayInstance;
                    return;
                }
                if (references.Length != index)
                    Array.Resize(ref references, index);
                _listeners = references;
            }

            private void Remove(WeakReference weakReference)
            {
                lock (this)
                {
                    for (int i = 0; i < _listeners.Length; i++)
                    {
                        if (ReferenceEquals(_listeners[i].Key, weakReference))
                        {
                            _listeners[i] = new KeyValuePair<WeakReference, string>(MvvmExtensions.GetWeakReference(null), string.Empty);
                            Update(null, null);
                            return;
                        }
                    }
                }
            }

            private static void RemoveStatic(WeakReference weakListenerInternal, WeakReference weakReference)
            {
                var listener = (WeakPropertyChangedListener)weakListenerInternal.Target;
                if (listener != null)
                    listener.Remove(weakReference);
            }

            #endregion
        }

        #endregion

        #region Fields

        private const string EventPrefix = "#@!weak";
        private const string PropertyChangedMember = "#@!weakpropchang";

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
                .GetOrAdd(target, EventPrefix + eventInfo.Name, CreateWeakListener, eventInfo);
            if (listenerInternal.IsEmpty)
                return null;
            return listenerInternal.Add(listener);
        }

        /// <summary>
        ///     Subscribes to the property changed event.
        /// </summary>
        public virtual IDisposable Subscribe(INotifyPropertyChanged propertyChanged, string propertyName, IEventListener listener, IDataContext context = null)
        {
            Should.NotBeNull(propertyChanged, "propertyChanged");
            Should.NotBeNull(propertyName, "propertyName");
            Should.NotBeNull(listener, "listener");
            var listenerInternal = ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(propertyChanged, PropertyChangedMember, CreateWeakPropertyListener, null);
            return listenerInternal.Add(listener, propertyName);
        }

        #endregion

        #region Methods

        private static WeakPropertyChangedListener CreateWeakPropertyListener(INotifyPropertyChanged propertyChanged, object state)
        {
            var listener = new WeakPropertyChangedListener();
            propertyChanged.PropertyChanged += listener.Handle;
            return listener;
        }

        private static WeakListenerInternal CreateWeakListener(object target, object state)
        {
            var eventInfo = (EventInfo)state;
            var listenerInternal = new WeakListenerInternal();
            Delegate handler = eventInfo.EventHandlerType == typeof(EventHandler)
                ? new EventHandler(listenerInternal.Handle)
                : ServiceProvider.ReflectionManager.TryCreateDelegate(eventInfo.EventHandlerType,
                    listenerInternal, WeakListenerInternal.HandleMethod);
            if (handler == null)
                return WeakListenerInternal.EmptyListener;
#if PCL_WINRT
            var addMethod = eventInfo.AddMethod;
#else
            var addMethod = eventInfo.GetAddMethod(true);            
#endif
            if (addMethod == null)
                return WeakListenerInternal.EmptyListener;
            addMethod.InvokeEx(target, handler);
            return listenerInternal;
        }

        private static WeakReference GetWeakReference(object target)
        {
            var pathObserver = target as IHasSelfWeakReference;
            if (pathObserver == null)
                return ServiceProvider.WeakReferenceFactory(target, true);
            return pathObserver.SelfReference;
        }

        #endregion
    }
}