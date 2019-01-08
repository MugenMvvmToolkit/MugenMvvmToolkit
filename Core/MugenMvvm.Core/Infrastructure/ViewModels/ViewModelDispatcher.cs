using System;
using System.Collections.Generic;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.BusyIndicator;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Infrastructure.Metadata;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Infrastructure;
using MugenMvvm.Models;

namespace MugenMvvm.Infrastructure.ViewModels
{
    public class ViewModelDispatcher : HasListenersBase<IViewModelDispatcherListener>, IViewModelDispatcher, IObservableMetadataContextListener
    {
        #region Fields

        protected readonly Dictionary<Guid, WeakReference> ViewModelsCache;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelDispatcher(IThreadDispatcher threadDispatcher, ITracer tracer)
        {
            Should.NotBeNull(threadDispatcher, nameof(threadDispatcher));
            Should.NotBeNull(tracer, nameof(tracer));
            ViewModelsCache = new Dictionary<Guid, WeakReference>();
            ThreadDispatcher = threadDispatcher;
            Tracer = tracer;
        }

        #endregion

        #region Properties

        protected IThreadDispatcher ThreadDispatcher { get; }

        protected ITracer Tracer { get; }

        #endregion

        #region Implementation of interfaces

        void IObservableMetadataContextListener.OnAdded(IObservableMetadataContext metadataContext, IMetadataContextKey key, object? newValue)
        {
        }

        void IObservableMetadataContextListener.OnChanged(IObservableMetadataContext metadataContext, IMetadataContextKey key, object? oldValue, object? newValue)
        {
            if (ViewModelMetadata.Id.Equals(key))
            {
                lock (ViewModelsCache)
                {
                    var oldId = ViewModelMetadata.Id.GetValue(metadataContext, oldValue);
                    if (ViewModelsCache.TryGetValue(oldId, out var value))
                    {
                        ViewModelsCache.Remove(oldId);
                        ViewModelsCache[ViewModelMetadata.Id.GetValue(metadataContext, newValue)] = value;
                    }
                }
            }
        }

        void IObservableMetadataContextListener.OnRemoved(IObservableMetadataContext metadataContext, IMetadataContextKey key, object? oldValue)
        {
            if (ViewModelMetadata.Id.Equals(key))
                RemoveFromCache(ViewModelMetadata.Id.GetValue(metadataContext, oldValue));
        }

        public IBusyIndicatorProvider GetBusyIndicatorProvider(IViewModel viewModel, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetBusyIndicatorProviderInternal(viewModel, metadata);
        }

        public IMessenger GetMessenger(IViewModel viewModel, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetMessengerInternal(viewModel, metadata);
        }

        public IObservableMetadataContext GetMetadataContext(IViewModel viewModel, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetMetadataContextInternal(viewModel, metadata);
        }

        public void OnLifecycleChanged(IViewModel viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(lifecycleState, nameof(lifecycleState));
            Should.NotBeNull(metadata, nameof(metadata));
            OnLifecycleChangedInternal(viewModel, lifecycleState, metadata);
        }

        public IViewModel? GetViewModelById(Guid id, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            return GetViewModelByIdInternal(id, metadata);
        }

        #endregion

        #region Methods

        protected virtual IViewModel? GetViewModelByIdInternal(Guid id, IReadOnlyMetadataContext metadata)
        {
            WeakReference value;
            lock (ViewModelsCache)
            {
                if (!ViewModelsCache.TryGetValue(id, out value))
                    return null;
            }

            var vm = (IViewModel)value.Target;
            if (vm == null)
                RemoveFromCache(id);
            return vm;
        }

        protected virtual IBusyIndicatorProvider GetBusyIndicatorProviderInternal(IViewModel viewModel, IReadOnlyMetadataContext metadata)
        {
            return new BusyIndicatorProvider();
        }

        protected virtual IMessenger GetMessengerInternal(IViewModel viewModel, IReadOnlyMetadataContext metadata)
        {
            return new Messenger(ThreadDispatcher, Tracer);
        }

        protected virtual IObservableMetadataContext GetMetadataContextInternal(IViewModel viewModel, IReadOnlyMetadataContext metadata)
        {
            return new MetadataContext(this);
        }

        protected virtual void OnLifecycleChangedInternal(IViewModel viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyMetadataContext metadata)
        {
            if (lifecycleState != ViewModelLifecycleState.Finalized)
                viewModel.Metadata.Set(ViewModelMetadata.LifecycleState, lifecycleState);
            if (lifecycleState == ViewModelLifecycleState.Created)
                AddToCache(viewModel.Metadata.Get(ViewModelMetadata.Id), viewModel);
            else if (lifecycleState == ViewModelLifecycleState.Disposed || lifecycleState == ViewModelLifecycleState.Finalized)
                RemoveFromCache(viewModel.Metadata.Get(ViewModelMetadata.Id));

            var traceLevel = lifecycleState == ViewModelLifecycleState.Finalized ? TraceLevel.Error : TraceLevel.Information;
            if (Tracer.CanTrace(traceLevel))
                Tracer.Trace(traceLevel, MessageConstants.TraceViewModelLifecycleFormat3.Format(viewModel.GetType(), viewModel.GetHashCode(), lifecycleState));
            var listeners = GetListenersInternal();
            if (listeners != null)
            {
                for (int i = 0; i < listeners.Length; i++)
                    listeners[i]?.OnLifecycleChanged(viewModel, lifecycleState, metadata);
            }
        }

        private void AddToCache(Guid id, IViewModel viewModel)
        {
            lock (ViewModelsCache)
            {
                ViewModelsCache[id] = MugenExtensions.GetWeakReference(viewModel);
            }
        }

        private void RemoveFromCache(Guid id)
        {
            lock (ViewModelsCache)
            {
                ViewModelsCache.Remove(id);
            }
        }

        #endregion
    }
}