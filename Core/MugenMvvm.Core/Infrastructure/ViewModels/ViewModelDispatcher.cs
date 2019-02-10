using System;
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

namespace MugenMvvm.Infrastructure.ViewModels
{
    public class ViewModelDispatcher : HasListenersBase<IViewModelDispatcherListener>, IViewModelDispatcher
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelDispatcher(IThreadDispatcher threadDispatcher, ITracer tracer)
        {
            Should.NotBeNull(threadDispatcher, nameof(threadDispatcher));
            Should.NotBeNull(tracer, nameof(tracer));
            ThreadDispatcher = threadDispatcher;
            Tracer = tracer;
        }

        #endregion

        #region Properties

        protected IThreadDispatcher ThreadDispatcher { get; }

        protected ITracer Tracer { get; }

        #endregion

        #region Implementation of interfaces

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

        public void Subscribe(IViewModel viewModel, object observer, ThreadExecutionMode executionMode, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(observer, nameof(observer));
            Should.NotBeNull(metadata, nameof(metadata));
            SubscribeInternal(viewModel, observer, executionMode, metadata);
        }

        public void Unsubscribe(IViewModel viewModel, object observer, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(observer, nameof(observer));
            Should.NotBeNull(metadata, nameof(metadata));
            UnsubscribeInternal(viewModel, observer, metadata);
        }

        public void OnLifecycleChanged(IViewModel viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(lifecycleState, nameof(lifecycleState));
            Should.NotBeNull(metadata, nameof(metadata));
            OnLifecycleChangedInternal(viewModel, lifecycleState, metadata);
        }

        public IViewModel? TryGetViewModel(Guid id, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            return TryGetViewModelInternal(id, metadata);
        }

        #endregion

        #region Methods

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
            return new MetadataContext();
        }

        protected virtual void OnLifecycleChangedInternal(IViewModel viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyMetadataContext metadata)
        {
            if (lifecycleState != ViewModelLifecycleState.Finalized)
                viewModel.Metadata.Set(ViewModelMetadata.LifecycleState, lifecycleState);

            var listeners = GetListenersInternal();
            if (listeners != null)
            {
                for (var i = 0; i < listeners.Length; i++)
                    listeners[i]?.OnLifecycleChanged(this, viewModel, lifecycleState, metadata);
            }

            var traceLevel = lifecycleState == ViewModelLifecycleState.Finalized ? TraceLevel.Error : TraceLevel.Information;
            if (Tracer.CanTrace(traceLevel))
                Tracer.Trace(traceLevel, MessageConstants.TraceViewModelLifecycleFormat3.Format(viewModel.GetType(), viewModel.GetHashCode(), lifecycleState));
        }

        protected virtual void SubscribeInternal(IViewModel viewModel, object observer, ThreadExecutionMode executionMode, IReadOnlyMetadataContext metadata)
        {
            var listeners = GetListenersInternal();
            if (listeners != null)
            {
                for (var i = 0; i < listeners.Length; i++)
                    listeners[i]?.OnSubscribe(this, viewModel, observer, executionMode, metadata);
            }
        }

        protected void UnsubscribeInternal(IViewModel viewModel, object observer, IReadOnlyMetadataContext metadata)
        {
            var listeners = GetListenersInternal();
            if (listeners != null)
            {
                for (var i = 0; i < listeners.Length; i++)
                    listeners[i]?.OnUnsubscribe(this, viewModel, observer, metadata);
            }
        }

        protected virtual IViewModel? TryGetViewModelInternal(Guid id, IReadOnlyMetadataContext metadata)
        {
            var listeners = GetListenersInternal();
            if (listeners != null)
            {
                for (var i = 0; i < listeners.Length; i++)
                {
                    var viewModel = listeners[i]?.TryGetViewModel(this, id, metadata);
                    if (viewModel != null)
                        return viewModel;
                }
            }

            return null;
        }

        #endregion
    }
}