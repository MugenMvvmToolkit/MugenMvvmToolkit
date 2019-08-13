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
                get
                {
                    if (_serviceConfiguration != null)
                        return _serviceConfiguration.Service;
                    if (_service == null)
                    {
                        if (_fallbackConfiguration != null)
                            return _fallbackConfiguration.Instance<TService>();
                        ExceptionManager.ThrowIocCannotFindBinding(typeof(TService));
                    }
                    return _service!;
                }
            }

            public static TService? InstanceOptional
            {
                get
                {
                    if (_serviceConfigurationOptional != null)
                        return _serviceConfigurationOptional.ServiceOptional;

                    if (_serviceConfiguration != null)
                        return _serviceConfiguration.Service;

                    if (_service == null)
                        return _fallbackConfiguration?.InstanceOptional<TService>();

                    return _service;
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