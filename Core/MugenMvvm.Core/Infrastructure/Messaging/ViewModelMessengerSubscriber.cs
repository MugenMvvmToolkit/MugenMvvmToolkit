using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Infrastructure.Serialization;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Metadata;

namespace MugenMvvm.Infrastructure.Messaging
{
    public sealed class ViewModelMessengerSubscriber : IMessengerSubscriber, IObservableMetadataContextListener, IHasMemento
    {
        #region Fields

        private static readonly IMetadataContextKey<ViewModelMessengerSubscriber> MetadataKey;
        private readonly int _hashCode;
        private readonly WeakReference _reference;

        #endregion

        #region Constructors

        static ViewModelMessengerSubscriber()
        {
            MetadataKey = MetadataContextKey.FromMember<ViewModelMessengerSubscriber>(typeof(ViewModelMessengerSubscriber), nameof(MetadataKey));
        }

        private ViewModelMessengerSubscriber(IViewModelBase viewModel)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            _reference = MugenExtensions.GetWeakReference(viewModel);
            _hashCode = viewModel.GetHashCode();
            viewModel.Metadata.AddListener(this);
            BroadcastAllMessages = viewModel.Metadata.Get(ViewModelMetadata.BroadcastAllMessages);
            BusyMessageHandlerType = viewModel.Metadata.Get(ViewModelMetadata.BusyMessageHandlerType);
        }

        #endregion

        #region Properties

        private IViewModelBase? Target => (IViewModelBase)_reference.Target;

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

        public bool Equals(IMessengerSubscriber other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;
            return other is ViewModelMessengerSubscriber handler && ReferenceEquals(Target, handler.Target);
        }

        public MessengerSubscriberResult Handle(object sender, object message, IMessengerContext messengerContext)
        {
            var viewModel = Target;
            if (viewModel == null)
                return MessengerSubscriberResult.Invalid;

            if (ReferenceEquals(sender, viewModel))
                return MessengerSubscriberResult.Ignored;

            var messenger = viewModel.TryGetServiceOptional<IEventPublisher>();
            if (message is IBusyToken busyToken)
            {
                var messageMode = BusyMessageHandlerType;
                if (messageMode.HasFlagEx(BusyMessageHandlerType.Handle))
                    viewModel.TryGetService<IBusyIndicatorProvider>()?.Begin(busyToken);
                if (messageMode.HasFlagEx(BusyMessageHandlerType.NotifySubscribers))
                    messenger?.Publish(sender, busyToken, messengerContext);
            }
            else if (BroadcastAllMessages || message is IBroadcastMessage || message is PropertyChangedEventArgs)
                messenger?.Publish(sender, message, messengerContext);

            return MessengerSubscriberResult.Handled;
        }

        void IObservableMetadataContextListener.OnAdded(IObservableMetadataContext metadataContext, IMetadataContextKey key, object? newValue)
        {
            if (key.Equals(ViewModelMetadata.BroadcastAllMessages))
                BroadcastAllMessages = ViewModelMetadata.BroadcastAllMessages.GetValue(metadataContext, newValue);
            else if (key.Equals(ViewModelMetadata.BusyMessageHandlerType))
                BusyMessageHandlerType = ViewModelMetadata.BusyMessageHandlerType.GetValue(metadataContext, newValue);
        }

        void IObservableMetadataContextListener.OnChanged(IObservableMetadataContext metadataContext, IMetadataContextKey key, object? oldValue, object? newValue)
        {
            if (key.Equals(ViewModelMetadata.BroadcastAllMessages))
                BroadcastAllMessages = ViewModelMetadata.BroadcastAllMessages.GetValue(metadataContext, newValue);
            else if (key.Equals(ViewModelMetadata.BusyMessageHandlerType))
                BusyMessageHandlerType = ViewModelMetadata.BusyMessageHandlerType.GetValue(metadataContext, newValue);
        }

        void IObservableMetadataContextListener.OnRemoved(IObservableMetadataContext metadataContext, IMetadataContextKey key, object? oldValue)
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
                return viewModel.Metadata.GetOrAdd(MetadataKey, viewModel, viewModel, (ctx, vm, _) => new ViewModelMessengerSubscriber(vm));
            return viewModel.Metadata.Get(MetadataKey);

        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
                return false;
            if (ReferenceEquals(this, obj))
                return true;
            if (obj is ViewModelMessengerSubscriber handler)
                return Equals(handler);
            return false;
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

            [DataMember(Name = "V")] public readonly IViewModelBase ViewModel;

            #endregion

            #region Constructors

            internal ViewModelMessengerSubscriberMemento(IViewModelBase viewModel)
            {
                ViewModel = viewModel;
            }

            internal ViewModelMessengerSubscriberMemento()
            {
            }

            #endregion

            #region Properties

            [IgnoreDataMember, XmlIgnore]
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