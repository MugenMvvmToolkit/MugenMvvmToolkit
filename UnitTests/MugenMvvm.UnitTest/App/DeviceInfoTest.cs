using System;
using MugenMvvm.App;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Metadata;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.App
{
    public class DeviceInfoTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            PlatformType type = PlatformType.Android;
            PlatformIdiom idiom = PlatformIdiom.Desktop;
            string version = "v";
            var meta = new MetadataContext();
            var deviceInfo = new DeviceInfo(type, () => idiom, () => version, meta);
            deviceInfo.Metadata.ShouldEqual(meta);
            deviceInfo.Idiom.ShouldEqual(idiom);
            deviceInfo.Platform.ShouldEqual(type);
            deviceInfo.RawVersion.ShouldEqual(version);
            deviceInfo.HasMetadata.ShouldBeFalse();
            meta.Set(ViewModelMetadata.Id, Guid.Empty);
            deviceInfo.HasMetadata.ShouldBeTrue();
        }

        #endregion
    }
}