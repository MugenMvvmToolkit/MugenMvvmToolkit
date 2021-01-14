using MugenMvvm.UnitTests.ViewModels.Internal;
using MugenMvvm.Views;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Views
{
    public class ViewMappingTest : UnitTestBase
    {
        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var id = "id";
            var viewType = typeof(string);
            var viewModelType = typeof(TestViewModel);
            var mapping = new ViewMapping(id, viewModelType, viewType, DefaultMetadata);
            mapping.Metadata.ShouldEqual(DefaultMetadata);
            mapping.Id.ShouldEqual(id);
            mapping.ViewType.ShouldEqual(viewType);
            mapping.ViewModelType.ShouldEqual(viewModelType);
        }
    }
}