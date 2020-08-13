using System;
using MugenMvvm.App;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Metadata;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.App
{
    public class PlatformInfoTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            PlatformType type = PlatformType.Android;
            var meta = new MetadataContext();
            var deviceInfo = new PlatformInfo(type, meta);
            deviceInfo.Metadata.ShouldEqual(meta);
            deviceInfo.Idiom.ShouldEqual(PlatformIdiom.Unknown);
            deviceInfo.Type.ShouldEqual(type);
            deviceInfo.ApplicationVersion.ShouldEqual("0.0");
            deviceInfo.DeviceVersion.ShouldEqual("0.0");
            deviceInfo.HasMetadata.ShouldBeFalse();
            meta.Set(ViewModelMetadata.Id, Guid.Empty);
            deviceInfo.HasMetadata.ShouldBeTrue();
        }

        #endregion
    }
}