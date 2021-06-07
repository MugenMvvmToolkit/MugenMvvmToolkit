using System;
using MugenMvvm.Interfaces.Internal;

namespace MugenMvvm.Tests.Internal
{
    public class TestFallbackServiceConfiguration : IFallbackServiceConfiguration
    {
        public Func<Type, object>? Instance { get; set; }

        public Func<Type, object?>? Optional { get; set; }

        TService IFallbackServiceConfiguration.Instance<TService>() where TService : class => (TService) Instance?.Invoke(typeof(TService))!;

        TService? IFallbackServiceConfiguration.Optional<TService>() where TService : class => (TService?) Optional?.Invoke(typeof(TService));
    }
}