using MugenMvvm.UnitTest.ViewModels.Internal;
using MugenMvvm.Views;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Views
{
    public class ViewInitializationResultTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void IsEmptyShouldBeTrueDefault()
        {
            ViewInitializationResult result = default;
            result.IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var view = new View(new ViewModelViewMapping("id", typeof(object), typeof(TestViewModel), DefaultMetadata), this);
            var vm = new TestViewModel();
            var result = new ViewInitializationResult(view, vm, DefaultMetadata);
            result.View.ShouldEqual(view);
            result.ViewModel.ShouldEqual(vm);
            result.Metadata.ShouldEqual(DefaultMetadata);
        }

        #endregion
    }
}