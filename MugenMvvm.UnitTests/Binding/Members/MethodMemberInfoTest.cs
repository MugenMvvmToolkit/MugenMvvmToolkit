using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Binding.Observation.Internal;
using MugenMvvm.UnitTests.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Binding.Members
{
    public class MethodMemberInfoTest : UnitTestBase
    {
        #region Methods

        public string Method1([Obfuscation] string v) => v;

        public T Method2<T>(T v) => v;

        [Fact]
        public void ConstructorShouldInitializeMember1()
        {
            var reflectedType = typeof(string);
            string name = "Test";
            var methodInfo = typeof(MethodMemberInfoTest).GetMethod(nameof(Method1))!;
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
            using var subscribe = TestComponentSubscriber.Subscribe(new TestMemberObserverProviderComponent
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
            memberInfo.AccessModifiers.ShouldEqual(MemberFlags.Public | MemberFlags.Instance);
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
            memberInfo.AccessModifiers.ShouldEqual(MemberFlags.Public | MemberFlags.Instance);
            memberInfo.IsGenericMethod.ShouldBeTrue();
            memberInfo.IsGenericMethodDefinition.ShouldBeTrue();

            memberInfo = (MethodMemberInfo) memberInfo.MakeGenericMethod(new[] {typeof(int)});
            memberInfo.Name.ShouldEqual(methodInfo.Name);
            memberInfo.Type.ShouldEqual(typeof(int));
            memberInfo.DeclaringType.ShouldEqual(methodInfo.DeclaringType);
            memberInfo.MemberType.ShouldEqual(MemberType.Method);
            memberInfo.AccessModifiers.ShouldEqual(MemberFlags.Public | MemberFlags.Instance);
            memberInfo.IsGenericMethod.ShouldBeTrue();
            memberInfo.IsGenericMethodDefinition.ShouldBeFalse();

            var parameters = memberInfo.GetParameters();
            parameters.Count.ShouldEqual(1);

            memberInfo.Invoke(this, new object[] {int.MaxValue}, DefaultMetadata).ShouldEqual(int.MaxValue);
        }

        [Fact]
        public void ConstructorShouldInitializeMember3()
        {
            var methodInfo = typeof(Enumerable).GetMethods().FirstOrDefault(info => info.Name == nameof(Enumerable.FirstOrDefault) && info.GetParameters().Length == 1);

            var memberInfo = new MethodMemberInfo(methodInfo.Name, methodInfo, true, methodInfo.ReflectedType!, null, null);
            memberInfo.Name.ShouldEqual(methodInfo.Name);
            memberInfo.Type.ShouldEqual(methodInfo.ReturnType);
            memberInfo.DeclaringType.ShouldEqual(methodInfo.GetParameters()[0].ParameterType);
            memberInfo.UnderlyingMember.ShouldEqual(methodInfo);
            memberInfo.MemberType.ShouldEqual(MemberType.Method);
            memberInfo.AccessModifiers.ShouldEqual(MemberFlags.Public | MemberFlags.Extension);
            memberInfo.IsGenericMethod.ShouldBeTrue();
            memberInfo.IsGenericMethodDefinition.ShouldBeTrue();

            memberInfo = (MethodMemberInfo) memberInfo.MakeGenericMethod(new[] {typeof(char)});
            memberInfo.Name.ShouldEqual(methodInfo.Name);
            memberInfo.Type.ShouldEqual(typeof(char));
            memberInfo.DeclaringType.ShouldEqual(typeof(IEnumerable<char>));
            memberInfo.MemberType.ShouldEqual(MemberType.Method);
            memberInfo.AccessModifiers.ShouldEqual(MemberFlags.Public | MemberFlags.Extension);
            memberInfo.IsGenericMethod.ShouldBeTrue();
            memberInfo.IsGenericMethodDefinition.ShouldBeFalse();

            memberInfo.Invoke("st", Default.Array<object?>(), DefaultMetadata).ShouldEqual('s');
        }

        #endregion
    }
}