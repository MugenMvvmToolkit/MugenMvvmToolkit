using System;
using MugenMvvm.Attributes;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Observers
{
    public class EventListenerCollection
    {
        #region Fields

        private object? _listeners;
        private ushort _removedSize;
        private ushort _size;

        #endregion

        #region Properties

        public bool HasListeners => _size - _removedSize > 0;

        #endregion

        #region Methods

        public static EventListenerCollection GetOrAdd(object target, string path, IAttachedValueProvider? attachedValueProvider = null)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(path, nameof(path));
            return attachedValueProvider.DefaultIfNull().GetOrAdd(target, path, (object?)null, (_, __) => new EventListenerCollection());
        }

        public static void Raise<T>(object target, string path, in T message, IReadOnlyMetadataContext? metadata, IAttachedValueProvider? attachedValueProvider = null)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(path, nameof(path));
            attachedValueProvider.DefaultIfNull().TryGet(target, path, out EventListenerCollection? collection);
            collection?.Raise(target, message, metadata);
        }

        public void Raise<TArg>(object? sender, in TArg args, IReadOnlyMetadataContext? metadata)
        {
            if (_listeners is object?[] listeners)
            {
                var hasDeadRef = false;
                var size = _size;
                for (var i = 0; i < size; i++)
                {
                    if (!WeakEventListener.TryHandle(listeners[i], sender, args, metadata) && RemoveAt(listeners, i))
                        hasDeadRef = true;
                }

                if (hasDeadRef && ReferenceEquals(_listeners, listeners))
                    TrimIfNeed(listeners);
            }
            else if (_listeners != null && !WeakEventListener.TryHandle(_listeners, sender, args, metadata))
            {
                _listeners = null;
                _size = 0;
                _removedSize = 0;
                OnListenersRemoved();
            }
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

            if (_size - _removedSize == 1)
                OnListenersAdded();
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
                OnListenersRemoved();
                return true;
            }

            return false;
        }

        public void Clear()
        {
            if (_size == 0)
                return;
            _listeners = null;
            _size = 0;
            _removedSize = 0;
            OnListenersRemoved();
        }

        protected virtual void OnListenersAdded()
        {
        }

        protected virtual void OnListenersRemoved()
        {
        }

        private void Unsubscribe(object? target)
        {
            if (ReferenceEquals(_listeners, target))
            {
                _listeners = null;
                _size = 0;
                _removedSize = 0;
                OnListenersRemoved();
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

        private bool RemoveAt(object?[] listeners, int index)
        {
            if (!ReferenceEquals(listeners, _listeners) || listeners[index] == null)
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
            TrimIfNeedInternal(listeners);
            if (_size - _removedSize == 0)
                OnListenersRemoved();
        }

        private void TrimIfNeedInternal(object?[] listeners)
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
            if (size != capacity)
            {
                Array.Resize(ref listeners, capacity);
                _listeners = listeners;
            }
        }

        #endregion
    }
}