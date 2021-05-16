using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface ISynchronizationListener : IComponent
    {
        void OnLocking(object target, IReadOnlyMetadataContext? metadata);

        void OnLocked(object target, IReadOnlyMetadataContext? metadata);

        void OnUnlocking(object target, IReadOnlyMetadataContext? metadata);

        void OnUnlocked(object target, IReadOnlyMetadataContext? metadata);
    }
}