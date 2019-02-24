using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Models
{
    public interface IDeviceInfo : IHasMetadata<IObservableMetadataContext>
    {
        PlatformType Platform { get; }

        PlatformIdiom Idiom { get; }

        string RawVersion { get; }
    }
}