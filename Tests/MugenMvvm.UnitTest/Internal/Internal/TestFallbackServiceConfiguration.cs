using System;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.UnitTest.Internal.Internal
{
    public class TestFallbackServiceConfiguration : IFallbackServiceConfiguration
    {
        #region Properties

        public Func<Type, object>? Instance { get; set; }

        public Func<Type, object>? Optional { get; set; }

        #endregion

        #region Implementation of interfaces

        TService IFallbackServiceConfiguration.Instance<TService>() where TService : class
        {
            return (TService) Instance?.Invoke(typeof(TService))!;
        }

        TService? IFallbackServiceConfiguration.Optional<TService>() where TService : class
        {
            return (TService?) Optional?.Invoke(typeof(TService));
        }

        #endregion
    }
}