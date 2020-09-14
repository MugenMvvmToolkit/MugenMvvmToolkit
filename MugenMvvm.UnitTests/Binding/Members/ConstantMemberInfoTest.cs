using System;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Members;
using MugenMvvm.UnitTests.Binding.Observation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Binding.Members
{
    public class ConstantMemberInfoTest : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData("test1", false, false)]
        [InlineData("test2", true, true)]
        public void ConstructorShouldInitializeMember(string name, object? result, bool canWrite)
        {
            var memberInfo = new ConstantMemberInfo(name, result, canWrite);
            memberInfo.Type.ShouldEqual(typeof(object));
            memberInfo.DeclaringType.ShouldEqual(typeof(object));
            memberInfo.UnderlyingMember.ShouldBeNull();
            memberInfo.MemberType.ShouldEqual(MemberType.Accessor);
            memberInfo.AccessModifiers.ShouldEqual(MemberFlags.Public | MemberFlags.Dynamic);
            memberInfo.CanRead.ShouldBeTrue();
            memberInfo.CanWrite.ShouldEqual(canWrite);

            memberInfo.TryObserve(this, new TestWeakEventListener(), DefaultMetadata).IsEmpty.ShouldBeTrue();
            memberInfo.GetValue(this, DefaultMetadata).ShouldEqual(result);
            if (canWrite)
                memberInfo.SetValue(this, result, DefaultMetadata);
            else
                ShouldThrow<InvalidOperationException>(() => memberInfo.SetValue(this, result, DefaultMetadata));
        }

        #endregion
    }
}