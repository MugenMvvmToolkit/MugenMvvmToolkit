using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.UnitTest.Metadata;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Navigation
{
    public class NavigationContextTest : MetadataOwnerTestBase
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var provider = new TestNavigationProvider();
            var type = NavigationType.Alert;
            var id = "t";
            var mode = NavigationMode.Remove;
            var context = new NavigationContext(provider, type, id, mode);
            context.NavigationProvider.ShouldEqual(provider);
            context.NavigationType.ShouldEqual(type);
            context.NavigationOperationId.ShouldEqual(id);
            context.NavigationMode.ShouldEqual(mode);
        }

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata, IMetadataContextProvider? metadataContextProvider)
        {
            return new NavigationContext(new TestNavigationProvider(), NavigationType.Alert, "t", NavigationMode.Remove, metadata, metadataContextProvider);
        }

        #endregion
    }
}