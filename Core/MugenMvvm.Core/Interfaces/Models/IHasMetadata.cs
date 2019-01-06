using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Models
{
    public interface IHasMetadata<out T>
        where T : class, IReadOnlyMetadataContext
    {
        T Metadata { get; }
    }
}