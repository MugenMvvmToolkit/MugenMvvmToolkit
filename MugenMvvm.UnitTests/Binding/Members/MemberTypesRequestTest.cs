using MugenMvvm.Binding.Members;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Binding.Members
{
    public class MemberTypesRequestTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            string name = "T";
            var types = new[] {typeof(string)};
            var memberTypesRequest = new MemberTypesRequest(name, types);
            memberTypesRequest.Types.ShouldEqual(types);
            memberTypesRequest.Name.ShouldEqual(name);
        }

        #endregion
    }
}