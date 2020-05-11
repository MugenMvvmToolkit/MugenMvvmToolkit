using System;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Metadata;
using MugenMvvm.Binding.Observers;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observers
{
    public class MemberPathLastMemberTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void DefaultShouldBeEmpty()
        {
            var member = default(MemberPathLastMember);
            member.IsAvailable.ShouldBeFalse();
            member.Error.ShouldBeNull();
            member.Target.ShouldEqual(BindingMetadata.UnsetValue);
            member.Member.ShouldEqual(ConstantMemberInfo.Unset);
            member.ThrowIfError().ShouldBeFalse();
            member.GetValueOrThrow(DefaultMetadata).ShouldEqual(BindingMetadata.UnsetValue);
        }

        [Fact]
        public void ConstructorShouldInitializeValues1()
        {
            var exception = new Exception();
            var member = new MemberPathLastMember(exception);
            member.IsAvailable.ShouldBeFalse();
            member.Error.ShouldEqual(exception);
            member.Target.ShouldEqual(BindingMetadata.UnsetValue);
            member.Member.ShouldEqual(ConstantMemberInfo.Unset);
            try
            {
                member.ThrowIfError();
                throw new NotSupportedException();
            }
            catch (Exception e)
            {
                e.ShouldEqual(member.Error);
            }

            try
            {
                member.GetValueOrThrow(DefaultMetadata);
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
            var getCount = 0;
            var setCount = 0;
            var target = new object();
            var value = new object();
            var memberInfo = new TestMemberAccessorInfo
            {
                Type = typeof(object),
                GetValue = (o, context) =>
                {
                    ++getCount;
                    o.ShouldEqual(target);
                    context.ShouldEqual(DefaultMetadata);
                    return target;
                },
                SetValue = (t, v, meta) =>
                {
                    ++setCount;
                    t.ShouldEqual(target);
                    v.ShouldEqual(value);
                    meta.ShouldEqual(DefaultMetadata);
                }
            };
            var member = new MemberPathLastMember(target, memberInfo);
            member.IsAvailable.ShouldBeTrue();
            member.Error.ShouldBeNull();
            member.Target.ShouldEqual(target);
            member.Member.ShouldEqual(memberInfo);
            member.ThrowIfError().ShouldBeTrue();

            member.GetValue(DefaultMetadata).ShouldEqual(target);
            getCount.ShouldEqual(1);

            getCount = 0;
            member.GetValueOrThrow(DefaultMetadata).ShouldEqual(target);
            getCount.ShouldEqual(1);

            member.SetValueWithConvert(value, DefaultMetadata);
            setCount.ShouldEqual(1);
        }

        #endregion
    }
}