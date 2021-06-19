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
        private int _version;

        public CommandEventHandler(IThreadDispatcher? threadDispatcher, ThreadExecutionMode eventExecutionMode)
        {
            Should.NotBeNull(eventExecutionMode, nameof(eventExecutionMode));
            _threadDispatcher = threadDispatcher;
            EventExecutionMode = eventExecutionMode;
            _version = -BoxingExtensions.CacheSize;
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
            {
                var dispatcher = _threadDispatcher.DefaultIfNull();
                if (dispatcher.CanExecuteInline(EventExecutionMode))
                    Raise();
                else
                    dispatcher.Execute(EventExecutionMode, this, BoxingExtensions.Box(Interlocked.Increment(ref _version)));
            }
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
            {
                _isNotificationsDirty = false;
                RaiseCanExecuteChanged();
            }
        }

        private void Raise()
        {
            foreach (var owner in Owners)
                _canExecuteChanged?.Invoke(owner, EventArgs.Empty);
        }

        void IThreadDispatcherHandler.Execute(object? v)
        {
            if (_version == (int)v!)
                Raise();
        }
    }
}