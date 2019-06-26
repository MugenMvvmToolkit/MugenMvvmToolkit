using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface IMementoResult : IMetadataOwner<IReadOnlyMetadataContext>
    {
        bool IsRestored { get; }

        object? Target { get; }
    }
}