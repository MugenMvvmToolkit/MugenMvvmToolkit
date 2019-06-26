using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Models
{
    public interface IMetadataOwner<out T>
        where T : class, IReadOnlyMetadataContext
    {
        bool HasMetadata { get; }

        T Metadata { get; }
    }
}