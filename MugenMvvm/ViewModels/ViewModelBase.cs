using System;
using System.Collections.Generic;
using System.ComponentModel;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Busy.Components;
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
        IBusyManagerListener, IHasDisposeCallback
    {
        private readonly IViewModelManager? _viewModelManager;
        private IBusyManager? _busyManager;
        private List<ActionToken>? _disposeTokens;
        private IMetadataContext? _metadata;
        private IMessenger? _messenger;

        protected ViewModelBase(IViewModelManager? viewModelManager = null)
        {
            _viewModelManager = viewModelManager;
            this.NotifyLifecycleChanged(ViewModelLifecycleState.Created, manager: _viewModelManager);
        }

        public bool IsBusy => BusyToken != null;

        public IBusyToken? BusyToken => _busyManager?.TryGetToken(this, (_, token, _) => !token.IsSuspended && !token.IsCompleted);

        public IBusyManager BusyManager => this.InitializeService(ref _busyManager, null, (vm, manager) => manager.AddComponent(vm), viewModelManager: _viewModelManager);

        public IMessenger Messenger => this.InitializeService(ref _messenger, viewModelManager: _viewModelManager);

        public bool IsDisposed { get; private set; }

        public bool HasMetadata => !_metadata.IsNullOrEmpty();

        public IMetadataContext Metadata => this.InitializeService(ref _metadata, viewModelManager: _viewModelManager);

        protected IViewModelManager ViewModelManager => _viewModelManager.DefaultIfNull();

        IBusyManager IHasService<IBusyManager>.Service => BusyManager;

        IMessenger? IHasService<IMessenger>.ServiceOptional => _messenger;

        IMessenger IHasService<IMessenger>.Service => Messenger;

        IBusyManager? IHasService<IBusyManager>.ServiceOptional => _busyManager;

        IViewModelManager IHasService<IViewModelManager>.Service => ViewModelManager;

        IViewModelManager? IHasService<IViewModelManager>.ServiceOptional => _viewModelManager;

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
                    _disposeTokens ??= new List<ActionToken>(2);
                    _disposeTokens.Add(token);
                }
            }

            if (inline)
                token.Dispose();
        }

        protected virtual IViewModelBase GetViewModel(Type viewModelType, IReadOnlyMetadataContext? metadata = null)
        {
            var vm = ViewModelManager.GetViewModel(viewModelType, metadata.WithValue(ViewModelMetadata.ParentViewModel, this));
            vm.Metadata.Set(ViewModelMetadata.ParentViewModel, this);
            return vm;
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
            if (_disposeTokens != null)
            {
                for (var i = 0; i < _disposeTokens.Count; i++)
                    _disposeTokens[i].Dispose();
                _disposeTokens = null;
            }

            ClearPropertyChangedSubscribers();
            this.NotifyLifecycleChanged(ViewModelLifecycleState.Disposed, manager: _viewModelManager);
        }

        protected override void OnPropertyChangedInternal(PropertyChangedEventArgs args)
        {
            base.OnPropertyChangedInternal(args);
            _messenger?.Publish(this, args);
        }

        protected T GetViewModel<T>(IReadOnlyMetadataContext? metadata = null) where T : IViewModelBase => (T) GetViewModel(typeof(T), metadata);

        void IBusyManagerListener.OnBeginBusy(IBusyManager busyManager, IBusyToken busyToken, IReadOnlyMetadataContext? metadata) => OnBeginBusy(busyManager, busyToken, metadata);

        void IBusyManagerListener.OnBusyStateChanged(IBusyManager busyManager, IReadOnlyMetadataContext? metadata) => OnBusyStateChanged(busyManager, metadata);
    }
}