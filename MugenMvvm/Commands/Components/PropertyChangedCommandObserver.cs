using System;
using System.ComponentModel;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Commands.Components
{
    public sealed class PropertyChangedCommandObserver : CommandObserverBase<INotifyPropertyChanged>, IHasPriority
    {
        private PropertyChangedEventHandler? _handler;

        public Func<object?, PropertyChangedEventArgs, bool>? CanNotify { get; set; }

        public int Priority => CommandComponentPriority.PropertyChangedObserver;

        private PropertyChangedEventHandler PropertyChangedEventHandler => _handler ??= Handle;

        protected override void OnAdded(INotifyPropertyChanged notifier) => notifier.PropertyChanged += PropertyChangedEventHandler;

        protected override void OnRemoved(INotifyPropertyChanged notifier) => notifier.PropertyChanged -= PropertyChangedEventHandler;

        protected override void OnDisposed() => _handler = null;

        private void Handle(object? sender, PropertyChangedEventArgs message)
        {
            if (CanNotify == null || CanNotify(sender, message))
                RaiseCanExecuteChanged();
        }
    }
}