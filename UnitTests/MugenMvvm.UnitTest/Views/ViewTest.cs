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
            var testViewModel = new TestViewModel();
            IView v = new View(mapping, view, testViewModel);
            v.Mapping.ShouldEqual(mapping);
            v.Target.ShouldEqual(view);
            v.ViewModel.ShouldEqual(testViewModel);
        }

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata, IMetadataContextManager? metadataContextManager)
        {
            return new View(new ViewModelViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata), this, new TestViewModel(), metadata, null, metadataContextManager);
        }

        #endregion
    }
}