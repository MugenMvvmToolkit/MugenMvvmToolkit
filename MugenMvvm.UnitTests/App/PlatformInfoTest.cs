using MugenMvvm.App;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTests.Metadata;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.App
{
    public class PlatformInfoTest : MetadataOwnerTestBase
    {
        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            PlatformType type = PlatformType.Android;
            var idiom = PlatformIdiom.Desktop;
            var appVersion = "1";
            var deviceVersion = "2";
            var meta = new MetadataContext();
            var deviceInfo = new PlatformInfo(type, idiom, appVersion, deviceVersion, meta);
            deviceInfo.Metadata.ShouldEqual(meta);
            deviceInfo.Idiom.ShouldEqual(idiom);
            deviceInfo.Type.ShouldEqual(type);
            deviceInfo.ApplicationVersion.ShouldEqual(appVersion);
            deviceInfo.DeviceVersion.ShouldEqual(deviceVersion);
            deviceInfo.HasMetadata.ShouldBeFalse();
            meta.Set(ViewModelMetadata.Id, "");
            deviceInfo.HasMetadata.ShouldBeTrue();
        }

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata) => new PlatformInfo(PlatformType.UnitTest, metadata: metadata);
    }
}