using System;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Binding.Observation;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observation
{
    public class MemberPathMembersTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void DefaultShouldBeEmpty()
        {
            var member = default(MemberPathMembers);
            member.IsAvailable.ShouldBeFalse();
            member.ThrowIfError().ShouldBeFalse();
            member.Error.ShouldBeNull();
            member.Target.ShouldEqual(BindingMetadata.UnsetValue);
            member.Members.ShouldEqual(ConstantMemberInfo.UnsetArray);
        }

        [Fact]
        public void ConstructorShouldInitializeValues1()
        {
            var exception = new Exception();
            var member = new MemberPathMembers(exception);
            member.IsAvailable.ShouldBeFalse();
            member.Error.ShouldEqual(exception);
            member.Target.ShouldEqual(BindingMetadata.UnsetValue);
            member.Members.ShouldEqual(ConstantMemberInfo.UnsetArray);
            try
            {
                member.ThrowIfError();
                throw new NotSupportedException();
            }
            catch (Exception e)
            {
                e.ShouldEqual(member.Error);
            }
        }

        [Fact]
        public void ConstructorShouldInitializeValues2()
        {
            var target = new object();
            var memberInfos = new[]
            {
                new TestAccessorMemberInfo
                {
                    Type = typeof(object)
                }
            };
            var member = new MemberPathMembers(target, memberInfos);
            member.IsAvailable.ShouldBeTrue();
            member.Error.ShouldBeNull();
            member.Target.ShouldEqual(target);
            member.Members.ShouldEqual(memberInfos);
            member.ThrowIfError().ShouldBeTrue();
        }

        #endregion
    }
}