using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.App
{
    //todo add applicationstatedispatcher
    public interface IMugenApplication : IComponentOwner<IMugenApplication>, IMetadataOwner<IMetadataContext>
    {
        IDeviceInfo DeviceInfo { get; }

        ApplicationLifecycleState State { get; }

        public void OnLifecycleChanged<TState>(ApplicationLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata = null);
    }
}