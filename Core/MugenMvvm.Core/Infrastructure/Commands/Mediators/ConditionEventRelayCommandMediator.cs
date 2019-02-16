using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Commands.Mediators;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure.Commands.Mediators
{
    public sealed class ConditionEventRelayCommandMediator : IConditionEventRelayCommandMediator, IThreadDispatcherHandler, ISuspendNotifications
    {
        #region Fields

        private readonly ThreadExecutionMode _eventExecutionMode;
        private readonly HashSet<string>? _ignoreProperties;
        private readonly IRelayCommand _relayCommand;
        private readonly IThreadDispatcher _threadDispatcher;
        private EventHandler? _canExecuteChanged;
        private bool _isNotificationsDirty;
        private Subscriber? _subscriber;
        private int _suspendCount;

        #endregion

        #region Constructors

        public ConditionEventRelayCommandMediator(IThreadDispatcher threadDispatcher, IReadOnlyCollection<object> notifiers,
            IReadOnlyCollection<string>? ignoreProperties, ThreadExecutionMode eventExecutionMode, IRelayCommand relayCommand)
        {
            _threadDispatcher = threadDispatcher;
            _eventExecutionMode = eventExecutionMode;
            _relayCommand = relayCommand;
            if (ignoreProperties != null && ignoreProperties.Count > 0)
                _ignoreProperties = new HashSet<string>(ignoreProperties, StringComparer.Ordinal);

            _subscriber = new Subscriber(this);
            foreach (var notifier in notifiers)
            {
                if (notifier is IViewModelBase vm && vm.TrySubscribe(_subscriber, eventExecutionMode))
                    continue;

                if (notifier is IHasService<IMessenger> hasMessenger)
                {
                    hasMessenger.Service.Subscribe(_subscriber, eventExecutionMode);
                    continue;
                }

                if (notifier is IMessenger messenger)
                {
                    messenger.Subscribe(_subscriber, eventExecutionMode);
                    continue;
                }

                if (notifier is NotifyPropertyChangedBase propertyChanged)
                    propertyChanged.PropertyChanged += _subscriber.GetPropertyChangedEventHandler();
            }
        }

        #endregion

        #region Properties

        public bool IsNotificationsSuspended => _suspendCount != 0;

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            _subscriber?.OnDispose();
            _canExecuteChanged = null;
            _subscriber = null;
        }

        public void AddCanExecuteChanged(EventHandler handler)
        {
            if (_subscriber != null)
                _canExecuteChanged += handler;
        }

        public void RemoveCanExecuteChanged(EventHandler handler)
        {
            // ReSharper disable once DelegateSubtraction
            _canExecuteChanged -= handler;
        }

        public void RaiseCanExecuteChanged()
        {
            if (_canExecuteChanged == null)
                return;

            if (IsNotificationsSuspended)
            {
                _isNotificationsDirty = true;
                return;
            }

            _threadDispatcher.Execute(this, _eventExecutionMode, null);
        }

        public IDisposable SuspendNotifications()
        {
            Interlocked.Increment(ref _suspendCount);
            return WeakActionToken.Create(this, @this => @this.EndSuspendNotifications());
        }

        void IThreadDispatcherHandler.Execute(object? state)
        {
            _canExecuteChanged?.Invoke(_relayCommand, EventArgs.Empty);
        }

        #endregion

        #region Methods

        private void EndSuspendNotifications()
        {
            if (Interlocked.Decrement(ref _suspendCount) == 0 && _isNotificationsDirty)
                RaiseCanExecuteChanged();
        }

        private void Handle(object message)
        {
            if (_ignoreProperties != null && message is PropertyChangedEventArgs args && _ignoreProperties.Contains(args.PropertyName))
                return;
            RaiseCanExecuteChanged();
        }

        #endregion

        #region Nested types

        private sealed class Subscriber : IMessengerSubscriber
        {
            #region Fields

            private readonly WeakReference _reference;
            private PropertyChangedEventHandler? _handler;

            #endregion

            #region Constructors

            public Subscriber(ConditionEventRelayCommandMediator mediator)
            {
                _reference = MugenExtensions.GetWeakReference(mediator);
            }

            #endregion

            #region Implementation of interfaces

            public bool Equals(IMessengerSubscriber other)
            {
                return ReferenceEquals(other, this);
            }

            public MessengerSubscriberResult Handle(object sender, object message, IMessengerContext messengerContext)
            {
                var mediator = (ConditionEventRelayCommandMediator)_reference.Target;
                if (mediator == null)
                    return MessengerSubscriberResult.Invalid;
                mediator.Handle(message);
                return MessengerSubscriberResult.Handled;
            }

            #endregion

            #region Methods

            public PropertyChangedEventHandler GetPropertyChangedEventHandler()
            {
                if (_handler == null)
                    _handler = OnPropertyChanged;
                return _handler;
            }

            public void OnDispose()
            {
                _reference.Target = null;
            }

            private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                var mediator = (ConditionEventRelayCommandMediator)_reference.Target;
                if (mediator == null)
                {
                    if (sender is INotifyPropertyChanged propertyChanged)
                        propertyChanged.PropertyChanged -= _handler;
                    return;
                }

                mediator.Handle(e);
            }

            #endregion
        }

        #endregion
    }
}