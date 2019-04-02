using System;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Infrastructure
{
    public class ServiceConfiguration<TService>
        where TService : class
    {
        #region Fields

        // ReSharper disable once StaticMemberInGenericType
        private static bool _hasOptionalValue;
        private static TService? _service;
        private static IHasService<TService>? _serviceConfiguration;

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
                    if (typeof(TService) == typeof(IServiceProvider))
                        ExceptionManager.ThrowIoCCannotFindBinding(typeof(IServiceProvider));

                    _service = Service<IServiceProvider>.Instance.GetService<TService>();
                }

                return _service!;
            }
        }

        public static TService? InstanceOptional
        {
            get
            {
                if (_serviceConfiguration != null)
                {
                    if (_serviceConfiguration is IHasServiceOptional<TService> optional)
                        return optional.ServiceOptional;
                    return _serviceConfiguration.Service;
                }

                if (_service == null || !_hasOptionalValue)
                {
                    Service<IServiceProvider>.Instance.TryGetService(out _service);
                    _hasOptionalValue = true;
                }

                return _service;
            }
        }

        #endregion

        #region Methods

        public static void Initialize(IHasService<TService>? serviceConfiguration)
        {
            InitializeInternal(null);
            _serviceConfiguration = serviceConfiguration;
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
            _hasOptionalValue = false;
        }

        #endregion
    }
}