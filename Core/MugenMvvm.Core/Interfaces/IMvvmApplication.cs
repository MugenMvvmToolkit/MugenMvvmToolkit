using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Models;

namespace MugenMvvm.Interfaces
{
    public interface IMvvmApplication : IHasMetadata<IObservableMetadataContext>
    {
        ApplicationState State { get; }

        IDeviceInfo DeviceInfo { get; }

        void OnStateChanged(ApplicationState value, IReadOnlyMetadataContext metadata);
    }
}