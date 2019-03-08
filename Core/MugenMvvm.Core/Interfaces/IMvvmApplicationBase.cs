using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces
{
    public interface IMvvmApplicationBase : IHasMetadata<IObservableMetadataContext>
    {
        IDeviceInfo DeviceInfo { get; }
    }
}