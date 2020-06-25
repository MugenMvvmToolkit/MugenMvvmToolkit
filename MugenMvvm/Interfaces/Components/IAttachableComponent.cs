using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IAttachableComponent
    {
        bool OnAttaching(object owner, IReadOnlyMetadataContext? metadata);

        void OnAttached(object owner, IReadOnlyMetadataContext? metadata);
    }
}