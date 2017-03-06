#region Copyright

// ****************************************************************************
// <copyright file="WeakEventManager.cs">
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
using System.ComponentModel;
using System.Reflection;
using MugenMvvmToolkit.Binding.Interfaces;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public class WeakEventManager : IWeakEventManager
    {
        #region Nested types

        private sealed class WeakPropertyChangedListener
        {
            #region Nested types

            private sealed class Unsubscriber : IDisposable
            {
                #region Fields

                private WeakPropertyChangedListener _eventListener;
                private WeakEventListenerWrapper _weakItem;
                private readonly string _propertyName;

                #endregion

                #region Constructors

                public Unsubscriber(WeakPropertyChangedListener eventListener, WeakEventListenerWrapper weakItem, string propertyName)
                {
                    _eventListener = eventListener;
                    _weakItem = weakItem;
                    _propertyName = propertyName;
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
                        listener.Remove(weakItem, _propertyName);
                    }
                }

                #endregion
            }

            #endregion

            #region Fields

            //Use an array to reduce the cost of memory and do not lock during a call event.
            private KeyValuePair<WeakEventListenerWrapper, string>[] _listeners;
            private ushort _size;
            private ushort _removedSize;

            #endregion

            #region Constructors

            public WeakPropertyChangedListener()
            {
                _listeners = Empty.Array<KeyValuePair<WeakEventListenerWrapper, string>>();
            }

            #endregion

            #region Methods

            public void Handle(object sender, PropertyChangedEventArgs args)
            {
                bool hasDeadRef = false;
                var listeners = _listeners;
                for (int i = 0; i < listeners.Length; i++)
                {
                    if (i >= _size)
                        break;
                    var pair = listeners[i];
                    if (pair.Key.IsEmpty)
                    {
                        hasDeadRef = true;
                        continue;
                    }
                    if (ToolkitExtensions.MemberNameEqual(args.PropertyName, pair.Value, true))
                    {
                        if (!pair.Key.EventListener.TryHandle(sender, args))
                            hasDeadRef = true;
                    }
                }
                if (hasDeadRef)
                {
                    lock (this)
                        Cleanup();
                }
            }

            internal IDisposable Add(IEventListener target, string path)
            {
                return AddInternal(target.ToWeakWrapper(), path);
            }

            private void Cleanup()
            {
                var size = _size;
                _size = 0;
                _removedSize = 0;
                for (int i = 0; i < size; i++)
                {
                    var reference = _listeners[i];
                    if (reference.Key.EventListener.IsAlive)
                        _listeners[_size++] = reference;
                }
                if (_size == 0)
                    _listeners = Empty.Array<KeyValuePair<WeakEventListenerWrapper, string>>();
                else if (_listeners.Length / (float)_size > 2)
                {
                    var listeners = new KeyValuePair<WeakEventListenerWrapper, string>[_size + (_size >> 2)];
                    Array.Copy(_listeners, 0, listeners, 0, _size);
                    _listeners = listeners;
                }
            }

            private IDisposable AddInternal(WeakEventListenerWrapper weakItem, string path)
            {
                lock (this)
                {
                    if (_listeners.Length == 0)
                    {
                        _listeners = new[] { new KeyValuePair<WeakEventListenerWrapper, string>(weakItem, path) };
                        _size = 1;
                        _removedSize = 0;
                    }
                    else
                    {
                        if (_removedSize == 0)
                        {
                            if (_size == _listeners.Length)
                                EventListenerList.EnsureCapacity(ref _listeners, _size, _size + 1);
                            _listeners[_size++] = new KeyValuePair<WeakEventListenerWrapper, string>(weakItem, path);
                        }
                        else
                        {
                            for (int i = 0; i < _size; i++)
                            {
                                if (_listeners[i].Key.IsEmpty)
                                {
                                    _listeners[i] = new KeyValuePair<WeakEventListenerWrapper, string>(weakItem, path);
                                    --_removedSize;
                                    break;
                                }
                            }
                        }
                    }
                }
                return new Unsubscriber(this, weakItem, path);
            }

            private void Remove(WeakEventListenerWrapper weakItem, string propertyName)
            {
                lock (this)
                {
                    for (int i = 0; i < _listeners.Length; i++)
                    {
                        var pair = _listeners[i];
                        if (!pair.Key.IsEmpty && pair.Value == propertyName && ReferenceEquals(pair.Key.Source, weakItem.Source))
                        {
                            ++_removedSize;
                            _listeners[i] = default(KeyValuePair<WeakEventListenerWrapper, string>);
                            return;
                        }
                    }
                }
            }

            #endregion
        }

        #endregion

        #region Fields

        private static readonly Func<object, object, EventListenerList> CreateContextListenerDelegate;
        private static readonly Func<object, object, EventListenerList> CreateWeakListenerDelegate;
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

        public virtual IDisposable TrySubscribe(object target, EventInfo eventInfo, IEventListener listener, IDataContext context = null)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(eventInfo, nameof(eventInfo));
            Should.NotBeNull(listener, nameof(listener));
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

        public virtual IDisposable Subscribe(INotifyPropertyChanged propertyChanged, string propertyName, IEventListener listener, IDataContext context = null)
        {
            Should.NotBeNull(propertyChanged, nameof(propertyChanged));
            Should.NotBeNull(propertyName, nameof(propertyName));
            Should.NotBeNull(listener, nameof(listener));
            return ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(propertyChanged, PropertyChangedMember, CreateWeakPropertyListenerDelegate, null)
                .Add(listener, propertyName);
        }

        #endregion

        #region Methods

        internal static IDisposable AddBindingContextListener(IBindingContext ctx, IEventListener listener, bool withUnsubscriber)
        {
            var l = GetBindingContextListener(ctx);
            if (l.IsEmpty)
                return null;
            if (withUnsubscriber)
                return l.AddWithUnsubscriber(listener);
            l.Add(listener);
            return null;
        }

        internal static void RemoveBindingContextListener(IBindingContext ctx, IEventListener listener)
        {
            var l = GetBindingContextListener(ctx);
            if (!l.IsEmpty)
                l.Remove(listener);
        }

        private static EventListenerList GetBindingContextListener(IBindingContext ctx)
        {
            var src = ctx.Source;
            if (src == null)
                return EventListenerList.EmptyListener;
            return ServiceProvider
                .AttachedValueProvider
                .GetOrAdd(src, BindingContextMember, CreateContextListenerDelegate, ctx);
        }

        private static EventListenerList CreateContextListener(object src, object state)
        {
            var context = (IBindingContext)state;
            var listenerInternal = new EventListenerList();
            context.ValueChanged += listenerInternal.Raise;
            return listenerInternal;
        }

        private static WeakPropertyChangedListener CreateWeakPropertyListener(INotifyPropertyChanged propertyChanged, object state)
        {
            var listener = new WeakPropertyChangedListener();
            propertyChanged.PropertyChanged += listener.Handle;
            return listener;
        }

        private static EventListenerList CreateWeakListener(object target, object state)
        {
            var eventInfo = (EventInfo)state;
            var listenerInternal = new EventListenerList();
            object handler = eventInfo.EventHandlerType == typeof(EventHandler)
                ? new EventHandler(listenerInternal.Raise)
                : ServiceProvider.ReflectionManager.TryCreateDelegate(eventInfo.EventHandlerType,
                    listenerInternal, EventListenerList.RaiseMethod);

            if (handler == null)
                return EventListenerList.EmptyListener;
#if NET_STANDARD
            var addMethod = eventInfo.AddMethod;
#else
            var addMethod = eventInfo.GetAddMethod(true);
#endif
            if (addMethod == null)
                return EventListenerList.EmptyListener;
            addMethod.InvokeEx(target, handler);
            return listenerInternal;
        }

        #endregion
    }
}