using System;
using System.ComponentModel;
using System.Threading;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Internal;

namespace MugenMvvm.Commands.Components
{
    public sealed class CommandEventHandler : MultiAttachableComponentBase<ICompositeCommand>, ICommandEventHandlerComponent,
        IMessengerHandler, IThreadDispatcherHandler, IValueHolder<Delegate>, ISuspendable, IHasDisposeCondition, IHasPriority
    {
        #region Fields

        private readonly IThreadDispatcher? _threadDispatcher;
        private EventHandler? _canExecuteChanged;
        private PropertyChangedEventHandler? _handler;
        private bool _isNotificationsDirty;
        private int _suspendCount;

        #endregion

        #region Constructors

        public CommandEventHandler(IThreadDispatcher? threadDispatcher, ThreadExecutionMode eventExecutionMode)
        {
            Should.NotBeNull(eventExecutionMode, nameof(eventExecutionMode));
            _threadDispatcher = threadDispatcher;
            EventExecutionMode = eventExecutionMode;
            IsDisposable = true;
        }

        #endregion

        #region Properties

        public ThreadExecutionMode EventExecutionMode { get; }

        public Func<object?, object?, bool>? CanNotify { get; set; }

        public bool IsSuspended => _suspendCount != 0;

        public int Priority => CommandComponentPriority.ConditionEvent;

        Delegate? IValueHolder<Delegate>.Value { get; set; }

        public bool IsDisposable { get; set; }

        #endregion

        #region Implementation of interfaces

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

        public void Dispose()
        {
            if (IsDisposable)
            {
                _canExecuteChanged = null;
                _handler = null;
            }
        }

        bool IMessengerHandler.CanHandle(Type messageType) => true;

        MessengerResult IMessengerHandler.Handle(IMessageContext messageContext)
        {
            Handle(messageContext.Sender, messageContext.Message);
            return MessengerResult.Handled;
        }

        public ActionToken Suspend(object? state = null, IReadOnlyMetadataContext? metadata = null)
        {
            Interlocked.Increment(ref _suspendCount);
            return new ActionToken((o, _) => ((CommandEventHandler) o!).EndSuspendNotifications(), this);
        }

        void IThreadDispatcherHandler.Execute(object? _)
        {
            foreach (var owner in Owners)
                _canExecuteChanged?.Invoke(owner, EventArgs.Empty);
        }

        #endregion

        #region Methods

        public ActionToken AddNotifier(object? notifier, IReadOnlyMetadataContext? metadata = null)
        {
            if (notifier is IHasService<IMessenger> hasMessenger)
                return AddNotifier(hasMessenger.Service, metadata);

            if (notifier is IMessenger messenger)
                return AddNotifier(messenger, metadata);

            if (notifier is INotifyPropertyChanged propertyChanged)
            {
                propertyChanged.PropertyChanged += GetPropertyChangedEventHandler();
                return new ActionToken((n, h) => ((INotifyPropertyChanged) n!).PropertyChanged -= ((CommandEventHandler) h!).GetPropertyChangedEventHandler(), notifier, this);
            }

            return default;
        }

        public ActionToken AddNotifier(IMessenger messenger, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            if (!messenger.TrySubscribe(this, EventExecutionMode, metadata))
                return default;
            return new ActionToken((m, h) => ((IMessenger) m!).TryUnsubscribe(h!), messenger, this);
        }

        private PropertyChangedEventHandler GetPropertyChangedEventHandler() => _handler ??= Handle;

        private void EndSuspendNotifications()
        {
            if (Interlocked.Decrement(ref _suspendCount) == 0 && _isNotificationsDirty)
                RaiseCanExecuteChanged();
        }

        private void Handle(object? sender, object? message)
        {
            if (CanNotify == null || CanNotify(sender, message))
            {
                foreach (var owner in Owners)
                    owner.RaiseCanExecuteChanged();
            }
        }

        #endregion
    }
}