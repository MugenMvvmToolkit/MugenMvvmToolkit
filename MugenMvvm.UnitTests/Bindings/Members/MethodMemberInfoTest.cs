﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MugenMvvm.Bindings.Attributes;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Bindings.Observation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Members
{
    public class MethodMemberInfoTest : UnitTestBase
    {
        [Theory]
        [InlineData(nameof(Method1), false)]
        [InlineData(nameof(NonObservable), true)]
        public void ConstructorShouldInitializeMember1(string method, bool nonObservable)
        {
            var reflectedType = typeof(string);
            string name = "Test";
            var methodInfo = typeof(MethodMemberInfoTest).GetMethod(method)!;
            MethodMemberInfo? memberInfo = null;

            var testEventListener = new TestWeakEventListener();
            var result = new ActionToken((o, o1) => { });
            var count = 0;
            var memberObserver = new MemberObserver((target, member, listener, meta) =>
            {
                ++count;
                target.ShouldEqual(this);
                listener.ShouldEqual(testEventListener);
                meta.ShouldEqual(DefaultMetadata);
                return result;
            }, this);

            var observerRequestCount = 0;
            using var t = MugenService.AddComponent(new TestMemberObserverProviderComponent
            {
                TryGetMemberObserver = (type, o, arg4) =>
                {
                    ++observerRequestCount;
                    o.ShouldEqual(memberInfo);
                    arg4.ShouldEqual(DefaultMetadata);
                    type.ShouldEqual(reflectedType);
                    return memberObserver;
                }
            });


            memberInfo = new MethodMemberInfo(name, methodInfo, false, reflectedType);
            memberInfo.Name.ShouldEqual(name);
            memberInfo.Type.ShouldEqual(methodInfo.ReturnType);
            memberInfo.DeclaringType.ShouldEqual(methodInfo.DeclaringType);
            memberInfo.UnderlyingMember.ShouldEqual(methodInfo);
            memberInfo.MemberType.ShouldEqual(MemberType.Method);
            memberInfo.MemberFlags.ShouldEqual((MemberFlags.Public | MemberFlags.Instance) | (nonObservable ? MemberFlags.NonObservable : default));
            memberInfo.IsGenericMethod.ShouldBeFalse();
            memberInfo.IsGenericMethodDefinition.ShouldBeFalse();

            var parameters = memberInfo.GetParameters();
            parameters.Count.ShouldEqual(1);

            var parameterInfo = parameters[0];
            parameterInfo.ParameterType.ShouldEqual(typeof(string));
            parameterInfo.HasDefaultValue.ShouldBeFalse();
            parameterInfo.UnderlyingParameter.ShouldEqual(methodInfo.GetParameters()[0]);
            parameterInfo.IsDefined(typeof(ObfuscationAttribute)).ShouldBeTrue();
            parameterInfo.IsDefined(typeof(InlineDataAttribute)).ShouldBeFalse();

            memberInfo.TryObserve(this, testEventListener, DefaultMetadata).ShouldEqual(result);
            count.ShouldEqual(1);
            observerRequestCount.ShouldEqual(1);

            memberInfo.Invoke(this, new object[] {name}, DefaultMetadata).ShouldEqual(name);
        }

        [Fact]
        public void ConstructorShouldInitializeMember2()
        {
            var methodInfo = typeof(MethodMemberInfoTest).GetMethod(nameof(Method2))!;

            var memberInfo = new MethodMemberInfo(methodInfo.Name, methodInfo, false, methodInfo.ReflectedType!, null, null);
            memberInfo.Name.ShouldEqual(methodInfo.Name);
            memberInfo.Type.ShouldEqual(methodInfo.ReturnType);
            memberInfo.DeclaringType.ShouldEqual(methodInfo.DeclaringType);
            memberInfo.UnderlyingMember.ShouldEqual(methodInfo);
            memberInfo.MemberType.ShouldEqual(MemberType.Method);
            memberInfo.MemberFlags.ShouldEqual(MemberFlags.Public | MemberFlags.Instance);
            memberInfo.IsGenericMethod.ShouldBeTrue();
            memberInfo.IsGenericMethodDefinition.ShouldBeTrue();

            memberInfo = (MethodMemberInfo) memberInfo.MakeGenericMethod(new[] {typeof(int)});
            memberInfo.Name.ShouldEqual(methodInfo.Name);
            memberInfo.Type.ShouldEqual(typeof(int));
            memberInfo.DeclaringType.ShouldEqual(methodInfo.DeclaringType);
            memberInfo.MemberType.ShouldEqual(MemberType.Method);
            memberInfo.MemberFlags.ShouldEqual(MemberFlags.Public | MemberFlags.Instance);
            memberInfo.IsGenericMethod.ShouldBeTrue();
            memberInfo.IsGenericMethodDefinition.ShouldBeFalse();

            var parameters = memberInfo.GetParameters();
            parameters.Count.ShouldEqual(1);

            memberInfo.Invoke(this, new object[] {int.MaxValue}, DefaultMetadata).ShouldEqual(int.MaxValue);
        }

        [Fact]
        public void ConstructorShouldInitializeMember3()
        {
            var methodInfo = typeof(Enumerable).GetMethods().FirstOrDefault(info => info.Name == nameof(Enumerable.FirstOrDefault) && info.GetParameters().Length == 1)!;

            var memberInfo = new MethodMemberInfo(methodInfo.Name, methodInfo, true, methodInfo.ReflectedType!, null, null);
            memberInfo.Name.ShouldEqual(methodInfo.Name);
            memberInfo.Type.ShouldEqual(methodInfo.ReturnType);
            memberInfo.DeclaringType.ShouldEqual(methodInfo.GetParameters()[0].ParameterType);
            memberInfo.UnderlyingMember.ShouldEqual(methodInfo);
            memberInfo.MemberType.ShouldEqual(MemberType.Method);
            memberInfo.MemberFlags.ShouldEqual(MemberFlags.Public | MemberFlags.Extension);
            memberInfo.IsGenericMethod.ShouldBeTrue();
            memberInfo.IsGenericMethodDefinition.ShouldBeTrue();

            memberInfo = (MethodMemberInfo) memberInfo.MakeGenericMethod(new[] {typeof(char)});
            memberInfo.Name.ShouldEqual(methodInfo.Name);
            memberInfo.Type.ShouldEqual(typeof(char));
            memberInfo.DeclaringType.ShouldEqual(typeof(IEnumerable<char>));
            memberInfo.MemberType.ShouldEqual(MemberType.Method);
            memberInfo.MemberFlags.ShouldEqual(MemberFlags.Public | MemberFlags.Extension);
            memberInfo.IsGenericMethod.ShouldBeTrue();
            memberInfo.IsGenericMethodDefinition.ShouldBeFalse();

            memberInfo.Invoke("st", Default.Array<object?>(), DefaultMetadata).ShouldEqual('s');
        }

        public string Method1([Obfuscation] string v) => v;

        [NonObservable]
        public string NonObservable([Obfuscation] string v) => v;

        public T Method2<T>(T v) => v;
    }
}