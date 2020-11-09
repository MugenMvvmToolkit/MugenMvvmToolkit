using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization.Components
{
    public interface ISerializationManagerComponent : IComponent<ISerializer>
    {
        bool IsSupported<TRequest, TResult>(ISerializer serializer, ISerializationFormatBase<TRequest, TResult> format, [AllowNull] TRequest request, IReadOnlyMetadataContext? metadata);

        bool TrySerialize<TRequest, TResult>(ISerializer serializer, ISerializationFormat<TRequest, TResult> format, TRequest request, ISerializationContext serializationContext,
            [NotNullWhen(true)] [AllowNull] ref TResult result);

        bool TryDeserialize<TRequest, TResult>(ISerializer serializer, IDeserializationFormat<TRequest, TResult> format, TRequest request, ISerializationContext serializationContext,
            [NotNullWhen(true)] [AllowNull] ref TResult result);
    }
}