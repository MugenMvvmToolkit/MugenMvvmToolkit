using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IDetachableComponent
    {
#if NET461
        bool OnDetaching(object owner, IReadOnlyMetadataContext? metadata);
#else
        bool OnDetaching(object owner, IReadOnlyMetadataContext? metadata) => true;
#endif

        void OnDetached(object owner, IReadOnlyMetadataContext? metadata);
    }
}