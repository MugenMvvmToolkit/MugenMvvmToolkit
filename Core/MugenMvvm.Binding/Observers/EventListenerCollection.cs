using System;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Binding.Observers
{
    public class EventListenerCollection : Unsubscriber.IHandler
    {
        #region Fields

        private object? _listeners;
        private ushort _removedSize;
        private ushort _size;

        #endregion

        #region Implementation of interfaces

        void Unsubscriber.IHandler.Unsubscribe(object? state1, object? state2)
        {
            if (ReferenceEquals(_listeners, state1))
            {
                _listeners = null;
                _size = 0;
                _removedSize = 0;
            }
            else if (_listeners is object?[] listeners)
            {
                for (var i = 0; i < listeners.Length; i++)
                {
                    if (ReferenceEquals(listeners[i], state1))
                    {
                        ++_removedSize;
                        listeners[i] = null;
                        return;
                    }
                }
            }
        }

        #endregion

        #region Methods

        public static EventListenerCollection GetOrAdd(object item, string path, IAttachedValueManager? valueManager = null)
        {
            return valueManager.ServiceIfNull().GetOrAdd(item, path, (object?)null, (object?)null, (_, __, ___) => new EventListenerCollection());
        }

        public static void Raise(object item, string path, object message, IAttachedValueManager? valueManager = null)
        {
            valueManager.ServiceIfNull().TryGetValue(item, path, out EventListenerCollection collection);
            collection?.Raise(item, message);
        }

        [Preserve(Conditional = true)]
        public void Raise<TArg>(object sender, TArg args)
        {
            if (_listeners is object[] listeners)
            {
                var hasDeadRef = false;
                for (var i = 0; i < listeners.Length; i++)
                {
                    if (i >= _size)
                        break;
                    if (!WeakEventListener.TryHandle(listeners[i], sender, args))
                        hasDeadRef = true;
                }

                if (hasDeadRef)
                    Cleanup(listeners);
            }
            else
                WeakEventListener.TryHandle(_listeners, sender, args);
        }

        public Unsubscriber Add(IEventListener listener)
        {
            var target = WeakEventListener.GetTarget(listener);
            if (_listeners == null)
            {
                _listeners = target;
                _size = 1;
                _removedSize = 0;
            }
            else
            {
                if (_listeners is object[] listeners)
                {
                    if (_removedSize == 0)
                    {
                        if (_size == listeners.Length)
                        {
                            EnsureCapacity(ref listeners, _size, _size + 1);
                            _listeners = listeners;
                        }

                        listeners[_size++] = target;
                    }
                    else
                    {
                        for (var i = 0; i < _size; i++)
                        {
                            if (listeners[i] == null)
                            {
                                listeners[i] = target;
                                --_removedSize;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    _listeners = new[] { _listeners, target };
                    _size = 2;
                    _removedSize = 0;
                }
            }

            return new Unsubscriber(this, target, null);
        }

        public bool Remove(IEventListener listener)
        {
            if (listener == null)
                return false;

            if (_listeners is object?[] listeners)
            {
                for (var i = 0; i < listeners.Length; i++)
                {
                    if (ReferenceEquals(WeakEventListener.GetListener(listeners[i]), listener))
                    {
                        ++_removedSize;
                        listeners[i] = null;
                        return true;
                    }
                }
            }
            else if (ReferenceEquals(WeakEventListener.GetListener(_listeners), listener))
            {
                _listeners = null;
                _size = 0;
                _removedSize = 0;
                return true;
            }

            return false;
        }

        public void Clear()
        {
            _listeners = null;
            _size = 0;
            _removedSize = 0;
        }

        private void Cleanup(object[] listeners)
        {
            var size = _size;
            _size = 0;
            _removedSize = 0;
            for (var i = 0; i < size; i++)
            {
                var reference = listeners[i];
                if (WeakEventListener.GetIsAlive(reference))
                    listeners[_size++] = reference;
            }

            if (_size == 0)
                _listeners = null;
            else if (listeners.Length / (float)_size > 2)
            {
                var array = new object[_size + (_size >> 2)];
                Array.Copy(listeners, 0, array, 0, _size);
                _listeners = array;
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
    }
}