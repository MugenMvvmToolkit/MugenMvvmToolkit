using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Windows.Input;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces.Commands.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Infrastructure.Commands.Components
{
    public sealed class ConditionEventCommandMediatorComponent : IConditionEventCommandMediatorComponent, IThreadDispatcherHandler, ISuspendable, IDisposable
    {
        #region Fields

        private readonly ICommand _command;

        private readonly ThreadExecutionMode _eventExecutionMode;
        private readonly HashSet<string>? _ignoreProperties;
        private readonly IThreadDispatcher _threadDispatcher;
        private EventHandler? _canExecuteChanged;
        private bool _isNotificationsDirty;
        private Subscriber? _subscriber;
        private int _suspendCount;

        #endregion

        #region Constructors

        public ConditionEventCommandMediatorComponent(IThreadDispatcher threadDispatcher, IReadOnlyCollection<object> notifiers,
            IReadOnlyCollection<string>? ignoreProperties, ThreadExecutionMode eventExecutionMode, ICommand command)
        {
            Should.NotBeNull(threadDispatcher, nameof(threadDispatcher));
            _threadDispatcher = threadDispatcher;
            _eventExecutionMode = eventExecutionMode;
            _command = command;
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

                if (notifier is INotifyPropertyChanged propertyChanged)
                    propertyChanged.PropertyChanged += _subscriber.GetPropertyChangedEventHandler();
            }
        }

        #endregion

        #region Properties

        public bool IsSuspended => _suspendCount != 0;

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

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

            if (IsSuspended)
            {
                _isNotificationsDirty = true;
                return;
            }

            _threadDispatcher.Execute(this, _eventExecutionMode, null);
        }

        public int GetPriority(object source)
        {
            return Priority;
        }

        public void Dispose()
        {
            _subscriber?.OnDispose();
            _canExecuteChanged = null;
            _subscriber = null;
        }

        public IDisposable Suspend()
        {
            Interlocked.Increment(ref _suspendCount);
            return WeakActionToken.Create(this, @this => @this.EndSuspendNotifications());
        }

        void IThreadDispatcherHandler.Execute(object? state)
        {
            _canExecuteChanged?.Invoke(_command, EventArgs.Empty);
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

            private PropertyChangedEventHandler? _handler;
            private IWeakReference? _reference;

            #endregion

            #region Constructors

            public Subscriber(ConditionEventCommandMediatorComponent component)
            {
                _reference = Service<IWeakReferenceProvider>.Instance.GetWeakReference(component);
            }

            #endregion

            #region Implementation of interfaces

            public bool Equals(IMessengerSubscriber other)
            {
                return ReferenceEquals(other, this);
            }

            public MessengerSubscriberResult Handle(object sender, object message, IMessengerContext messengerContext)
            {
                var mediator = (ConditionEventCommandMediatorComponent?)_reference?.Target;
                if (mediator == null)
                    return MessengerSubscriberResult.Invalid;
                mediator.Handle(message);
                return MessengerSubscriberResult.Handled;
            }

            #endregion

            #region Methods

            public PropertyChangedEventHandler GetPropertyChangedEventHandler()
            {
                return _handler ??= OnPropertyChanged;
            }

            public void OnDispose()
            {
                _reference?.Release();
                _reference = null;
            }

            private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
            {
                var mediator = (ConditionEventCommandMediatorComponent?)_reference?.Target;
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