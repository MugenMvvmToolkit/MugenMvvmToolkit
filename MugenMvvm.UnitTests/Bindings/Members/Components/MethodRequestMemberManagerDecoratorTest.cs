using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Tests.Bindings.Members;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Members.Components
{
    public class MethodRequestMemberManagerDecoratorTest : UnitTestBase
    {
        private readonly MethodRequestMemberManagerDecorator _decorator;

        public MethodRequestMemberManagerDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _decorator = new MethodRequestMemberManagerDecorator();
            MemberManager.AddComponent(_decorator);
        }

        [Fact]
        public void TryGetMembersShouldIgnoreNotSupportedRequest()
        {
            _decorator.TryGetMembers(MemberManager, typeof(object), MemberType.All, MemberFlags.All, "", DefaultMetadata).IsEmpty.ShouldBeTrue();
            _decorator.TryGetMembers(MemberManager, typeof(object), MemberType.All, MemberFlags.All, new MemberTypesRequest("", Array.Empty<Type>()), DefaultMetadata).IsEmpty
                      .ShouldBeTrue();
            _decorator.TryGetMembers(MemberManager, typeof(object), MemberType.All, MemberFlags.All, this, DefaultMetadata).IsEmpty.ShouldBeTrue();
        }

        protected override IMemberManager GetMemberManager() => new MemberManager(ComponentCollectionManager);

        [Theory]
        [MemberData(nameof(GetData))]
        public void TryGetMembersShouldSelectMethod(IMemberInfo[] members, Type[] memberTypes, IMemberInfo[] expectedResult)
        {
            var type = GetType();
            var memberType = MemberType.All;
            var memberFlags = MemberFlags.Instance.AsFlags();
            var request = new MemberTypesRequest("", memberTypes);
            var selectorCount = 0;
            var providerCount = 0;

            MemberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (mm, t, m, f, r, meta) =>
                {
                    ++selectorCount;
                    mm.ShouldEqual(MemberManager);
                    ((IEnumerable<IMemberInfo>)r).ShouldEqual(members);
                    type.ShouldEqual(t);
                    memberType.ShouldEqual(m);
                    memberFlags.ShouldEqual(f);
                    meta.ShouldEqual(DefaultMetadata);
                    return members;
                }
            });
            MemberManager.AddComponent(new TestMemberProviderComponent
            {
                TryGetMembers = (mm, t, s, types, arg3) =>
                {
                    ++providerCount;
                    mm.ShouldEqual(MemberManager);
                    types.ShouldEqual(memberType);
                    type.ShouldEqual(t);
                    s.ShouldEqual(request.Name);
                    arg3.ShouldEqual(DefaultMetadata);
                    return members;
                }
            });

            var list = MemberManager.TryGetMembers(type, memberType, memberFlags, request, DefaultMetadata);
            list.ShouldEqual(expectedResult);
            selectorCount.ShouldEqual(1);
            providerCount.ShouldEqual(1);
        }

        public static IEnumerable<object?[]> GetData()
        {
            var methods = new IMethodMemberInfo[]
            {
                new TestMethodMemberInfo
                {
                    GetParameters = () => new[] { new TestParameterInfo { ParameterType = typeof(object) } }
                },
                new TestMethodMemberInfo
                {
                    GetParameters = () => new[]
                    {
                        new TestParameterInfo { ParameterType = typeof(object) },
                        new TestParameterInfo { ParameterType = typeof(string) }
                    }
                },
                new TestMethodMemberInfo
                {
                    GetParameters = () => new[]
                    {
                        new TestParameterInfo { ParameterType = typeof(object) },
                        new TestParameterInfo { ParameterType = typeof(string) },
                        new TestParameterInfo { ParameterType = typeof(int) }
                    }
                },
                new TestMethodMemberInfo
                {
                    GetParameters = () => new[]
                    {
                        new TestParameterInfo { ParameterType = typeof(object) },
                        new TestParameterInfo { ParameterType = typeof(string) },
                        new TestParameterInfo { ParameterType = typeof(int) }
                    }
                }
            };
            var additionMembers = new IMemberInfo[] { new TestEventInfo(), new TestAccessorMemberInfo() };
            var members = methods.Concat(additionMembers).ToArray();
            return new[]
            {
                new object[] { members, methods[0].GetParameters().Select(info => info.ParameterType).ToArray(), new[] { methods[0] }.Concat(additionMembers) },
                new object[] { members, methods[1].GetParameters().Select(info => info.ParameterType).ToArray(), new[] { methods[1] }.Concat(additionMembers) },
                new object[]
                {
                    members, methods[2].GetParameters().Select(info => info.ParameterType).ToArray(), new[] { methods[2], methods[3] }.Concat(additionMembers)
                },
                new object[] { members, Array.Empty<Type>(), additionMembers },
                new object[] { members, new[] { typeof(Guid) }, additionMembers }
            };
        }
    }
}