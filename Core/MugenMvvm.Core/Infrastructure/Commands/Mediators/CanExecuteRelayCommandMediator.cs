using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure.Commands.Mediators
{
    internal sealed class CanExecuteRelayCommandMediator<T> : RelayCommandMediatorWrapperBase, IThreadDispatcherHandler
    {
        #region Fields

        private readonly ThreadExecutionMode _eventExecutionMode;
        private readonly IThreadDispatcher _threadDispatcher;
        private readonly HashSet<string>? _ignoreProperties;

        private EventHandler? _canExecuteChangedInternal;
        private Delegate? _canExecuteDelegate;
        private bool _isNotificationsDirty;
        private RelayCommandCanExecuteMediatorSubscriber? _subscriber;
        private int _suspendCount;

        #endregion

        #region Constructors

        public CanExecuteRelayCommandMediator(IRelayCommandMediator mediator, IThreadDispatcher threadDispatcher, ThreadExecutionMode eventExecutionMode,
            Delegate canExecute, IReadOnlyCollection<object> notifiers, IReadOnlyCollection<string>? ignoreProperties)
            : base(mediator)
        {
            _canExecuteDelegate = canExecute;
            _threadDispatcher = threadDispatcher;
            _eventExecutionMode = eventExecutionMode;
            if (notifiers.Count > 0)
            {
                if (ignoreProperties != null && ignoreProperties.Count > 0)
                    _ignoreProperties = new HashSet<string>(ignoreProperties, StringComparer.Ordinal);

                foreach (var notifier in notifiers)
                {
                    if (notifier is IViewModel vm)
                    {
                        vm.InternalMessenger.Subscribe(GetSubscriber());
                        continue;
                    }

                    if (notifier is NotifyPropertyChangedBase propertyChanged)
                        propertyChanged.PropertyChanged += GetSubscriber().GetPropertyChangedEventHandler();
                }
            }
        }

        #endregion

        #region Properties

        public override bool HasCanExecute => _canExecuteDelegate != null || base.HasCanExecute;

        public override bool IsNotificationsSuspended => _suspendCount != 0 || base.IsNotificationsSuspended;

        #endregion

        #region Implementation of interfaces

        void IThreadDispatcherHandler.Execute(object? state)
        {
            _canExecuteChangedInternal?.Invoke(this, EventArgs.Empty);
            base.RaiseCanExecuteChanged();
        }

        #endregion

        #region Methods

        public override void AddCanExecuteChanged(EventHandler handler)
        {
            if (_canExecuteDelegate != null)
                _canExecuteChangedInternal += handler;
            base.AddCanExecuteChanged(handler);
        }

        public override bool CanExecute(object parameter)
        {
            return base.CanExecute(parameter) && CanExecuteInternal(parameter);
        }

        public override void Dispose()
        {
            _subscriber?.OnDispose();
            _canExecuteDelegate = null;
            _canExecuteChangedInternal = null;
            _subscriber = null;
            base.Dispose();
        }

        public override void RaiseCanExecuteChanged()
        {
            if (_canExecuteChangedInternal == null)
                return;

            if (_suspendCount != 0)
            {
                _isNotificationsDirty = true;
                return;
            }

            _threadDispatcher.Execute(this, _eventExecutionMode, null);
        }

        public override void RemoveCanExecuteChanged(EventHandler handler)
        {
            // ReSharper disable once DelegateSubtraction
            _canExecuteChangedInternal -= handler;
            base.RemoveCanExecuteChanged(handler);
        }

        public override IDisposable SuspendNotifications()
        {
            Interlocked.Increment(ref _suspendCount);
            var baseToken = base.SuspendNotifications();
            if (ReferenceEquals(Default.Disposable, baseToken))
                return WeakActionToken.Create(this, @this => @this.EndSuspendNotifications());
            return WeakActionToken.Create(this, baseToken, (@this, t) =>
            {
                t.Dispose();
                @this.EndSuspendNotifications();
            });
        }

        private void EndSuspendNotifications()
        {
            if (Interlocked.Decrement(ref _suspendCount) != 0)
                return;
            if (_isNotificationsDirty)
                RaiseCanExecuteChanged();
        }

        private bool CanExecuteInternal(object parameter)
        {
            var canExecuteDelegate = _canExecuteDelegate;
            if (canExecuteDelegate == null)
                return false;
            if (canExecuteDelegate is Func<bool> func)
                return func();
            return ((Func<T, bool>)canExecuteDelegate).Invoke((T)parameter);
        }

        private void Handle(object message)
        {
            if (_ignoreProperties != null && message is PropertyChangedEventArgs args && _ignoreProperties.Contains(args.PropertyName))
                return;
            RaiseCanExecuteChanged();
        }

        private RelayCommandCanExecuteMediatorSubscriber GetSubscriber()
        {
            if (_subscriber == null)
                _subscriber = new RelayCommandCanExecuteMediatorSubscriber(this);
            return _subscriber;
        }

        #endregion

        #region Nested types

        private sealed class RelayCommandCanExecuteMediatorSubscriber : IMessengerSubscriber
        {
            #region Fields

            private readonly WeakReference _reference;
            private PropertyChangedEventHandler? _handler;

            #endregion

            #region Constructors

            public RelayCommandCanExecuteMediatorSubscriber(CanExecuteRelayCommandMediator<T> mediator)
            {
                _reference = MugenExtensions.GetWeakReference(mediator);
            }

            #endregion

            #region Implementation of interfaces

            public bool Equals(IMessengerSubscriber other)
            {
                return ReferenceEquals(other, this);
            }

            public SubscriberResult Handle(object sender, object message, IMessengerContext messengerContext)
            {
                var mediator = (CanExecuteRelayCommandMediator<T>)_reference.Target;
                if (mediator == null)
                    return SubscriberResult.Invalid;
                mediator.Handle(message);
                return SubscriberResult.Handled;
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
                var mediator = (CanExecuteRelayCommandMediator<T>)_reference.Target;
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