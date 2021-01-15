using System;
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
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presenters;
using MugenMvvm.Interfaces.Presenters.Components;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using MugenMvvm.Messaging;
using MugenMvvm.Messaging.Components;
using MugenMvvm.Navigation;
using MugenMvvm.Navigation.Components;
using MugenMvvm.Presenters;
using MugenMvvm.Presenters.Components;
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
        public static MugenApplicationConfiguration DefaultConfiguration(this MugenApplicationConfiguration configuration, SynchronizationContext? synchronizationContext,
            IServiceProvider? serviceProvider = null, bool isOnMainThread = true)
        {
            if (serviceProvider != null)
                MugenService.Configuration.InitializeInstance(serviceProvider);

            configuration.WithAppService(new ComponentCollectionManager());

            configuration.Application.AddComponent(new AppLifecycleTracker());

            configuration.WithAppService(new CommandManager())
                         .WithComponent(new DelegateCommandProvider())
                         .WithComponent(new CommandCleaner());

            configuration.WithAppService(new EntityManager())
                         .WithComponent(new EntityTrackingCollectionProvider())
                         .WithComponent(new ReflectionEntityStateSnapshotProvider());

            configuration.WithAppService(new AttachedValueManager())
                         .WithComponent(new ConditionalWeakTableAttachedValueStorage())
                         .WithComponent(new MetadataOwnerAttachedValueStorage())
                         .WithComponent(new StaticTypeAttachedValueStorage())
                         .WithComponent(new ValueHolderAttachedValueStorage());

            configuration.WithAppService(new ReflectionManager())
                         .WithComponent(new ExpressionReflectionDelegateProvider())
                         .WithComponent(new ReflectionDelegateProviderCache());

            configuration.WithAppService(new Logger());

            configuration.WithAppService(new WeakReferenceManager())
                         .WithComponent(new ValueHolderWeakReferenceProviderCache())
                         .WithComponent(new WeakReferenceProvider());

            configuration.WithAppService(new Messenger())
                         .WithComponent(new MessagePublisher())
                         .WithComponent(new MessengerHandlerSubscriber());

            var entryManager = new NavigationEntryManager();
            configuration.WithAppService(new NavigationDispatcher())
                         .WithComponent(new NavigationCallbackInvoker())
                         .WithComponent(new NavigationCallbackManager())
                         .WithComponent(new NavigationContextProvider())
                         .WithComponent(new NavigationEntryDateTracker())
                         .WithComponent(new NavigationTargetDispatcher())
                         .WithComponent(entryManager);

            configuration.WithAppService(new Presenter())
                         .WithComponent(new NavigationCallbackPresenterDecorator())
                         .WithComponent(new ViewModelPresenter())
                         .WithComponent(new ViewPresenterDecorator())
                         .WithComponent(entryManager)
                         .WithComponent(ViewModelPresenterMediatorProvider.Get(GetViewModelPresenterMediator));

            configuration.WithAppService(new Serializer())
                         .WithComponent(new SerializationManager());

            var threadDispatcher = new ThreadDispatcher();
            if (synchronizationContext != null)
                threadDispatcher.AddComponent(new SynchronizationContextThreadDispatcher(synchronizationContext, isOnMainThread));
            configuration.WithAppService(threadDispatcher);

            configuration.WithAppService(new ValidationManager())
                         .WithComponent(new ValidatorProvider());

            configuration.WithAppService(new ViewModelManager())
                         .WithComponent(new CacheViewModelProvider())
                         .WithComponent(new TypeViewModelProvider())
                         .WithComponent(new InheritParentViewModelServiceResolver())
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
                         .WithComponent(new UndefinedMappingViewInitializer())
                         .WithComponent(new ViewLifecycleAwareViewModelHandler())
                         .WithComponent(new ViewMappingProvider());
            configuration.WithAppService(new WrapperManager())
                         .WithComponent(new ViewWrapperManagerDecorator());

            return configuration;
        }

        public static MugenApplicationConfiguration TraceConfiguration(this MugenApplicationConfiguration configuration, bool traceApp = true, bool traceBinding = true,
            bool traceMessenger = true, bool traceNavigation = true, bool tracePresenter = true, bool traceViewModel = true, bool traceView = true,
            bool includeConsoleLogger = true)
        {
            if (traceApp)
                DebugTracer.TraceApp(configuration.Application);
            if (traceBinding)
                DebugTracer.TraceBindings(configuration.ServiceConfiguration<IBindingManager>().Service());
            if (traceMessenger)
                DebugTracer.TraceMessenger(configuration.ServiceConfiguration<IMessenger>().Service());
            if (traceNavigation)
                DebugTracer.TraceNavigation(configuration.ServiceConfiguration<INavigationDispatcher>().Service());
            if (tracePresenter)
                DebugTracer.TracePresenter(configuration.ServiceConfiguration<IPresenter>().Service());
            if (traceViewModel)
                DebugTracer.TraceViewModel(configuration.ServiceConfiguration<IViewModelManager>().Service());
            if (traceView)
                DebugTracer.TraceView(configuration.ServiceConfiguration<IViewManager>().Service());
            if (includeConsoleLogger)
                DebugTracer.AddConsoleLogger(configuration.ServiceConfiguration<ILogger>().Service());
            return configuration;
        }

        public static ServiceConfiguration<TService> WithAppService<TService>(this MugenApplicationConfiguration configuration, IComponentOwner<TService> service)
            where TService : class
        {
            MugenService.Configuration.InitializeInstance((TService) service);
            return configuration.ServiceConfiguration<TService>();
        }

        public static ServiceConfiguration<TService> WithComponent<TService>(this ServiceConfiguration<TService> configuration, IComponent<TService> component,
            IReadOnlyMetadataContext? metadata = null)
            where TService : class, IComponentOwner
        {
            configuration.Service().Components.Add(component, metadata);
            return configuration;
        }

        public static TService Service<TService>(this ServiceConfiguration<TService> _) where TService : class => MugenService.Instance<TService>();

        public static IMugenApplication GetApplication(this MugenApplicationConfiguration configuration) => configuration.Application;

        private static IViewModelPresenterMediator? GetViewModelPresenterMediator(IPresenter presenter, IViewModelBase viewModel, IViewMapping mapping,
            IReadOnlyMetadataContext? metadata)
        {
            var viewPresenter = presenter.GetComponents<IViewPresenterProviderComponent>(metadata).TryGetViewPresenter(presenter, viewModel, mapping, metadata);
            if (viewPresenter == null)
                return null;
            return new ViewModelPresenterMediator<object>(viewModel, mapping, viewPresenter);
        }
    }
}