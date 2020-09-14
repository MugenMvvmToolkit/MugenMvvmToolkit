using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Binding.Members.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Binding.Members.Components
{
    public class MethodRequestMemberManagerDecoratorTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMembersShouldIgnoreNotSupportedRequest()
        {
            var manager = new MemberManager();
            var component = new MethodRequestMemberManagerDecorator();
            manager.AddComponent(component);
            component.TryGetMembers(manager, typeof(object), MemberType.All, MemberFlags.All, "", DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            component.TryGetMembers(manager, typeof(object), MemberType.All, MemberFlags.All, new MemberTypesRequest("", Default.Array<Type>()), DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            component.TryGetMembers(manager, typeof(object), MemberType.All, MemberFlags.All, this, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
        }

        [Theory]
        [MemberData(nameof(GetData))]
        public void TryGetMembersShouldSelectMethod(IMemberInfo[] members, Type[] memberTypes, IMemberInfo[] expectedResult)
        {
            var type = GetType();
            var memberType = MemberType.All;
            var memberFlags = MemberFlags.Instance;
            var request = new MemberTypesRequest("", memberTypes);
            var selectorCount = 0;
            var providerCount = 0;

            var manager = new MemberManager();
            var selector = new TestMemberManagerComponent(manager)
            {
                TryGetMembers = (t, m, f, r, meta) =>
                {
                    ++selectorCount;
                    ((IEnumerable<IMemberInfo>) r).SequenceEqual(members).ShouldBeTrue();
                    type.ShouldEqual(t);
                    memberType.ShouldEqual(m);
                    memberFlags.ShouldEqual(f);
                    meta.ShouldEqual(DefaultMetadata);
                    return members;
                }
            };
            var provider = new TestMemberProviderComponent(manager)
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
            };
            var component = new MethodRequestMemberManagerDecorator();
            manager.AddComponent(selector);
            manager.AddComponent(provider);
            manager.AddComponent(component);

            var list = manager.TryGetMembers(type, memberType, memberFlags, request, DefaultMetadata).AsList();
            list.SequenceEqual(expectedResult).ShouldBeTrue();
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
                new object[] {members, methods[0].GetParameters().Select(info => info.ParameterType).ToArray(), new[] {methods[0]}.Concat(additionMembers)},
                new object[] {members, methods[1].GetParameters().Select(info => info.ParameterType).ToArray(), new[] {methods[1]}.Concat(additionMembers)},
                new object[] {members, methods[2].GetParameters().Select(info => info.ParameterType).ToArray(), new[] {methods[2], methods[3]}.Concat(additionMembers)},
                new object[] {members, Default.Array<Type>(), additionMembers},
                new object[] {members, new[] {typeof(Guid)}, additionMembers}
            };
        }

        #endregion
    }
}