using System;
using MugenMvvm.App;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Windows.App
{
    public sealed class WindowsPlatformInfo : PlatformInfo
    {
        private string? _appVersion;
        private string? _deviceVersion;

        public WindowsPlatformInfo(PlatformType type, IReadOnlyMetadataContext? metadata = null)
            : base(type, metadata)
        {
        }

        public override PlatformIdiom Idiom => PlatformIdiom.Desktop;

        public override string ApplicationVersion => _appVersion ??= Environment.Version.ToString();

        public override string DeviceVersion => _deviceVersion ??= Environment.OSVersion.ToString();
    }
}