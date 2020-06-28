using System;
using MugenMvvm.Binding.Delegates;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members;
using MugenMvvm.Internal;
using MugenMvvm.UnitTest.Binding.Observation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members
{
    public class DelegateObservableMemberInfoTest : UnitTestBase
    {
        #region Properties

        protected virtual MemberType MemberType => MemberType.Event;

        #endregion

        #region Methods

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            string name = "n";
            Type declaringType = typeof(int);
            Type memberType = typeof(object);
            var accessModifiers = MemberFlags.Dynamic;
            object underlyingMember = new object();
            var state = (this, "");
            TryObserveDelegate<DelegateObservableMemberInfo<string, (DelegateObservableMemberInfoTest, string)>, string> tryObserve = (member, target, listener, metadata) => default;
            RaiseDelegate<DelegateObservableMemberInfo<string, (DelegateObservableMemberInfoTest, string)>, string> raise = (member, target, message, metadata) => { };
            var memberInfo = Create(name, declaringType, memberType, accessModifiers, underlyingMember, state, tryObserve, raise);
            memberInfo.Type.ShouldEqual(memberType);
            memberInfo.Name.ShouldEqual(name);
            memberInfo.MemberType.ShouldEqual(MemberType);
            memberInfo.AccessModifiers.ShouldEqual(accessModifiers);
            memberInfo.UnderlyingMember.ShouldEqual(underlyingMember);
            memberInfo.DeclaringType.ShouldEqual(declaringType);
            memberInfo.State.ShouldEqual(state);
        }

        [Fact]
        public void TryObserverShouldUseDelegate()
        {
            IMemberInfo? m = null;
            var invokeCount = 0;
            string t = "";
            var l = new TestWeakEventListener();
            var result = new ActionToken((o, o1) => { });
            var memberInfo = Create<string, object?>("", typeof(string), typeof(string), MemberFlags.Dynamic, null, null, (member, target, listener, metadata) =>
            {
                ++invokeCount;
                member.ShouldEqual(m);
                target.ShouldEqual(t);
                listener.ShouldEqual(l);
                metadata.ShouldEqual(DefaultMetadata);
                return result;
            }, null);
            m = memberInfo;
            memberInfo.TryObserve(t, l, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }


        [Fact]
        public void RaiseShouldUseDelegate()
        {
            IMemberInfo? m = null;
            var invokeCount = 0;
            string t = "";
            var msg = new object();
            var memberInfo = Create<string, object?>("", typeof(string), typeof(string), MemberFlags.Dynamic, null, null, (member, target, listener, metadata) => default, (member, target, message, metadata) =>
            {
                ++invokeCount;
                member.ShouldEqual(m);
                target.ShouldEqual(t);
                message.ShouldEqual(msg);
                metadata.ShouldEqual(DefaultMetadata);
            });
            m = memberInfo;
            memberInfo.Raise(t, msg, DefaultMetadata);
            invokeCount.ShouldEqual(1);
        }

        protected virtual DelegateObservableMemberInfo<TTarget, TState> Create<TTarget, TState>(string name, Type declaringType, Type memberType, MemberFlags accessModifiers, object? underlyingMember, in TState state,
            TryObserveDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? tryObserve, RaiseDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? raise) where TTarget : class?
        {
            return new DelegateObservableMemberInfo<TTarget, TState>(name, declaringType, memberType, accessModifiers, underlyingMember, state, tryObserve, raise);
        }

        #endregion
    }
}