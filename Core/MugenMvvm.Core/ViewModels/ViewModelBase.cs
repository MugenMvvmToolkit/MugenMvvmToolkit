using System;
using System.Runtime.Serialization;
using System.Threading;
using System.Xml.Serialization;
using MugenMvvm.Infrastructure;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Models;
using MugenMvvm.Attributes;

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

        protected ViewModelBase(IContext? metadata)
        {
            var dispatcher = Singleton<IViewModelDispatcher>.Instance;
            Metadata = metadata ?? dispatcher.GetMetadataContext(this, Default.Context);
            dispatcher.OnLifecycleChanged(this, ViewModelLifecycleState.Created, Default.Context);
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
                    _internalMessenger!.Subscribe((IMessengerSubscriber)GetDispatcherHandler(), ThreadExecutionMode.Main);
                return _internalMessenger!;
            }
        }

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

        public IContext Metadata { get; }

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
            Singleton<IViewModelDispatcher>.Instance.OnLifecycleChanged(this, ViewModelLifecycleState.Disposed, Default.Context);
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
            Singleton<IViewModelDispatcher>.Instance.OnLifecycleChanged(this, ViewModelLifecycleState.Finalized, Default.Context);
        }

        protected virtual IBusyIndicatorProvider GetBusyIndicatorProvider()
        {
            return Singleton<IViewModelDispatcher>.Instance.GetBusyIndicatorProvider(this, Default.Context);
        }

        protected virtual IMessenger GetInternalMessenger()
        {
            return Singleton<IViewModelDispatcher>.Instance.GetMessenger(this, Default.Context);
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

        #endregion

        #region Nested types

        private protected class ViewModelDispatcherHandler : DispatcherHandler, IBusyIndicatorProviderListener, IMessengerSubscriber
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

            public void OnBeginBusy(IBusyInfo busyInfo)
            {
                Target.Publish(busyInfo);
            }

            public void OnBusyInfoChanged()
            {
                Target.OnPropertyChanged(Default.IsBusyChangedArgs);
                Target.OnPropertyChanged(Default.BusyInfoChangedArgs);
            }

            public bool Equals(IMessengerSubscriber other)
            {
                if (ReferenceEquals(null, other))
                    return false;
                if (ReferenceEquals(this, other))
                    return true;
                return other is ViewModelDispatcherHandler handler && ReferenceEquals(Target, handler.Target);
            }

            public SubscriberResult Handle(object sender, object message, IMessengerContext messengerContext)
            {
                if (ReferenceEquals(sender, Target))
                    return SubscriberResult.Ignored;

                //                if (message is IBusyToken busyToken)
                //                {
                //                    Target.BusyIndicatorProvider.Begin(busyToken);
                //                }
                //                else if (Target.BroadcastAllMessages || message is IBroadcastMessage)
                //                    Target._internalMessenger?.Publish(sender, message, messengerContext);

                return SubscriberResult.Handled;
            }

            #endregion

            #region Methods

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj))
                    return false;
                if (ReferenceEquals(this, obj))
                    return true;
                if (obj is ViewModelDispatcherHandler handler)
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
            protected internal IContext Context;

            [DataMember(Name = "T")]
            internal Type ViewModelType;

            protected static readonly object RestorationLocker;

            #endregion

            #region Constructors

            static ViewModelMemento()
            {
                RestorationLocker = new object();
            }

            internal ViewModelMemento()
            {
            }

            protected internal ViewModelMemento(IViewModel viewModel)
            {
                _viewModel = viewModel;
                Context = viewModel.Metadata;
                ViewModelType = viewModel.GetType();
            }

            #endregion

            #region Properties

            [IgnoreDataMember]
            public Type TargetType => ViewModelType;

            #endregion

            #region Implementation of interfaces

            public void Preserve(ISerializationContext serializationContext)
            {
                if (_viewModel == null)
                    return;
                Context = _viewModel.Metadata;
                OnPreserveInternal(_viewModel!, serializationContext);
            }

            public IMementoResult Restore(ISerializationContext serializationContext)
            {
                Should.NotBeNull(Context, nameof(Context));
                if (serializationContext.Mode != SerializationMode.Clone && _viewModel != null)
                    return new MementoResult(_viewModel, serializationContext.Metadata);

                var dispatcher = Singleton<IViewModelDispatcher>.Instance;
                lock (RestorationLocker)
                {
                    if (serializationContext.Mode != SerializationMode.Clone)
                    {
                        if (_viewModel != null)
                            return new MementoResult(_viewModel, serializationContext.Metadata);

                        if (Context.TryGet(ViewModelMetadataKeys.Id, out var id))
                        {
                            _viewModel = dispatcher.TryGetViewModelById(id, serializationContext.Metadata);
                            if (_viewModel != null)
                                return new MementoResult(_viewModel, serializationContext.Metadata);
                        }
                    }

                    _viewModel = RestoreInternal(serializationContext);
                    _viewModel.Metadata.Merge(Context);
                    _viewModel.Metadata.Set(ViewModelMetadataKeys.LifecycleState, ViewModelLifecycleState.Restored);
                    OnRestoredInternal(_viewModel, serializationContext);
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
                return (IViewModel)serializationContext.ServiceProvider.GetService(ViewModelType);//todo fix to viewmodel provider
            }

            protected virtual void OnRestoredInternal(IViewModel viewModel, ISerializationContext serializationContext)
            {
            }

            #endregion
        }

        #endregion
    }
}