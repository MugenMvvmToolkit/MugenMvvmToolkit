using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ISerializationContext : IMetadataOwner<IMetadataContext> //todo review?
    {
        ISerializer Serializer { get; }
    }
}