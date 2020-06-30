using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.UnitTest.Metadata;
using MugenMvvm.UnitTest.Navigation.Internal;
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
            var target = new object();
            var provider = new TestNavigationProvider();
            var type = NavigationType.Alert;
            var id = "t";
            var navigationEntry = new NavigationEntry(target, provider, id, type);
            navigationEntry.Target.ShouldEqual(target);
            navigationEntry.NavigationProvider.ShouldEqual(provider);
            navigationEntry.NavigationType.ShouldEqual(type);
            navigationEntry.NavigationId.ShouldEqual(id);
        }

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata, IMetadataContextManager? metadataContextManager)
        {
            return new NavigationEntry(this, new TestNavigationProvider(), "f", NavigationType.Alert, metadata, metadataContextManager);
        }

        #endregion
    }
}