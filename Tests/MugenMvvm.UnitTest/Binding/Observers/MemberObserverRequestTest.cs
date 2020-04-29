using MugenMvvm.Binding.Observers;
using MugenMvvm.UnitTest.Binding.Members;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observers
{
    public class MemberObserverRequestTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void DefaultShouldBeEmpty()
        {
            default(MemberObserverRequest).IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            var path = "p";
            var memberInfo = GetType().GetMethod(nameof(ConstructorShouldInitializeValues));
            var args = Default.EmptyArray<object?>();
            var member = new TestMemberAccessorInfo();

            var request = new MemberObserverRequest(path, memberInfo, args, member);
            request.IsEmpty.ShouldBeFalse();
            request.Path.ShouldEqual(path);
            request.Arguments.ShouldEqual(args);
            request.MemberInfo.ShouldEqual(member);
            request.ReflectionMember.ShouldEqual(memberInfo);
        }

        #endregion
    }
}