using System;
using System.ComponentModel;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Commands.Components
{
    public sealed class CommandNotifier : MultiAttachableComponentBase<ICompositeCommand>, IHasDisposeCondition, IHasPriority, IComponent<ICompositeCommand>
    {
        private WeakHandler? _weakHandler;

        public CommandNotifier()
        {
            IsDisposable = true;
            _weakHandler = new WeakHandler(this);
        }

        public Func<object?, object?, bool>? CanNotify { get; set; }

        public bool IsDisposable { get; set; }

        public int Priority => CommandComponentPriority.Notifier;

        public ActionToken AddNotifier(object? notifier, IReadOnlyMetadataContext? metadata = null)
        {
            if (notifier is IHasService<IMessenger> hasMessenger)
                return AddNotifier(hasMessenger.GetService(false)!, metadata);

            if (notifier is IMessenger messenger)
                return AddNotifier(messenger, metadata);

            if (_weakHandler != null && notifier is INotifyPropertyChanged propertyChanged)
            {
                propertyChanged.PropertyChanged += _weakHandler.GetPropertyChangedEventHandler();
                return ActionToken.FromDelegate((n, h) => ((INotifyPropertyChanged)n!).PropertyChanged -= ((WeakHandler)h!).GetPropertyChangedEventHandler(), propertyChanged,
                    _weakHandler);
            }

            return default;
        }

        public ActionToken AddNotifier(IMessenger messenger, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(messenger, nameof(messenger));
            if (_weakHandler == null || !messenger.TrySubscribe(_weakHandler, ThreadExecutionMode.Current, metadata))
                return default;
            return ActionToken.FromDelegate((m, h) => ((IMessenger)m!).TryUnsubscribe(h!), messenger, _weakHandler);
        }

        public void Dispose()
        {
            if (IsDisposable)
            {
                _weakHandler?.OnDispose();
                _weakHandler = null;
            }
        }

        private void Handle(object? sender, object? message)
        {
            if (CanNotify == null || CanNotify(sender, message))
            {
                foreach (var owner in Owners)
                    owner.RaiseCanExecuteChanged();
            }
        }

        internal sealed class WeakHandler : IMessengerHandler
        {
            private PropertyChangedEventHandler? _handler;
            private IWeakReference? _reference;

            public WeakHandler(CommandNotifier component)
            {
                _reference = component.ToWeakReference();
            }

            public PropertyChangedEventHandler GetPropertyChangedEventHandler() => _handler ??= OnPropertyChanged;

            public void OnDispose()
            {
                _reference?.Release();
                _reference = null;
            }

            public bool CanHandle(Type messageType) => true;

            public MessengerResult Handle(IMessageContext messageContext)
            {
                var mediator = (CommandNotifier?)_reference?.Target;
                if (mediator == null)
                    return MessengerResult.Invalid;
                mediator.Handle(messageContext.Sender, messageContext.Message);
                return MessengerResult.Handled;
            }

            private void OnPropertyChanged(object? sender, PropertyChangedEventArgs e)
            {
                var component = (CommandNotifier?)_reference?.Target;
                if (component == null)
                {
                    if (sender is INotifyPropertyChanged propertyChanged)
                        propertyChanged.PropertyChanged -= _handler;
                    return;
                }

                component.Handle(sender, e);
            }
        }
    }
}