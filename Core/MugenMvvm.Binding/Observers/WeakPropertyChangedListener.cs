using System;
using System.ComponentModel;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Extensions;

namespace MugenMvvm.Binding.Observers
{
    public sealed class WeakPropertyChangedListener : ActionToken.IHandler
    {
        #region Fields

        private WeakEventListener<string>[] _listeners;
        private ushort _removedSize;
        private ushort _size;

        #endregion

        #region Constructors

        public WeakPropertyChangedListener()
        {
            _listeners = Default.EmptyArray<WeakEventListener<string>>();
        }

        #endregion

        #region Implementation of interfaces

        void ActionToken.IHandler.Invoke(object? target, object? state)
        {
            var propertyName = (string) state!;
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

        public void Raise(object sender, PropertyChangedEventArgs args)
        {
            var hasDeadRef = false;
            var listeners = _listeners;
            var size = _size;
            for (var i = 0; i < size; i++)
            {
                var listener = listeners[i];
                if (!listener.IsEmpty && MugenExtensions.MemberNameEqual(args.PropertyName, listener.State, true) && !listener.TryHandle(sender, args) && RemoveAt(listeners, i))
                    hasDeadRef = true;
            }

            if (hasDeadRef)
                TrimIfNeed();
        }

        public ActionToken Add(IEventListener target, string path)
        {
            var weakItem = target.ToWeak(path);
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

            return new ActionToken(this, weakItem.Target, path);
        }

        private bool RemoveAt(WeakEventListener<string>[] listeners, int index)
        {
            if (!ReferenceEquals(listeners, _listeners))
                return false;

            listeners[index] = default;
            if (index == _size - 1)
                --_size;
            else
                ++_removedSize;
            return true;
        }

        private void TrimIfNeed()
        {
            if (_size == _removedSize)
            {
                _size = 0;
                _removedSize = 0;
                _listeners = Default.EmptyArray<WeakEventListener<string>>();
                return;
            }

            if (_listeners.Length / (float) (_size - _removedSize) <= 2)
                return;

            var size = _size;
            _size = 0;
            _removedSize = 0;
            for (var i = 0; i < size; i++)
            {
                var reference = _listeners[i];
                _listeners[i] = default;
                if (reference.IsAlive)
                    _listeners[_size++] = reference;
            }

            if (_size == 0)
            {
                _listeners = Default.EmptyArray<WeakEventListener<string>>();
                return;
            }

            var capacity = _size + 1;
            if (size != capacity)
                Array.Resize(ref _listeners, capacity);
        }

        #endregion
    }
}