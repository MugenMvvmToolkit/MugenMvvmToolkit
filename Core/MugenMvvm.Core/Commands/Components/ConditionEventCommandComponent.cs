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
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Messaging.Components;

namespace MugenMvvm.Commands.Components
{
    public sealed class ConditionEventCommandComponent : AttachableComponentBase<ICompositeCommand>, IConditionEventCommandComponent,
        IThreadDispatcherHandler<object?>, IValueHolder<Delegate>, ISuspendable, IDisposable, IHasPriority
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

        public ConditionEventCommandComponent(IThreadDispatcher? threadDispatcher, ThreadExecutionMode eventExecutionMode, IReadOnlyList<object> notifiers, Func<object, bool>? canNotify)
        {
            _threadDispatcher = threadDispatcher;
            _eventExecutionMode = eventExecutionMode;
            _canNotify = canNotify;

            _subscriber = new Subscriber(this);
            for (var index = 0; index < notifiers.Count; index++)
            {
                var notifier = notifiers[index];
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

        public int Priority => CommandComponentPriority.ConditionEvent;

        Delegate? IValueHolder<Delegate>.Value { get; set; }

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

            _threadDispatcher.DefaultIfNull().Execute(_eventExecutionMode, this);
        }

        public void Dispose()
        {
            _subscriber?.OnDispose();
            _canExecuteChanged = null;
            _subscriber = null;
        }

        public ActionToken Suspend()
        {
            Interlocked.Increment(ref _suspendCount);
            return new ActionToken((o, _) => ((ConditionEventCommandComponent)o!).EndSuspendNotifications(), this);
        }

        void IThreadDispatcherHandler<object?>.Execute(object? _)
        {
            _canExecuteChanged?.Invoke(Owner, EventArgs.Empty);
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
            if (_canNotify == null || _canNotify(message))
                RaiseCanExecuteChanged();
        }

        #endregion

        #region Nested types

        private sealed class Subscriber : IMessengerHandlerRaw
        {
            #region Fields

            private PropertyChangedEventHandler? _handler;
            private IWeakReference? _reference;

            #endregion

            #region Constructors

            public Subscriber(ConditionEventCommandComponent component)
            {
                _reference = component.ToWeakReference();
            }

            #endregion

            #region Implementation of interfaces

            public bool CanHandle(Type messageType)
            {
                return true;
            }

            public MessengerResult Handle(IMessageContext messageContext)
            {
                var mediator = (ConditionEventCommandComponent?)_reference?.Target;
                if (mediator == null)
                    return MessengerResult.Invalid;
                mediator.Handle(messageContext.Message);
                return MessengerResult.Handled;
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
                var component = (ConditionEventCommandComponent?)_reference?.Target;
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