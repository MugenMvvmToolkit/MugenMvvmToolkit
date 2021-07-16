using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Tests.ViewModels;
using MugenMvvm.UnitTests.Metadata;
using MugenMvvm.Views;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Views
{
    public class ViewTest : MetadataOwnerTestBase
    {
        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var mapping = new ViewMapping("id", typeof(TestViewModel), typeof(object), Metadata);
            var view = new object();
            var testViewModel = new TestViewModel();
            IView v = new View(mapping, view, testViewModel);
            v.Mapping.ShouldEqual(mapping);
            v.Target.ShouldEqual(view);
            v.ViewModel.ShouldEqual(testViewModel);
        }

        protected override IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata) =>
            new View(new ViewMapping("id", typeof(TestViewModel), typeof(object), Metadata), this, new TestViewModel(), metadata);
    }
}