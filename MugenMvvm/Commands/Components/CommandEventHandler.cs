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
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Internal;

namespace MugenMvvm.Commands.Components
{
    public sealed class CommandEventHandler : MultiAttachableComponentBase<ICompositeCommand>, ICommandEventHandlerComponent, IThreadDispatcherHandler, IValueHolder<Delegate>,
        ISuspendableComponent<ICompositeCommand>, IHasPriority, IDisposableComponent<ICompositeCommand>
    {
        private readonly IThreadDispatcher? _threadDispatcher;
        private EventHandler? _canExecuteChanged;
        private bool _isNotificationsDirty;
        private volatile int _suspendCount;
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

        Delegate? IValueHolder<Delegate>.Value { get; set; }

        public ActionToken Suspend()
        {
            Interlocked.Increment(ref _suspendCount);
            return ActionToken.FromDelegate((o, _) => ((CommandEventHandler)o!).EndSuspendNotifications(), this);
        }

        public void AddCanExecuteChanged(ICompositeCommand command, EventHandler? handler, IReadOnlyMetadataContext? metadata) => _canExecuteChanged += handler;

        public void RemoveCanExecuteChanged(ICompositeCommand command, EventHandler? handler, IReadOnlyMetadataContext? metadata) => _canExecuteChanged -= handler;

        public void RaiseCanExecuteChanged(ICompositeCommand? command = null, IReadOnlyMetadataContext? metadata = null)
        {
            if (_canExecuteChanged == null)
                return;

            if (_suspendCount != 0)
            {
                _isNotificationsDirty = true;
                return;
            }

            var dispatcher = _threadDispatcher.DefaultIfNull();
            if (dispatcher.CanExecuteInline(EventExecutionMode))
                Raise();
            else
                dispatcher.Execute(EventExecutionMode, this, BoxingExtensions.Box(Interlocked.Increment(ref _version)));
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

        void IDisposableComponent<ICompositeCommand>.Dispose(ICompositeCommand owner, IReadOnlyMetadataContext? metadata) => _canExecuteChanged = null;

        bool ISuspendableComponent<ICompositeCommand>.IsSuspended(ICompositeCommand owner, IReadOnlyMetadataContext? metadata) => _suspendCount != 0;

        ActionToken ISuspendableComponent<ICompositeCommand>.TrySuspend(ICompositeCommand owner, object? state, IReadOnlyMetadataContext? metadata) => Suspend();

        void IThreadDispatcherHandler.Execute(object? v)
        {
            if (_version == (int)v!)
                Raise();
        }
    }
}