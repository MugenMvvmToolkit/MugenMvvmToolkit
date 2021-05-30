using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;

namespace MugenMvvm.Serialization
{
    public sealed class Serializer : ComponentOwnerBase<ISerializer>, ISerializer
    {
        public Serializer(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
        }

        public bool IsSupported<TRequest, TResult>(ISerializationFormatBase<TRequest, TResult> format, TRequest? request = default, IReadOnlyMetadataContext? metadata = null)
            => GetComponents<ISerializationManagerComponent>(metadata).IsSupported(this, format, request, metadata);

        public bool TrySerialize<TRequest, TResult>(ISerializationFormat<TRequest, TResult> format, TRequest request, [NotNullWhen(true)] ref TResult? result,
            IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<ISerializationManagerComponent>(metadata).TrySerialize(this, format, request, GetContext(format, request, metadata), ref result);

        public bool TryDeserialize<TRequest, TResult>(IDeserializationFormat<TRequest, TResult> format, TRequest request, [NotNullWhen(true)] ref TResult? result,
            IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<ISerializationManagerComponent>(metadata).TryDeserialize(this, format, request, GetContext(format, request, metadata), ref result);

        private ISerializationContext GetContext<TRequest, TResult>(ISerializationFormatBase<TRequest, TResult> format, TRequest request, IReadOnlyMetadataContext? metadata) =>
            GetComponents<ISerializationContextProviderComponent>(metadata).TryGetSerializationContext(this, format, request, metadata) ??
            new SerializationContext<TRequest, TResult>(format, request, metadata);
    }
}