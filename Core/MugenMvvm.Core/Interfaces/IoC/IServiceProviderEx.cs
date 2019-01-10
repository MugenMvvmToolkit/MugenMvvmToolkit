using System;

namespace MugenMvvm.Interfaces.IoC
{
    public interface IServiceProviderEx : IServiceProvider
    {
        bool TryGetService(Type serviceType, out object? service);
    }
}