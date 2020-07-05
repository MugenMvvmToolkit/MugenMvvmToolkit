using System.Diagnostics.CodeAnalysis;
using System.IO;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Serialization.Components
{
    public interface ISerializerComponent : IComponent<ISerializer>
    {
        bool TrySerialize<TRequest>(ISerializer serializer, Stream stream, [DisallowNull] in TRequest request, ISerializationContext serializationContext);

        bool TryDeserialize(ISerializer serializer, Stream stream, ISerializationContext serializationContext, out object? value);
    }
}