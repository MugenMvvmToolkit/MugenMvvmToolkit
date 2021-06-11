using System;
using System.Threading;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Internal;

namespace MugenMvvm.Commands.Components
{
    public sealed class CommandEventHandler : MultiAttachableComponentBase<ICompositeCommand>, ICommandEventHandlerComponent, IThreadDispatcherHandler, IValueHolder<Delegate>,
        ISuspendable, IHasPriority, IDisposable
    {
        private readonly IThreadDispatcher? _threadDispatcher;
        private EventHandler? _canExecuteChanged;
        private bool _isNotificationsDirty;
        private int _suspendCount;

        public CommandEventHandler(IThreadDispatcher? threadDispatcher, ThreadExecutionMode eventExecutionMode)
        {
            Should.NotBeNull(eventExecutionMode, nameof(eventExecutionMode));
            _threadDispatcher = threadDispatcher;
            EventExecutionMode = eventExecutionMode;
        }

        public ThreadExecutionMode EventExecutionMode { get; }

        public int Priority => CommandComponentPriority.ConditionEvent;

        public bool IsSuspended => _suspendCount != 0;

        Delegate? IValueHolder<Delegate>.Value { get; set; }

        public void AddCanExecuteChanged(ICompositeCommand command, EventHandler? handler, IReadOnlyMetadataContext? metadata) => _canExecuteChanged += handler;

        public void RemoveCanExecuteChanged(ICompositeCommand command, EventHandler? handler, IReadOnlyMetadataContext? metadata) => _canExecuteChanged -= handler;

        public void RaiseCanExecuteChanged(ICompositeCommand? command = null, IReadOnlyMetadataContext? metadata = null)
        {
            if (_canExecuteChanged == null)
                return;

            if (IsSuspended)
                _isNotificationsDirty = true;
            else
                _threadDispatcher.DefaultIfNull().Execute(EventExecutionMode, this, null);
        }

        public void Dispose() => _canExecuteChanged = null;

        public ActionToken Suspend(object? state = null, IReadOnlyMetadataContext? metadata = null)
        {
            Interlocked.Increment(ref _suspendCount);
            return ActionToken.FromDelegate((o, _) => ((CommandEventHandler)o!).EndSuspendNotifications(), this);
        }

        private void EndSuspendNotifications()
        {
            if (Interlocked.Decrement(ref _suspendCount) == 0 && _isNotificationsDirty)
                RaiseCanExecuteChanged();
        }

        void IThreadDispatcherHandler.Execute(object? _)
        {
            foreach (var owner in Owners)
                _canExecuteChanged?.Invoke(owner, EventArgs.Empty);
        }
    }
}