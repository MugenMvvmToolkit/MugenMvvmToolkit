using MugenMvvm.Binding.Resources;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Resources
{
    public class ResourceResolverRequestTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void DefaultShouldBeEmpty()
        {
            default(ResourceResolverRequest).IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var target = typeof(object);
            var source = "";
            var state = "test";
            var memberManagerRequest = new ResourceResolverRequest(target, source, state);
            memberManagerRequest.Target.ShouldEqual(target);
            memberManagerRequest.Source.ShouldEqual(source);
            memberManagerRequest.State.ShouldEqual(state);
            memberManagerRequest.IsEmpty.ShouldBeFalse();
        }

        #endregion
    }
}