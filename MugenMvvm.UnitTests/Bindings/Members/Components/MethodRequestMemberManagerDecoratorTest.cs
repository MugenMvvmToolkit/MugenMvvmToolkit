using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Bindings.Members.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Members.Components
{
    public class MethodRequestMemberManagerDecoratorTest : UnitTestBase
    {
        private readonly MemberManager _memberManager;
        private readonly MethodRequestMemberManagerDecorator _decorator;

        public MethodRequestMemberManagerDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _memberManager = new MemberManager(ComponentCollectionManager);
            _decorator = new MethodRequestMemberManagerDecorator();
            _memberManager.AddComponent(_decorator);
        }

        [Fact]
        public void TryGetMembersShouldIgnoreNotSupportedRequest()
        {
            _decorator.TryGetMembers(_memberManager, typeof(object), MemberType.All, MemberFlags.All, "", DefaultMetadata).IsEmpty.ShouldBeTrue();
            _decorator.TryGetMembers(_memberManager, typeof(object), MemberType.All, MemberFlags.All, new MemberTypesRequest("", Default.Array<Type>()), DefaultMetadata).IsEmpty
                      .ShouldBeTrue();
            _decorator.TryGetMembers(_memberManager, typeof(object), MemberType.All, MemberFlags.All, this, DefaultMetadata).IsEmpty.ShouldBeTrue();
        }

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

            _memberManager.AddComponent(new TestMemberManagerComponent(_memberManager)
            {
                TryGetMembers = (t, m, f, r, meta) =>
                {
                    ++selectorCount;
                    ((IEnumerable<IMemberInfo>) r).ShouldEqual(members);
                    type.ShouldEqual(t);
                    memberType.ShouldEqual(m);
                    memberFlags.ShouldEqual(f);
                    meta.ShouldEqual(DefaultMetadata);
                    return members;
                }
            });
            _memberManager.AddComponent(new TestMemberProviderComponent(_memberManager)
            {
                TryGetMembers = (t, s, types, arg3) =>
                {
                    ++providerCount;
                    types.ShouldEqual(memberType);
                    type.ShouldEqual(t);
                    s.ShouldEqual(request.Name);
                    arg3.ShouldEqual(DefaultMetadata);
                    return members;
                }
            });

            var list = _memberManager.TryGetMembers(type, memberType, memberFlags, request, DefaultMetadata).AsList();
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
                    GetParameters = () => new[] {new TestParameterInfo {ParameterType = typeof(object)}}
                },
                new TestMethodMemberInfo
                {
                    GetParameters = () => new[]
                    {
                        new TestParameterInfo {ParameterType = typeof(object)},
                        new TestParameterInfo {ParameterType = typeof(string)}
                    }
                },
                new TestMethodMemberInfo
                {
                    GetParameters = () => new[]
                    {
                        new TestParameterInfo {ParameterType = typeof(object)},
                        new TestParameterInfo {ParameterType = typeof(string)},
                        new TestParameterInfo {ParameterType = typeof(int)}
                    }
                },
                new TestMethodMemberInfo
                {
                    GetParameters = () => new[]
                    {
                        new TestParameterInfo {ParameterType = typeof(object)},
                        new TestParameterInfo {ParameterType = typeof(string)},
                        new TestParameterInfo {ParameterType = typeof(int)}
                    }
                }
            };
            var additionMembers = new IMemberInfo[] {new TestEventInfo(), new TestAccessorMemberInfo()};
            var members = methods.Concat(additionMembers).ToArray();
            return new[]
            {
                new object[] {members, methods[0].GetParameters().AsList().Select(info => info.ParameterType).ToArray(), new[] {methods[0]}.Concat(additionMembers)},
                new object[] {members, methods[1].GetParameters().AsList().Select(info => info.ParameterType).ToArray(), new[] {methods[1]}.Concat(additionMembers)},
                new object[] {members, methods[2].GetParameters().AsList().Select(info => info.ParameterType).ToArray(), new[] {methods[2], methods[3]}.Concat(additionMembers)},
                new object[] {members, Default.Array<Type>(), additionMembers},
                new object[] {members, new[] {typeof(Guid)}, additionMembers}
            };
        }
    }
}