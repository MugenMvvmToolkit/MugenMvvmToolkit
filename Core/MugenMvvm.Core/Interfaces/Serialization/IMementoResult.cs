using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface IMementoResult : IMetadataOwner<IReadOnlyMetadataContext>
    {
        bool IsRestored { get; }

        object? Target { get; }
    }
}