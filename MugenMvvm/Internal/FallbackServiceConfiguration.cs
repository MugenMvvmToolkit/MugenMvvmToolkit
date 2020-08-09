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
            if (typeof(TService) == typeof(IServiceProvider))
                return (TService) _container;
            var service = _container.GetService(typeof(TService));
            if (service == null)
                ExceptionManager.ThrowCannotResolveService(typeof(TService));
            return (TService) service;
        }

        public TService? Optional<TService>() where TService : class
        {
            if (typeof(TService) == typeof(IServiceProvider))
                return (TService) _container;
            return (TService?) _container.GetService(typeof(TService));
        }

        #endregion
    }
}