using System;
using MugenMvvm.App.Configuration;
using MugenMvvm.Commands;
using MugenMvvm.Commands.Components;
using MugenMvvm.Components;
using MugenMvvm.Entities;
using MugenMvvm.Entities.Components;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using MugenMvvm.Messaging;
using MugenMvvm.Messaging.Components;
using MugenMvvm.Metadata;
using MugenMvvm.Metadata.Components;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.Presenters;
using MugenMvvm.Presenters.Components;
using MugenMvvm.Serialization;
using MugenMvvm.Threading;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;
using MugenMvvm.ViewModels;
using MugenMvvm.ViewModels.Components;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using MugenMvvm.Wrapping;
using MugenMvvm.Wrapping.Components;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static MugenApplicationConfiguration DefaultConfiguration(this MugenApplicationConfiguration configuration, IServiceProvider? serviceProvider = null)
        {
            if (serviceProvider != null)
                MugenService.Configuration.InitializeInstance(serviceProvider);
            configuration.WithAppService(new ComponentCollectionManager());

            configuration.WithAppService(new MetadataContextManager())
                .WithComponent(new MetadataContextProviderComponent());

            configuration.WithAppService(new CommandManager())
                .WithComponent(new DelegateCommandProvider());

            configuration.WithAppService(new EntityManager())
                .WithComponent(new EntityTrackingCollectionProvider())
                .WithComponent(new ReflectionEntityStateSnapshotProvider());

            configuration.WithAppService(new AttachedValueManager())
                .WithComponent(new ConditionalWeakTableAttachedValueProvider())
                .WithComponent(new MetadataOwnerAttachedValueProvider())
                .WithComponent(new StaticTypeAttachedValueProvider())
                .WithComponent(new ValueHolderAttachedValueProvider());

            configuration.WithAppService(new ReflectionManager())
                .WithComponent(new ExpressionReflectionDelegateProvider())
                .WithComponent(new ReflectionDelegateProviderCache());

            configuration.WithAppService(new Tracer());

            configuration.WithAppService(new WeakReferenceManager())
                .WithComponent(new ValueHolderWeakReferenceProviderCache())
                .WithComponent(new WeakReferenceProviderComponent());

            configuration.WithAppService(new Messenger())
                .WithComponent(new MessagePublisher())
                .WithComponent(new MessengerHandlerSubscriber());

            configuration.WithAppService(new NavigationDispatcher())
                .WithComponent(new NavigationCallbackInvoker())
                .WithComponent(new NavigationCallbackManager())
                .WithComponent(new NavigationContextProvider())
                .WithComponent(new NavigationEntryDateTracker())
                .WithComponent(new NavigationEntryManager());

            configuration.WithAppService(new Presenter())
                .WithComponent(new ConditionPresenterDecorator())
                .WithComponent(new NavigationCallbackPresenterDecorator())
                .WithComponent(new ViewModelPresenter())
                .WithComponent(new ViewPresenterDecorator());

            configuration.WithAppService(new Serializer());

            configuration.WithAppService(new ThreadDispatcher());

            configuration.WithAppService(new ValidationManager())
                .WithComponent(new ValidatorProviderComponent());

            configuration.WithAppService(new ViewModelManager())
                .WithComponent(new CacheViewModelProvider())
                .WithComponent(new TypeViewModelProvider())
                .WithComponent(new ViewModelCleaner())
                .WithComponent(new ViewModelLifecycleTracker())
                .WithComponent(new ViewModelServiceResolver());

            configuration.WithAppService(new ViewManager())
                .WithComponent(new ExecutionModeViewManagerDecorator())
                .WithComponent(new RawViewLifecycleDispatcher())
                .WithComponent(new ViewCleaner())
                .WithComponent(new ViewInitializer())
                .WithComponent(new ViewLifecycleTracker())
                .WithComponent(new ViewManagerComponent())
                .WithComponent(new ViewModelViewAwareInitializer())
                .WithComponent(new ViewModelViewInitializerDecorator())
                .WithComponent(new ViewMappingProvider());
            configuration.WithAppService(new WrapperManager())
                .WithComponent(new ViewWrapperManagerDecorator());

            return configuration;
        }

        public static ServiceConfiguration<TService> WithAppService<TService>(this MugenApplicationConfiguration configuration, IComponentOwner<TService> service, IReadOnlyMetadataContext? metadata = null)
            where TService : class
        {
            MugenService.Configuration.InitializeInstance((TService)service);
            return configuration.ServiceConfiguration<TService>();
        }

        public static ServiceConfiguration<TService> WithComponent<TService>(this ServiceConfiguration<TService> configuration, IComponent<TService> component, IReadOnlyMetadataContext? metadata = null)
            where TService : class, IComponentOwner
        {
            configuration.Service().Components.Add(component, metadata);
            return configuration;
        }

        public static TService Service<TService>(this ServiceConfiguration<TService> _) where TService : class
        {
            return MugenService.Instance<TService>();
        }

        public static IMugenApplication GetApplication(this MugenApplicationConfiguration configuration)
        {
            return configuration.Application;
        }

        #endregion
    }
}