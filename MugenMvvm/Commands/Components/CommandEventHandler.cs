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
    public sealed class CommandEventHandler : MultiAttachableComponentBase<ICompositeCommand>, ICommandEventHandlerComponent, IThreadDispatcherHandler, IValueHolder<Delegate>,
        ISuspendable, IHasDisposeCondition, IHasPriority
    {
        private readonly IThreadDispatcher? _threadDispatcher;
        private EventHandler? _canExecuteChanged;
        private WeakHandler? _weakHandler;
        private bool _isNotificationsDirty;
        private int _suspendCount;

        public CommandEventHandler(IThreadDispatcher? threadDispatcher, ThreadExecutionMode eventExecutionMode)
        {
            Should.NotBeNull(eventExecutionMode, nameof(eventExecutionMode));
            _threadDispatcher = threadDispatcher;
            EventExecutionMode = eventExecutionMode;
            IsDisposable = true;
            _weakHandler = new WeakHandler(this);
        }

        public ThreadExecutionMode EventExecutionMode { get; }

        public Func<object?, object?, bool>? CanNotify { get; set; }

        public bool IsDisposable { get; set; }

        public int Priority => CommandComponentPriority.ConditionEvent;

        public bool IsSuspended => _suspendCount != 0;

        Delegate? IValueHolder<Delegate>.Value { get; set; }

        public ActionToken AddNotifier(object? notifier, IReadOnlyMetadataContext? metadata = null)
        {
            if (notifier is IHasService<IMessenger> hasMessenger)
                return AddNotifier(hasMessenger.Service, metadata);

            if (notifier is IMessenger messenger)
                return AddNotifier(messenger, metadata);

            if (_weakHandler != null && notifier is INotifyPropertyChanged propertyChanged)
            {
                propertyChanged.PropertyChanged += _weakHandler.GetPropertyChangedEventHandler();
                return new ActionToken((n, h) => ((INotifyPropertyChanged) n!).PropertyChanged -= ((WeakHandler) h!).GetPropertyChangedEventHandler(), propertyChanged,
                    _weakHandler);
            }

            return default;
        }

        public ActionToken AddNotifier(IMessenger messenger, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            if (_weakHandler == null || !messenger.TrySubscribe(_weakHandler, EventExecutionMode, metadata))
                return default;
            return new ActionToken((m, h) => ((IMessenger) m!).TryUnsubscribe(h!), messenger, _weakHandler);
        }

        public void AddCanExecuteChanged(ICompositeCommand command, EventHandler? handler, IReadOnlyMetadataContext? metadata)
        {
            if (_weakHandler != null)
                _canExecuteChanged += handler;
        }

        public void RemoveCanExecuteChanged(ICompositeCommand command, EventHandler? handler, IReadOnlyMetadataContext? metadata)
        {
            if (_weakHandler != null)
                _canExecuteChanged -= handler;
        }

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
                _weakHandler?.OnDispose();
                _canExecuteChanged = null;
                _weakHandler = null;
            }
        }

        public ActionToken Suspend(object? state = null, IReadOnlyMetadataContext? metadata = null)
        {
            Interlocked.Increment(ref _suspendCount);
            return new ActionToken((o, _) => ((CommandEventHandler) o!).EndSuspendNotifications(), this);
        }

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

        void IThreadDispatcherHandler.Execute(object? _)
        {
            foreach (var owner in Owners)
                _canExecuteChanged?.Invoke(owner, EventArgs.Empty);
        }

        internal sealed class WeakHandler : IMessengerHandler
        {
            #region Fields

            private PropertyChangedEventHandler? _handler;
            private IWeakReference? _reference;

            #endregion

            #region Constructors

            public WeakHandler(CommandEventHandler component)
            {
                _reference = component.ToWeakReference();
            }

            #endregion

            #region Implementation of interfaces

            public bool CanHandle(Type messageType) => true;

            public MessengerResult Handle(IMessageContext messageContext)
            {
                var mediator = (CommandEventHandler?) _reference?.Target;
                if (mediator == null)
                    return MessengerResult.Invalid;
                mediator.Handle(messageContext.Sender, messageContext.Message);
                return MessengerResult.Handled;
            }

            #endregion

            #region Methods

            public PropertyChangedEventHandler GetPropertyChangedEventHandler() => _handler ??= OnPropertyChanged;

            public void OnDispose()
            {
                _reference?.Release();
                _reference = null;
            }

            private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                var component = (CommandEventHandler?) _reference?.Target;
                if (component == null)
                {
                    if (sender is INotifyPropertyChanged propertyChanged)
                        propertyChanged.PropertyChanged -= _handler;
                    return;
                }

                component.Handle(sender, e);
            }

            #endregion
        }
    }
}