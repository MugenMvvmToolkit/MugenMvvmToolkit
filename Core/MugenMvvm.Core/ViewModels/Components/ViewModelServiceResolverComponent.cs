using System;
using MugenMvvm.Attributes;
using MugenMvvm.BusyIndicator;
using MugenMvvm.Interfaces.BusyIndicator;
using MugenMvvm.Interfaces.Components;
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
    public sealed class ViewModelServiceResolverComponent : IViewModelServiceResolverComponent, IHasPriority
    {
        #region Fields

        private readonly IComponentCollectionProvider? _componentCollectionProvider;
        private readonly IMetadataContextProvider? _metadataContextProvider;
        private readonly IThreadDispatcher? _threadDispatcher;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelServiceResolverComponent(IThreadDispatcher? threadDispatcher = null, IComponentCollectionProvider? componentCollectionProvider = null,
            IMetadataContextProvider? metadataContextProvider = null)
        {
            _threadDispatcher = threadDispatcher;
            _metadataContextProvider = metadataContextProvider;
            _componentCollectionProvider = componentCollectionProvider;
        }

        #endregion

        #region Properties

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        public object? TryGetService(IViewModelBase viewModel, Type service, IReadOnlyMetadataContext? metadata)
        {
            if (service == typeof(IMetadataContext))
                return _metadataContextProvider.ServiceIfNull().GetMetadataContext(viewModel);
            if (service == typeof(IMessenger))
            {
                var messenger = new Messenger(_threadDispatcher, _componentCollectionProvider, _metadataContextProvider);
                messenger.Components.Add(MessengerHandlerComponent.Instance);
                messenger.Components.Add(ViewModelMessengerSubscriberComponent.Instance);
                messenger.Components.Add(MessengerHandlerSubscriberComponent.InstanceWeak);
                return messenger;
            }

            if (service == typeof(IBusyIndicatorProvider))
                return new BusyIndicatorProvider(_componentCollectionProvider);
            return null;
        }

        #endregion
    }
}