using System;
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

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static IMugenApplication DefaultConfiguration(this IMugenApplication application, IServiceProvider? serviceProvider = null)
        {
            Should.NotBeNull(application, nameof(application));
            if (serviceProvider != null)
                MugenService.Configuration.InitializeInstance(serviceProvider);
            application
                .WithService(new CommandProvider())
                .WithService(new ComponentCollectionProvider())
                .WithService(new EntityManager())
                .WithService(new AttachedValueProvider())
                .WithService(new ReflectionDelegateProvider())
                .WithService(new Tracer())
                .WithService(new WeakReferenceProvider())
                .WithService(new Messenger())
                .WithService(new MetadataContextProvider())
                .WithService(new NavigationDispatcher())
                .WithService(new Presenter())
                .WithService(new Serializer())
                .WithService(new ThreadDispatcher())
                .WithService(new ValidationManager())
                .WithService(new ViewModelManager())
                .WithService(new ViewManager())
                .WithService(new WrapperManager());

            MugenService.CommandProvider
                .WithComponent(new DelegateCommandProvider());
            MugenService.EntityManager
                .WithComponent(new EntityTrackingCollectionProvider())
                .WithComponent(new ReflectionEntityStateSnapshotProvider());
            MugenService.AttachedValueProvider
                .WithComponent(new ConditionalWeakTableAttachedValueProvider())
                .WithComponent(new MetadataOwnerAttachedValueProvider())
                .WithComponent(new ValueHolderAttachedValueProvider());
            MugenService.ReflectionDelegateProvider
                .WithComponent(new ExpressionReflectionDelegateProvider())
                .WithComponent(new ReflectionDelegateProviderCache());
            MugenService.WeakReferenceProvider
                .WithComponent(new ValueHolderWeakReferenceProviderCache())
                .WithComponent(new WeakReferenceProviderComponent());
            MugenService.Messenger
                .WithComponent(new MessagePublisher())
                .WithComponent(new MessengerHandlerSubscriber());
            MugenService.MetadataContextProvider
                .WithComponent(new MetadataContextProviderComponent());
            MugenService.NavigationDispatcher
                .WithComponent(new NavigationCallbackInvoker())
                .WithComponent(new NavigationCallbackManager())
                .WithComponent(new NavigationContextProvider())
                .WithComponent(new NavigationEntryDateTracker())
                .WithComponent(new NavigationEntryProvider());
            MugenService.Presenter
                .WithComponent(new ConditionPresenterDecorator())
                .WithComponent(new NavigationCallbackPresenterDecorator())
                .WithComponent(new ViewModelMediatorPresenter());
            MugenService.ValidationManager
                .WithComponent(new ValidatorProviderComponent());
            MugenService.ViewModelManager
                .WithComponent(new CacheViewModelProvider())
                .WithComponent(new TypeViewModelProvider())
                .WithComponent(new ViewModelCleaner())
                .WithComponent(new ViewModelLifecycleTracker())
                .WithComponent(new ViewModelServiceResolver());
            MugenService.ViewManager
                .WithComponent(new ExecutionModeViewManagerDecorator())
                .WithComponent(new RawViewLifecycleDispatcher())
                .WithComponent(new ViewCleaner())
                .WithComponent(new ViewInitializer())
                .WithComponent(new ViewLifecycleTracker())
                .WithComponent(new ViewManagerComponent())
                .WithComponent(new ViewModelViewAwareInitializer())
                .WithComponent(new ViewModelViewInitializerDecorator())
                .WithComponent(new ViewModelViewMappingProvider());
            return application;
        }

        public static IMugenApplication WithService<TService>(this IMugenApplication application, IComponentOwner<TService> service, bool registerInstance = true, IReadOnlyMetadataContext? metadata = null)
            where TService : class
        {
            Should.NotBeNull(application, nameof(application));
            application.Components.Add(service, metadata);
            if (registerInstance && service is TService s)
                MugenService.Configuration.InitializeInstance(s);
            return application;
        }

        public static IComponentOwner<T> WithComponent<T>(this IComponentOwner<T> componentOwner, IComponent<T> component, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            Should.NotBeNull(componentOwner, nameof(componentOwner));
            componentOwner.Components.Add(component, metadata);
            return componentOwner;
        }

        #endregion
    }
}