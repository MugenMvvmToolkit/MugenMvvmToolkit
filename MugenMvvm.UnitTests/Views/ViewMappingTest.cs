using MugenMvvm.Tests.ViewModels;
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
            var mapping = new ViewMapping(id, viewModelType, viewType, Metadata);
            mapping.Metadata.ShouldEqual(Metadata);
            mapping.Id.ShouldEqual(id);
            mapping.ViewType.ShouldEqual(viewType);
            mapping.ViewModelType.ShouldEqual(viewModelType);
        }
    }
}