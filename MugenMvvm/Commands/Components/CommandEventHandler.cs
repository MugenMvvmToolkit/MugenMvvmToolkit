using System;
using System.Threading;
using MugenMvvm.Constants;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Commands.Components
{
    public sealed class CommandEventHandler : ICommandEventHandlerComponent, ISuspendableComponent<ICompositeCommand>, IHasPriority, IDisposableComponent<ICompositeCommand>
    {
        private EventHandler? _canExecuteChanged;
        private bool _isNotificationsDirty;
        private volatile int _suspendCount;

        public int Priority => CommandComponentPriority.ConditionEvent;

        public void AddCanExecuteChanged(ICompositeCommand command, EventHandler? handler, IReadOnlyMetadataContext? metadata) => _canExecuteChanged += handler;

        public void RemoveCanExecuteChanged(ICompositeCommand command, EventHandler? handler, IReadOnlyMetadataContext? metadata) => _canExecuteChanged -= handler;

        public void RaiseCanExecuteChanged(ICompositeCommand command, IReadOnlyMetadataContext? metadata)
        {
            if (_canExecuteChanged == null)
                return;

            if (_suspendCount == 0)
                _canExecuteChanged?.Invoke(command, EventArgs.Empty);
            else
                _isNotificationsDirty = true;
        }

        private void EndSuspendNotifications(ICompositeCommand command)
        {
            if (Interlocked.Decrement(ref _suspendCount) == 0 && _isNotificationsDirty)
            {
                _isNotificationsDirty = false;
                RaiseCanExecuteChanged(command, null);
            }
        }

        void IDisposableComponent<ICompositeCommand>.Dispose(ICompositeCommand owner, IReadOnlyMetadataContext? metadata) => _canExecuteChanged = null;

        bool ISuspendableComponent<ICompositeCommand>.IsSuspended(ICompositeCommand owner, IReadOnlyMetadataContext? metadata) => _suspendCount != 0;

        ActionToken ISuspendableComponent<ICompositeCommand>.TrySuspend(ICompositeCommand owner, object? state, IReadOnlyMetadataContext? metadata)
        {
            Interlocked.Increment(ref _suspendCount);
            return ActionToken.FromDelegate((o, c) => ((CommandEventHandler) o!).EndSuspendNotifications((ICompositeCommand) c!), this, owner);
        }
    }
}