using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.UnitTest.Metadata;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Navigation
{
    public class NavigationEntryTest : MetadataOwnerTestBase
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var provider = new TestNavigationProvider();
            var type = NavigationType.Alert;
            var id = "t";
            var navigationEntry = new NavigationEntry(provider, id, type);
            navigationEntry.NavigationProvider.ShouldEqual(provider);
            navigationEntry.NavigationType.ShouldEqual(type);
            navigationEntry.NavigationOperationId.ShouldEqual(id);
        }

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata, IMetadataContextProvider? metadataContextProvider)
        {
            return new NavigationEntry(new TestNavigationProvider(), "f", NavigationType.Alert, metadata, metadataContextProvider);
        }

        #endregion
    }
}