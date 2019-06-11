using System;
using MugenMvvm.Attributes;
using MugenMvvm.Enums;
using MugenMvvm.Infrastructure.BusyIndicator;
using MugenMvvm.Infrastructure.Messaging;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Infrastructure;

namespace MugenMvvm.Infrastructure.ViewModels
{
    public class ServiceResolverViewModelDispatcherComponent : IServiceResolverViewModelDispatcherComponent
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public ServiceResolverViewModelDispatcherComponent(IThreadDispatcher threadDispatcher, IComponentCollectionProvider componentCollectionProvider,
            IMetadataContextProvider metadataContextProvider)
        {
            Should.NotBeNull(threadDispatcher, nameof(threadDispatcher));
            Should.NotBeNull(componentCollectionProvider, nameof(componentCollectionProvider));
            Should.NotBeNull(metadataContextProvider, nameof(metadataContextProvider));
            ThreadDispatcher = threadDispatcher;
            MetadataContextProvider = metadataContextProvider;
            ComponentCollectionProvider = componentCollectionProvider;
        }

        #endregion

        #region Properties

        protected IComponentCollectionProvider ComponentCollectionProvider { get; }

        protected IMetadataContextProvider MetadataContextProvider { get; }

        protected IThreadDispatcher ThreadDispatcher { get; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public IReadOnlyMetadataContext? OnLifecycleChanged(IViewModelDispatcher dispatcher, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState,
            IReadOnlyMetadataContext metadata)
        {
            return null;
        }

        public object? TryGetService(IViewModelDispatcher dispatcher, IViewModelBase viewModel, Type service, IReadOnlyMetadataContext metadata)
        {
            if (service == typeof(IObservableMetadataContext))
                return MetadataContextProvider.GetObservableMetadataContext(viewModel, null);
            if (service == typeof(IMessenger))
                return new Messenger(ThreadDispatcher, ComponentCollectionProvider, MetadataContextProvider);
            if (service == typeof(IBusyIndicatorProvider))
                return new BusyIndicatorProvider(null, ComponentCollectionProvider);
            return null;
        }

        #endregion
    }
}