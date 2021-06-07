using System;

namespace MugenMvvm.Tests.Internal
{
    public class TestServiceProvider : IServiceProvider
    {
        public Func<Type, object>? GetService { get; set; }

        object IServiceProvider.GetService(Type serviceType) => GetService?.Invoke(serviceType)!;
    }
}