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
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;
using MugenMvvm.Messaging;
using MugenMvvm.Messaging.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.ViewModels.Components
{
    public sealed class ViewModelServiceProvider : IViewModelServiceProviderComponent, IHasPriority
    {
        private readonly IComponentCollectionManager? _componentCollectionManager;
        private readonly IReflectionManager? _reflectionManager;
        private readonly IValidationManager? _validationManager;
        private readonly IThreadDispatcher? _threadDispatcher;

        [Preserve(Conditional = true)]
        public ViewModelServiceProvider(IReflectionManager? reflectionManager = null, IValidationManager? validationManager = null, IThreadDispatcher? threadDispatcher = null,
            IComponentCollectionManager? componentCollectionManager = null)
        {
            _reflectionManager = reflectionManager;
            _validationManager = validationManager;
            _threadDispatcher = threadDispatcher;
            _componentCollectionManager = componentCollectionManager;
        }

        public int Priority { get; set; } = ViewModelComponentPriority.ServiceResolver;

        public object? TryGetService(IViewModelManager viewModelManager, IViewModelBase viewModel, object request, IReadOnlyMetadataContext? metadata)
        {
            if (request is not Type service)
                return null;

            if (service == typeof(IMetadataContext))
                return new MetadataContext();
            if (service == typeof(IMessenger))
            {
                var messenger = new Messenger(_componentCollectionManager);
                messenger.Components.TryAdd(new MessagePublisher(_threadDispatcher), metadata);
                messenger.Components.TryAdd(new MessengerHandlerSubscriber(_reflectionManager), metadata);
                return messenger;
            }

            if (service == typeof(IValidator))
                return _validationManager.DefaultIfNull().TryGetValidator((object) viewModel, metadata);

            if (service == typeof(IBusyManager))
            {
                var busyManager = new BusyManager(_componentCollectionManager);
                busyManager.Components.TryAdd(new BusyTokenManager());
                return busyManager;
            }

            return null;
        }
    }
}