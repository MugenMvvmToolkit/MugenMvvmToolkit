using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface ILockerChangedListener<in T> : IComponent where T : class
    {
        void OnChanged(T owner, ILocker locker, IReadOnlyMetadataContext? metadata);
    }
}