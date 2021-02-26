using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;

namespace MugenMvvm.App
{
    public class PlatformInfo : MetadataOwnerBase, IPlatformInfo
    {
        public static readonly IPlatformInfo UnitTest = new PlatformInfo(PlatformType.UnitTest);
        private string? _applicationVersion;
        private string? _deviceVersion;
        private PlatformIdiom? _idiom;

        public PlatformInfo(PlatformType type, PlatformIdiom? idiom = null, string? applicationVersion = null, string? deviceVersion = null,
            IReadOnlyMetadataContext? metadata = null) : base(metadata)
        {
            Should.NotBeNull(type, nameof(type));
            Type = type;
            _idiom = idiom;
            _applicationVersion = applicationVersion;
            _deviceVersion = deviceVersion;
        }

        public PlatformIdiom Idiom => _idiom ??= GetIdiom();

        public string ApplicationVersion => _applicationVersion ??= GetAppVersion();

        public string DeviceVersion => _deviceVersion ??= GetDeviceVersion();

        public PlatformType Type { get; }

        protected virtual PlatformIdiom GetIdiom() => PlatformIdiom.Unknown;

        protected virtual string GetAppVersion() => Environment.Version.ToString();

        protected virtual string GetDeviceVersion() => Environment.OSVersion.ToString();
    }
}