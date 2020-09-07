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
using MugenMvvm.Models;

namespace MugenMvvm.ViewModels
{
    public abstract class ViewModelBase : NotifyPropertyChangedBase, IViewModelBase, IHasService<IBusyManager>, IBusyManagerListener, IDisposable
    {
        #region Fields

        private IBusyManager? _busyManager;
        private List<ActionToken>? _disposeTokens;
        private bool _isDisposed;
        private IMetadataContext? _metadata;

        #endregion

        #region Constructors

        protected ViewModelBase()
        {
            this.NotifyLifecycleChanged(ViewModelLifecycleState.Created);
        }

        #endregion

        #region Properties

        public bool IsBusy => BusyToken != null;

        public IBusyToken? BusyToken => _busyManager?.TryGetToken(this, (_, token, ___) => !token.IsSuspended && !token.IsCompleted);

        public bool HasMetadata => !_metadata.IsNullOrEmpty();

        public IMetadataContext Metadata => this.InitializeService(ref _metadata);

        public IBusyManager BusyManager => this.InitializeService(ref _busyManager, (vm, manager) => manager.AddComponent(vm));

        IBusyManager IHasService<IBusyManager>.Service => BusyManager;

        IBusyManager? IHasService<IBusyManager>.ServiceOptional => _busyManager;

        #endregion

        #region Implementation of interfaces

        void IBusyManagerListener.OnBeginBusy(IBusyManager busyManager, IBusyToken busyToken, IReadOnlyMetadataContext? metadata) => OnBeginBusy(busyManager, busyToken, metadata);

        void IBusyManagerListener.OnBusyStateChanged(IBusyManager busyManager, IReadOnlyMetadataContext? metadata) => OnBusyStateChanged(busyManager, metadata);

        public void Dispose()
        {
            if (_isDisposed)
                return;
            lock (this)
            {
                if (_isDisposed)
                    return;
                _isDisposed = true;
            }

            OnDispose(true);
        }

        #endregion

        #region Methods

        public void RegisterDisposeToken(ActionToken token)
        {
            if (_isDisposed)
            {
                token.Dispose();
                return;
            }

            var inline = false;
            lock (this)
            {
                if (_isDisposed)
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

        protected virtual void OnBeginBusy(IBusyManager busyManager, IBusyToken busyToken, IReadOnlyMetadataContext? metadata)
        {
        }

        protected virtual void OnBusyStateChanged(IBusyManager busyManager, IReadOnlyMetadataContext? metadata)
        {
            OnPropertyChanged(Default.IsBusyPropertyChangedArgs);
            OnPropertyChanged(Default.BusyTokenPropertyChangedArgs);
        }

        protected virtual void OnDispose(bool disposing)
        {
            if (!disposing)
            {
                this.NotifyLifecycleChanged(ViewModelLifecycleState.Finalized);
                return;
            }

            this.NotifyLifecycleChanged(ViewModelLifecycleState.Disposing);
            if (_disposeTokens != null)
            {
                for (var i = 0; i < _disposeTokens.Count; i++)
                    _disposeTokens[i].Dispose();
                _disposeTokens = null;
            }

            ClearPropertyChangedSubscribers();
            this.NotifyLifecycleChanged(ViewModelLifecycleState.Disposed);
        }

        protected override void OnPropertyChangedInternal(PropertyChangedEventArgs args)
        {
            base.OnPropertyChangedInternal(args);
            this.TryGetService<IMessenger>(true)?.Publish(this, args);
        }

        #endregion
    }
}