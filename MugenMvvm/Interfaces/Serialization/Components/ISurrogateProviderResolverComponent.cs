using System;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Serialization.Components
{
    public interface ISurrogateProviderResolverComponent : IComponent<ISerializer>
    {
        ISurrogateProvider? TryGetSurrogateProvider(ISerializer serializer, Type type, ISerializationContext? serializationContext);
    }
}