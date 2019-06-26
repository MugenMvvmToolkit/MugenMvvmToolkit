using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization.Components
{
    public interface ISerializationContextProviderComponent : IComponent<ISerializer>
    {
        ISerializationContext? TryGetSerializationContext(ISerializer serializer, IServiceProvider? serviceProvider, IReadOnlyMetadataContext? metadata);
    }
}