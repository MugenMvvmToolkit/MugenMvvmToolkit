using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Delegates;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Enums;
using MugenMvvm.UnitTests.Bindings.Members.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Members
{
    public class DelegateMethodMemberInfoTest : DelegateObservableMemberInfoTest
    {
        #region Properties

        protected override MemberType MemberType => MemberType.Method;

        #endregion

        #region Methods

        [Fact]
        public void ShouldNotBeGeneric()
        {
            var memberInfo = new DelegateMethodMemberInfo<string, object?, object?>("", typeof(object), typeof(object), MemberFlags.Dynamic, null, null, (member, target, args, metadata) => "", null, null, false, null,
                null);
            memberInfo.IsGenericMethod.ShouldBeFalse();
            memberInfo.IsGenericMethodDefinition.ShouldBeFalse();
            ShouldThrow<NotSupportedException>(() => memberInfo.GetGenericMethodDefinition());
            ShouldThrow<NotSupportedException>(() => memberInfo.MakeGenericMethod(new Type[0]));
            ShouldThrow<NotSupportedException>(() => memberInfo.GetGenericArguments());
        }

        [Fact]
        public void GetParametersShouldUseDelegate()
        {
            var memberInfo = new DelegateMethodMemberInfo<string, object?, object?>("", typeof(object), typeof(object), MemberFlags.Dynamic, null, null, (member, target, args, metadata) => "", null, null, false, null,
                null);
            memberInfo.GetParameters().IsEmpty.ShouldBeTrue();

            var invokeCount = 0;
            var parameters = new List<IParameterInfo> {null!};
            memberInfo = new DelegateMethodMemberInfo<string, object?, object?>("", typeof(object), typeof(object), MemberFlags.Dynamic, null, null, (member, target, args, metadata) => "", info =>
            {
                ++invokeCount;
                info.ShouldEqual(memberInfo);
                return parameters;
            }, null, false, null, null);
            memberInfo.GetParameters().ShouldEqual(parameters);
        }

        [Fact]
        public void TryGetAccessorShouldUseDelegate()
        {
            var flags = ArgumentFlags.Metadata;
            var values = new object[] {this};
            var memberInfo = new DelegateMethodMemberInfo<string, object?, object?>("", typeof(object), typeof(object), MemberFlags.Dynamic, null, null, (member, target, args, metadata) => "", null, null, false, null,
                null);
            memberInfo.TryGetAccessor(flags, values, DefaultMetadata).ShouldBeNull();

            var invokeCount = 0;
            var accessor = new TestAccessorMemberInfo();
            memberInfo = new DelegateMethodMemberInfo<string, object?, object?>("", typeof(object), typeof(object), MemberFlags.Dynamic, null, null, (member, target, args, metadata) => "", null,
                (member, argumentFlags, args, metadata) =>
                {
                    ++invokeCount;
                    member.ShouldEqual(memberInfo);
                    argumentFlags.ShouldEqual(flags);
                    args.ShouldEqual(values);
                    metadata.ShouldEqual(DefaultMetadata);
                    return accessor;
                }, false, null, null);
            memberInfo.TryGetAccessor(flags, values, DefaultMetadata).ShouldEqual(accessor);
        }

        [Fact]
        public void InvokeShouldUseDelegate()
        {
            IMemberInfo? m = null;
            var invokeCount = 0;
            string t = "";
            var objects = new object?[] {"t", 1};
            var result = "test";
            var memberInfo = new DelegateMethodMemberInfo<string, string, object?>("", typeof(string), typeof(string), MemberFlags.Dynamic, null, null, (member, target, args, metadata) =>
            {
                ++invokeCount;
                member.ShouldEqual(m);
                target.ShouldEqual(t);
                objects.ShouldEqual(args.AsList());
                metadata.ShouldEqual(DefaultMetadata);
                return result;
            }, null, null, false, null, null);
            m = memberInfo;
            memberInfo.Invoke(t, objects, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        protected override DelegateObservableMemberInfo<TTarget, TState> Create<TTarget, TState>(string name, Type declaringType, Type memberType, EnumFlags<MemberFlags> accessModifiers, object? underlyingMember,
            in TState state,
            bool tryObserveByMember, TryObserveDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? tryObserve, RaiseDelegate<DelegateObservableMemberInfo<TTarget, TState>, TTarget>? raise) =>
            new DelegateMethodMemberInfo<TTarget, object?, TState>(name, declaringType, memberType, accessModifiers, underlyingMember, state, (member, target, args, metadata) => "",
                null, null, tryObserveByMember, tryObserve, raise);

        #endregion
    }
}