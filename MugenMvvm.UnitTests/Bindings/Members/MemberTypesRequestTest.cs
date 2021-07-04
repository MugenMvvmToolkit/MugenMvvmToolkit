using MugenMvvm.Bindings.Members;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Members
{
    public class MemberTypesRequestTest : UnitTestBase
    {
        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            string name = "T";
            var types = new[] {typeof(string)};
            var memberTypesRequest = new MemberTypesRequest(name, types);
            memberTypesRequest.Types.ShouldEqual(types);
            memberTypesRequest.Name.ShouldEqual(name);
        }
    }
}