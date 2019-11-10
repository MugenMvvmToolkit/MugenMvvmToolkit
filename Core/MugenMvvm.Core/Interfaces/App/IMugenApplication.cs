using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.App
{
    public interface IMugenApplication : IComponentOwner<IMugenApplication>, IMetadataOwner<IMetadataContext>
    {
        IDeviceInfo DeviceInfo { get; }

        ApplicationState State { get; }

        void SetApplicationState(ApplicationState state, IReadOnlyMetadataContext? metadata = null);
    }
}