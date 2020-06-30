using System;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Observation
{
    public class MemberListenerCollection : ActionToken.IHandler
    {
        #region Fields

        private WeakEventListener<string>[] _listeners;
        private ushort _removedSize;
        private ushort _size;

        #endregion

        #region Constructors

        public MemberListenerCollection()
        {
            _listeners = Default.Array<WeakEventListener<string>>();
        }

        #endregion

        #region Properties

        public int Count => _size - _removedSize;

        #endregion

        #region Implementation of interfaces

        void ActionToken.IHandler.Invoke(object? target, object? state)
        {
            var propertyName = (string)state!;
            var listeners = _listeners;
            var size = _size;
            for (var i = 0; i < size; i++)
            {
                var listener = listeners[i];
                if (ReferenceEquals(listener.Target, target) && listener.State == propertyName)
                {
                    if (RemoveAt(listeners, i))
                        TrimIfNeed();
                    break;
                }
            }
        }

        #endregion

        #region Methods

        public void Raise<T>(object? sender, in T message, string memberName, IReadOnlyMetadataContext? metadata)
        {
            var hasDeadRef = false;
            var listeners = _listeners;
            var size = _size;
            for (var i = 0; i < size; i++)
            {
                var listener = listeners[i];
                if (!listener.IsEmpty && MugenExtensions.MemberNameEqual(memberName, listener.State, true) && !listener.TryHandle(sender, message, metadata) && RemoveAt(listeners, i))
                    hasDeadRef = true;
            }

            if (hasDeadRef)
                TrimIfNeed();
        }

        public ActionToken Add(IEventListener target, string memberName)
        {
            var weakItem = target.ToWeak(memberName);
            if (_removedSize == 0)
            {
                if (_size == _listeners.Length)
                    Array.Resize(ref _listeners, _size + 2);
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

            if (_size - _removedSize == 1)
                OnListenersAdded();
            OnListenerAdded(memberName);
            return new ActionToken(this, weakItem.Target, memberName);
        }

        public void Clear()
        {
            if (_size == 0)
                return;
            _listeners = Default.Array<WeakEventListener<string>>();
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

        protected virtual void OnListenerAdded(string memberName)
        {
        }

        protected virtual void OnListenerRemoved(string memberName)
        {
        }

        private bool RemoveAt(WeakEventListener<string>[] listeners, int index)
        {
            if (!ReferenceEquals(listeners, _listeners))
                return false;

            var listener = listeners[index];
            if (!listener.IsEmpty)
                OnListenerRemoved(listener.State);
            listeners[index] = default;
            if (index == _size - 1)
                --_size;
            else
                ++_removedSize;
            return true;
        }

        private void TrimIfNeed()
        {
            TrimIfNeedInternal();
            if (_size - _removedSize == 0)
                OnListenersRemoved();
        }

        private void TrimIfNeedInternal()
        {
            if (_size == _removedSize)
            {
                _size = 0;
                _removedSize = 0;
                _listeners = Default.Array<WeakEventListener<string>>();
                return;
            }

            if (_listeners.Length / (float)(_size - _removedSize) <= 2)
                return;

            var size = _size;
            _size = 0;
            _removedSize = 0;
            for (var i = 0; i < size; i++)
            {
                var listener = _listeners[i];
                _listeners[i] = default;
                if (listener.IsAlive)
                    _listeners[_size++] = listener;
                else if (!listener.IsEmpty)
                    OnListenerRemoved(listener.State);
            }

            if (_size == 0)
                _listeners = Default.Array<WeakEventListener<string>>();
            else
            {
                var capacity = _size + 1;
                if (size != capacity)
                    Array.Resize(ref _listeners, capacity);
            }
        }

        #endregion
    }
}