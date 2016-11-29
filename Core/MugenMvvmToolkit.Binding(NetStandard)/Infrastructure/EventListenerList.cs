#region Copyright

// ****************************************************************************
// <copyright file="EventListenerList.cs">
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
using System.Reflection;
using MugenMvvmToolkit.Attributes;
using MugenMvvmToolkit.Binding.Interfaces.Models;
using MugenMvvmToolkit.Binding.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Binding.Infrastructure
{
    public class EventListenerList
    {
        #region Nested types

        internal sealed class Unsubscriber : IDisposable
        {
            #region Fields

            private EventListenerList _eventListener;
            private WeakEventListenerWrapper _weakItem;

            #endregion

            #region Constructors

            public Unsubscriber(EventListenerList eventListener, WeakEventListenerWrapper weakItem)
            {
                _eventListener = eventListener;
                _weakItem = weakItem;
            }

            #endregion

            #region Implementation of IDisposable

            public void Dispose()
            {
                EventListenerList listener = _eventListener;
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

        internal static readonly EventListenerList EmptyListener;
        internal static readonly MethodInfo RaiseMethod;

        //Use an array to reduce the cost of memory and do not lock during a call event.
        private WeakEventListenerWrapper[] _listeners;
        private ushort _size;
        private ushort _removedSize;

        #endregion

        #region Constructors

        static EventListenerList()
        {
            RaiseMethod = typeof(EventListenerList).GetMethodEx(nameof(Raise), MemberFlags.Public | MemberFlags.Instance);
            EmptyListener = new EventListenerList(true);
        }

        private EventListenerList(bool _)
        {
        }

        public EventListenerList()
        {
            _listeners = Empty.Array<WeakEventListenerWrapper>();
        }

        #endregion

        #region Properties

        internal bool IsEmpty => _listeners == null;

        #endregion

        #region Methods

        public static EventListenerList GetOrAdd(object item, string path)
        {
            return ServiceProvider.AttachedValueProvider.GetOrAdd(item, path, (o, o1) => new EventListenerList(), null);
        }

        public static void Raise(object item, string path, object message)
        {
            ServiceProvider.AttachedValueProvider.GetValue<EventListenerList>(item, path, false)?.Raise(item, message);
        }

        [Preserve(Conditional = true)]
        public void Raise<TArg>(object sender, TArg args)
        {
            bool hasDeadRef = false;
            WeakEventListenerWrapper[] listeners = _listeners;
            for (int i = 0; i < listeners.Length; i++)
            {
                if (i >= _size)
                    break;
                if (!listeners[i].EventListener.TryHandle(sender, args))
                    hasDeadRef = true;
            }
            if (hasDeadRef)
            {
                //it's normal here.
                lock (this)
                    Cleanup();
            }
        }

        public void Add(IEventListener target)
        {
            AddInternal(target.ToWeakWrapper(), false);
        }

        public IDisposable AddWithUnsubscriber(IEventListener target)
        {
            return AddInternal(target.ToWeakWrapper(), true);
        }

        public void Remove(IEventListener listener)
        {
            if (!listener.IsWeak)
            {
                Remove(listener.ToWeakWrapper());
                return;
            }
            //it's normal here.
            lock (this)
            {
                for (int i = 0; i < _listeners.Length; i++)
                {
                    if (ReferenceEquals(_listeners[i].EventListener, listener))
                    {
                        RemoveAt(i);
                        return;
                    }
                }
            }
        }

        public void Clear()
        {
            lock (this)
            {
                _listeners = Empty.Array<WeakEventListenerWrapper>();
                _size = 0;
                _removedSize = 0;
            }
        }

        private IDisposable AddInternal(WeakEventListenerWrapper weakItem, bool withUnsubscriber)
        {
            lock (this)
            {
                if (_listeners.Length == 0)
                {
                    _listeners = new[] { weakItem };
                    _size = 1;
                    _removedSize = 0;
                }
                else
                {
                    if (_removedSize == 0)
                    {
                        if (_size == _listeners.Length)
                            EnsureCapacity(ref _listeners, _size, _size + 1);
                        _listeners[_size++] = weakItem;
                    }
                    else
                    {
                        for (int i = 0; i < _size; i++)
                        {
                            if (_listeners[i].IsEmpty)
                            {
                                _listeners[i] = weakItem;
                                --_removedSize;
                                break;
                            }
                        }
                    }
                }
            }
            if (withUnsubscriber)
                return new Unsubscriber(this, weakItem);
            return null;
        }

        private void Remove(WeakEventListenerWrapper weakItem)
        {
            //it's normal here.
            lock (this)
            {
                for (int i = 0; i < _listeners.Length; i++)
                {
                    var wrapper = _listeners[i];
                    if (!wrapper.IsEmpty && ReferenceEquals(wrapper.Source, weakItem.Source))
                    {
                        RemoveAt(i);
                        return;
                    }
                }
            }
        }

        private void RemoveAt(int index)
        {
            ++_removedSize;
            _listeners[index] = WeakEventListenerWrapper.Empty;
        }

        private void Cleanup()
        {
            var size = _size;
            _size = 0;
            _removedSize = 0;
            for (int i = 0; i < size; i++)
            {
                var reference = _listeners[i];
                if (reference.EventListener.IsAlive)
                    _listeners[_size++] = reference;
            }
            if (_size == 0)
                _listeners = Empty.Array<WeakEventListenerWrapper>();
            else if (_listeners.Length / (float)_size > 2)
            {
                var listeners = new WeakEventListenerWrapper[_size + (_size >> 2)];
                Array.Copy(_listeners, 0, listeners, 0, _size);
                _listeners = listeners;
            }
        }

        internal static void EnsureCapacity<T>(ref T[] listeners, int size, int min)
        {
            if (listeners.Length >= min)
                return;
            var length = listeners.Length;
            if (length <= 4)
                ++length;
            else
                length = length + (length >> 2);
            if (length > 0)
            {
                var objArray = new T[length];
                if (size > 0)
                    Array.Copy(listeners, 0, objArray, 0, size);
                listeners = objArray;
            }
            else
                listeners = Empty.Array<T>();
        }

        #endregion
    }
}