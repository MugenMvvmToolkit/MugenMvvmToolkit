using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml.Serialization;
using MugenMvvm.Attributes;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Infrastructure.Serialization;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Models;

namespace MugenMvvm.ViewModels
{
    public abstract class ViewModelBase : NotifyPropertyChangedBase, IViewModel
    {
        #region Fields

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

        public IMessenger InternalMessenger
        {
            get
            {
                if (_internalMessenger == null && MugenExtensions.LazyInitializeLock(ref _internalMessenger, this, vm => vm.GetInternalMessenger(), this))
                    _internalMessenger
                !.Subscribe((IMessengerSubscriber)GetDispatcherHandler(), ThreadExecutionMode.Main);
                return _internalMessenger!;
            }
        }

        public IBusyIndicatorProvider BusyIndicatorProvider
        {
            get
            {
                if (_busyIndicatorProvider == null &&
                    MugenExtensions.LazyInitializeLock(ref _busyIndicatorProvider, this, vm => vm.GetBusyIndicatorProvider(), this))
                    _busyIndicatorProvider
                !.AddListener((IBusyIndicatorProviderListener)GetDispatcherHandler());
                return _busyIndicatorProvider!;
            }
        }

        public IObservableMetadataContext Metadata { get; }

        public bool IsBusy => BusyInfo != null;

        public IBusyInfo? BusyInfo => BusyIndicatorProvider.BusyInfo;

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _state, DisposedState) == DisposedState)
                return;
            GC.SuppressFinalize(this);
            //                if (Settings.DisposeCommands)
            //                    ReflectionExtensions.DisposeCommands(this);
            ClearPropertyChangedSubscribers();
            OnDisposeInternal(true);
            OnDispose(true);
            Disposed?.Invoke(this, EventArgs.Empty);
            Disposed = null!;

            _busyIndicatorProvider?.ClearBusy();
            _busyIndicatorProvider?.RemoveAllListeners();
            _internalMessenger?.UnsubscribeAll();
            //            ToolkitServiceProvider.ViewManager.CleanupViewAsync(this);            
            //            ToolkitServiceProvider.AttachedValueProvider.Clear(this);
            Metadata.Clear();
            CleanupWeakReference();
            Singleton<IViewModelDispatcher>.Instance.OnLifecycleChanged(this, ViewModelLifecycleState.Disposed, Default.MetadataContext);
        }

        public IMemento? GetMemento()
        {
            if (_memento == null)
                MugenExtensions.LazyInitialize(ref _memento, GetMementoInternal());
            return _memento;
        }

        public event Action<IDisposableObject, EventArgs> Disposed;

        #endregion

        #region Methods

        protected void Publish(object message, IMessengerContext? messengerContext = null)
        {
            _internalMessenger?.Publish(this, message, messengerContext);
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

        protected virtual IMessenger GetInternalMessenger()
        {
            return Singleton<IViewModelDispatcher>.Instance.GetMessenger(this, Default.MetadataContext);
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
            return new ViewModelHandler(this);
        }

        #endregion

        #region Nested types

        internal sealed class ViewModelHandler : DispatcherHandler, IBusyIndicatorProviderListener, IMessengerSubscriber, IObservableMetadataContextListener, IHasMemento
        {
            #region Constructors

            public ViewModelHandler(NotifyPropertyChangedBase target)
                : base(target)
            {
                OnContextChanged(Target.Metadata, null);
            }

            #endregion

            #region Properties

            public new ViewModelBase Target => (ViewModelBase)base.Target;

            private bool BroadcastAllMessages { get; set; }

            private BusyMessageHandlerType BusyMessageHandlerType { get; set; }

            #endregion

            #region Implementation of interfaces

            public void OnBeginBusy(IBusyInfo busyInfo)
            {
                Target.Publish(busyInfo);
            }

            public void OnBusyInfoChanged()
            {
                Target.OnPropertyChanged(Default.IsBusyChangedArgs);
                Target.OnPropertyChanged(Default.BusyInfoChangedArgs);
            }

            public IMemento? GetMemento()
            {
                return new ViewModelHandlerMemento(this);
            }

            public bool Equals(IMessengerSubscriber other)
            {
                if (ReferenceEquals(null, other))
                    return false;
                if (ReferenceEquals(this, other))
                    return true;
                return other is ViewModelHandler handler && ReferenceEquals(Target, handler.Target);
            }

            public SubscriberResult Handle(object sender, object message, IMessengerContext messengerContext)
            {
                if (ReferenceEquals(sender, Target))
                    return SubscriberResult.Ignored;

                if (message is IBusyToken busyToken)
                {
                    var messageMode = BusyMessageHandlerType;
                    if (messageMode.HasFlagEx(BusyMessageHandlerType.Handle))
                        Target.BusyIndicatorProvider.Begin(busyToken);
                    if (messageMode.HasFlagEx(BusyMessageHandlerType.NotifySubscribers))
                        Target._internalMessenger?.Publish(sender, busyToken, messengerContext);
                }
                else if (BroadcastAllMessages || message is IBroadcastMessage)
                    Target._internalMessenger?.Publish(sender, message, messengerContext);

                return SubscriberResult.Handled;
            }

            public void OnContextChanged(IObservableMetadataContext metadataContext, IMetadataContextKey? key)
            {
                if (key == null || key.Equals(ViewModelMetadataKeys.BroadcastAllMessages))
                    BroadcastAllMessages = Target.Metadata.Get(ViewModelMetadataKeys.BroadcastAllMessages);
                if (key == null || key.Equals(ViewModelMetadataKeys.BusyMessageHandlerType))
                    BusyMessageHandlerType = Target.Metadata.Get(ViewModelMetadataKeys.BusyMessageHandlerType, BusyMessageHandlerType.Handle);
            }

            #endregion

            #region Methods

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                if (obj is ViewModelHandler handler)
                    return Equals(handler);
                return false;
            }

            public override int GetHashCode()
            {
                return Target.GetHashCode();
            }

            public override string ToString()
            {
                return Target.ToString();
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

            [DataMember(Name = "C")]
            protected internal IObservableMetadataContext Metadata;

            [DataMember(Name = "T")]
            internal Type? ViewModelType;

            [DataMember(Name = "S")]
            internal IList<MessengerSubscriberInfo>? Subscribers;

            [DataMember(Name = "B")]
            internal IList<IBusyIndicatorProviderListener>? BusyListeners;

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
                Metadata = _viewModel.Metadata;
                if (_viewModel is ViewModelBase vm)
                {
                    Subscribers = vm._internalMessenger?.GetSubscribers().ToSerializable(serializationContext.Serializer);
                    BusyListeners = vm._busyIndicatorProvider?.GetListeners().ToSerializable(serializationContext.Serializer);
                }
                else
                {
                    Subscribers = _viewModel.InternalMessenger.GetSubscribers().ToSerializable(serializationContext.Serializer);
                    BusyListeners = _viewModel.BusyIndicatorProvider?.GetListeners().ToSerializable(serializationContext.Serializer);
                }
                OnPreserveInternal(_viewModel!, serializationContext);
            }

            public IMementoResult Restore(ISerializationContext serializationContext)
            {
                Should.NotBeNull(Metadata, nameof(Metadata));
                Should.NotBeNull(ViewModelType, nameof(ViewModelType));
                if (_viewModel != null)
                    return new MementoResult(_viewModel, serializationContext.Metadata);

                var dispatcher = Singleton<IViewModelDispatcher>.Instance;
                lock (RestorationLocker)
                {
                    if (_viewModel != null)
                        return new MementoResult(_viewModel, serializationContext.Metadata);

                    if (Metadata.TryGet(ViewModelMetadataKeys.Id, out var id))
                    {
                        _viewModel = dispatcher.GetViewModelById(id, serializationContext.Metadata);
                        if (_viewModel != null)
                            return new MementoResult(_viewModel, serializationContext.Metadata);
                    }

                    _viewModel = RestoreInternal(serializationContext);
                    dispatcher.OnLifecycleChanged(_viewModel, ViewModelLifecycleState.Restoring, serializationContext.Metadata);
                    RestoreInternal(_viewModel);
                    OnRestoringInternal(_viewModel, serializationContext);
                    dispatcher.OnLifecycleChanged(_viewModel, ViewModelLifecycleState.Restored, serializationContext.Metadata);
                    return new MementoResult(_viewModel, serializationContext.Metadata);
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

        [Serializable]
        [DataContract(Namespace = BuildConstants.DataContractNamespace)]
        [Preserve(Conditional = true, AllMembers = true)]
        public sealed class ViewModelHandlerMemento : IMemento
        {
            [DataMember(Name = "V")]
            public readonly ViewModelBase ViewModel;

            #region Constructors

            internal ViewModelHandlerMemento(ViewModelHandler handler)
            {
                ViewModel = handler.Target;
            }

            internal ViewModelHandlerMemento()
            {
            }

            #endregion

            #region Properties

            [IgnoreDataMember]
            public Type TargetType => typeof(ViewModelHandler);

            #endregion

            #region Implementation of interfaces

            public void Preserve(ISerializationContext serializationContext)
            {
            }

            public IMementoResult Restore(ISerializationContext serializationContext)
            {
                Should.NotBeNull(ViewModel, nameof(ViewModel));
                return new MementoResult(ViewModel.GetDispatcherHandler(), serializationContext.Metadata);
            }

            #endregion
        }

        #endregion
    }
}