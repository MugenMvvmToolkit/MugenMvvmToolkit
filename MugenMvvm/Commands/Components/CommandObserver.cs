using System;
using System.Windows.Input;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Commands.Components
{
    public sealed class CommandObserver : CommandObserverBase<ICommand>, IHasPriority
    {
        private EventHandler? _handler;

        public int Priority => CommandComponentPriority.CommandObserver;

        private EventHandler Handler => _handler ??= Handle;

        protected override void OnAdded(ICommand notifier) => notifier.CanExecuteChanged += Handler;

        protected override void OnRemoved(ICommand notifier) => notifier.CanExecuteChanged -= Handler;

        protected override void OnDisposed() => _handler = null;

        private void Handle(object? sender, EventArgs? e) => RaiseCanExecuteChanged();
    }
}