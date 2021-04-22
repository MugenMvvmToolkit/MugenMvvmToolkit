using System;
using System.Runtime.CompilerServices;
using System.Threading;
using MugenMvvm.App.Components;
using MugenMvvm.App.Configuration;
using MugenMvvm.Bindings.Interfaces.Core;
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
            configuration.WithAppService(MugenService.Optional<IComponentCollectionManager>() ?? new ComponentCollectionManager())
                         .WithComponent(new AsyncInitializationAssertBehavior(isInitializing, canIgnore));
            return configuration;
        }

        public static MugenApplicationConfiguration DefaultConfiguration(this MugenApplicationConfiguration configuration, SynchronizationContext? synchronizationContext,
            IServiceProvider? serviceProvider = null)
        {
            if (serviceProvider == null)
            {
                if (!configuration.HasService<IServiceProvider>())
                    configuration.InitializeService(new MugenServiceProvider());
            }
            else
                configuration.InitializeService(serviceProvider);

            if (!configuration.HasService<IComponentCollectionManager>())
                configuration.WithAppService(new ComponentCollectionManager());

            configuration.Application.AddComponent(new AppLifecycleTracker());

            configuration.WithAppService(MugenService.Optional<ICommandManager>() ?? new CommandManager())
                         .WithComponent(new DelegateCommandProvider())
                         .WithComponent(new CommandCleaner());

            configuration.WithAppService(MugenService.Optional<IEntityManager>() ?? new EntityManager())
                         .WithComponent(new EntityTrackingCollectionProvider())
                         .WithComponent(new ReflectionEntityStateSnapshotProvider());

            configuration.WithAppService(MugenService.Optional<IAttachedValueManager>() ?? new AttachedValueManager())
                         .WithComponent(new ConditionalWeakTableAttachedValueStorage())
                         .WithComponent(new MetadataOwnerAttachedValueStorage())
                         .WithComponent(new StaticTypeAttachedValueStorage())
                         .WithComponent(new ValueHolderAttachedValueStorage());

            configuration.WithAppService(MugenService.Optional<IReflectionManager>() ?? new ReflectionManager())
                         .WithComponent(new ExpressionReflectionDelegateProvider())
                         .WithComponent(new ReflectionDelegateProviderCache());

            configuration.WithAppService(MugenService.Optional<IWeakReferenceManager>() ?? new WeakReferenceManager())
                         .WithComponent(new ValueHolderWeakReferenceProviderCache())
                         .WithComponent(new WeakReferenceProvider());

            configuration.WithAppService(MugenService.Optional<IMessenger>() ?? new Messenger())
                         .WithComponent(new MessagePublisher())
                         .WithComponent(new MessengerHandlerSubscriber());

            var entryManager = new NavigationEntryManager();
            configuration.WithAppService(MugenService.Optional<INavigationDispatcher>() ?? new NavigationDispatcher())
                         .WithComponent(new NavigationCallbackInvoker())
                         .WithComponent(new NavigationCallbackManager())
                         .WithComponent(new NavigationContextProvider())
                         .WithComponent(new NavigationEntryDateTracker())
                         .WithComponent(new NavigationTargetDispatcher())
                         .WithComponent(entryManager);

            configuration.WithAppService(MugenService.Optional<IPresenter>() ?? new Presenter())
                         .WithComponent(new NavigationCallbackPresenterDecorator())
                         .WithComponent(new ViewModelPresenter())
                         .WithComponent(new ViewPresenterDecorator())
                         .WithComponent(entryManager)
                         .WithComponent(ViewModelPresenterMediatorProvider.Get(GetViewModelPresenterMediator));

            configuration.WithAppService(MugenService.Optional<ISerializer>() ?? new Serializer())
                         .WithComponent(new SerializationManager());

            var threadDispatcher = MugenService.Optional<IThreadDispatcher>() ?? new ThreadDispatcher();
            if (synchronizationContext != null)
                threadDispatcher.AddComponent(new SynchronizationContextThreadDispatcher(synchronizationContext));
            configuration.WithAppService(threadDispatcher);

            configuration.WithAppService(MugenService.Optional<IValidationManager>() ?? new ValidationManager())
                         .WithComponent(new ValidatorProvider());

            configuration.WithAppService(MugenService.Optional<IViewModelManager>() ?? new ViewModelManager())
                         .WithComponent(new CacheViewModelProvider())
                         .WithComponent(new ViewModelProvider())
                         .WithComponent(new InheritParentViewModelServiceProvider())
                         .WithComponent(new ViewModelCleaner())
                         .WithComponent(new ViewModelLifecycleTracker())
                         .WithComponent(new ViewModelMetadataInitializer())
                         .WithComponent(new ViewModelServiceProvider());

            configuration.WithAppService(MugenService.Optional<IViewManager>() ?? new ViewManager())
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
                         .WithComponent(new ViewMappingProvider());
            configuration.WithAppService(MugenService.Optional<IWrapperManager>() ?? new WrapperManager())
                         .WithComponent(new ViewWrapperManagerDecorator());

            return configuration;
        }

        public static MugenApplicationConfiguration TraceConfiguration(this MugenApplicationConfiguration configuration, bool traceApp = true, bool traceBinding = true,
            bool traceMessenger = true, bool traceNavigation = true, bool tracePresenter = true, bool traceViewModel = true, bool traceView = true,
            bool includeTraceLogger = true)
        {
            var cfg = configuration.WithAppService(MugenService.Optional<ILogger>() ?? new Logger());
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ServiceConfiguration<TService> WithAppService<TService>(this MugenApplicationConfiguration configuration, IComponentOwner<TService> service)
            where TService : class =>
            configuration.InitializeService((TService) service);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ServiceConfiguration<TService> WithComponent<TService>(this ServiceConfiguration<TService> configuration, IComponent<TService> component,
            IReadOnlyMetadataContext? metadata = null)
            where TService : class, IComponentOwner
        {
            configuration.Service.Components.Add(component, metadata);
            return configuration;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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