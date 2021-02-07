using System;
using System.Runtime.CompilerServices;
using MugenMvvm.Bindings.Interfaces.Compiling;
using MugenMvvm.Bindings.Interfaces.Convert;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Interfaces.Parsing;
using MugenMvvm.Bindings.Interfaces.Resources;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Entities;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Messaging;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Presentation;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Threading;
using MugenMvvm.Interfaces.Validation;
using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Wrapping;
using MugenMvvm.Internal;

namespace MugenMvvm
{
    public static class MugenService
    {
        public static IMugenApplication Application
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Instance<IMugenApplication>();
        }

        public static ICommandManager CommandManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Instance<ICommandManager>();
        }

        public static IComponentCollectionManager ComponentCollectionManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Instance<IComponentCollectionManager>();
        }

        public static IAttachedValueManager AttachedValueManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Instance<IAttachedValueManager>();
        }

        public static IEntityManager EntityManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Instance<IEntityManager>();
        }

        public static IReflectionManager ReflectionManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Instance<IReflectionManager>();
        }

        public static IWeakReferenceManager WeakReferenceManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Instance<IWeakReferenceManager>();
        }

        public static IMessenger Messenger
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Instance<IMessenger>();
        }

        public static INavigationDispatcher NavigationDispatcher
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Instance<INavigationDispatcher>();
        }

        public static IPresenter Presenter
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Instance<IPresenter>();
        }

        public static ISerializer Serializer
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Instance<ISerializer>();
        }

        public static IThreadDispatcher ThreadDispatcher
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Instance<IThreadDispatcher>();
        }

        public static IValidationManager ValidationManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Instance<IValidationManager>();
        }

        public static IViewModelManager ViewModelManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Instance<IViewModelManager>();
        }

        public static IViewManager ViewManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Instance<IViewManager>();
        }

        public static IWrapperManager WrapperManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Instance<IWrapperManager>();
        }

        public static IGlobalValueConverter GlobalValueConverter
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Instance<IGlobalValueConverter>();
        }

        public static IBindingManager BindingManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Instance<IBindingManager>();
        }

        public static IMemberManager MemberManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Instance<IMemberManager>();
        }

        public static IObservationManager ObservationManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Instance<IObservationManager>();
        }

        public static IResourceManager ResourceManager
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Instance<IResourceManager>();
        }

        public static IExpressionParser ExpressionParser
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Instance<IExpressionParser>();
        }

        public static IExpressionCompiler ExpressionCompiler
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Instance<IExpressionCompiler>();
        }

        public static IServiceProvider ServiceProvider
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Instance<IServiceProvider>();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TService Instance<TService>() where TService : class => Configuration<TService>.Instance;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TService? Optional<TService>() where TService : class => Configuration<TService>.Optional;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ActionToken AddComponent<T>(IComponent<T> component, IReadOnlyMetadataContext? metadata = null) where T : class, IComponentOwner<T> =>
            Instance<T>().AddComponent(component, metadata);

        public static class Configuration
        {
            public static IFallbackServiceConfiguration? FallbackConfiguration;

            public static void Initialize<TService>(IHasService<TService>? serviceConfiguration) where TService : class => Configuration<TService>.Initialize(serviceConfiguration);

            public static void InitializeInstance<TService>(TService service) where TService : class => Configuration<TService>.Initialize(service);

            public static void Clear<TService>() where TService : class => Configuration<TService>.Clear();
        }

        private static class Configuration<TService> where TService : class
        {
            private static TService? _service;
            private static IHasService<TService>? _serviceConfiguration;

            public static TService Instance
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    if (_service != null)
                        return _service;
                    if (_serviceConfiguration != null)
                        return _serviceConfiguration.GetService(false)!;
                    return GetFallbackService();
                }
            }

            public static TService? Optional
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    if (_service != null)
                        return _service;
                    if (_serviceConfiguration != null)
                        return _serviceConfiguration.GetService(true);
                    return Configuration.FallbackConfiguration?.Optional<TService>();
                }
            }

            public static void Initialize(IHasService<TService>? serviceConfiguration)
            {
                _service = null;
                _serviceConfiguration = serviceConfiguration;
            }

            public static void Initialize(TService service)
            {
                Should.NotBeNull(service, nameof(service));
                _serviceConfiguration = null;
                _service = service;
            }

            public static void Clear(bool clearFallback = false)
            {
                _service = null;
                _serviceConfiguration = null;
                if (clearFallback)
                    Configuration.FallbackConfiguration = null;
            }

            private static TService GetFallbackService()
            {
                var instance = Configuration.FallbackConfiguration?.Instance<TService>();
                if (instance == null)
                    ExceptionManager.ThrowObjectNotInitialized(typeof(Configuration<TService>), typeof(TService).Name);
                return instance;
            }
        }
    }
}