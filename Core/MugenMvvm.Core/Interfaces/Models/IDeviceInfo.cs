using MugenMvvm.Enums;
using MugenMvvm.Models;

namespace MugenMvvm.Interfaces.Models
{
    public interface IDeviceInfo
    {
        PlatformType Platform { get; }

        PlatformIdiom Idiom { get; }

        string RawVersion { get; }
    }
}