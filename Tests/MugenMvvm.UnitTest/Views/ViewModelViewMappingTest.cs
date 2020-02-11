using MugenMvvm.UnitTest.ViewModels;
using MugenMvvm.Views;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Views
{
    public class ViewModelViewMappingTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var id = "id";
            var viewType = typeof(string);
            var viewModelType = typeof(TestViewModel);
            var mapping = new ViewModelViewMapping(id, viewType, viewModelType, DefaultMetadata);
            mapping.Metadata.ShouldEqual(DefaultMetadata);
            mapping.Id.ShouldEqual(id);
            mapping.ViewType.ShouldEqual(viewType);
            mapping.ViewModelType.ShouldEqual(viewModelType);
        }

        #endregion
    }
}