using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IDetachableComponent
    {
        bool OnDetaching(object owner, IReadOnlyMetadataContext? metadata);

        void OnDetached(object owner, IReadOnlyMetadataContext? metadata);
    }
}