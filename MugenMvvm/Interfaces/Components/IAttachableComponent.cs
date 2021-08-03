using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IAttachableComponent : IComponent
    {
        void OnAttaching(object owner, IReadOnlyMetadataContext? metadata);

        void OnAttached(object owner, IReadOnlyMetadataContext? metadata);
    }
}