using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IHasAttachConditionComponent : IComponent
    {
        bool CanAttach(object owner, IReadOnlyMetadataContext? metadata);
    }
}