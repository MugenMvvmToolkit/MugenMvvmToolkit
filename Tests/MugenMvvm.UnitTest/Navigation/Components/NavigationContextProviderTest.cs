using MugenMvvm.Enums;
using MugenMvvm.Metadata;
using MugenMvvm.Navigation.Components;
using MugenMvvm.UnitTest.Navigation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Navigation.Components
{
    public class NavigationContextProviderTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetNavigationContextShouldReturnNavigationContext()
        {
            var component = new NavigationContextProvider();
            var provider = new TestNavigationProvider();
            var type = NavigationType.Alert;
            var id = "t";
            var mode = NavigationMode.Close;
            var metadata = new MetadataContext();

            var context = component.TryGetNavigationContext(provider, id, type, mode, metadata)!;
            context.NavigationProvider.ShouldEqual(provider);
            context.NavigationType.ShouldEqual(type);
            context.NavigationId.ShouldEqual(id);
            context.NavigationMode.ShouldEqual(mode);
            context.Metadata.ShouldEqual(metadata);
        }

        #endregion
    }
}