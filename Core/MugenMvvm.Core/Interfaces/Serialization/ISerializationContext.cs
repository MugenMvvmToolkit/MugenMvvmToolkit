using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface ISerializationContext : IMetadataOwner<IMetadataContext>, IHasTarget
    {
    }
}