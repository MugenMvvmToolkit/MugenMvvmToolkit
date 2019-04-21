using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.App
{
    public interface IMvvmApplicationBase : IHasMetadata<IObservableMetadataContext>
    {
        IDeviceInfo DeviceInfo { get; }
    }
}