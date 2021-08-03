using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Components
{
    public interface IHasDetachConditionComponent : IComponent
    {
        bool CanDetach(object owner, IReadOnlyMetadataContext? metadata);
    }
}