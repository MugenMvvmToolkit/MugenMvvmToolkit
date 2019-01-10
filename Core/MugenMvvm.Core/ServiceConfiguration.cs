using System;
using MugenMvvm.Interfaces.IoC;

namespace MugenMvvm
{
    public class ServiceConfiguration<TService>
        where TService : class
    {
        #region Fields

        // ReSharper disable once StaticMemberInGenericType
        private static bool _hasOptionalValue;
        private static TService? _service;
        private static IServiceConfiguration<TService>? _serviceConfiguration;

        #endregion

        #region Properties

        public static TService Instance
        {
            get
            {
                if (_serviceConfiguration != null)
                    return _serviceConfiguration.Instance;
                if (_service == null)
                    _service = Service<IServiceProvider>.Instance.GetService<TService>();
                return _service!;
            }
        }

        public static TService? InstanceOptional
        {
            get
            {
                if (_serviceConfiguration != null)
                    return _serviceConfiguration.InstanceOptional;

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

        public static void Initialize(IServiceConfiguration<TService>? serviceConfiguration)
        {
            Initialize(service: null);
            _serviceConfiguration = serviceConfiguration;
        }

        public static void Initialize(TService service)
        {
            _serviceConfiguration = null;
            _service = service;
            _hasOptionalValue = false;
        }

        #endregion
    }
}