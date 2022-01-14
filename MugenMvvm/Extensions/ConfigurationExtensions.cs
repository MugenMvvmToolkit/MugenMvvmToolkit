using System;
using System.Threading;
using MugenMvvm.App.Components;
using MugenMvvm.App.Configuration;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Collections.Components;
using MugenMvvm.Commands;
using MugenMvvm.Commands.Components;
using MugenMvvm.Components;
using MugenMvvm.Entities;
using MugenMvvm.Entities.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Presentation.Components;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using MugenMvvm.Messaging;
using MugenMvvm.Messaging.Components;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.Presentation;
using MugenMvvm.Presentation.Components;
using MugenMvvm.Serialization;
using MugenMvvm.Serialization.Components;
using MugenMvvm.Threading;
using MugenMvvm.Threading.Components;
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
        public static MugenApplicationConfiguration WithAsyncInitializationAssert(this MugenApplicationConfiguration configuration, Func<bool> isInitializing,
            Func<object, bool>? canIgnore = null)
        {
            configuration.WithService(MugenService.Optional<IComponentCollectionManager>() ?? new ComponentCollectionManager())
                         .WithComponent(new AsyncInitializationAssertBehavior(isInitializing, canIgnore));
            return configuration;
        }

        public static MugenApplicationConfiguration DefaultConfiguration(this MugenApplicationConfiguration configuration, SynchronizationContext? synchronizationContext,
            IServiceProvider? serviceProvider = null)
        {
            var entryManager = new NavigationEntryManager();
            return configuration.WithService(MugenService.Optional<IComponentCollectionManager>() ?? new ComponentCollectionManager())
                                .WithComponent(new CollectionDecoratorManagerInitializer())
                                .AppConfiguration()
                                .WithService(serviceProvider ?? (configuration.HasService<IServiceProvider>() ? null : new MugenServiceProvider()))
                                .WithComponent(new AppLifecycleTracker())
                                .WithService(MugenService.Optional<ICommandManager>() ?? new CommandManager())
                                .WithComponent(new CommandProvider())
                                .WithComponent(new CommandCleaner())
                                .WithService(MugenService.Optional<IEntityManager>() ?? new EntityManager())
                                .WithComponent(new EntityTrackingCollectionProvider())
                                .WithComponent(new ReflectionEntityStateSnapshotProvider())
                                .WithService(MugenService.Optional<IAttachedValueManager>() ?? new AttachedValueManager())
                                .WithComponent(new ConditionalWeakTableAttachedValueStorage())
                                .WithComponent(new MetadataOwnerAttachedValueStorage())
                                .WithComponent(new StaticTypeAttachedValueStorage())
                                .WithComponent(new ValueHolderAttachedValueStorage())
                                .WithService(MugenService.Optional<IReflectionManager>() ?? new ReflectionManager())
                                .WithComponent(new ExpressionReflectionDelegateProvider())
                                .WithComponent(new ReflectionDelegateProviderCache())
                                .WithService(MugenService.Optional<IWeakReferenceManager>() ?? new WeakReferenceManager())
                                .WithComponent(new ValueHolderWeakReferenceProviderCache())
                                .WithComponent(new WeakReferenceProvider())
                                .WithService(MugenService.Optional<IMessenger>() ?? new Messenger())
                                .WithComponent(new MessagePublisher())
                                .WithComponent(new MessengerHandlerSubscriber())
                                .WithService(MugenService.Optional<INavigationDispatcher>() ?? new NavigationDispatcher())
                                .WithComponent(new NavigationCallbackInvoker())
                                .WithComponent(new NavigationCallbackManager())
                                .WithComponent(new NavigationContextProvider())
                                .WithComponent(new NavigationEntryDateTracker())
                                .WithComponent(new NavigationTargetDispatcher())
                                .WithComponent(new ForceCloseNavigationHandler())
                                .WithComponent(entryManager)
                                .WithService(MugenService.Optional<IPresenter>() ?? new Presenter())
                                .WithComponent(new NavigationCallbackPresenterDecorator())
                                .WithComponent(new ViewModelPresenter())
                                .WithComponent(new ViewPresenterDecorator())
                                .WithComponent(entryManager)
                                .WithComponent(ViewModelPresenterMediatorProvider.Get(GetViewModelPresenterMediator))
                                .WithService(MugenService.Optional<ISerializer>() ?? new Serializer())
                                .WithComponent(new SerializationManager())
                                .WithService(MugenService.Optional<IThreadDispatcher>() ?? new ThreadDispatcher())
                                .WithComponent(synchronizationContext == null ? null : new SynchronizationContextThreadDispatcher(synchronizationContext))
                                .WithService(MugenService.Optional<IValidationManager>() ?? new ValidationManager())
                                .WithComponent(new ValidatorProvider())
                                .WithService(MugenService.Optional<IViewModelManager>() ?? new ViewModelManager())
                                .WithComponent(new CacheViewModelProvider())
                                .WithComponent(new ViewModelProvider())
                                .WithComponent(new ViewModelCleaner())
                                .WithComponent(new ViewModelLifecycleTracker())
                                .WithComponent(new ViewModelMetadataInitializer())
                                .WithComponent(new ViewModelServiceProvider())
                                .WithService(MugenService.Optional<IViewManager>() ?? new ViewManager())
                                .WithComponent(new ExecutionModeViewManagerDecorator())
                                .WithComponent(new RawViewLifecycleDispatcher())
                                .WithComponent(new ViewCleaner())
                                .WithComponent(new ViewInitializer())
                                .WithComponent(new ViewLifecycleTracker())
                                .WithComponent(new ViewModelViewManager())
                                .WithComponent(new ViewModelViewAwareInitializer())
                                .WithComponent(new ViewModelViewInitializerDecorator())
                                .WithComponent(new UndefinedMappingViewInitializer())
                                .WithComponent(new ViewLifecycleAwareViewModelHandler())
                                .WithComponent(new ViewMappingProvider())
                                .WithService(MugenService.Optional<IWrapperManager>() ?? new WrapperManager())
                                .WithComponent(new ViewWrapperManagerDecorator());
        }

        public static MugenApplicationConfiguration TraceConfiguration(this MugenApplicationConfiguration configuration, bool traceApp = true, bool traceBinding = true,
            bool traceMessenger = true, bool traceNavigation = true, bool tracePresenter = true, bool traceViewModel = true, bool traceView = true,
            bool includeTraceLogger = true)
        {
            var cfg = configuration.WithService(MugenService.Optional<ILogger>() ?? new Logger());
            if (traceApp)
                DebugTracer.TraceApp(configuration.Application);
            if (traceBinding)
                DebugTracer.TraceBindings(configuration.GetService<IBindingManager>());
            if (traceMessenger)
                DebugTracer.TraceMessenger(configuration.GetService<IMessenger>());
            if (traceNavigation)
                DebugTracer.TraceNavigation(configuration.GetService<INavigationDispatcher>());
            if (tracePresenter)
                DebugTracer.TracePresenter(configuration.GetService<IPresenter>());
            if (traceViewModel)
                DebugTracer.TraceViewModel(configuration.GetService<IViewModelManager>());
            if (traceView)
                DebugTracer.TraceView(configuration.GetService<IViewManager>());
            if (includeTraceLogger)
                DebugTracer.AddTraceLogger(cfg.Service);
            return configuration;
        }

        public static MugenApplicationConfiguration WithService<TService>(this MugenApplicationConfiguration configuration, TService? service)
            where TService : class =>
            service == null ? configuration : configuration.InitializeService(service);

        public static MugenApplicationConfiguration WithComponent(this MugenApplicationConfiguration configuration, IComponent<IMugenApplication>? component,
            IReadOnlyMetadataContext? metadata = null)
        {
            if (component != null)
                configuration.Application.Components.Add(component, metadata);
            return configuration;
        }

        public static MugenServiceConfiguration<TService> WithComponent<TService>(this MugenServiceConfiguration<TService> configuration, IComponent<TService>? component,
            IReadOnlyMetadataContext? metadata = null)
            where TService : class, IComponentOwner
        {
            if (component != null)
                configuration.Service.Components.Add(component, metadata);
            return configuration;
        }

        public static MugenApplicationConfiguration WithMetadataValue<T>(this MugenApplicationConfiguration configuration, IMetadataContextKey<T> key, T value) =>
            configuration.WithMetadata(configuration.Metadata.WithValue(key, value));

        public static MugenApplicationConfiguration WithMetadataValue<TService, T>(this MugenServiceConfiguration<TService> configuration, IMetadataContextKey<T> key, T value)
            where TService : class => configuration.AppConfiguration().WithMetadataValue(key, value);

        public static IMugenApplication GetApplication(this MugenApplicationConfiguration configuration) => configuration.Application;

        private static IViewModelPresenterMediator? GetViewModelPresenterMediator(IPresenter presenter, IViewModelBase viewModel, IViewMapping mapping,
            IReadOnlyMetadataContext? metadata)
        {
            var viewPresenter = presenter.GetComponents<IViewPresenterMediatorProviderComponent>(metadata).TryGetViewPresenter(presenter, viewModel, mapping, metadata);
            if (viewPresenter == null)
                return null;
            return new ViewModelPresenterMediator<object>(viewModel, mapping, viewPresenter);
        }
    }
}