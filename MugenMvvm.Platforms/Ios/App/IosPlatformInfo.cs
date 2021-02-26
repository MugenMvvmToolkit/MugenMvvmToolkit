using Foundation;
using MugenMvvm.App;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using UIKit;

namespace MugenMvvm.Ios.App
{
    public class IosPlatformInfo : PlatformInfo
    {
        public IosPlatformInfo(IReadOnlyMetadataContext? metadata = null)
            : base(PlatformType.iOS, null, null, null, metadata)
        {
        }

        protected override string GetDeviceVersion() => UIDevice.CurrentDevice.SystemVersion;

        protected override PlatformIdiom GetIdiom() =>
            UIDevice.CurrentDevice.UserInterfaceIdiom switch
            {
                UIUserInterfaceIdiom.Pad => PlatformIdiom.Tablet,
                UIUserInterfaceIdiom.Phone => PlatformIdiom.Phone,
                UIUserInterfaceIdiom.TV => PlatformIdiom.TV,
                _ => PlatformIdiom.Unknown
            };

        protected override string GetAppVersion()
        {
            var version = NSBundle.MainBundle.InfoDictionary.ObjectForKey(new NSString("CFBundleShortVersionString"))?.ToString();
            if (version == null)
                return "0.0";
            if (version.StartsWith("0."))
                version = version.Substring(2);
            return version;
        }
    }
}