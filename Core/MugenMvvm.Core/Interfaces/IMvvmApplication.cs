using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces
{
    public interface IMvvmApplication : IHasMetadata<IObservableMetadataContext>
    {
        IDeviceInfo DeviceInfo { get; }
    }
}