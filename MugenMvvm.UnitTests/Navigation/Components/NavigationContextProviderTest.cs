using MugenMvvm.Enums;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation.Components;
using MugenMvvm.Tests.Navigation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Navigation.Components
{
    public class NavigationContextProviderTest : UnitTestBase
    {
        [Fact]
        public void TryGetNavigationContextShouldReturnNavigationContext()
        {
            var component = new NavigationContextProvider();
            var target = new object();
            var type = NavigationType.Alert;
            var id = "t";
            var mode = NavigationMode.Close;
            var metadata = new MetadataContext();

            var context = component.TryGetNavigationContext(null!, target, TestNavigationProvider.Instance, id, type, mode, metadata)!;
            context.Target.ShouldEqual(target);
            context.NavigationProvider.ShouldEqual(TestNavigationProvider.Instance);
            context.NavigationType.ShouldEqual(type);
            context.NavigationId.ShouldEqual(id);
            context.NavigationMode.ShouldEqual(mode);
            context.Metadata.ShouldEqual(metadata);
        }
    }
}