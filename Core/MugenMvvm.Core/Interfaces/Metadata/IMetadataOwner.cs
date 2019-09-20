namespace MugenMvvm.Interfaces.Metadata
{
    public interface IMetadataOwner<out T>
        where T : class, IReadOnlyMetadataContext
    {
        bool HasMetadata { get; }

        T Metadata { get; }
    }
}