using System;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IMugenServiceProvider : IServiceProvider
    {
        object? GetService(Type serviceType, IReadOnlyMetadataContext? metadata);
    }
}