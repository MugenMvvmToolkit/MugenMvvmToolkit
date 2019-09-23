using System;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Binding.Observers
{
    public sealed class EventListenerCollection//todo opt
    {
        #region Fields

        private WeakEventListener[] _listeners;
        private ushort _removedSize;
        private ushort _size;

        #endregion

        #region Constructors

        public EventListenerCollection()
        {
            _listeners = Default.EmptyArray<WeakEventListener>();
        }

        #endregion

        #region Methods

        public static EventListenerCollection GetOrAdd(object item, string path, IAttachedDictionaryProvider? provider = null)
        {
            return provider.ServiceIfNull().GetOrAdd(item, path, (object?)null, (object?)null, (_, __, ___) => new EventListenerCollection());
        }

        public static void Raise(object item, string path, object message, IAttachedDictionaryProvider? provider = null)
        {
            provider.ServiceIfNull().TryGetValue(item, path, out EventListenerCollection collection);
            collection?.Raise(item, message);
        }

        [Preserve(Conditional = true)]
        public void Raise<TArg>(object sender, TArg args)
        {
            var hasDeadRef = false;
            var listeners = _listeners;
            for (var i = 0; i < listeners.Length; i++)
            {
                if (i >= _size)
                    break;
                if (!listeners[i].TryHandle(sender, args))
                    hasDeadRef = true;
            }

            if (hasDeadRef)
            {
                lock (this)
                {
                    Cleanup();
                }
            }
        }

        public void Add(IEventListener target)
        {
            AddInternal(target.ToWeak(), false);
        }

        public IDisposable AddWithUnsubscriber(IEventListener target)
        {
            return AddInternal(target.ToWeak(), true)!;
        }

        public void Remove(IEventListener listener)
        {
            if (!listener.IsWeak)
            {
                Remove(listener.ToWeak());
                return;
            }

            lock (this)//todo review locks for binding
            {
                for (var i = 0; i < _listeners.Length; i++)
                {
                    if (ReferenceEquals(_listeners[i].Listener, listener))
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
                _listeners = Default.EmptyArray<WeakEventListener>();
                _size = 0;
                _removedSize = 0;
            }
        }

        private IDisposable? AddInternal(WeakEventListener weakItem, bool withUnsubscriber)
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
                        for (var i = 0; i < _size; i++)
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

        private void Remove(WeakEventListener weakItem)
        {
            lock (this)
            {
                for (var i = 0; i < _listeners.Length; i++)
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
            _listeners[index] = default;
        }

        private void Cleanup()
        {
            var size = _size;
            _size = 0;
            _removedSize = 0;
            for (var i = 0; i < size; i++)
            {
                var reference = _listeners[i];
                if (reference.IsAlive)
                    _listeners[_size++] = reference;
            }

            if (_size == 0)
                _listeners = Default.EmptyArray<WeakEventListener>();
            else if (_listeners.Length / (float)_size > 2)
            {
                var listeners = new WeakEventListener[_size + (_size >> 2)];
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
                listeners = Default.EmptyArray<T>();
        }

        #endregion

        #region Nested types

        private sealed class Unsubscriber : IDisposable
        {
            #region Fields

            private EventListenerCollection? _eventListener;
            private WeakEventListener _weakItem;

            #endregion

            #region Constructors

            public Unsubscriber(EventListenerCollection eventListener, WeakEventListener weakItem)
            {
                _eventListener = eventListener;
                _weakItem = weakItem;
            }

            #endregion

            #region Implementation of interfaces

            public void Dispose()
            {
                var listener = _eventListener;
                var weakItem = _weakItem;
                if (listener != null && !weakItem.IsEmpty)
                {
                    _eventListener = null;
                    _weakItem = default;
                    listener.Remove(weakItem);
                }
            }

            #endregion
        }

        #endregion
    }
}