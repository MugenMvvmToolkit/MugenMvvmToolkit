using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Navigation;
using MugenMvvm.UnitTest.Metadata;
using MugenMvvm.UnitTest.Navigation.Internal;
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
            var mode = NavigationMode.Close;
            var target = new object();
            var context = new NavigationContext(target, provider, id, type, mode);
            context.Target.ShouldEqual(target);
            context.NavigationProvider.ShouldEqual(provider);
            context.NavigationType.ShouldEqual(type);
            context.NavigationId.ShouldEqual(id);
            context.NavigationMode.ShouldEqual(mode);
        }

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata)
        {
            return new NavigationContext(null, new TestNavigationProvider(), "t", NavigationType.Alert, NavigationMode.Close, metadata);
        }

        #endregion
    }
}