using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.Tests.Navigation;
using MugenMvvm.UnitTests.Metadata;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Navigation
{
    public class NavigationContextTest : MetadataOwnerTestBase
    {
        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var type = NavigationType.Alert;
            var id = "t";
            var mode = NavigationMode.Close;
            var target = new object();
            var context = new NavigationContext(target, TestNavigationProvider.Instance, id, type, mode);
            context.Target.ShouldEqual(target);
            context.NavigationProvider.ShouldEqual(TestNavigationProvider.Instance);
            context.NavigationType.ShouldEqual(type);
            context.NavigationId.ShouldEqual(id);
            context.NavigationMode.ShouldEqual(mode);
        }

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata) =>
            new NavigationContext(null, TestNavigationProvider.Instance, "t", NavigationType.Alert, NavigationMode.Close, metadata);
    }
}