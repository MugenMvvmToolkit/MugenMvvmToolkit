using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.Tests.Navigation;
using MugenMvvm.UnitTests.Metadata;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Navigation
{
    public class NavigationEntryTest : MetadataOwnerTestBase
    {
        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var target = new object();
            var type = NavigationType.Alert;
            var id = "t";
            var navigationEntry = new NavigationEntry(target, TestNavigationProvider.Instance, id, type);
            navigationEntry.Target.ShouldEqual(target);
            navigationEntry.NavigationProvider.ShouldEqual(TestNavigationProvider.Instance);
            navigationEntry.NavigationType.ShouldEqual(type);
            navigationEntry.NavigationId.ShouldEqual(id);
        }

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata) =>
            new NavigationEntry(this, TestNavigationProvider.Instance, "f", NavigationType.Alert, metadata);
    }
}