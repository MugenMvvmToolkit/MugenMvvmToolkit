using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Entities.Components
{
    public interface IEntityStateChangedListener : IComponent<IEntityTrackingCollection>
    {
        void OnEntityStateChanged(IEntityTrackingCollection collection, object entity, EntityState from, EntityState to, IReadOnlyMetadataContext? metadata);
    }
}