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
using MugenMvvm.Internal;
using MugenMvvm.Messaging;
using MugenMvvm.Messaging.Components;

namespace MugenMvvm.ViewModels.Components
{
    public sealed class ViewModelServiceResolver : IViewModelServiceResolverComponent, IHasPriority
    {
        #region Fields

        private readonly IComponentCollectionManager? _componentCollectionManager;
        private readonly IMetadataContextManager? _metadataContextManager;
        private readonly IReflectionManager? _reflectionManager;
        private readonly IThreadDispatcher? _threadDispatcher;

        #endregion

        #region Constructors

        [Preserve(Conditional = true)]
        public ViewModelServiceResolver(IReflectionManager? reflectionManager = null, IThreadDispatcher? threadDispatcher = null,
            IComponentCollectionManager? componentCollectionManager = null, IMetadataContextManager? metadataContextManager = null)
        {
            _reflectionManager = reflectionManager;
            _threadDispatcher = threadDispatcher;
            _metadataContextManager = metadataContextManager;
            _componentCollectionManager = componentCollectionManager;
        }

        #endregion

        #region Properties

        public int Priority { get; set; } = ViewModelComponentPriority.ServiceResolver;

        #endregion

        #region Implementation of interfaces

        public object? TryGetService<TRequest>(IViewModelManager viewModelManager, IViewModelBase viewModel, in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            if (TypeChecker.IsValueType<TRequest>() || !(request is Type service))
                return null;

            if (service == typeof(IMetadataContext))
                return _metadataContextManager.DefaultIfNull().GetMetadataContext(viewModel);
            if (service == typeof(IMessenger))
            {
                var messenger = new Messenger(_componentCollectionManager, _metadataContextManager);
                messenger.Components.Add(new MessagePublisher(_threadDispatcher), metadata);
                messenger.Components.Add(new MessengerHandlerSubscriber(_reflectionManager), metadata);
                return messenger;
            }

            if (service == typeof(IBusyManager))
            {
                var busyManager = new BusyManager(_componentCollectionManager);
                busyManager.Components.Add(new BusyManagerComponent());
                return busyManager;
            }

            return null;
        }

        #endregion
    }
}