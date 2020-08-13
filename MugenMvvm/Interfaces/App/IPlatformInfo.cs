using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.App
{
    public interface IPlatformInfo : IMetadataOwner<IMetadataContext>
    {
        PlatformType Type { get; }

        PlatformIdiom Idiom { get; }

        string ApplicationVersion { get; }

        string DeviceVersion { get; }
    }
}