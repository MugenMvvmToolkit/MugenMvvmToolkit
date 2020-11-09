using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization.Components
{
    public interface IDeserializerComponent<in TRequest, TResult> : IComponent<ISerializer>
    {
        bool IsSupported(ISerializer serializer, IDeserializationFormat<TRequest, TResult> format, [AllowNull] TRequest request, IReadOnlyMetadataContext? metadata);

        bool TryDeserialize(ISerializer serializer, IDeserializationFormat<TRequest, TResult> format, TRequest request, ISerializationContext serializationContext,
            [NotNullWhen(true)] [AllowNull] ref TResult result);
    }
}