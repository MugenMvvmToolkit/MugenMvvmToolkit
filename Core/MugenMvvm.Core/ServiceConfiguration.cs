using System.Runtime.CompilerServices;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm
{
    public static class ServiceConfiguration
    {
        #region Fields

        private static IFallbackServiceConfiguration? _fallbackConfiguration;

        #endregion

        #region Methods

        public static void InitializeFallback(IFallbackServiceConfiguration? fallbackConfiguration)
        {
            _fallbackConfiguration = fallbackConfiguration;
        }

        public static void Initialize<TService>(IHasService<TService>? serviceConfiguration) where TService : class
        {
            Configuration<TService>.Initialize(serviceConfiguration);
        }

        public static void Initialize<TService>(TService service) where TService : class
        {
            Configuration<TService>.Initialize(service);
        }

        #endregion

        #region Nested types

        public static class Configuration<TService> where TService : class
        {
            #region Fields

            // ReSharper disable once StaticMemberInGenericType
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

            public static TService? InstanceOptional
            {
                get
                {
                    if (_service != null)
                        return _service;

                    if (_serviceConfigurationOptional != null)
                        return _serviceConfigurationOptional.ServiceOptional;

                    if (_serviceConfiguration != null)
                        return _serviceConfiguration.Service;

                    return _fallbackConfiguration?.InstanceOptional<TService>();

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

            private static TService GetFallbackService()
            {
                if (_fallbackConfiguration != null)
                    return _fallbackConfiguration.Instance<TService>();
                ExceptionManager.ThrowObjectNotInitialized(typeof(Service<TService>), typeof(TService).Name);
                return null!;
            }

            private static void InitializeInternal(TService? service)
            {
                _serviceConfiguration = null;
                _service = service;
            }

            #endregion
        }

        #endregion
    }
}