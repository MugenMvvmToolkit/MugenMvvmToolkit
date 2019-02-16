using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Attributes;
using MugenMvvm.Constants;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.Internal;
using MugenMvvm.Interfaces;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Infrastructure;
using MugenMvvm.Metadata;

namespace MugenMvvm.Infrastructure.ViewModels
{
    public class ViewModelDispatcher : HasListenersBase<IViewModelDispatcherListener>, IViewModelDispatcher //todo remove tracer
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelDispatcher(ITracer tracer)
        {
            Should.NotBeNull(tracer, nameof(tracer));
            Tracer = tracer;
            ServiceResolvers = new Dictionary<Type, IViewModelDispatcherServiceResolver>(MemberInfoComparer.Instance);
        }

        #endregion

        #region Properties

        protected Dictionary<Type, IViewModelDispatcherServiceResolver> ServiceResolvers { get; }

        protected ITracer Tracer { get; }

        #endregion

        #region Implementation of interfaces

        public object GetService(IViewModelBase viewModel, Type service, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(service, nameof(service));
            Should.NotBeNull(metadata, nameof(metadata));
            return GetServiceInternal(viewModel, service, metadata);
        }

        public void AddServiceResolver(IViewModelDispatcherServiceResolver resolver)
        {
            Should.NotBeNull(resolver, nameof(resolver));
            AddServiceResolverInternal(resolver);
        }

        public void RemoveServiceResolver(IViewModelDispatcherServiceResolver resolver)
        {
            Should.NotBeNull(resolver, nameof(resolver));
            RemoveServiceResolverInternal(resolver);
        }

        public IReadOnlyList<IViewModelDispatcherServiceResolver> GetServiceResolvers()
        {
            return GetServiceResolversInternal();
        }

        public bool Subscribe(IViewModelBase viewModel, object observer, ThreadExecutionMode executionMode, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(observer, nameof(observer));
            Should.NotBeNull(metadata, nameof(metadata));
            return SubscribeInternal(viewModel, observer, executionMode, metadata);
        }

        public bool Unsubscribe(IViewModelBase viewModel, object observer, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(observer, nameof(observer));
            Should.NotBeNull(metadata, nameof(metadata));
            return UnsubscribeInternal(viewModel, observer, metadata);
        }

        public void OnLifecycleChanged(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(viewModel, nameof(viewModel));
            Should.NotBeNull(lifecycleState, nameof(lifecycleState));
            Should.NotBeNull(metadata, nameof(metadata));
            OnLifecycleChangedInternal(viewModel, lifecycleState, metadata);
        }

        public IViewModelBase? TryGetViewModel(Guid id, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(metadata, nameof(metadata));
            return TryGetViewModelInternal(id, metadata);
        }

        #endregion

        #region Methods

        protected virtual object GetServiceInternal(IViewModelBase viewModel, Type service, IReadOnlyMetadataContext metadata)
        {
            IViewModelDispatcherServiceResolver resolver;
            lock (ServiceResolvers)
            {
                ServiceResolvers.TryGetValue(service, out resolver);
            }

            if (resolver == null)
                throw ExceptionManager.IoCCannotFindBinding(service);
            return resolver.Resolve(viewModel, service, metadata);
        }

        protected virtual void AddServiceResolverInternal(IViewModelDispatcherServiceResolver resolver)
        {
            var services = resolver.Services;
            lock (ServiceResolvers)
            {
                for (int i = 0; i < services.Count; i++)
                    ServiceResolvers[services[i]] = resolver;
            }
        }

        protected virtual void RemoveServiceResolverInternal(IViewModelDispatcherServiceResolver resolver)
        {
            lock (ServiceResolvers)
            {
                var services = resolver.Services;
                for (int i = 0; i < services.Count; i++)
                {
                    var type = services[i];
                    if (ServiceResolvers.TryGetValue(type, out var r) && ReferenceEquals(r, resolver))
                        ServiceResolvers.Remove(type);
                }
            }
        }

        protected virtual IReadOnlyList<IViewModelDispatcherServiceResolver> GetServiceResolversInternal()
        {
            lock (ServiceResolvers)
            {
                return ServiceResolvers.Values.ToArray();
            }
        }

        protected virtual void OnLifecycleChangedInternal(IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, IReadOnlyMetadataContext metadata)
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
            if (Tracer.CanTrace(traceLevel))//todo remove tracer
                Tracer.Trace(traceLevel, MessageConstants.TraceViewModelLifecycleFormat3.Format(viewModel.GetType(), viewModel.GetHashCode(), lifecycleState));
        }

        protected virtual bool SubscribeInternal(IViewModelBase viewModel, object observer, ThreadExecutionMode executionMode, IReadOnlyMetadataContext metadata)
        {
            bool subscribed = false;
            var listeners = GetListenersInternal();
            if (listeners != null)
            {
                for (var i = 0; i < listeners.Length; i++)
                {
                    var result = listeners[i]?.OnSubscribe(this, viewModel, observer, executionMode, metadata);
                    if (result.GetValueOrDefault())
                        subscribed = true;
                }
            }

            return subscribed;
        }

        protected virtual bool UnsubscribeInternal(IViewModelBase viewModel, object observer, IReadOnlyMetadataContext metadata)
        {
            bool unsubscribed = false;
            var listeners = GetListenersInternal();
            if (listeners != null)
            {
                for (var i = 0; i < listeners.Length; i++)
                {
                    var result = listeners[i]?.OnUnsubscribe(this, viewModel, observer, metadata);
                    if (result.GetValueOrDefault())
                        unsubscribed = true;
                }
            }

            return unsubscribed;
        }

        protected virtual IViewModelBase? TryGetViewModelInternal(Guid id, IReadOnlyMetadataContext metadata)
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