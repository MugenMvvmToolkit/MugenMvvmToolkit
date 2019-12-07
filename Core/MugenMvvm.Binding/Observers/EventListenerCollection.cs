using System;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Binding.Observers
{
    public class EventListenerCollection
    {
        #region Fields

        private object? _listeners;
        private ushort _removedSize;
        private ushort _size;

        #endregion

        #region Methods

        public static EventListenerCollection GetOrAdd(object item, string path, IAttachedValueManager? valueManager = null)
        {
            return valueManager.DefaultIfNull().GetOrAdd(item, path, (object?)null, (_, __) => new EventListenerCollection());
        }

        public static void Raise(object item, string path, object message, IAttachedValueManager? valueManager = null)
        {
            valueManager.DefaultIfNull().TryGetValue(item, path, out EventListenerCollection collection);
            collection?.Raise(item, message);
        }

        [Preserve(Conditional = true)]
        public void Raise<TArg>(object sender, TArg args)
        {
            if (_listeners is object?[] listeners)
            {
                var hasDeadRef = false;
                var size = _size;
                for (var i = 0; i < size; i++)
                {
                    if (!WeakEventListener.TryHandle(listeners[i], sender, args) && RemoveAt(listeners, i))
                        hasDeadRef = true;
                }

                if (hasDeadRef && ReferenceEquals(_listeners, listeners))
                    TrimIfNeed(listeners);
            }
            else
                WeakEventListener.TryHandle(_listeners, sender, args);
        }

        public ActionToken Add(IEventListener listener)
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
                if (_listeners is object?[] listeners)
                {
                    if (_removedSize == 0)
                    {
                        if (_size == listeners.Length)
                        {
                            Array.Resize(ref listeners, _size + 2);
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
                    _listeners = new[] { _listeners, target, null };
                    _size = 2;
                    _removedSize = 0;
                }
            }

            return new ActionToken((@this, t) => ((EventListenerCollection)@this!).Unsubscribe(t), this, target);
        }

        public bool Remove(IEventListener listener)
        {
            if (listener == null)
                return false;

            if (_listeners is object?[] listeners)
            {
                var size = _size;
                for (var i = 0; i < size; i++)
                {
                    if (ReferenceEquals(WeakEventListener.GetListener(listeners[i]), listener))
                    {
                        if (RemoveAt(listeners, i))
                            TrimIfNeed(listeners);
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

        private bool RemoveAt(object?[] listeners, int index)
        {
            if (!ReferenceEquals(listeners, _listeners))
                return false;

            listeners[index] = null;
            if (index == _size - 1)
                --_size;
            else
                ++_removedSize;
            return true;
        }

        private void TrimIfNeed(object?[] listeners)
        {
            if (_size == _removedSize)
            {
                _size = 0;
                _removedSize = 0;
                _listeners = null;
                return;
            }

            if (listeners.Length / (float)(_size - _removedSize) <= 2)
                return;

            var size = _size;
            _size = 0;
            _removedSize = 0;
            for (var i = 0; i < size; i++)
            {
                var reference = listeners[i];
                listeners[i] = null;
                if (WeakEventListener.GetIsAlive(reference))
                    listeners[_size++] = reference;
            }

            if (_size == 0)
            {
                _listeners = null;
                return;
            }

            var capacity = _size + 1;
            if (size == capacity)
                return;

            Array.Resize(ref listeners, capacity);
            _listeners = listeners;
        }

        private void Unsubscribe(object? target)
        {
            if (ReferenceEquals(_listeners, target))
            {
                _listeners = null;
                _size = 0;
                _removedSize = 0;
            }
            else if (_listeners is object?[] listeners)
            {
                var size = _size;
                for (var i = 0; i < size; i++)
                {
                    var t = listeners[i];
                    if (ReferenceEquals(target, t))
                    {
                        if (RemoveAt(listeners, i))
                            TrimIfNeed(listeners);
                        break;
                    }
                }
            }
        }

        #endregion
    }
}