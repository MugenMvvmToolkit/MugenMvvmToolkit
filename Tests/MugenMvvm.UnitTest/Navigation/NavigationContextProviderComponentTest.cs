using MugenMvvm.Enums;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Navigation
{
    public class NavigationContextProviderComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetNavigationContextShouldReturnNavigationContext()
        {
            var component = new NavigationContextProviderComponent();
            var provider = new TestNavigationProvider();
            var type = NavigationType.Alert;
            var id = "t";
            var mode = NavigationMode.Remove;
            var metadata = new MetadataContext();

            var context = component.TryGetNavigationContext(provider, id, type, mode, metadata)!;
            context.NavigationProvider.ShouldEqual(provider);
            context.NavigationType.ShouldEqual(type);
            context.NavigationOperationId.ShouldEqual(id);
            context.NavigationMode.ShouldEqual(mode);
            context.Metadata.ShouldEqual(metadata);
        }

        #endregion
    }
}