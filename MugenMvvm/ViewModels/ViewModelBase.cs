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
    public abstract class ViewModelBase : NotifyPropertyChangedBase, IViewModelBase, IHasService<IBusyManager>, IHasService<IViewModelManager>, IBusyManagerListener, IHasDisposeCallback
    {
        #region Fields

        private readonly IViewModelManager? _viewModelManager;
        private IBusyManager? _busyManager;
        private List<ActionToken>? _disposeTokens;
        private IMetadataContext? _metadata;

        #endregion

        #region Constructors

        protected ViewModelBase(IViewModelManager? viewModelManager = null)
        {
            _viewModelManager = viewModelManager;
            this.NotifyLifecycleChanged(ViewModelLifecycleState.Created, manager: _viewModelManager);
        }

        #endregion

        #region Properties

        public bool IsBusy => BusyToken != null;

        public IBusyToken? BusyToken => _busyManager?.TryGetToken(this, (_, token, ___) => !token.IsSuspended && !token.IsCompleted);

        public bool HasMetadata => !_metadata.IsNullOrEmpty();

        public IMetadataContext Metadata => this.InitializeService(ref _metadata);

        public IBusyManager BusyManager => this.InitializeService(ref _busyManager, null, (vm, manager) => manager.AddComponent(vm));

        public bool IsDisposed { get; private set; }

        protected IViewModelManager ViewModelManager => _viewModelManager.DefaultIfNull();

        IBusyManager IHasService<IBusyManager>.Service => BusyManager;

        IViewModelManager IHasService<IViewModelManager>.Service => ViewModelManager;

        IViewModelManager? IHasService<IViewModelManager>.ServiceOptional => _viewModelManager;

        IBusyManager? IHasService<IBusyManager>.ServiceOptional => _busyManager;

        #endregion

        #region Implementation of interfaces

        void IBusyManagerListener.OnBeginBusy(IBusyManager busyManager, IBusyToken busyToken, IReadOnlyMetadataContext? metadata) => OnBeginBusy(busyManager, busyToken, metadata);

        void IBusyManagerListener.OnBusyStateChanged(IBusyManager busyManager, IReadOnlyMetadataContext? metadata) => OnBusyStateChanged(busyManager, metadata);

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

        #endregion

        #region Methods

        protected T GetViewModel<T>(IReadOnlyMetadataContext? metadata = null) where T : IViewModelBase => (T) GetViewModel(typeof(T), metadata);

        protected virtual IViewModelBase GetViewModel(Type viewModelType, IReadOnlyMetadataContext? metadata = null)
            => ViewModelManager.GetViewModel(viewModelType, metadata.WithValue(ViewModelMetadata.ParentViewModel, this));

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
                this.NotifyLifecycleChanged(ViewModelLifecycleState.Finalized, manager: ViewModelManager);
                return;
            }

            this.NotifyLifecycleChanged(ViewModelLifecycleState.Disposing, manager: ViewModelManager);
            if (_disposeTokens != null)
            {
                for (var i = 0; i < _disposeTokens.Count; i++)
                    _disposeTokens[i].Dispose();
                _disposeTokens = null;
            }

            ClearPropertyChangedSubscribers();
            this.NotifyLifecycleChanged(ViewModelLifecycleState.Disposed, manager: ViewModelManager);
        }

        protected override void OnPropertyChangedInternal(PropertyChangedEventArgs args)
        {
            base.OnPropertyChangedInternal(args);
            this.TryGetService<IMessenger>(true)?.Publish(this, args);
        }

        #endregion
    }
}