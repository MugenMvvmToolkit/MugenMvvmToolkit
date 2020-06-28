using System;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Serialization.Components
{
    public interface ISurrogateProviderComponent : IComponent<ISerializer>
    {
        ISurrogateProvider? TryGetSurrogateProvider(Type type, ISerializationContext? serializationContext);
    }
}