using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.App
{
    public interface IMugenApplication : IComponentOwner<IMugenApplication>, IMetadataOwner<IMetadataContext>//todo configuration ext
    {
        IDeviceInfo DeviceInfo { get; }

        void OnLifecycleChanged<TState>(ApplicationLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata = null);

        void Initialize<TState>(IDeviceInfo device, in TState state, IReadOnlyMetadataContext? metadata = null);

        void Start<TState>(in TState state, IReadOnlyMetadataContext? metadata = null);
    }
}