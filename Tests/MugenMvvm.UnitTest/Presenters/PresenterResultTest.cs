using MugenMvvm.Enums;
using MugenMvvm.Presenters;
using MugenMvvm.UnitTest.Navigation;
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
            presenterResult.NavigationOperationId.ShouldEqual(id);
            presenterResult.NavigationProvider.ShouldEqual(provider);
            presenterResult.Metadata.ShouldEqual(DefaultMetadata);
        }

        #endregion
    }
}