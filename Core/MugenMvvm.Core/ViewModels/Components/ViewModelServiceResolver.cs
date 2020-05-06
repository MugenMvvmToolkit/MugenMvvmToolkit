using System;
using MugenMvvm.Attributes;
using MugenMvvm.Busy;
using MugenMvvm.Busy.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;
using MugenMvvm.Messaging;
using MugenMvvm.Messaging.Components;

namespace MugenMvvm.ViewModels.Components
{
    public sealed class ViewModelServiceResolver : IViewModelServiceResolverComponent, IHasPriority
    {
        #region Fields

        private readonly IComponentCollectionProvider? _componentCollectionProvider;
        private readonly IMetadataContextProvider? _metadataContextProvider;
        private readonly IReflectionDelegateProvider? _reflectionDelegateProvider;
        private readonly IThreadDispatcher? _threadDispatcher;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelServiceResolver(IReflectionDelegateProvider? reflectionDelegateProvider = null, IThreadDispatcher? threadDispatcher = null,
            IComponentCollectionProvider? componentCollectionProvider = null, IMetadataContextProvider? metadataContextProvider = null)
        {
            _reflectionDelegateProvider = reflectionDelegateProvider;
            _threadDispatcher = threadDispatcher;
            _metadataContextProvider = metadataContextProvider;
            _componentCollectionProvider = componentCollectionProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewModelComponentPriority.ServiceResolver;

        #endregion

        #region Implementation of interfaces

        public object? TryGetService(IViewModelBase viewModel, Type service, IReadOnlyMetadataContext? metadata)
        {
            if (service == typeof(IMetadataContext))
                return _metadataContextProvider.DefaultIfNull().GetMetadataContext(viewModel);
            if (service == typeof(IMessenger))
            {
                var messenger = new Messenger(_componentCollectionProvider, _metadataContextProvider);
                messenger.Components.Add(new MessagePublisher(_threadDispatcher), metadata);
                messenger.Components.Add(new MessengerHandlerSubscriber(_reflectionDelegateProvider), metadata);
                return messenger;
            }

            if (service == typeof(IBusyManager))
            {
                var busyManager = new BusyManager(_componentCollectionProvider);
                busyManager.Components.Add(new BusyManagerComponent());
                return busyManager;
            }

            return null;
        }

        #endregion
    }
}