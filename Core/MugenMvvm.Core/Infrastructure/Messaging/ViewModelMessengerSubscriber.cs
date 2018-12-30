using System;
using System.ComponentModel;
using System.Runtime.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Infrastructure.Serialization;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Models;
using MugenMvvm.ViewModels;

namespace MugenMvvm.Infrastructure.Messaging
{
    public sealed class ViewModelMessengerSubscriber : IMessengerSubscriber, IObservableMetadataContextListener, IHasMemento
    {
        #region Fields

        private readonly int _hashCode;
        private readonly WeakReference _reference;

        #endregion

        #region Constructors

        private ViewModelMessengerSubscriber(IViewModel viewModel)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            _reference = MugenExtensions.GetWeakReference(viewModel);
            _hashCode = viewModel.GetHashCode();
            OnContextChanged(Target.Metadata, null);
            viewModel.Metadata.AddListener(this);
        }

        #endregion

        #region Properties

        private IViewModel? Target => (IViewModel)_reference.Target;

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

        public SubscriberResult Handle(object sender, object message, IMessengerContext messengerContext)
        {
            var viewModel = Target;
            if (viewModel == null)
                return SubscriberResult.Invalid;

            if (ReferenceEquals(sender, viewModel))
                return SubscriberResult.Ignored;

            if (message is IBusyToken busyToken)
            {
                var messageMode = BusyMessageHandlerType;
                if (messageMode.HasFlagEx(BusyMessageHandlerType.Handle))
                    viewModel.BusyIndicatorProvider.Begin(busyToken);
                if (messageMode.HasFlagEx(BusyMessageHandlerType.NotifySubscribers))
                    viewModel.Publish(sender, busyToken, messengerContext);
            }
            else if (BroadcastAllMessages || message is IBroadcastMessage || message is PropertyChangedEventArgs)
                viewModel.Publish(sender, message, messengerContext);

            return SubscriberResult.Handled;
        }

        public void OnContextChanged(IObservableMetadataContext metadataContext, IMetadataContextKey? key)
        {
            if (key == null || key.Equals(ViewModelMetadata.BroadcastAllMessages))
                BroadcastAllMessages = metadataContext.Get(ViewModelMetadata.BroadcastAllMessages);
            if (key == null || key.Equals(ViewModelMetadata.BusyMessageHandlerType))
                BusyMessageHandlerType = metadataContext.Get(ViewModelMetadata.BusyMessageHandlerType, BusyMessageHandlerType.Handle);
        }

        #endregion

        #region Methods

        public static ViewModelMessengerSubscriber GetSubscriber(IViewModel viewModel)
        {
            if (viewModel is ViewModelBase vm)
            {
                if (vm.Subscriber == null)
                {
                    lock (vm)
                    {
                        vm.Subscriber = new ViewModelMessengerSubscriber(vm);
                    }
                }

                return vm.Subscriber;
            }

            return Singleton<IAttachedValueProvider>.Instance.GetOrAdd(viewModel, AttachedMemberConstants.ViewModelMessengerSubscriberKey, (model, o) => new ViewModelMessengerSubscriber(model), null);
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

            [DataMember(Name = "V")]
            public readonly IViewModel ViewModel;

            #endregion

            #region Constructors

            internal ViewModelMessengerSubscriberMemento(IViewModel viewModel)
            {
                ViewModel = viewModel;
            }

            internal ViewModelMessengerSubscriberMemento()
            {
            }

            #endregion

            #region Properties

            [IgnoreDataMember]
            public Type TargetType => typeof(ViewModelMessengerSubscriber);

            #endregion

            #region Implementation of interfaces

            public void Preserve(ISerializationContext serializationContext)
            {
            }

            public IMementoResult Restore(ISerializationContext serializationContext)
            {
                Should.NotBeNull(ViewModel, nameof(ViewModel));
                return new MementoResult(GetSubscriber(ViewModel), serializationContext.Metadata);
            }

            #endregion
        }

        #endregion
    }
}