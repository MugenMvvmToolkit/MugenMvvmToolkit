using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Serialization;
using MugenMvvm.Interfaces.Serialization.Components;

namespace MugenMvvm.Serialization.Components
{
    public sealed class SerializationManager : ISerializationManagerComponent, IHasPriority
    {
        public int Priority { get; set; } = SerializationComponentPriority.Serializer;

        public bool IsSupported<TRequest, TResult>(ISerializer serializer, ISerializationFormatBase<TRequest, TResult> format, TRequest? request,
            IReadOnlyMetadataContext? metadata)
        {
            if (format.IsSerialization)
                return serializer.Components.Get<ISerializerComponent<TRequest, TResult>>(metadata)!.IsSupported(serializer, (ISerializationFormat<TRequest, TResult>) format,
                    request, metadata);
            return serializer.Components.Get<IDeserializerComponent<TRequest, TResult>>(metadata)!.IsSupported(serializer, (IDeserializationFormat<TRequest, TResult>) format,
                request, metadata);
        }

        public bool TrySerialize<TRequest, TResult>(ISerializer serializer, ISerializationFormat<TRequest, TResult> format, TRequest request,
            ISerializationContext serializationContext,
            [NotNullWhen(true)] ref TResult? result) =>
            serializer.Components.Get<ISerializerComponent<TRequest, TResult>>(serializationContext.GetMetadataOrDefault())
                      .TrySerialize(serializer, format, request, serializationContext, ref result);

        public bool TryDeserialize<TRequest, TResult>(ISerializer serializer, IDeserializationFormat<TRequest, TResult> format, TRequest request,
            ISerializationContext serializationContext,
            [NotNullWhen(true)] ref TResult? result) =>
            serializer.Components.Get<IDeserializerComponent<TRequest, TResult>>(serializationContext.GetMetadataOrDefault())
                      .TryDeserialize(serializer, format, request, serializationContext, ref result);
    }
}