using System.Collections.Generic;
using MugenMvvm.Components;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Commands.Components
{
    public abstract class CommandObserverBase<T> : MultiAttachableComponentBase<ICompositeCommand>, IDisposableComponent<ICompositeCommand> where T : class
    {
        private readonly HashSet<T> _observers;

        protected CommandObserverBase()
        {
            _observers = new HashSet<T>(InternalEqualityComparer.Reference);
        }

        public bool Add(T notifier)
        {
            Should.NotBeNull(notifier, nameof(notifier));
            lock (_observers)
            {
                if (!_observers.Add(notifier))
                    return false;
                OnAdded(notifier);
                return true;
            }
        }

        public bool Contains(T notifier)
        {
            Should.NotBeNull(notifier, nameof(notifier));
            lock (_observers)
            {
                return _observers.Contains(notifier);
            }
        }

        public bool Remove(T notifier)
        {
            Should.NotBeNull(notifier, nameof(notifier));
            lock (_observers)
            {
                if (!_observers.Remove(notifier))
                    return false;
                OnRemoved(notifier);
                return true;
            }
        }

        protected abstract void OnAdded(T notifier);

        protected abstract void OnRemoved(T notifier);

        protected virtual void OnDisposed()
        {
        }

        protected void RaiseCanExecuteChanged()
        {
            foreach (var owner in Owners)
                owner.RaiseCanExecuteChanged();
        }

        void IDisposableComponent<ICompositeCommand>.OnDisposing(ICompositeCommand owner, IReadOnlyMetadataContext? metadata)
        {
        }

        void IDisposableComponent<ICompositeCommand>.OnDisposed(ICompositeCommand owner, IReadOnlyMetadataContext? metadata)
        {
            lock (_observers)
            {
                foreach (var observer in _observers)
                    OnRemoved(observer);
                _observers.Clear();
            }

            OnDisposed();
        }
    }
}