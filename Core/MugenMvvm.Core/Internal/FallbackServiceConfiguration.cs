using System;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Internal
{
    public sealed class FallbackServiceConfiguration : IFallbackServiceConfiguration
    {
        #region Fields

        private readonly IServiceProvider _container;

        #endregion

        #region Constructors

        public FallbackServiceConfiguration(IServiceProvider container)
        {
            Should.NotBeNull(container, nameof(container));
            _container = container;
        }

        #endregion

        #region Implementation of interfaces

        public TService Instance<TService>() where TService : class
        {
            return _container.GetService<TService>();
        }

        public TService? Optional<TService>() where TService : class
        {
            if (_container.TryGetService<TService>(out var service))
                return service;
            return null;
        }

        #endregion
    }
}