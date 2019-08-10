using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.IoC;

namespace MugenMvvm.Internal
{
    public sealed class FallbackServiceConfiguration : IFallbackServiceConfiguration
    {
        #region Fields

        private readonly IIocContainer _container;

        #endregion

        #region Constructors

        public FallbackServiceConfiguration(IIocContainer container)
        {
            Should.NotBeNull(container, nameof(container));
            _container = container;
        }

        #endregion

        #region Implementation of interfaces

        public TService Instance<TService>() where TService : class
        {
            return (TService) _container.Get(typeof(TService));
        }

        public TService InstanceOptional<TService>() where TService : class
        {
            if (_container.TryGet(typeof(TService), out var service))
                return (TService) service;
            return null;
        }

        #endregion
    }
}