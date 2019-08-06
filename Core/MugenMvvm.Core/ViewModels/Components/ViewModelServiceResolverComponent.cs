using System;
using MugenMvvm.Attributes;
using MugenMvvm.BusyIndicator;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;
using MugenMvvm.Messaging;

namespace MugenMvvm.ViewModels.Components
{
    public class ViewModelServiceResolverComponent : IViewModelServiceResolverComponent
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelServiceResolverComponent(IThreadDispatcher threadDispatcher, IComponentCollectionProvider componentCollectionProvider,
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

        public object? TryGetService(IViewModelBase viewModel, Type service, IReadOnlyMetadataContext? metadata)
        {
            if (service == typeof(IMetadataContext))
                return MetadataContextProvider.GetMetadataContext(viewModel, null);
            if (service == typeof(IMessenger))
                return new Messenger(ThreadDispatcher, ComponentCollectionProvider, MetadataContextProvider);
            if (service == typeof(IBusyIndicatorProvider))
                return new BusyIndicatorProvider(ComponentCollectionProvider);
            return null;
        }

        int IComponent.GetPriority(object source)
        {
            return Priority;
        }

        #endregion
    }
}