using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface ILockerChangedListener : IComponent
    {
        void OnChanged(object owner, ILocker locker, IReadOnlyMetadataContext? metadata);
    }
}