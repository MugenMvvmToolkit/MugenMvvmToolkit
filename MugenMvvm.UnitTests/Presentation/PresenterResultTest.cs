using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Presentation;
using MugenMvvm.Tests.Navigation;
using MugenMvvm.UnitTests.Metadata;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Presentation
{
    public class PresenterResultTest : MetadataOwnerTestBase
    {
        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var target = new object();
            var id = "test";
            var navigationType = NavigationType.Alert;
            var presenterResult = new PresenterResult(target, id, TestNavigationProvider.Instance, navigationType, Metadata);
            presenterResult.Target.ShouldEqual(target);
            presenterResult.NavigationType.ShouldEqual(navigationType);
            presenterResult.NavigationId.ShouldEqual(id);
            presenterResult.NavigationProvider.ShouldEqual(TestNavigationProvider.Instance);
            presenterResult.Metadata.ShouldEqual(Metadata);
        }

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata) =>
            new PresenterResult(this, "1", TestNavigationProvider.Instance, NavigationType.Alert, metadata);
    }
}