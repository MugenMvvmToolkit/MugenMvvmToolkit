using MugenMvvm.Binding.Members;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members
{
    public class MemberTypesRequestTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void DefaultShouldBeEmpty()
        {
            default(MemberTypesRequest).IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            string name = "T";
            var types = new[] {typeof(string)};
            var memberTypesRequest = new MemberTypesRequest(name, types);
            memberTypesRequest.IsEmpty.ShouldBeFalse();
            memberTypesRequest.Types.ShouldEqual(types);
            memberTypesRequest.Name.ShouldEqual(name);
        }

        #endregion
    }
}