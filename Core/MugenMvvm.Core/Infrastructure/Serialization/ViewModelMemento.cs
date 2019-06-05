using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Infrastructure;
using MugenMvvm.Metadata;

namespace MugenMvvm.Infrastructure.Serialization
{
    [Serializable]
    [DataContract(Namespace = BuildConstants.DataContractNamespace)]
    [Preserve(Conditional = true, AllMembers = true)]
    public class ViewModelMemento : IMemento //todo saving/restoring state/ cancel restore/ split methods to control default behavior
    {
        #region Fields

        [IgnoreDataMember]
        [XmlIgnore]
        [NonSerialized]
        private IViewModelBase? _viewModel;

        [DataMember(Name = "B")]
        protected internal IList<IBusyIndicatorProviderListener?>? BusyListeners;

        [DataMember(Name = "C")]
        protected internal IObservableMetadataContext? Metadata;

        [DataMember(Name = "N")]
        protected internal bool NoState;

        [DataMember(Name = "S")]
        protected internal IList<MessengerSubscriberInfo>? Subscribers;

        [DataMember(Name = "T")]
        protected internal Type? ViewModelType;

        protected static readonly object RestorationLocker = new object();

        #endregion

        #region Constructors

        internal ViewModelMemento()
        {
        }

        public ViewModelMemento(IViewModelBase viewModel)
        {
            _viewModel = viewModel;
            Metadata = viewModel.Metadata;
            ViewModelType = viewModel.GetType();
        }

        #endregion

        #region Properties

        [IgnoreDataMember]
        [XmlIgnore]
        public Type TargetType => ViewModelType!;

        #endregion

        #region Implementation of interfaces

        public void Preserve(ISerializationContext serializationContext)
        {
            if (_viewModel == null)
                return;
            var dispatcher = serializationContext.ServiceProvider.GetService<IViewModelDispatcher>();
            dispatcher.OnLifecycleChanged(_viewModel, ViewModelLifecycleState.Preserving, serializationContext.Metadata);

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
                Subscribers = _viewModel.TryGetServiceOptional<IMessenger>()?.GetSubscribers().ToSerializable(serializationContext.Serializer);
                BusyListeners = _viewModel.TryGetServiceOptional<IBusyIndicatorProvider>()?.Listeners.GetItems().ToSerializable(serializationContext.Serializer);
            }

            OnPreserveInternal(_viewModel!, NoState, serializationContext);
            dispatcher.OnLifecycleChanged(_viewModel, ViewModelLifecycleState.Preserved, serializationContext.Metadata);
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

                if (!serializationContext.Metadata.Get(SerializationMetadata.NoCache))
                {
                    _viewModel = dispatcher.TryGetViewModel(Metadata);
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

        protected virtual void OnPreserveInternal(IViewModelBase viewModel, bool noState, ISerializationContext serializationContext)
        {
        }

        protected virtual IViewModelBase RestoreInternal(ISerializationContext serializationContext)
        {
            return (IViewModelBase)serializationContext.ServiceProvider.GetService(ViewModelType);
        }

        protected virtual void OnRestoringInternal(IViewModelBase viewModel, ISerializationContext serializationContext)
        {
        }

        private void RestoreInternal(IViewModelBase viewModel)
        {
            var listeners = Metadata!.Listeners.GetItems();
            foreach (var listener in listeners)
                viewModel.Metadata.AddListener(listener);
            viewModel.Metadata.Merge(Metadata);

            if (BusyListeners != null && viewModel is IHasService<IBusyIndicatorProvider> hasBusyIndicatorProvider)
            {
                foreach (var busyListener in BusyListeners)
                {
                    if (busyListener != null)
                        hasBusyIndicatorProvider.Service.AddListener(busyListener);
                }
            }

            if (Subscribers != null && viewModel is IHasService<IMessenger> hasMessenger)
            {
                foreach (var subscriber in Subscribers)
                {
                    if (subscriber.Subscriber != null)
                        hasMessenger.Service.Subscribe(subscriber.Subscriber, subscriber.ExecutionMode, Default.Metadata);
                }
            }
        }

        #endregion
    }
}