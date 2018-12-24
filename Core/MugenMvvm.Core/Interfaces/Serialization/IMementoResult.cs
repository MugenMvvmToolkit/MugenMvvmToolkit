using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Serialization
{
    public interface IMementoResult
    {
        bool IsRestored { get; }

        IReadOnlyMetadataContext Metadata { get; }

        object? Target { get; }
    }
}