using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Infrastructure.Serialization;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Infrastructure;
using MugenMvvm.ViewModels;

namespace MugenMvvm.Infrastructure.ViewModels
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    [Preserve(Conditional = true, AllMembers = true)]
    public class ViewModelMemento : IMemento
    {
        #region Fields

        protected static readonly object RestorationLocker;

        [IgnoreDataMember, XmlIgnore, NonSerialized]
        private IViewModel? _viewModel;

        [DataMember(Name = "B")]
        internal IList<IBusyIndicatorProviderListener?>? BusyListeners;

        [DataMember(Name = "C")]
        protected internal IObservableMetadataContext? Metadata;

        [DataMember(Name = "S")]
        internal IList<MessengerSubscriberInfo>? Subscribers;

        [DataMember(Name = "T")]
        internal Type? ViewModelType;

        [DataMember(Name = "N")]
        protected internal bool NoState;

        #endregion

        #region Constructors

        static ViewModelMemento()
        {
            RestorationLocker = new object();
        }

        internal ViewModelMemento()
        {
        }

        public ViewModelMemento(IViewModel viewModel)
        {
            _viewModel = viewModel;
            Metadata = viewModel.Metadata;
            ViewModelType = viewModel.GetType();
        }

        #endregion

        #region Properties

        [IgnoreDataMember, XmlIgnore]
        public Type TargetType => ViewModelType!;

        #endregion

        #region Implementation of interfaces

        public void Preserve(ISerializationContext serializationContext)
        {
            if (_viewModel == null)
                return;
            if (_viewModel.Metadata.Get(ViewModelMetadata.NoState))
            {
                NoState = true;
                Metadata = null;
                Subscribers = null;
                BusyListeners = null;
            }
            else
            {
                NoState = false;
                Metadata = _viewModel.Metadata;
                if (_viewModel is ViewModelBase vm)
                {
                    Subscribers = vm.GetInternalMessenger(false)?.GetSubscribers().ToSerializable(serializationContext.Serializer);
                    BusyListeners = vm.GetBusyIndicatorProvider(false)?.GetListeners().ToSerializable(serializationContext.Serializer);
                }
                else
                {
                    Subscribers = _viewModel.InternalMessenger.GetSubscribers().ToSerializable(serializationContext.Serializer);
                    BusyListeners = _viewModel.BusyIndicatorProvider?.GetListeners().ToSerializable(serializationContext.Serializer);
                }
            }

            OnPreserveInternal(_viewModel!, serializationContext);
        }

        public IMementoResult Restore(ISerializationContext serializationContext)
        {
            if (NoState)
                return MementoResult.Unrestored;

            Should.NotBeNull(Metadata, nameof(Metadata));
            Should.NotBeNull(ViewModelType, nameof(ViewModelType));
            if (_viewModel != null)
                return new MementoResult(_viewModel, serializationContext);

            var dispatcher = serializationContext.ServiceProvider.GetService<IViewModelDispatcher>();
            lock (RestorationLocker)
            {
                if (_viewModel != null)
                    return new MementoResult(_viewModel, serializationContext);

                if (!serializationContext.Metadata.Get(SerializationMetadata.NoCache) && Metadata.TryGet(ViewModelMetadata.Id, out var id))
                {
                    _viewModel = dispatcher.TryGetViewModel(id, serializationContext.Metadata);
                    if (_viewModel != null)
                        return new MementoResult(_viewModel, serializationContext);
                }

                _viewModel = RestoreInternal(serializationContext);
                dispatcher.OnLifecycleChanged(_viewModel, ViewModelLifecycleState.Restoring, serializationContext.Metadata);
                RestoreInternal(_viewModel);
                OnRestoringInternal(_viewModel, serializationContext);
                dispatcher.OnLifecycleChanged(_viewModel, ViewModelLifecycleState.Restored, serializationContext.Metadata);
                return new MementoResult(_viewModel, serializationContext);
            }
        }

        #endregion

        #region Methods

        protected virtual void OnPreserveInternal(IViewModel viewModel, ISerializationContext serializationContext)
        {
        }

        protected virtual IViewModel RestoreInternal(ISerializationContext serializationContext)
        {
            return (IViewModel)serializationContext.ServiceProvider.GetService(ViewModelType);
        }

        protected virtual void OnRestoringInternal(IViewModel viewModel, ISerializationContext serializationContext)
        {
        }

        private void RestoreInternal(IViewModel viewModel)
        {
            var listeners = Metadata.GetListeners();
            foreach (var listener in listeners)
                viewModel.Metadata.AddListener(listener);
            viewModel.Metadata.Merge(Metadata);

            if (BusyListeners != null)
            {
                foreach (var busyListener in BusyListeners)
                {
                    if (busyListener != null)
                        viewModel.BusyIndicatorProvider.AddListener(busyListener);
                }
            }

            if (Subscribers != null)
            {
                foreach (var subscriber in Subscribers)
                {
                    if (subscriber.Subscriber != null)
                        viewModel.InternalMessenger.Subscribe(subscriber.Subscriber, subscriber.ExecutionMode);
                }
            }
        }

        #endregion
    }
}