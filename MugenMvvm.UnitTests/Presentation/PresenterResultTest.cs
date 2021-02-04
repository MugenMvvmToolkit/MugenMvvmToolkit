using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Presentation;
using MugenMvvm.UnitTests.Metadata;
using MugenMvvm.UnitTests.Navigation.Internal;
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
            var provider = new TestNavigationProvider();
            var navigationType = NavigationType.Alert;
            var presenterResult = new PresenterResult(target, id, provider, navigationType, DefaultMetadata);
            presenterResult.Target.ShouldEqual(target);
            presenterResult.NavigationType.ShouldEqual(navigationType);
            presenterResult.NavigationId.ShouldEqual(id);
            presenterResult.NavigationProvider.ShouldEqual(provider);
            presenterResult.Metadata.ShouldEqual(DefaultMetadata);
        }

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata) =>
            new PresenterResult(this, "1", new TestNavigationProvider(), NavigationType.Alert, metadata);
    }
}