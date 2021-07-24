using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IAttachableComponent
    {
#if NET461
        bool OnAttaching(object owner, IReadOnlyMetadataContext? metadata);
#else
        bool OnAttaching(object owner, IReadOnlyMetadataContext? metadata) => true;
#endif

        void OnAttached(object owner, IReadOnlyMetadataContext? metadata);
    }
}