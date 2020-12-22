using System;
using System.Collections.Generic;
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
    public sealed class CommandConditionEventManager : AttachableComponentBase<ICompositeCommand>, ICommandConditionEventManagerComponent,
        IThreadDispatcherHandler, IValueHolder<Delegate>, ISuspendable, IDisposable, IHasPriority
    {
        #region Fields

        private readonly Func<object, bool>? _canNotify;
        private readonly ThreadExecutionMode _eventExecutionMode;
        private readonly IThreadDispatcher? _threadDispatcher;
        private EventHandler? _canExecuteChanged;
        private bool _isNotificationsDirty;
        private Subscriber? _subscriber;
        private int _suspendCount;

        #endregion

        #region Constructors

        public CommandConditionEventManager(IThreadDispatcher? threadDispatcher, ThreadExecutionMode eventExecutionMode, IReadOnlyList<object> notifiers, Func<object, bool>? canNotify)
        {
            Should.NotBeNull(eventExecutionMode, nameof(eventExecutionMode));
            Should.NotBeNull(notifiers, nameof(notifiers));
            _threadDispatcher = threadDispatcher;
            _eventExecutionMode = eventExecutionMode;
            _canNotify = canNotify;
            _subscriber = new Subscriber(this);
            for (var index = 0; index < notifiers.Count; index++)
            {
                var notifier = notifiers[index];
                if (notifier is IHasService<IMessenger> hasMessenger)
                {
                    hasMessenger.Service.TrySubscribe(_subscriber, eventExecutionMode);
                    continue;
                }

                if (notifier is IMessenger messenger)
                {
                    messenger.TrySubscribe(_subscriber, eventExecutionMode);
                    continue;
                }

                if (notifier is INotifyPropertyChanged propertyChanged)
                    propertyChanged.PropertyChanged += _subscriber.GetPropertyChangedEventHandler();
            }
        }

        #endregion

        #region Properties

        public bool IsSuspended => _suspendCount != 0;

        public int Priority => CommandComponentPriority.ConditionEvent;

        Delegate? IValueHolder<Delegate>.Value { get; set; }

        #endregion

        #region Implementation of interfaces

        public void AddCanExecuteChanged(ICompositeCommand command, EventHandler? handler, IReadOnlyMetadataContext? metadata)
        {
            if (_subscriber != null)
                _canExecuteChanged += handler;
        }

        public void RemoveCanExecuteChanged(ICompositeCommand command, EventHandler? handler, IReadOnlyMetadataContext? metadata) =>
            // ReSharper disable once DelegateSubtraction
            _canExecuteChanged -= handler;

        public void RaiseCanExecuteChanged(ICompositeCommand? command = null, IReadOnlyMetadataContext? metadata = null)
        {
            if (_canExecuteChanged == null)
                return;

            if (IsSuspended)
            {
                _isNotificationsDirty = true;
                return;
            }

            _threadDispatcher.DefaultIfNull().Execute(_eventExecutionMode, this, null);
        }

        public void Dispose()
        {
            _subscriber?.OnDispose();
            _canExecuteChanged = null;
            _subscriber = null;
        }

        public ActionToken Suspend(object? state = null, IReadOnlyMetadataContext? metadata = null)
        {
            Interlocked.Increment(ref _suspendCount);
            return new ActionToken((o, _) => ((CommandConditionEventManager) o!).EndSuspendNotifications(), this);
        }

        void IThreadDispatcherHandler.Execute(object? _) => _canExecuteChanged?.Invoke(Owner, EventArgs.Empty);

        #endregion

        #region Methods

        private void EndSuspendNotifications()
        {
            if (Interlocked.Decrement(ref _suspendCount) == 0 && _isNotificationsDirty)
                RaiseCanExecuteChanged();
        }

        private void Handle(object message)
        {
            if (_canNotify == null || _canNotify(message))
                RaiseCanExecuteChanged();
        }

        #endregion

        #region Nested types

        private sealed class Subscriber : IMessengerHandler
        {
            #region Fields

            private PropertyChangedEventHandler? _handler;
            private IWeakReference? _reference;

            #endregion

            #region Constructors

            public Subscriber(CommandConditionEventManager component)
            {
                _reference = component.ToWeakReference();
            }

            #endregion

            #region Implementation of interfaces

            public bool CanHandle(Type messageType) => true;

            public MessengerResult Handle(IMessageContext messageContext)
            {
                var mediator = (CommandConditionEventManager?) _reference?.Target;
                if (mediator == null)
                    return MessengerResult.Invalid;
                mediator.Handle(messageContext.Message);
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
                var component = (CommandConditionEventManager?) _reference?.Target;
                if (component == null)
                {
                    if (sender is INotifyPropertyChanged propertyChanged)
                        propertyChanged.PropertyChanged -= _handler;
                    return;
                }

                component.Handle(e);
            }

            #endregion
        }

        #endregion
    }
}