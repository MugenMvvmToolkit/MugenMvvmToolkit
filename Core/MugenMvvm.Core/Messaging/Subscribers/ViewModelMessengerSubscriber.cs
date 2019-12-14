using System;
using System.ComponentModel;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Messaging.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.Messaging.Subscribers
{
    public sealed class ViewModelMessengerSubscriber : MessengerHandlerComponent.IMessengerSubscriber, IMetadataContextListener//todo review memento
    {
        #region Fields

        private readonly int _hashCode;
        private readonly IWeakReference _reference;

        private static readonly IMetadataContextKey<ViewModelMessengerSubscriber> MetadataKey =
            MetadataContextKey.FromMember<ViewModelMessengerSubscriber>(typeof(ViewModelMessengerSubscriber), nameof(MetadataKey), true);

        #endregion

        #region Constructors

        private ViewModelMessengerSubscriber(IViewModelBase viewModel)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            _reference = viewModel.ToWeakReference();
            _hashCode = viewModel.GetHashCode();
            viewModel.Metadata.AddComponent(this);
            BroadcastAllMessages = viewModel.Metadata.Get(ViewModelMetadata.BroadcastAllMessages);
            BusyMessageHandlerType = viewModel.Metadata.Get(ViewModelMetadata.BusyMessageHandlerType);
        }

        #endregion

        #region Properties

        private IViewModelBase? Target => (IViewModelBase?)_reference.Target;

        private bool BroadcastAllMessages { get; set; }

        private BusyMessageHandlerType BusyMessageHandlerType { get; set; }

        #endregion

        #region Implementation of interfaces

        public bool CanHandle(Type messageType)
        {
            if (BroadcastAllMessages)
                return true;
            return typeof(IBusyToken).IsAssignableFrom(messageType) || typeof(IBroadcastMessage).IsAssignableFrom(messageType) || typeof(ProgressChangedEventArgs).IsAssignableFrom(messageType);
        }

        public MessengerResult Handle(IMessageContext messageContext)
        {
            var viewModel = Target;
            if (viewModel == null)
                return MessengerResult.Invalid;

            if (ReferenceEquals(messageContext.Sender, viewModel))
                return MessengerResult.Ignored;

            var message = messageContext.Message;
            var messenger = viewModel.TryGetServiceOptional<IMessagePublisher>();
            if (message is IBusyToken busyToken)
            {
                var messageMode = BusyMessageHandlerType;
                if (messageMode.HasFlagEx(BusyMessageHandlerType.Handle))
                    viewModel.TryGetService<IBusyIndicatorProvider>()?.Begin(busyToken);
                if (messageMode.HasFlagEx(BusyMessageHandlerType.NotifySubscribers))
                    messenger?.Publish(messageContext);
            }
            else if (BroadcastAllMessages || message is IBroadcastMessage || message is PropertyChangedEventArgs)
                messenger?.Publish(messageContext);

            return MessengerResult.Handled;
        }

        void IMetadataContextListener.OnAdded(IMetadataContext metadataContext, IMetadataContextKey key, object? newValue)
        {
            if (key.Equals(ViewModelMetadata.BroadcastAllMessages))
                BroadcastAllMessages = ViewModelMetadata.BroadcastAllMessages.GetValue(metadataContext, newValue);
            else if (key.Equals(ViewModelMetadata.BusyMessageHandlerType))
                BusyMessageHandlerType = ViewModelMetadata.BusyMessageHandlerType.GetValue(metadataContext, newValue);
        }

        void IMetadataContextListener.OnChanged(IMetadataContext metadataContext, IMetadataContextKey key, object? oldValue, object? newValue)
        {
            if (key.Equals(ViewModelMetadata.BroadcastAllMessages))
                BroadcastAllMessages = ViewModelMetadata.BroadcastAllMessages.GetValue(metadataContext, newValue);
            else if (key.Equals(ViewModelMetadata.BusyMessageHandlerType))
                BusyMessageHandlerType = ViewModelMetadata.BusyMessageHandlerType.GetValue(metadataContext, newValue);
        }

        void IMetadataContextListener.OnRemoved(IMetadataContext metadataContext, IMetadataContextKey key, object? oldValue)
        {
            if (key.Equals(ViewModelMetadata.BroadcastAllMessages))
                BroadcastAllMessages = metadataContext.Get(ViewModelMetadata.BroadcastAllMessages);
            else if (key.Equals(ViewModelMetadata.BusyMessageHandlerType))
                BusyMessageHandlerType = metadataContext.Get(ViewModelMetadata.BusyMessageHandlerType);
        }

        #endregion

        #region Methods

        public static ViewModelMessengerSubscriber? TryGetSubscriber(IViewModelBase viewModel, bool createIfNeed)
        {
            if (!(viewModel is IHasService<IBusyIndicatorProvider>) && !(viewModel is IHasService<IMessenger>))
                return null;

            if (createIfNeed)
                return viewModel.Metadata.GetOrAdd(MetadataKey, viewModel, (ctx, vm) => new ViewModelMessengerSubscriber(vm));
            return viewModel.Metadata.Get(MetadataKey);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            var target = Target;
            if (ReferenceEquals(obj, target))
                return true;
            return obj is ViewModelMessengerSubscriber handler && ReferenceEquals(target, handler.Target);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        #endregion
    }
}