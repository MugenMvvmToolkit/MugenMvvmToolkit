using System.Runtime.CompilerServices;
using JetBrains.Annotations;
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

namespace MugenMvvm
{
    public static class MugenService
    {
        #region Fields

        private static IFallbackServiceConfiguration? _fallbackConfiguration;

        #endregion

        #region Properties

        public static IMugenApplication Application => Instance<IMugenApplication>();

        public static ICommandManager CommandManager => Instance<ICommandManager>();

        public static IComponentCollectionManager ComponentCollectionManager => Instance<IComponentCollectionManager>();

        public static IAttachedValueManager AttachedValueManager => Instance<IAttachedValueManager>();

        public static IEntityManager EntityManager => Instance<IEntityManager>();

        public static IReflectionManager ReflectionManager => Instance<IReflectionManager>();

        public static IWeakReferenceManager WeakReferenceManager => Instance<IWeakReferenceManager>();

        public static IMessenger Messenger => Instance<IMessenger>();

        public static IMetadataContextManager MetadataContextManager => Instance<IMetadataContextManager>();

        public static INavigationDispatcher NavigationDispatcher => Instance<INavigationDispatcher>();

        public static IPresenter Presenter => Instance<IPresenter>();

        public static ISerializer Serializer => Instance<ISerializer>();

        public static IThreadDispatcher ThreadDispatcher => Instance<IThreadDispatcher>();

        public static IValidationManager ValidationManager => Instance<IValidationManager>();

        public static IViewModelManager ViewModelManager => Instance<IViewModelManager>();

        public static IViewManager ViewManager => Instance<IViewManager>();

        public static IWrapperManager WrapperManager => Instance<IWrapperManager>();

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TService Instance<TService>() where TService : class
        {
            return Configuration<TService>.Instance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TService? Optional<TService>() where TService : class
        {
            return Configuration<TService>.Optional;
        }

        #endregion

        #region Nested types

        public static class Configuration
        {
            #region Methods

            public static IFallbackServiceConfiguration? GetFallbackConfiguration()
            {
                return _fallbackConfiguration;
            }

            public static void InitializeFallback(IFallbackServiceConfiguration? fallbackConfiguration)
            {
                _fallbackConfiguration = fallbackConfiguration;
            }

            public static void Initialize<TService>(IHasService<TService>? serviceConfiguration) where TService : class
            {
                Configuration<TService>.Initialize(serviceConfiguration);
            }

            public static void InitializeInstance<TService>(TService service) where TService : class
            {
                Configuration<TService>.Initialize(service);
            }

            public static void Clear<TService>() where TService : class
            {
                Configuration<TService>.Clear();
            }

            #endregion
        }

        private static class Configuration<TService> where TService : class
        {
            #region Fields

            private static TService? _service;
            private static IHasService<TService>? _serviceConfiguration;
            private static IHasOptionalService<TService>? _serviceConfigurationOptional;

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
                get
                {
                    if (_service != null)
                        return _service;

                    if (_serviceConfigurationOptional != null)
                        return _serviceConfigurationOptional.Service;

                    if (_serviceConfiguration != null)
                        return _serviceConfiguration.Service;

                    return _fallbackConfiguration?.Optional<TService>();
                }
            }

            #endregion

            #region Methods

            public static void Initialize(IHasService<TService>? serviceConfiguration)
            {
                _service = null;
                _serviceConfiguration = serviceConfiguration;
                _serviceConfigurationOptional = serviceConfiguration as IHasOptionalService<TService>;
            }

            public static void Initialize(TService service)
            {
                Should.NotBeNull(service, nameof(service));
                _serviceConfiguration = null;
                _serviceConfigurationOptional = null;
                _service = service;
            }

            public static void Clear(bool clearFallback = false)
            {
                _service = null;
                _serviceConfiguration = null;
                _serviceConfigurationOptional = null;
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