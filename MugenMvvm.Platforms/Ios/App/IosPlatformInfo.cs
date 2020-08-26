using Foundation;
using MugenMvvm.App;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using UIKit;

namespace MugenMvvm.Ios.App
{
    public class IosPlatformInfo : PlatformInfo
    {
        #region Fields

        private string? _applicationVersion;
        private string? _deviceVersion;
        private PlatformIdiom? _idiom;

        #endregion

        #region Constructors

        protected internal IosPlatformInfo(IReadOnlyMetadataContext? metadata = null)
            : base(PlatformType.iOS, metadata)
        {
        }

        #endregion

        #region Properties

        public override string ApplicationVersion => _applicationVersion ??= GetVersion();

        public override string DeviceVersion => _deviceVersion ??= UIDevice.CurrentDevice.SystemVersion;

        public override PlatformIdiom Idiom => _idiom ??= GetIdiom();

        #endregion

        #region Methods

        private static PlatformIdiom GetIdiom() =>
            UIDevice.CurrentDevice.UserInterfaceIdiom switch
            {
                UIUserInterfaceIdiom.Pad => PlatformIdiom.Tablet,
                UIUserInterfaceIdiom.Phone => PlatformIdiom.Phone,
                UIUserInterfaceIdiom.TV => PlatformIdiom.TV,
                _ => PlatformIdiom.Unknown
            };

        private static string GetVersion()
        {
            var version = NSBundle.MainBundle.InfoDictionary.ObjectForKey(new NSString("CFBundleShortVersionString"))?.ToString();
            if (version == null)
                return "0.0";
            if (version.StartsWith("0."))
                version = version.Substring(2);
            return version;
        }

        #endregion
    }
}