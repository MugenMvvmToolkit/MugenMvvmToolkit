using System;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Observation
{
    public class EventListenerCollection
    {
        private const int MinValueTrim = 3;
        private const int MaxValueTrim = 100;

        private object? _listeners;
        private bool _raising;
        private ushort _removedSize;
        private ushort _size;

        public int Count => _size - _removedSize;

        public static EventListenerCollection GetOrAdd(object target, string path, IAttachedValueManager? attachedValueManager = null)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(path, nameof(path));
            return target.AttachedValues(attachedValueManager: attachedValueManager).GetOrAdd(path, target, (_, __) => new EventListenerCollection())!;
        }

        public static void Raise(object target, string path, object? message, IReadOnlyMetadataContext? metadata, IAttachedValueManager? attachedValueManager = null)
        {
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(path, nameof(path));
            if (target.AttachedValues(metadata, attachedValueManager).TryGet(path, out var collection))
                ((EventListenerCollection) collection!).Raise(target, message, metadata);
        }

        internal static int GetCapacity(int size)
        {
            if (size == ushort.MaxValue)
                ExceptionManager.ThrowNotSupported("size > " + ushort.MaxValue);
            if (size < 6)
                return size + 2;
            return Math.Min((int) (size * 1.43f), ushort.MaxValue);
        }

        public void Raise(object? sender, object? args, IReadOnlyMetadataContext? metadata)
        {
            if (Count == 0)
                return;
            var raising = _raising;
            _raising = true;
            try
            {
                if (_listeners is object?[] listeners)
                {
                    var hasDeadRef = false;
                    var size = _size;
                    for (var i = 0; i < size; i++)
                        if (!WeakEventListener.TryHandle(listeners[i], sender, args, metadata) && RemoveAt(listeners, i))
                            hasDeadRef = true;

                    if (hasDeadRef && _listeners == listeners)
                        TrimIfNeed(listeners, true);
                }
                else if (_listeners != null && !WeakEventListener.TryHandle(_listeners, sender, args, metadata))
                {
                    _listeners = null;
                    _size = 0;
                    _removedSize = 0;
                    OnListenersRemoved();
                }
            }
            finally
            {
                _raising = raising;
            }
        }

        public ActionToken Add(IEventListener listener)
        {
            if (_size > MaxValueTrim && _removedSize == 0 && _listeners is object?[] l && l.Length == _size)
                ClearDeadReferences(l);

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
                            Array.Resize(ref listeners, GetCapacity(_size));
                            _listeners = listeners;
                        }

                        listeners[_size++] = target;
                    }
                    else
                    {
                        for (var i = 0; i < _size; i++)
                            if (listeners[i] == null)
                            {
                                listeners[i] = target;
                                --_removedSize;
                                break;
                            }
                    }
                }
                else
                {
                    _listeners = new[] {_listeners, target, null};
                    _size = 2;
                    _removedSize = 0;
                }
            }

            if (_size - _removedSize == 1)
                OnListenersAdded();
            return new ActionToken((@this, t) => ((EventListenerCollection) @this!).Unsubscribe(t), this, target);
        }

        public bool Remove(IEventListener? listener)
        {
            if (listener == null)
                return false;

            if (_listeners is object?[] listeners)
            {
                var size = _size;
                for (var i = 0; i < size; i++)
                    if (WeakEventListener.GetListener(listeners[i]) == listener)
                    {
                        if (RemoveAt(listeners, i))
                            TrimIfNeed(listeners, false);
                        return true;
                    }
            }
            else if (WeakEventListener.GetListener(_listeners) == listener)
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
            if (_listeners == target)
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
                    if (target == t)
                    {
                        if (RemoveAt(listeners, i))
                            TrimIfNeed(listeners, false);
                        break;
                    }
                }
            }
        }

        private void ClearDeadReferences(object?[] listeners)
        {
            var trim = false;
            for (var i = 0; i < listeners.Length; i++)
            {
                var listener = listeners[i];
                if (listener != null && !WeakEventListener.GetIsAlive(listener))
                {
                    RemoveAtInternal(listeners, i);
                    trim = true;
                }
            }

            if (trim)
                TrimIfNeed(listeners, false);
        }

        private bool RemoveAt(object?[] listeners, int index)
        {
            if (listeners != _listeners || listeners[index] == null)
                return false;

            RemoveAtInternal(listeners, index);
            return true;
        }

        private void RemoveAtInternal(object?[] listeners, int index)
        {
            listeners[index] = null;
            if (index == _size - 1)
                --_size;
            else
                ++_removedSize;
        }

        private void TrimIfNeed(object?[] listeners, bool fromRaise)
        {
            if (fromRaise || !_raising)
                TrimIfNeedInternal(listeners);
            if (_size - _removedSize == 0)
                OnListenersRemoved();
        }

        private void TrimIfNeedInternal(object?[] listeners)
        {
            if (listeners.Length <= MinValueTrim)
                return;

            if (_size == _removedSize)
            {
                _size = 0;
                _removedSize = 0;
                _listeners = null;
                return;
            }

            if (GetCapacity(_size - _removedSize) + 1 >= listeners.Length)
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

            var capacity = GetCapacity(_size);
            if (size != capacity)
            {
                Array.Resize(ref listeners, capacity);
                _listeners = listeners;
            }
        }
    }
}