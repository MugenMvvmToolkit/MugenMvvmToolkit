using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Members;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members
{
    public class MemberManagerRequestTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void DefaultShouldBeEmpty()
        {
            default(MemberManagerRequest).IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var type = typeof(object);
            var name = "test";
            var memberTypes = MemberType.Event;
            var flags = MemberFlags.All;
            var memberManagerRequest = new MemberManagerRequest(type, name, memberTypes, flags);
            memberManagerRequest.Type.ShouldEqual(type);
            memberManagerRequest.Name.ShouldEqual(name);
            memberManagerRequest.MemberTypes.ShouldEqual(memberTypes);
            memberManagerRequest.Flags.ShouldEqual(flags);
            memberManagerRequest.IsEmpty.ShouldBeFalse();
        }

        #endregion
    }
}