#region Copyright

// ****************************************************************************
// <copyright file="WeakEventManager.cs">
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
                    var pair = listeners[i];
                    if (ToolkitExtensions.MemberNameEqual(args.PropertyName, pair.Value, true))
                    {
                        if (!pair.Key.EventListener.TryHandle(sender, args))
                            hasDeadRef = true;
                    }
                }
                if (hasDeadRef)
                {
                    lock (this)
                        Update(WeakEventListenerWrapper.Empty, null);
                }
            }

            internal IDisposable Add(IEventListener target, string path)
            {
                return AddInternal(target.ToWeakWrapper(), path);
            }

            private IDisposable AddInternal(WeakEventListenerWrapper weakItem, string path)
            {
                lock (this)
                {
                    if (_listeners.Length == 0)
                        _listeners = new[] { new KeyValuePair<WeakEventListenerWrapper, string>(weakItem, path) };
                    else
                        Update(weakItem, path);
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
                        if (pair.Value == propertyName && ReferenceEquals(pair.Key.Source, weakItem.Source))
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
                if (index == 0)
                {
                    _listeners = Empty.Array<KeyValuePair<WeakEventListenerWrapper, string>>();
                    return;
                }
                if (references.Length != index)
                    Array.Resize(ref references, index);
                _listeners = references;
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
#if PCL_WINRT
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
