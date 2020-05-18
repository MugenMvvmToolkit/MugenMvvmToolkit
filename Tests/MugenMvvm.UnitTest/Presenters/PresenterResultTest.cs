using MugenMvvm.Enums;
using MugenMvvm.Metadata;
using MugenMvvm.Presenters;
using MugenMvvm.UnitTest.Navigation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Presenters
{
    public class PresenterResultTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void IsEmptyShouldBeTrueForDefault()
        {
            PresenterResult result = default;
            result.IsEmpty.ShouldBeTrue();
            result.UpdateMetadata(DefaultMetadata).IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var id = "test";
            var provider = new TestNavigationProvider();
            var navigationType = NavigationType.Alert;
            var presenterResult = new PresenterResult(id, provider, navigationType, DefaultMetadata);
            presenterResult.IsEmpty.ShouldBeFalse();
            presenterResult.NavigationType.ShouldEqual(navigationType);
            presenterResult.NavigationId.ShouldEqual(id);
            presenterResult.NavigationProvider.ShouldEqual(provider);
            presenterResult.Metadata.ShouldEqual(DefaultMetadata);
        }

        [Fact]
        public void UpdateMetadataShouldChangeMetadata()
        {
            var id = "test";
            var provider = new TestNavigationProvider();
            var navigationType = NavigationType.Alert;
            var presenterResult = new PresenterResult(id, provider, navigationType, DefaultMetadata);
            presenterResult.IsEmpty.ShouldBeFalse();
            presenterResult.NavigationType.ShouldEqual(navigationType);
            presenterResult.NavigationId.ShouldEqual(id);
            presenterResult.NavigationProvider.ShouldEqual(provider);
            presenterResult.Metadata.ShouldEqual(DefaultMetadata);

            var updatedMetadata = new MetadataContext();
            presenterResult = presenterResult.UpdateMetadata(updatedMetadata);
            presenterResult.IsEmpty.ShouldBeFalse();
            presenterResult.NavigationType.ShouldEqual(navigationType);
            presenterResult.NavigationId.ShouldEqual(id);
            presenterResult.NavigationProvider.ShouldEqual(provider);
            presenterResult.Metadata.ShouldEqual(updatedMetadata);
        }

        #endregion
    }
}