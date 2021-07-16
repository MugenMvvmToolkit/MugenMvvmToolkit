using System;
using MugenMvvm.Bindings.Delegates;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Enums;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Members
{
    public class DelegateAccessorMemberInfoTest : DelegateObservableMemberInfoTest
    {
        [Fact]
        public void CanReadShouldBeFalseNullGetter()
        {
            var memberInfo = new DelegateAccessorMemberInfo<string, string, object?>("", typeof(string), typeof(string), MemberFlags.Dynamic, null, null, null,
                (member, target, value, metadata) => { }, null, null);
            memberInfo.CanRead.ShouldBeFalse();
            memberInfo.CanWrite.ShouldBeTrue();
            ShouldThrow<InvalidOperationException>(() => memberInfo.GetValue(this, Metadata));
        }

        [Fact]
        public void CanWriteShouldBeFalseNullGetter()
        {
            var memberInfo = new DelegateAccessorMemberInfo<string, string, object?>("", typeof(string), typeof(string), MemberFlags.Dynamic, null, null,
                (member, target, metadata) => "", null, null, null);
            memberInfo.CanRead.ShouldBeTrue();
            memberInfo.CanWrite.ShouldBeFalse();
            ShouldThrow<InvalidOperationException>(() => memberInfo.SetValue(this, "", Metadata));
        }

        [Fact]
        public void GetValueShouldUseDelegate()
        {
            IMemberInfo? m = null;
            var invokeCount = 0;
            string t = "";
            var result = "test";
            var memberInfo = new DelegateAccessorMemberInfo<string, string, object?>("", typeof(string), typeof(string), MemberFlags.Dynamic, null, null,
                (member, target, metadata) =>
                {
                    ++invokeCount;
                    member.ShouldEqual(m);
                    target.ShouldEqual(t);
                    metadata.ShouldEqual(Metadata);
                    return result;
                }, null, null, null);
            m = memberInfo;
            memberInfo.GetValue(t, Metadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void SetValueShouldUseDelegate()
        {
            IMemberInfo? m = null;
            var invokeCount = 0;
            string t = "";
            var v = "test";
            var memberInfo = new DelegateAccessorMemberInfo<string, string, object?>("", typeof(string), typeof(string), MemberFlags.Dynamic, null, null, null,
                (member, target, value, metadata) =>
                {
                    ++invokeCount;
                    member.ShouldEqual(m);
                    target.ShouldEqual(t);
                    metadata.ShouldEqual(Metadata);
                    value.ShouldEqual(v);
                }, null, null);
            m = memberInfo;
            memberInfo.SetValue(t, v, Metadata);
            invokeCount.ShouldEqual(1);
        }

        protected override MemberType MemberType => MemberType.Accessor;

        protected override DelegateObservableMemberInfo<TTarget, TState> Create<TTarget, TState>(string name, Type declaringType, Type memberType,
            EnumFlags<MemberFlags> accessModifiers, object? underlyingMember,
            in TState state, TryObserveDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? tryObserve,
            RaiseDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? raise) =>
            new DelegateAccessorMemberInfo<TTarget, object, TState>(name, declaringType, memberType, accessModifiers, underlyingMember, state, (member, target, metadata) => "",
                (member, target, value, metadata) => { }, tryObserve, raise);
    }
}