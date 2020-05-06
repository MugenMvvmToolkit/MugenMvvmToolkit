using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.UnitTest.Metadata;
using MugenMvvm.UnitTest.ViewModels.Internal;
using MugenMvvm.Views;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Views
{
    public class ViewTest : MetadataOwnerTestBase
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var mapping = new ViewModelViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata);
            var view = new object();
            IView v = new View(mapping, view);
            v.Mapping.ShouldEqual(mapping);
            v.View.ShouldEqual(view);
        }

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata, IMetadataContextProvider? metadataContextProvider)
        {
            return new View(new ViewModelViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata), this, metadata, metadataContextProvider);
        }

        #endregion
    }
}