using System;
using System.ComponentModel;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.Models;

namespace MugenMvvm.ViewModels
{
    public abstract class ViewModelBase : NotifyPropertyChangedBase, IViewModelBase, IHasService<IBusyManager>, IHasService<IViewModelManager>, IHasService<IMessenger>,
        IBusyManagerListener, IHasDisposeCallback, IValueHolder<IServiceProvider>, IServiceProvider
    {
        private readonly IViewModelManager? _viewModelManager;
        private IBusyManager? _busyManager;
        private ListInternal<ActionToken> _disposeTokens;
        private IMetadataContext? _metadata;
        private IMessenger? _messenger;
        private IServiceProvider? _serviceProvider;

        protected ViewModelBase(IViewModelManager? viewModelManager = null, IReadOnlyMetadataContext? metadata = null)
        {
            _viewModelManager = viewModelManager;
            this.NotifyLifecycleChanged(ViewModelLifecycleState.Created, manager: _viewModelManager, metadata: metadata);
        }

        public bool IsBusy => BusyToken != null;

        public IBusyToken? BusyToken => _busyManager?.TryGetToken<object?>(null, (_, token, _) => !token.IsSuspended && !token.IsCompleted);

        public IMugenApplication Application => MugenExtensions.DefaultIfNull<IMugenApplication>(null, this);

        public IBusyManager BusyManager => _busyManager ?? this.InitializeService(ref _busyManager, null, (vm, manager) => manager.AddComponent(vm));

        public IMessenger Messenger => _messenger ?? this.InitializeService(ref _messenger);

        public bool IsDisposed { get; private set; }

        public bool HasMetadata => !_metadata.IsNullOrEmpty();

        public IMetadataContext Metadata => _metadata ?? this.InitializeService(ref _metadata);

        protected IViewModelManager ViewModelManager => _viewModelManager.DefaultIfNull();

        internal bool IsInitialized { get; set; }

        IServiceProvider? IValueHolder<IServiceProvider>.Value
        {
            get => _serviceProvider;
            set => _serviceProvider = value;
        }

        public virtual IViewModelBase GetViewModel(Type viewModelType, IReadOnlyMetadataContext? metadata = null) =>
            ViewModelManager.GetViewModel(viewModelType, metadata.WithValue(ViewModelMetadata.ParentViewModel, this));

        public T GetViewModel<T>(IReadOnlyMetadataContext? metadata = null) where T : IViewModelBase => (T)GetViewModel(typeof(T), metadata);

        public void RegisterDisposeToken(IDisposable disposable) => RegisterDisposeToken(ActionToken.FromDisposable(disposable));

        public void Dispose()
        {
            if (IsDisposed || !CanDispose())
                return;
            lock (this)
            {
                if (IsDisposed)
                    return;
                IsDisposed = true;
            }

            OnDispose(true);
        }

        public void RegisterDisposeToken(ActionToken token)
        {
            if (token.IsEmpty)
                return;

            if (IsDisposed)
            {
                token.Dispose();
                return;
            }

            var inline = false;
            lock (this)
            {
                if (IsDisposed)
                    inline = true;
                else
                {
                    if (_disposeTokens.IsEmpty)
                        _disposeTokens = new ListInternal<ActionToken>(2);
                    _disposeTokens.Add(token);
                }
            }

            if (inline)
                token.Dispose();
        }

        protected virtual void OnBeginBusy(IBusyManager busyManager, IBusyToken busyToken, IReadOnlyMetadataContext? metadata)
        {
        }

        protected virtual void OnBusyStateChanged(IBusyManager busyManager, IReadOnlyMetadataContext? metadata)
        {
            OnPropertyChanged(Default.IsBusyPropertyChangedArgs);
            OnPropertyChanged(Default.BusyTokenPropertyChangedArgs);
        }

        protected virtual bool CanDispose() => true;

        protected virtual void OnDispose(bool disposing)
        {
            if (!disposing)
            {
                this.NotifyLifecycleChanged(ViewModelLifecycleState.Finalized, manager: _viewModelManager);
                return;
            }

            this.NotifyLifecycleChanged(ViewModelLifecycleState.Disposing, manager: _viewModelManager);
            if (!_disposeTokens.IsEmpty)
            {
                for (var i = 0; i < _disposeTokens.Count; i++)
                    _disposeTokens.Items[i].Dispose();
                _disposeTokens = default;
            }

            ClearPropertyChangedSubscribers();
            this.NotifyLifecycleChanged(ViewModelLifecycleState.Disposed, manager: _viewModelManager);
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);
            if (!IsSuspended)
                _messenger?.Publish(this, args);
        }

        void IBusyManagerListener.OnBeginBusy(IBusyManager busyManager, IBusyToken busyToken, IReadOnlyMetadataContext? metadata) => OnBeginBusy(busyManager, busyToken, metadata);

        void IBusyManagerListener.OnBusyStateChanged(IBusyManager busyManager, IReadOnlyMetadataContext? metadata) => OnBusyStateChanged(busyManager, metadata);

        IBusyManager? IHasService<IBusyManager>.GetService(bool optional) => optional ? _busyManager : BusyManager;

        IMessenger? IHasService<IMessenger>.GetService(bool optional) => optional ? _messenger : Messenger;

        IViewModelManager IHasService<IViewModelManager>.GetService(bool optional) => ViewModelManager;

        object? IServiceProvider.GetService(Type serviceType) => _serviceProvider?.GetService(serviceType);
    }
}