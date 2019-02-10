using System;
using System.ComponentModel;
using System.Threading;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.BusyIndicator;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Infrastructure.ViewModels;
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
            Metadata = metadata ?? ViewModelDispatcher.GetMetadataContext(this, Default.MetadataContext);
            ViewModelDispatcher.OnLifecycleChanged(this, ViewModelLifecycleState.Created, Default.MetadataContext);
        }

        protected ViewModelBase()
            : this(null)
        {
        }

        #endregion

        #region Properties

        public IMessenger InternalMessenger => GetInternalMessenger(true);

        public IBusyIndicatorProvider BusyIndicatorProvider => GetBusyIndicatorProvider(true);

        public IObservableMetadataContext Metadata { get; }

        public bool IsBusy => BusyInfo != null;

        public IBusyInfo? BusyInfo => GetBusyIndicatorProvider(false)?.BusyInfo;

        protected static IViewModelDispatcher ViewModelDispatcher => Service<IViewModelDispatcher>.Instance;

        #endregion

        #region Implementation of interfaces

        public void Dispose()
        {
            if (Interlocked.Exchange(ref _state, DisposedState) == DisposedState)
                return;
            GC.SuppressFinalize(this);
            ViewModelDispatcher.OnLifecycleChanged(this, ViewModelLifecycleState.Disposing, Default.MetadataContext);
            OnDisposeInternal(true);
            OnDispose(true);
            ClearPropertyChangedSubscribers();
            GetBusyIndicatorProvider(false)?.ClearBusy();
            GetBusyIndicatorProvider(false)?.RemoveAllListeners();
            GetInternalMessenger(false)?.UnsubscribeAll();
            Subscriber = null;
            ViewModelDispatcher.OnLifecycleChanged(this, ViewModelLifecycleState.Disposed, Default.MetadataContext);
            Metadata.RemoveAllListeners();
            Metadata.Clear();
            CleanupWeakReference();
            //todo cleanup, all listeners
        }

        public IMemento? GetMemento()
        {
            if (_memento == null)
                MugenExtensions.LazyInitialize(ref _memento, GetMementoInternal());
            return _memento;
        }

        public void Publish(object sender, object message, IMessengerContext? messengerContext = null)
        {
            Should.NotBeNull(sender, nameof(sender));
            Should.NotBeNull(message, nameof(message));
            PublishInternal(sender, message, messengerContext);
        }

        #endregion

        #region Methods

        public void Publish(object message, IMessengerContext? messengerContext = null)
        {
            Should.NotBeNull(message, nameof(message));
            PublishInternal(this, message, messengerContext);
        }

        public void InvalidateCommands()
        {
            Publish(Default.EmptyPropertyChangedArgs);
        }

        protected void OnFinalized()
        {
            _state = DisposedState;
            OnDisposeInternal(false);
            OnDispose(false);
            ViewModelDispatcher.OnLifecycleChanged(this, ViewModelLifecycleState.Finalized, Default.MetadataContext);
        }

        protected internal IBusyIndicatorProvider? GetBusyIndicatorProvider(bool initializeIfNeed)
        {
            return GetBusyIndicatorProvider(initializeIfNeed, initializeIfNeed ? (IBusyIndicatorProviderListener)GetDispatcherHandler() : null);
        }

        protected virtual IBusyIndicatorProvider? GetBusyIndicatorProvider(bool initializeIfNeed, IBusyIndicatorProviderListener listener)
        {
            if (!initializeIfNeed)
                return _busyIndicatorProvider;

            if (_busyIndicatorProvider == null && MugenExtensions.LazyInitializeLock(ref _busyIndicatorProvider, this,
                    vm => ViewModelDispatcher.GetBusyIndicatorProvider(vm, Default.MetadataContext), this))
            {
                if (_busyIndicatorProvider is BusyIndicatorProvider provider)
                    provider.InternalListener = listener;
                else
                    _busyIndicatorProvider!.AddListener(listener);
            }
            return _busyIndicatorProvider!;
        }

        protected internal virtual IMessenger? GetInternalMessenger(bool initializeIfNeed)
        {
            if (!initializeIfNeed)
                return _internalMessenger;
            if (_internalMessenger == null)
                MugenExtensions.LazyInitializeLock(ref _internalMessenger, this, vm => ViewModelDispatcher.GetMessenger(vm, Default.MetadataContext), this);
            return _internalMessenger!;
        }

        protected virtual void PublishInternal(object sender, object message, IMessengerContext? messengerContext)
        {
            GetInternalMessenger(false)?.Publish(sender, message, messengerContext);
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

        protected override void RaisePropertyChangedEvent(PropertyChangedEventArgs args)
        {
            base.RaisePropertyChangedEvent(args);
            Publish(args);
        }

        private protected override DispatcherHandler CreateDispatcherHandler()
        {
            return new ViewModelDispatcherHandler(this);
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

        #endregion
    }
}