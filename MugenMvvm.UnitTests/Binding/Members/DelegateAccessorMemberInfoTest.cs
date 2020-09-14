using System;
using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Binding.Members
{
    public class DelegateAccessorMemberInfoTest : DelegateObservableMemberInfoTest
    {
        #region Properties

        protected override MemberType MemberType => MemberType.Accessor;

        #endregion

        #region Methods

        [Fact]
        public void CanReadShouldBeFalseNullGetter()
        {
            var memberInfo = new DelegateAccessorMemberInfo<string, string, object?>("", typeof(string), typeof(string), MemberFlags.Dynamic, null, null, null, (member, target, value, metadata) => { }, false, null,
                null);
            memberInfo.CanRead.ShouldBeFalse();
            memberInfo.CanWrite.ShouldBeTrue();
            ShouldThrow<InvalidOperationException>(() => memberInfo.GetValue(this, DefaultMetadata));
        }

        [Fact]
        public void CanWriteShouldBeFalseNullGetter()
        {
            var memberInfo = new DelegateAccessorMemberInfo<string, string, object?>("", typeof(string), typeof(string), MemberFlags.Dynamic, null, null, (member, target, metadata) => "", null, false, null, null);
            memberInfo.CanRead.ShouldBeTrue();
            memberInfo.CanWrite.ShouldBeFalse();
            ShouldThrow<InvalidOperationException>(() => memberInfo.SetValue(this, "", DefaultMetadata));
        }

        [Fact]
        public void GetValueShouldUseDelegate()
        {
            IMemberInfo? m = null;
            var invokeCount = 0;
            string t = "";
            var result = "test";
            var memberInfo = new DelegateAccessorMemberInfo<string, string, object?>("", typeof(string), typeof(string), MemberFlags.Dynamic, null, null, (member, target, metadata) =>
            {
                ++invokeCount;
                member.ShouldEqual(m);
                target.ShouldEqual(t);
                metadata.ShouldEqual(DefaultMetadata);
                return result;
            }, null, false, null, null);
            m = memberInfo;
            memberInfo.GetValue(t, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void SetValueShouldUseDelegate()
        {
            IMemberInfo? m = null;
            var invokeCount = 0;
            string t = "";
            var v = "test";
            var memberInfo = new DelegateAccessorMemberInfo<string, string, object?>("", typeof(string), typeof(string), MemberFlags.Dynamic, null, null, null, (member, target, value, metadata) =>
            {
                ++invokeCount;
                member.ShouldEqual(m);
                target.ShouldEqual(t);
                metadata.ShouldEqual(DefaultMetadata);
                value.ShouldEqual(v);
            }, false, null, null);
            m = memberInfo;
            memberInfo.SetValue(t, v, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }

        protected override DelegateObservableMemberInfo<TTarget, TState> Create<TTarget, TState>(string name, Type declaringType, Type memberType, MemberFlags accessModifiers, object? underlyingMember, in TState state,
            bool tryObserveByMember, TryObserveDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? tryObserve, RaiseDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? raise) =>
            new DelegateAccessorMemberInfo<TTarget, object, TState>(name, declaringType, memberType, accessModifiers, underlyingMember, state, (member, target, metadata) => "",
                (member, target, value, metadata) => { }, tryObserveByMember, tryObserve, raise);

        #endregion
    }
}