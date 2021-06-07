using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using MugenMvvm.App;
using MugenMvvm.Bindings.Compiling;
using MugenMvvm.Bindings.Converting;
using MugenMvvm.Bindings.Converting.Components;
using MugenMvvm.Bindings.Core;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Converting;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Parsing;
using MugenMvvm.Bindings.Resources;
using MugenMvvm.Bindings.Resources.Components;
using MugenMvvm.Busy;
using MugenMvvm.Busy.Components;
using MugenMvvm.Collections;
using MugenMvvm.Commands;
using MugenMvvm.Commands.Components;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Entities;
using MugenMvvm.Entities.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Busy;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.ViewModels.Components;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using MugenMvvm.Messaging;
using MugenMvvm.Messaging.Components;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.Presentation;
using MugenMvvm.Serialization;
using MugenMvvm.Serialization.Components;
using MugenMvvm.Tests.Navigation;
using MugenMvvm.Tests.Threading;
using MugenMvvm.Threading;
using MugenMvvm.Validation;
using MugenMvvm.Validation.Components;
using MugenMvvm.ViewModels;
using MugenMvvm.ViewModels.Components;
using MugenMvvm.Views;
using MugenMvvm.Views.Components;
using MugenMvvm.Wrapping;

namespace MugenMvvm.Tests
{
    public abstract class MugenUnitTestBase : IViewModelLifecycleListener, IServiceProvider, IDisposable
    {
        protected static readonly CancellationToken DefaultCancellationToken = new CancellationTokenSource().Token;
        protected static readonly IReadOnlyMetadataContext DefaultMetadata = new ReadOnlyMetadataContext(Array.Empty<KeyValuePair<IMetadataContextKey, object?>>());

        private ListInternal<ActionToken>? _disposeTokens;
        private Dictionary<Type, PropertyInfo>? _services;
        private IMugenApplication? _application;
        private IBusyManager? _busyManager;
        private ICommandManager? _commandManager;
        private IComponentCollectionManager? _collectionManager;
        private IAttachedValueManager? _attachedValueManager;
        private IEntityManager? _entityManager;
        private IReflectionManager? _reflectionManager;
        private IWeakReferenceManager? _weakReferenceManager;
        private IMessenger? _messenger;
        private INavigationDispatcher? _navigationDispatcher;
        private IPresenter? _presenter;
        private ISerializer? _serializer;
        private IThreadDispatcher? _threadDispatcher;
        private IValidationManager? _validationManager;
        private IViewModelManager? _viewModelManager;
        private IViewManager? _viewManager;
        private IWrapperManager? _wrapperManager;
        private IGlobalValueConverter? _globalValueConverter;
        private IBindingManager? _bindingManager;
        private IMemberManager? _memberManager;
        private IObservationManager? _observationManager;
        private IResourceManager? _resourceManager;
        private IExpressionParser? _expressionParser;
        private IExpressionCompiler? _expressionCompiler;
        private IServiceProvider? _serviceProvider;
        private ILogger? _logger;
        private bool _isDisposed;

        protected IMugenApplication Application => _application ??= GetApplication();

        protected ICommandManager CommandManager => _commandManager ??= GetCommandManager();

        protected IComponentCollectionManager ComponentCollectionManager => _collectionManager ??= GetComponentCollectionManager();

        protected IAttachedValueManager AttachedValueManager => _attachedValueManager ??= GetAttachedValueManager();

        protected IBusyManager BusyManager => _busyManager ??= GetBusyManager();

        protected IEntityManager EntityManager => _entityManager ??= GetEntityManager();

        protected IReflectionManager ReflectionManager => _reflectionManager ??= GetReflectionManager();

        protected ILogger Logger => _logger ??= GetLogger();

        protected IWeakReferenceManager WeakReferenceManager => _weakReferenceManager ??= GetWeakReferenceManager();

        protected IMessenger Messenger => _messenger ??= GetMessenger();

        protected INavigationDispatcher NavigationDispatcher => _navigationDispatcher ??= GetNavigationDispatcher();

        protected IPresenter Presenter => _presenter ??= GetPresenter();

        protected ISerializer Serializer => _serializer ??= GetSerializer();

        protected IThreadDispatcher ThreadDispatcher => _threadDispatcher ??= GetThreadDispatcher();

        protected IValidationManager ValidationManager => _validationManager ??= GetValidationManager();

        protected IViewModelManager ViewModelManager => _viewModelManager ??= GetViewModelManager();

        protected IViewManager ViewManager => _viewManager ??= GetViewManager();

        protected IWrapperManager WrapperManager => _wrapperManager ??= GetWrapperManager();

        protected IGlobalValueConverter GlobalValueConverter => _globalValueConverter ??= GetGlobalValueConverter();

        protected IBindingManager BindingManager => _bindingManager ??= GetBindingManager();

        protected IMemberManager MemberManager => _memberManager ??= GetMemberManager();

        protected IObservationManager ObservationManager => _observationManager ??= GetObservationManager();

        protected IResourceManager ResourceManager => _resourceManager ??= GetResourceManager();

        protected IExpressionParser ExpressionParser => _expressionParser ??= GetExpressionParser();

        protected IExpressionCompiler ExpressionCompiler => _expressionCompiler ??= GetExpressionCompiler();

        protected IServiceProvider ServiceProvider => _serviceProvider ??= GetServiceProvider();

        protected static void GcCollect()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.WaitForFullGCComplete();
            GC.Collect();
        }

        protected static string NewId() => Guid.NewGuid().ToString("N");

        protected static ActionToken WithGlobalService<T>(T service) where T : class
        {
            var oldService = MugenService.Optional<T>();
            MugenService.Configuration.InitializeInstance(service);
            return ActionToken.FromDelegate((o, _) =>
            {
                if (o == null)
                    MugenService.Configuration.Clear<T>();
                else
                    MugenService.Configuration.InitializeInstance((T)o);
            }, oldService);
        }

        protected static PresenterResult GetPresenterResult(object? target = null, NavigationType? navigationType = null,
            string? navigationId = null, INavigationProvider? navigationProvider = null, IReadOnlyMetadataContext? metadata = null) =>
            new(target, navigationId ?? NewId(), navigationProvider ?? TestNavigationProvider.Instance, navigationType ?? NavigationType.Popup, metadata);

        protected static NavigationCallback GetNavigationCallback(NavigationCallbackType callbackType, string? navigationId = null, NavigationType? navigationType = null) =>
            new(callbackType, navigationId ?? NewId(), navigationType ?? NavigationType.Popup);

        protected static NavigationContext GetNavigationContext(object? target = null, NavigationMode? navigationMode = null, NavigationType? navigationType = null,
            string? navigationId = null, INavigationProvider? navigationProvider = null, IReadOnlyMetadataContext? metadata = null) =>
            new(target, navigationProvider ?? TestNavigationProvider.Instance, navigationId ?? NewId(), navigationType ?? NavigationType.Popup, navigationMode ?? NavigationMode.New
                ,
                metadata);

        public void Dispose()
        {
            if (_isDisposed)
                return;
            lock (this)
            {
                if (_isDisposed)
                    return;
                _isDisposed = true;
            }

            OnDispose();
        }

        public virtual object? GetService(Type serviceType)
        {
            if (_services == null)
            {
                var services = new Dictionary<Type, PropertyInfo>();
                foreach (var propertyInfo in GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
                {
                    if (typeof(IComponentOwner).IsAssignableFrom(propertyInfo.PropertyType))
                        services[propertyInfo.PropertyType] = propertyInfo;
                }

                _services = services;
            }

            if (_services.TryGetValue(serviceType, out var p))
                return p.GetValue(this);
            return null;
        }

        protected virtual IAttachedValueManager GetAttachedValueManager()
        {
            var attachedValueManager = new AttachedValueManager(ComponentCollectionManager);
            attachedValueManager.AddComponent(new ConditionalWeakTableAttachedValueStorage());
            attachedValueManager.AddComponent(new StaticTypeAttachedValueStorage());
            attachedValueManager.AddComponent(new MetadataOwnerAttachedValueStorage());
            attachedValueManager.AddComponent(new ValueHolderAttachedValueStorage());
            return attachedValueManager;
        }

        protected virtual IBusyManager GetBusyManager()
        {
            var busyManager = new BusyManager(ComponentCollectionManager);
            busyManager.AddComponent(new BusyTokenManager { Priority = ComponentPriority.Min });
            return busyManager;
        }

        protected virtual IEntityManager GetEntityManager()
        {
            var entityManager = new EntityManager(ComponentCollectionManager);
            entityManager.AddComponent(new EntityTrackingCollectionProvider(ComponentCollectionManager) { Priority = ComponentPriority.Min });
            entityManager.AddComponent(new ReflectionEntityStateSnapshotProvider(ReflectionManager) { Priority = ComponentPriority.Min });
            return entityManager;
        }

        protected virtual IReflectionManager GetReflectionManager()
        {
            var reflectionManager = new ReflectionManager(ComponentCollectionManager);
            reflectionManager.AddComponent(new ExpressionReflectionDelegateProvider { Priority = ComponentPriority.Min });
            reflectionManager.AddComponent(new ReflectionDelegateProviderCache());
            return reflectionManager;
        }

        protected virtual IWeakReferenceManager GetWeakReferenceManager()
        {
            var weakReferenceManager = new WeakReferenceManager(ComponentCollectionManager);
            weakReferenceManager.AddComponent(new WeakReferenceProvider { Priority = ComponentPriority.Min });
            return weakReferenceManager;
        }

        protected virtual IMessenger GetMessenger()
        {
            var messenger = new Messenger(ComponentCollectionManager);
            messenger.AddComponent(new MessagePublisher(ThreadDispatcher) { Priority = ComponentPriority.Min });
            messenger.AddComponent(new MessengerHandlerSubscriber(ReflectionManager) { Priority = ComponentPriority.Min });
            return messenger;
        }

        protected virtual INavigationDispatcher GetNavigationDispatcher() => new NavigationDispatcher(ComponentCollectionManager);

        protected virtual IPresenter GetPresenter() => new Presenter(ComponentCollectionManager);

        protected virtual ISerializer GetSerializer()
        {
            var serializer = new Serializer(ComponentCollectionManager);
            serializer.AddComponent(new SerializationManager { Priority = ComponentPriority.Min });
            return serializer;
        }

        protected virtual IThreadDispatcher GetThreadDispatcher()
        {
            var threadDispatcher = new ThreadDispatcher(ComponentCollectionManager);
            threadDispatcher.AddComponent(new TestThreadDispatcherComponent { Priority = ComponentPriority.Min });
            return threadDispatcher;
        }

        protected virtual IValidationManager GetValidationManager()
        {
            var validationManager = new ValidationManager(ComponentCollectionManager);
            validationManager.AddComponent(new ValidatorProvider(ComponentCollectionManager) { Priority = ComponentPriority.Min });
            return validationManager;
        }

        protected virtual IViewModelManager GetViewModelManager()
        {
            var viewModelManager = new ViewModelManager(ComponentCollectionManager);
            viewModelManager.AddComponent(new ViewModelServiceProvider(ReflectionManager, ValidationManager, ThreadDispatcher, ComponentCollectionManager)
                { Priority = ComponentPriority.Min });
            viewModelManager.AddComponent(this);
            viewModelManager.AddComponent(new ViewModelProvider(ServiceProvider) { Priority = ComponentPriority.Min });
            return viewModelManager;
        }

        protected virtual IViewManager GetViewManager()
        {
            var viewManager = new ViewManager(ComponentCollectionManager);
            viewManager.AddComponent(new ViewModelViewManager(AttachedValueManager, ComponentCollectionManager) { Priority = ComponentPriority.Min });
            return viewManager;
        }

        protected virtual IWrapperManager GetWrapperManager() => new WrapperManager(ComponentCollectionManager);

        protected virtual IGlobalValueConverter GetGlobalValueConverter()
        {
            var globalValueConverter = new GlobalValueConverter(ComponentCollectionManager);
            globalValueConverter.AddComponent(new DefaultGlobalValueConverter { Priority = ComponentPriority.Min });
            return globalValueConverter;
        }

        protected virtual IBindingManager GetBindingManager() => new BindingManager(ComponentCollectionManager);

        protected virtual IMemberManager GetMemberManager() => new MemberManager(ComponentCollectionManager);

        protected virtual IObservationManager GetObservationManager() => new ObservationManager(ComponentCollectionManager);

        protected virtual IResourceManager GetResourceManager()
        {
            var resourceManager = new ResourceManager(ComponentCollectionManager);
            resourceManager.AddComponent(new TypeResolver { Priority = ComponentPriority.Min });
            return resourceManager;
        }

        protected virtual IExpressionParser GetExpressionParser() => new ExpressionParser(ComponentCollectionManager);

        protected virtual IExpressionCompiler GetExpressionCompiler() => new ExpressionCompiler(ComponentCollectionManager);

        protected virtual IServiceProvider GetServiceProvider() => new MugenServiceProvider(ViewModelManager);

        protected virtual IMugenApplication GetApplication() => new MugenApplication(null, ComponentCollectionManager);

        protected virtual ILogger GetLogger() => new Logger(ComponentCollectionManager);

        protected virtual ICommandManager GetCommandManager()
        {
            var commandManager = new CommandManager(ComponentCollectionManager);
            commandManager.AddComponent(new CommandProvider(ThreadDispatcher, ComponentCollectionManager) { Priority = ComponentPriority.Min });
            return commandManager;
        }

        protected virtual IComponentCollectionManager GetComponentCollectionManager() => new ComponentCollectionManager();

        protected virtual void OnDispose()
        {
            if (_disposeTokens != null)
            {
                for (var i = 0; i < _disposeTokens.Count; i++)
                    _disposeTokens.Items[i].Dispose();
                _disposeTokens = null;
            }
        }

        protected void RegisterDisposeToken(ActionToken token)
        {
            if (_isDisposed)
            {
                token.Dispose();
                return;
            }

            var inline = false;
            lock (this)
            {
                if (_isDisposed)
                    inline = true;
                else
                {
                    _disposeTokens ??= new ListInternal<ActionToken>(2);
                    _disposeTokens.Add(token);
                }
            }

            if (inline)
                token.Dispose();
        }

        protected T GetViewModel<T>(IReadOnlyMetadataContext? metadata = null) where T : class, IViewModelBase => ViewModelManager.GetViewModel<T>(metadata);

        protected IViewModelBase GetViewModel(Type type, IReadOnlyMetadataContext? metadata = null) => ViewModelManager.GetViewModel(type, metadata);

        void IViewModelLifecycleListener.OnLifecycleChanged(IViewModelManager viewModelManager, IViewModelBase viewModel, ViewModelLifecycleState lifecycleState, object? state,
            IReadOnlyMetadataContext? metadata)
        {
            if (lifecycleState == ViewModelLifecycleState.Created && viewModel is IValueHolder<IServiceProvider> holder && holder.Value == null)
                holder.Value = this;
        }
    }
}