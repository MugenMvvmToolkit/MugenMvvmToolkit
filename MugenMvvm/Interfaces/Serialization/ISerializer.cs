using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ISerializer : IComponentOwner<ISerializer>
    {
        bool IsSupported<TRequest, TResult>(ISerializationFormatBase<TRequest, TResult> format, IReadOnlyMetadataContext? metadata = null);

        bool TrySerialize<TRequest, TResult>(ISerializationFormat<TRequest, TResult> format, TRequest request, [NotNullWhen(true), AllowNull] ref TResult result, IReadOnlyMetadataContext? metadata = null);

        bool TryDeserialize<TRequest, TResult>(IDeserializationFormat<TRequest, TResult> format, TRequest request, [NotNullWhen(true), AllowNull] ref TResult result, IReadOnlyMetadataContext? metadata = null);
    }
}