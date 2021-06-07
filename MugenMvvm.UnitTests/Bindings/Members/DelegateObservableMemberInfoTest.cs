﻿using System;
using MugenMvvm.Bindings.Delegates;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.Tests.Bindings.Observation;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Members
{
    [Collection(SharedContext)]
    public class DelegateObservableMemberInfoTest : UnitTestBase
    {
        public DelegateObservableMemberInfoTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            RegisterDisposeToken(WithGlobalService(ObservationManager));
            RegisterDisposeToken(WithGlobalService(MemberManager));
        }

        [Fact]
        public void ConstructorShouldInitializeValues()
        {
            string name = "n";
            Type declaringType = typeof(int);
            Type memberType = typeof(object);
            var accessModifiers = MemberFlags.Dynamic;
            object underlyingMember = new();
            var state = (this, "");
            TryObserveDelegate<DelegateObservableMemberInfo<string, (DelegateObservableMemberInfoTest, string)>, string> tryObserve = (member, target, listener, metadata) =>
                default;
            RaiseDelegate<DelegateObservableMemberInfo<string, (DelegateObservableMemberInfoTest, string)>, string> raise = (member, target, message, metadata) => { };
            var memberInfo = Create(name, declaringType, memberType, accessModifiers, underlyingMember, state, tryObserve, raise);
            memberInfo.Type.ShouldEqual(memberType);
            memberInfo.Name.ShouldEqual(name);
            memberInfo.MemberType.ShouldEqual(MemberType);
            memberInfo.MemberFlags.ShouldEqual(accessModifiers);
            memberInfo.UnderlyingMember.ShouldEqual(underlyingMember);
            memberInfo.DeclaringType.ShouldEqual(declaringType);
            memberInfo.State.ShouldEqual(state);
        }

        [Fact]
        public void RaiseShouldUseDelegate()
        {
            IMemberInfo? m = null;
            var invokeCount = 0;
            string t = "";
            var msg = new object();
            var memberInfo = Create<string, object?>("", typeof(string), typeof(string), MemberFlags.Dynamic, null, null, (member, target, listener, metadata) => default,
                (member, target, message, metadata) =>
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

        [Fact]
        public void TryObserverShouldUseDelegate()
        {
            IMemberInfo? m = null;
            var invokeCount = 0;
            string t = "";
            var l = new TestWeakEventListener();
            var result = ActionToken.FromDelegate((o, o1) => { });
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
        public void TryObserverShouldUseObservationManager()
        {
            var invokeCount = 0;
            var actionToken = ActionToken.FromDelegate((o, o1) => { });
            var memberInfo = Create<string, object?>("n", typeof(object), typeof(string), MemberFlags.All, null, null, null, null);
            ObservationManager.AddComponent(new TestMemberObserverProviderComponent
            {
                TryGetMemberObserver = (_, type, o, arg3) =>
                {
                    ++invokeCount;
                    type.ShouldEqual(memberInfo.DeclaringType);
                    o.ShouldEqual(memberInfo);
                    arg3.ShouldEqual(DefaultMetadata);
                    return new MemberObserver((o1, o2, listener, arg4) => actionToken, memberInfo);
                }
            });

            var token = memberInfo.TryObserve(this, new TestWeakEventListener(), DefaultMetadata);
            invokeCount.ShouldEqual(1);
            token.ShouldEqual(actionToken);
        }

        protected override IObservationManager GetObservationManager() => new ObservationManager(ComponentCollectionManager);

        protected override IMemberManager GetMemberManager() => new MemberManager(ComponentCollectionManager);

        protected virtual MemberType MemberType => MemberType.Event;

        protected virtual DelegateObservableMemberInfo<TTarget, TState> Create<TTarget, TState>(string name, Type declaringType, Type memberType,
            EnumFlags<MemberFlags> accessModifiers, object? underlyingMember,
            in TState state, TryObserveDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? tryObserve,
            RaiseDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? raise)
            where TTarget : class? => new(name, declaringType, memberType, accessModifiers, underlyingMember, state, tryObserve, raise);
    }
}