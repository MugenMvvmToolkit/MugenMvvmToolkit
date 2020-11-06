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
using MugenMvvm.Interfaces.Presenters;
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
        #region Fields

        private static IFallbackServiceConfiguration? _fallbackConfiguration;

        #endregion

        #region Properties

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

        public static IResourceResolver ResourceResolver
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => Instance<IResourceResolver>();
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

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TService Instance<TService>() where TService : class => Configuration<TService>.Instance;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TService? Optional<TService>() where TService : class => Configuration<TService>.Optional;

        public static ActionToken AddComponent<T>(IComponent<T> component, IReadOnlyMetadataContext? metadata = null) where T : class, IComponentOwner => Instance<T>().AddComponentWithToken(component, metadata);

        #endregion

        #region Nested types

        public static class Configuration
        {
            #region Methods

            public static IFallbackServiceConfiguration? GetFallbackConfiguration() => _fallbackConfiguration;

            public static void InitializeFallback(IFallbackServiceConfiguration? fallbackConfiguration) => _fallbackConfiguration = fallbackConfiguration;

            public static void Initialize<TService>(IHasService<TService>? serviceConfiguration) where TService : class => Configuration<TService>.Initialize(serviceConfiguration);

            public static void InitializeInstance<TService>(TService service) where TService : class => Configuration<TService>.Initialize(service);

            public static void Clear<TService>() where TService : class => Configuration<TService>.Clear();

            #endregion
        }

        private static class Configuration<TService> where TService : class
        {
            #region Fields

            private static TService? _service;
            private static IHasService<TService>? _serviceConfiguration;

            #endregion

            #region Properties

            public static TService Instance
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get
                {
                    if (_service != null)
                        return _service;
                    if (_serviceConfiguration != null)
                        return _serviceConfiguration.Service;
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
                        return _serviceConfiguration.ServiceOptional;
                    return _fallbackConfiguration?.Optional<TService>();
                }
            }

            #endregion

            #region Methods

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
                    _fallbackConfiguration = null;
            }

            private static TService GetFallbackService()
            {
                if (_fallbackConfiguration != null)
                    return _fallbackConfiguration.Instance<TService>();
                ExceptionManager.ThrowObjectNotInitialized(typeof(Configuration<TService>), typeof(TService).Name);
                return null;
            }

            #endregion
        }

        #endregion
    }
}