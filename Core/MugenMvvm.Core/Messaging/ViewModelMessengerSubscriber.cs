using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Messaging.Components;
using MugenMvvm.Metadata;
using MugenMvvm.Serialization;

namespace MugenMvvm.Messaging
{
    public sealed class ViewModelMessengerSubscriber : MessengerHandlerComponent.IMessengerSubscriber, IMetadataContextListener, IHasMemento //todo review memento
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

        public IMemento? GetMemento()
        {
            var viewModel = Target;
            if (viewModel == null)
                return null;
            return new ViewModelMessengerSubscriberMemento(viewModel);
        }

        public bool CanHandle(IMessageContext messageContext)
        {
            if (BroadcastAllMessages)
                return true;
            var message = messageContext.Message;
            return message is IBusyToken || message is IBroadcastMessage || message is PropertyChangedEventArgs;
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

        #region Nested types

        [Serializable]
        [DataContract(Namespace = BuildConstants.DataContractNamespace)]
        [Preserve(Conditional = true, AllMembers = true)]
        public sealed class ViewModelMessengerSubscriberMemento : IMemento
        {
            #region Fields

            [DataMember(Name = "V")]
            public readonly IViewModelBase ViewModel;

            #endregion

            #region Constructors

            internal ViewModelMessengerSubscriberMemento(IViewModelBase viewModel)
            {
                ViewModel = viewModel;
            }

#pragma warning disable CS8618
            internal ViewModelMessengerSubscriberMemento()
            {
            }
#pragma warning restore CS8618

            #endregion

            #region Properties

            [IgnoreDataMember]
            [XmlIgnore]
            public Type TargetType => typeof(ViewModelMessengerSubscriber);

            #endregion

            #region Implementation of interfaces

            public void Preserve(ISerializationContext serializationContext)
            {
            }

            public IMementoResult Restore(ISerializationContext serializationContext)
            {
                Should.NotBeNull(ViewModel, nameof(ViewModel));
                var subscriber = TryGetSubscriber(ViewModel, true);
                if (subscriber == null)
                    return MementoResult.Unrestored;
                return new MementoResult(subscriber, serializationContext);
            }

            #endregion
        }

        #endregion
    }
}