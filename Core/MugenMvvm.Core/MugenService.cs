using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Commands;
using MugenMvvm.Interfaces.Components;
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

        public static ICommandProvider CommandProvider => Instance<ICommandProvider>();

        public static IComponentCollectionProvider ComponentCollectionProvider => Instance<IComponentCollectionProvider>();

        public static IAttachedValueProvider AttachedValueProvider => Instance<IAttachedValueProvider>();

        public static IReflectionDelegateProvider ReflectionDelegateProvider => Instance<IReflectionDelegateProvider>();

        public static ITracer Tracer => Instance<ITracer>();

        public static IWeakReferenceProvider WeakReferenceProvider => Instance<IWeakReferenceProvider>();

        public static IMessenger Messenger => Instance<IMessenger>();

        public static IMetadataContextProvider MetadataContextProvider => Instance<IMetadataContextProvider>();

        public static INavigationDispatcher NavigationDispatcher => Instance<INavigationDispatcher>();

        public static IPresenter Presenter => Instance<IPresenter>();

        public static ISerializer Serializer => Instance<ISerializer>();

        public static IThreadDispatcher ThreadDispatcher => Instance<IThreadDispatcher>();

        public static IValidatorProvider ValidatorProvider => Instance<IValidatorProvider>();

        public static IViewModelManager ViewModelManager => Instance<IViewModelManager>();

        public static IViewManager ViewManager => Instance<IViewManager>();

        public static IWrapperManager WrapperManager => Instance<IWrapperManager>();

        #endregion

        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public static TService Instance<TService>() where TService : class
        {
            return Configuration<TService>.Instance;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        [Pure]
        public static TService? Optional<TService>() where TService : class
        {
            return Configuration<TService>.Optional;
        }

        #endregion

        #region Nested types

        public static class Configuration
        {
            #region Methods

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
            private static IHasServiceOptional<TService>? _serviceConfigurationOptional;

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
                        return _serviceConfigurationOptional.ServiceOptional;

                    if (_serviceConfiguration != null)
                        return _serviceConfiguration.Service;

                    return _fallbackConfiguration?.Optional<TService>();
                }
            }

            #endregion

            #region Methods

            public static void Initialize(IHasService<TService>? serviceConfiguration)
            {
                InitializeInternal(null);
                _serviceConfiguration = serviceConfiguration;
                _serviceConfigurationOptional = serviceConfiguration as IHasServiceOptional<TService>;
            }

            public static void Initialize(TService service)
            {
                Should.NotBeNull(service, nameof(service));
                InitializeInternal(service);
            }

            public static void Clear()
            {
                _service = null;
                _serviceConfiguration = null;
                _serviceConfigurationOptional = null;
            }

            private static TService GetFallbackService()
            {
                if (_fallbackConfiguration != null)
                    return _fallbackConfiguration.Instance<TService>();
                ExceptionManager.ThrowObjectNotInitialized(typeof(Configuration<TService>), typeof(TService).Name);
                return null;
            }

            private static void InitializeInternal(TService? service)
            {
                _serviceConfiguration = null;
                _serviceConfigurationOptional = null;
                _service = service;
            }

            #endregion
        }

        #endregion
    }
}