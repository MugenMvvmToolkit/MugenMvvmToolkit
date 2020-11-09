using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization.Components
{
    public interface ISerializerComponent<in TRequest, TResult> : IComponent<ISerializer>
    {
        bool IsSupported(ISerializer serializer, ISerializationFormat<TRequest, TResult> format, [AllowNull] TRequest request, IReadOnlyMetadataContext? metadata);

        bool TrySerialize(ISerializer serializer, ISerializationFormat<TRequest, TResult> format, TRequest request, ISerializationContext serializationContext,
            [NotNullWhen(true)] [AllowNull] ref TResult result);
    }
}