using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization.Components
{
    public interface ISerializationContextProviderComponent : IComponent<ISerializer>
    {
        ISerializationContext? TryGetSerializationContext<TRequest, TResult>(ISerializer serializer, ISerializationFormatBase<TRequest, TResult> format, TRequest request, IReadOnlyMetadataContext? metadata);
    }
}