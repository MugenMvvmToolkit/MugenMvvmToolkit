using System;
using System.Collections.Generic;
using System.ComponentModel;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Commands.Components
{
    public sealed class PropertyChangedCommandObserver : MultiAttachableComponentBase<ICompositeCommand>, IHasPriority, IDisposableComponent<ICompositeCommand>
    {
        private readonly HashSet<INotifyPropertyChanged> _observers;
        private PropertyChangedEventHandler? _handler;

        public PropertyChangedCommandObserver()
        {
            _observers = new HashSet<INotifyPropertyChanged>(InternalEqualityComparer.Reference);
        }

        public Func<object?, PropertyChangedEventArgs, bool>? CanNotify { get; set; }

        public int Priority => CommandComponentPriority.PropertyChangedObserver;

        private PropertyChangedEventHandler PropertyChangedEventHandler => _handler ??= Handle;

        public bool Add(INotifyPropertyChanged notifier)
        {
            Should.NotBeNull(notifier, nameof(notifier));
            lock (_observers)
            {
                if (!_observers.Add(notifier))
                    return false;
                notifier.PropertyChanged += PropertyChangedEventHandler;
                return true;
            }
        }

        public bool Contains(INotifyPropertyChanged notifier)
        {
            Should.NotBeNull(notifier, nameof(notifier));
            lock (_observers)
            {
                return _observers.Contains(notifier);
            }
        }

        public bool Remove(INotifyPropertyChanged notifier)
        {
            Should.NotBeNull(notifier, nameof(notifier));
            lock (_observers)
            {
                if (!_observers.Remove(notifier))
                    return false;
                notifier.PropertyChanged -= PropertyChangedEventHandler;
                return true;
            }
        }

        private void Handle(object? sender, PropertyChangedEventArgs message)
        {
            if (CanNotify == null || CanNotify(sender, message))
            {
                foreach (var owner in Owners)
                    owner.RaiseCanExecuteChanged();
            }
        }

        void IDisposableComponent<ICompositeCommand>.OnDisposing(ICompositeCommand owner, IReadOnlyMetadataContext? metadata)
        {
        }

        void IDisposableComponent<ICompositeCommand>.OnDisposed(ICompositeCommand owner, IReadOnlyMetadataContext? metadata)
        {
            lock (_observers)
            {
                foreach (var observer in _observers)
                    observer.PropertyChanged -= PropertyChangedEventHandler;
                _observers.Clear();
            }

            _handler = null;
        }
    }
}