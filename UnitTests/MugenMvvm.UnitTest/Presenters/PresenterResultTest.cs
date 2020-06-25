using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Presenters;
using MugenMvvm.UnitTest.Metadata;
using MugenMvvm.UnitTest.Navigation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Presenters
{
    public class PresenterResultTest : MetadataOwnerTestBase
    {
        #region Methods

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

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata, IMetadataContextProvider? metadataContextProvider)
        {
            return new PresenterResult(this, "1", new TestNavigationProvider(), NavigationType.Alert, metadata, metadataContextProvider);
        }

        #endregion
    }
}