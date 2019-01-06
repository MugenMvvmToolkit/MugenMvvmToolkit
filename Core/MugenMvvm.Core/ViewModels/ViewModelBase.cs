using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Infrastructure.Serialization;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Infrastructure;
using MugenMvvm.Models;

namespace MugenMvvm.ViewModels
{
    public abstract class ViewModelBase : NotifyPropertyChangedBase, IViewModel
    {
        #region Fields

        internal ViewModelMessengerSubscriber? Subscriber;
        private IBusyIndicatorProvider? _busyIndicatorProvider;
        private IMessenger? _internalMessenger;
        private IMemento? _memento;
        private int _state;

        private const int DisposedState = 1;

        #endregion

        #region Constructors

        protected ViewModelBase(IObservableMetadataContext? metadata)
        {
            var dispatcher = Singleton<IViewModelDispatcher>.Instance;
            Metadata = metadata ?? dispatcher.GetMetadataContext(this, Default.MetadataContext);
            dispatcher.OnLifecycleChanged(this, ViewModelLifecycleState.Created, Default.MetadataContext);
        }

        protected ViewModelBase()
            : this(null)
        {
        }

        #endregion

        #region Properties

        public bool IsDisposed => _state == DisposedState;

        public IBusyIndicatorProvider BusyIndicatorProvider
        {
            get
            {
                if (_busyIndicatorProvider == null &&
                    MugenExtensions.LazyInitializeLock(ref _busyIndicatorProvider, this, vm => vm.GetBusyIndicatorProvider(), this))
                    _busyIndicatorProvider!.AddListener((IBusyIndicatorProviderListener)GetDispatcherHandler());
                return _busyIndicatorProvider!;
            }
        }

        public IObservableMetadataContext Metadata { get; }

        public bool IsBusy => BusyInfo != null;

        public IBusyInfo? BusyInfo => _busyIndicatorProvider?.BusyInfo;

        #endregion

        #region Events

        public event Action<IViewModel, EventArgs> Disposed;

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _state, DisposedState) == DisposedState)
                return;
            GC.SuppressFinalize(this);
            ClearPropertyChangedSubscribers();
            OnDisposeInternal(true);
            OnDispose(true);
            Disposed?.Invoke(this, EventArgs.Empty);
            Disposed = null!;

            _busyIndicatorProvider?.ClearBusy();
            _busyIndicatorProvider?.RemoveAllListeners();
            GetInternalMessenger(false)?.UnsubscribeAll();
            //todo cleanup
            Singleton<IViewModelDispatcher>.Instance.OnLifecycleChanged(this, ViewModelLifecycleState.Disposed, Default.MetadataContext);
            CleanupWeakReference();
        }

        public IMemento? GetMemento()
        {
            if (_memento == null)
                MugenExtensions.LazyInitialize(ref _memento, GetMementoInternal());
            return _memento;
        }

        public void Publish(object message, IMessengerContext? messengerContext = null)
        {
            Should.NotBeNull(message, nameof(message));
            PublishInternal(this, message, messengerContext);
        }

        public void Publish(object sender, object message, IMessengerContext? messengerContext = null)
        {
            Should.NotBeNull(sender, nameof(sender));
            Should.NotBeNull(message, nameof(message));
            PublishInternal(sender, message, messengerContext);
        }

        public void Subscribe(object item, ThreadExecutionMode? executionMode = null)
        {
            Should.NotBeNull(item, nameof(item));
            SubscribeInternal(item, executionMode);
        }

        public void Unsubscribe(object item)
        {
            Should.NotBeNull(item, nameof(item));
            UnsubscribeInternal(item);
        }

        #endregion

        #region Methods

        public void InvalidateCommands()
        {
            Publish(Default.EmptyPropertyChangedArgs);
        }

        protected void OnFinalized()
        {
            _state = DisposedState;
            OnDisposeInternal(false);
            OnDispose(false);
            Singleton<IViewModelDispatcher>.Instance.OnLifecycleChanged(this, ViewModelLifecycleState.Finalized, Default.MetadataContext);
        }

        protected virtual IBusyIndicatorProvider GetBusyIndicatorProvider()
        {
            return Singleton<IViewModelDispatcher>.Instance.GetBusyIndicatorProvider(this, Default.MetadataContext);
        }

        protected virtual IMessenger? GetInternalMessenger(bool initializeIfNeed)
        {
            if (!initializeIfNeed)
                return _internalMessenger;
            if (_internalMessenger == null)
                MugenExtensions.LazyInitializeLock(ref _internalMessenger, this, vm => Singleton<IViewModelDispatcher>.Instance.GetMessenger(vm, Default.MetadataContext), this);
            return _internalMessenger!;
        }

        protected virtual void PublishInternal(object sender, object message, IMessengerContext? messengerContext)
        {
            GetInternalMessenger(false)?.Publish(sender, message, messengerContext);
        }

        protected virtual void SubscribeInternal(object item, ThreadExecutionMode? executionMode)
        {
            if (item is IViewModel viewModel)
            {
                if (viewModel.Metadata.Get(ViewModelMetadata.BusyMessageHandlerType).HasFlagEx(BusyMessageHandlerType.Handle))
                {
                    var tokens = _busyIndicatorProvider?.GetTokens();
                    if (tokens != null)
                    {
                        for (var index = 0; index < tokens.Count; index++)
                            viewModel.BusyIndicatorProvider.Begin(tokens[index]);
                    }
                }
                GetInternalMessenger(true)!.Subscribe(ViewModelMessengerSubscriber.GetSubscriber(viewModel), executionMode);
            }

            if (item is IMessengerSubscriber subscriber)
                GetInternalMessenger(true)!.Subscribe(subscriber, executionMode);
            else if (item is IMessengerHandler handler)
                GetInternalMessenger(true)!.Subscribe(new MessengerHandlerSubscriber(handler), executionMode);
        }

        protected virtual void UnsubscribeInternal(object item)
        {
            if (item is IViewModel viewModel)
                GetInternalMessenger(false)?.Unsubscribe(ViewModelMessengerSubscriber.GetSubscriber(viewModel));

            if (item is IMessengerSubscriber subscriber)
                GetInternalMessenger(false)?.Unsubscribe(subscriber);
            else if (item is IMessengerHandler handler)
                GetInternalMessenger(false)?.Unsubscribe(new MessengerHandlerSubscriber(handler));
        }

        internal virtual void OnDisposeInternal(bool disposing)
        {
        }

        protected virtual void OnDispose(bool disposing)
        {
        }

        protected virtual IMemento GetMementoInternal()
        {
            return new ViewModelMemento(this);
        }

        private protected override DispatcherHandler CreateDispatcherHandler()
        {
            return new ViewModelDispatcherHandler(this);
        }

        protected override void RaisePropertyChangedEvent(PropertyChangedEventArgs args)
        {
            base.RaisePropertyChangedEvent(args);
            Publish(args);
        }

        #endregion

        #region Nested types

        private sealed class ViewModelDispatcherHandler : DispatcherHandler, IBusyIndicatorProviderListener
        {
            #region Constructors

            public ViewModelDispatcherHandler(NotifyPropertyChangedBase target)
                : base(target)
            {
            }

            #endregion

            #region Properties

            private new ViewModelBase Target => (ViewModelBase)base.Target;

            #endregion

            #region Implementation of interfaces

            public void OnBeginBusy(IBusyIndicatorProvider busyIndicatorProvider, IBusyInfo busyInfo)
            {
                Target.Publish(busyInfo);
            }

            public void OnBusyInfoChanged(IBusyIndicatorProvider busyIndicatorProvider)
            {
                Target.OnPropertyChanged(Default.IsBusyChangedArgs);
                Target.OnPropertyChanged(Default.BusyInfoChangedArgs);
            }

            #endregion
        }

        [Serializable]
        [DataContract(Namespace = BuildConstants.DataContractNamespace)]
        [Preserve(Conditional = true, AllMembers = true)]
        public class ViewModelMemento : IMemento
        {
            #region Fields

            [IgnoreDataMember]
            [XmlIgnore]
            private IViewModel? _viewModel;

            [DataMember(Name = "B")]
            internal IList<IBusyIndicatorProviderListener?> BusyListeners;

            [DataMember(Name = "C")]
            protected internal IObservableMetadataContext Metadata;

            [DataMember(Name = "S")]
            internal IList<MessengerSubscriberInfo> Subscribers;

            [DataMember(Name = "T")]
            internal Type? ViewModelType;

            [DataMember(Name = "N")]
            protected internal bool NoState { get; set; }

            protected static readonly object RestorationLocker;

            #endregion

            #region Constructors

            static ViewModelMemento()
            {
                RestorationLocker = new object();
            }

            protected internal ViewModelMemento()
            {
            }

            protected internal ViewModelMemento(IViewModel viewModel)
            {
                _viewModel = viewModel;
                Metadata = viewModel.Metadata;
                ViewModelType = viewModel.GetType();
            }

            #endregion

            #region Properties

            [IgnoreDataMember]
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
                        BusyListeners = vm._busyIndicatorProvider?.GetListeners().ToSerializable(serializationContext.Serializer);
                    }
                    else
                        BusyListeners = _viewModel.BusyIndicatorProvider?.GetListeners().ToSerializable(serializationContext.Serializer);
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

                var dispatcher = Singleton<IViewModelDispatcher>.Instance;
                lock (RestorationLocker)
                {
                    if (_viewModel != null)
                        return new MementoResult(_viewModel, serializationContext);

                    if (!serializationContext.Metadata.Get(SerializationMetadata.NoCache) && Metadata.TryGet(ViewModelMetadata.Id, out var id))
                    {
                        _viewModel = dispatcher.GetViewModelById(id, serializationContext.Metadata);
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

                if (Subscribers != null && viewModel is ViewModelBase vm)
                {
                    foreach (var subscriber in Subscribers)
                    {
                        if (subscriber.Subscriber != null)
                            vm.GetInternalMessenger(true)!.Subscribe(subscriber.Subscriber, subscriber.ExecutionMode);
                    }
                }
            }

            #endregion
        }

        #endregion
    }
}