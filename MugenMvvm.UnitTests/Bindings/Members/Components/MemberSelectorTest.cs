using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Metadata;
using MugenMvvm.Tests.Bindings.Members;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Members.Components
{
    public class MemberSelectorTest : UnitTestBase
    {
        [Theory]
        [MemberData(nameof(GetArgumentFlagsData))]
        public void TrySelectMembersWithGetArgumentFlagsData(Type type, IReadOnlyList<IMemberInfo> members, MemberType memberTypes, EnumFlags<MemberFlags> flags,
            IReadOnlyList<IMemberInfo> result) =>
            TrySelectMembersShouldSelectCorrectMembers(members, type, memberTypes, flags, result);

        [Theory]
        [MemberData(nameof(GetMemberFlagsData))]
        public void TrySelectMembersWithGetMemberFlagsData(Type type, IReadOnlyList<IMemberInfo> members, MemberType memberTypes, EnumFlags<MemberFlags> flags,
            IReadOnlyList<IMemberInfo> result) =>
            TrySelectMembersShouldSelectCorrectMembers(members, type, memberTypes, flags, result);

        [Theory]
        [MemberData(nameof(GetMemberFlagsExData))]
        public void TrySelectMembersWithGetMemberFlagsExData(Type type, IReadOnlyList<IMemberInfo> members, MemberType memberTypes, EnumFlags<MemberFlags> flags,
            IReadOnlyList<IMemberInfo> result) =>
            TrySelectMembersShouldSelectCorrectMembers(members, type, memberTypes, flags, result);

        [Theory]
        [MemberData(nameof(GetMemberTypesData))]
        public void TrySelectMembersWithGetMemberTypesData(Type type, IReadOnlyList<IMemberInfo> members, MemberType memberTypes, EnumFlags<MemberFlags> flags,
            IReadOnlyList<IMemberInfo> result) =>
            TrySelectMembersShouldSelectCorrectMembers(members, type, memberTypes, flags, result);

        [Theory]
        [MemberData(nameof(GetMethodsData))]
        public void TrySelectMembersWithGetMethodsData(Type type, IReadOnlyList<IMemberInfo> members, MemberType memberTypes, EnumFlags<MemberFlags> flags,
            IReadOnlyList<IMemberInfo> result) =>
            TrySelectMembersShouldSelectCorrectMembers(members, type, memberTypes, flags, result);

        [Theory]
        [MemberData(nameof(GetTypesData))]
        public void TrySelectMembersWithGetTypesData(Type type, IReadOnlyList<IMemberInfo> members, MemberType memberTypes, EnumFlags<MemberFlags> flags,
            IReadOnlyList<IMemberInfo> result) =>
            TrySelectMembersShouldSelectCorrectMembers(members, type, memberTypes, flags, result);

        public static IEnumerable<object?[]> GetMemberTypesData()
        {
            var list = new List<object?[]>();
            var members = new List<IMemberInfo>
            {
                GetAccessorInfo(typeof(object), MemberType.Accessor, MemberFlags.InstancePublic),
                GetAccessorInfo(typeof(object), MemberType.Accessor, MemberFlags.InstanceNonPublic),
                GetAccessorInfo(typeof(object), MemberType.Accessor, MemberFlags.StaticPublic),
                GetAccessorInfo(typeof(object), MemberType.Accessor, MemberFlags.StaticNonPublic),

                GetAccessorInfo(typeof(object), MemberType.Method, MemberFlags.InstancePublic),
                GetAccessorInfo(typeof(object), MemberType.Event, MemberFlags.InstanceNonPublic),
                GetAccessorInfo(typeof(object), MemberType.Method, MemberFlags.StaticPublic),
                GetAccessorInfo(typeof(object), MemberType.Event, MemberFlags.StaticNonPublic)
            };

            //filter member types
            list.Add(new object[]
                { typeof(object), members.ToArray(), MemberType.Accessor, MemberFlags.InstancePublic, Filter(members, MemberType.Accessor, MemberFlags.InstancePublic) });
            list.Add(new object[]
                { typeof(object), members.ToArray(), MemberType.Method, MemberFlags.InstancePublic, Filter(members, MemberType.Method, MemberFlags.InstancePublic) });
            list.Add(new object[]
                { typeof(object), members.ToArray(), MemberType.Event, MemberFlags.InstancePublic, Filter(members, MemberType.Event, MemberFlags.InstancePublic) });
            return list;
        }

        public static IEnumerable<object?[]> GetMemberFlagsData()
        {
            var list = new List<object?[]>();
            var members = new List<IMemberInfo>
            {
                GetAccessorInfo(typeof(object), MemberType.Accessor, MemberFlags.InstancePublic),
                GetAccessorInfo(typeof(object), MemberType.Accessor, MemberFlags.InstanceNonPublic),
                GetAccessorInfo(typeof(object), MemberType.Accessor, MemberFlags.StaticPublic),
                GetAccessorInfo(typeof(object), MemberType.Accessor, MemberFlags.StaticNonPublic),

                GetAccessorInfo(typeof(object), MemberType.Method, MemberFlags.InstancePublic),
                GetAccessorInfo(typeof(object), MemberType.Event, MemberFlags.InstanceNonPublic),
                GetAccessorInfo(typeof(object), MemberType.Method, MemberFlags.StaticPublic),
                GetAccessorInfo(typeof(object), MemberType.Event, MemberFlags.StaticNonPublic)
            };

            //filter member flags
            list.Add(new object[]
                { typeof(object), members.ToArray(), MemberType.Accessor, MemberFlags.InstanceNonPublic, Filter(members, MemberType.Accessor, MemberFlags.InstanceNonPublic) });
            list.Add(new object[] { typeof(object), members.ToArray(), MemberType.Method, MemberFlags.StaticPublic, Filter(members, MemberType.Method, MemberFlags.StaticPublic) });
            list.Add(new object[]
                { typeof(object), members.ToArray(), MemberType.Event, MemberFlags.StaticNonPublic, Filter(members, MemberType.Event, MemberFlags.StaticNonPublic) });
            return list;
        }

        public static IEnumerable<object?[]> GetTypesData()
        {
            var enumerableMember = GetAccessorInfo(typeof(IEnumerable), MemberType.Accessor, MemberFlags.InstancePublic);
            var enumerableGenericMember = GetAccessorInfo(typeof(IEnumerable<object>), MemberType.Accessor, MemberFlags.InstancePublic);
            var listMember = GetAccessorInfo(typeof(List<object>), MemberType.Accessor, MemberFlags.InstancePublic);
            var objectMember = GetAccessorInfo(typeof(object), MemberType.Accessor, MemberFlags.InstancePublic);

            var list = new List<object?[]>();
            var members = new List<IMemberInfo> { listMember, enumerableMember, objectMember, enumerableGenericMember };

            //filter member flags
            list.Add(new object[] { typeof(IEnumerable), members.ToArray(), MemberType.Accessor, MemberFlags.InstancePublic, new[] { enumerableMember } });
            list.Add(new object[] { typeof(IEnumerable<object>), members.ToArray(), MemberType.Accessor, MemberFlags.InstancePublic, new[] { enumerableGenericMember } });
            list.Add(new object[] { typeof(List<object>), members.ToArray(), MemberType.Accessor, MemberFlags.InstancePublic, new[] { listMember } });
            list.Add(new object[] { typeof(TestList), members.ToArray(), MemberType.Accessor, MemberFlags.InstancePublic, new[] { listMember } });
            members.Remove(listMember);
            list.Add(new object[] { typeof(TestList), members.ToArray(), MemberType.Accessor, MemberFlags.InstancePublic, new[] { enumerableGenericMember } });
            list.Add(new object[] { typeof(IList<object>), members.ToArray(), MemberType.Accessor, MemberFlags.InstancePublic, new[] { enumerableGenericMember } });
            members.Remove(enumerableGenericMember);
            list.Add(new object[] { typeof(TestList), members.ToArray(), MemberType.Accessor, MemberFlags.InstancePublic, new[] { enumerableMember } });
            list.Add(new object[] { typeof(IList<object>), members.ToArray(), MemberType.Accessor, MemberFlags.InstancePublic, new[] { enumerableMember } });
            return list;
        }

        public static IEnumerable<object?[]> GetMemberFlagsExData()
        {
            var attached = GetAccessorInfo(typeof(object), MemberType.Accessor, MemberFlags.Public | MemberFlags.Attached);
            var instance = GetAccessorInfo(typeof(object), MemberType.Accessor, MemberFlags.InstancePublic);
            var ext = GetAccessorInfo(typeof(object), MemberType.Accessor, MemberFlags.Public | MemberFlags.Extension);
            var dynamic = GetAccessorInfo(typeof(object), MemberType.Accessor, MemberFlags.Public | MemberFlags.Dynamic);

            var list = new List<object?[]>();
            var members = new List<IMemberInfo> { attached, ext, dynamic, instance };

            //filter member flags
            list.Add(new object[] { typeof(IEnumerable), members.ToArray(), MemberType.Accessor, MemberFlags.All, new[] { attached } });
            members.Remove(attached);
            list.Add(new object[] { typeof(IEnumerable<object>), members.ToArray(), MemberType.Accessor, MemberFlags.All, new[] { instance } });
            members.Remove(instance);
            list.Add(new object[] { typeof(List<object>), members.ToArray(), MemberType.Accessor, MemberFlags.All, new[] { ext } });
            members.Remove(ext);
            list.Add(new object[] { typeof(TestList), members.ToArray(), MemberType.Accessor, MemberFlags.All, new[] { dynamic } });
            return list;
        }

        public static IEnumerable<object?[]> GetMethodsData()
        {
            var list = new List<object?[]>();
            var parameters1 = new[]
            {
                new TestParameterInfo { ParameterType = typeof(object) },
                new TestParameterInfo { ParameterType = typeof(string) }
            };
            var parameters2 = new[]
            {
                new TestParameterInfo { ParameterType = typeof(object) }
            };
            var m1 = new TestMethodMemberInfo
            {
                GetParameters = () => parameters1,
                DeclaringType = typeof(object),
                MemberFlags = MemberFlags.InstancePublic,
                MemberType = MemberType.Method
            };
            var m2 = new TestMethodMemberInfo
            {
                GetParameters = () => parameters2,
                DeclaringType = typeof(object),
                MemberFlags = MemberFlags.InstancePublic,
                MemberType = MemberType.Method
            };

            list.Add(new object[] { typeof(object), new[] { m1, m2 }, MemberType.Method, MemberFlags.InstancePublic, new[] { m1, m2 } });
            return list;
        }

        public static IEnumerable<object?[]> GetArgumentFlagsData()
        {
            var getter = new TestMethodMemberInfo { MemberFlags = MemberFlags.InstancePublic, DeclaringType = typeof(object), Type = typeof(object) };
            var defaultMethod = new MethodAccessorMemberInfo("", getter, null, new object?[0], default, typeof(object));
            var optionalMethod = new MethodAccessorMemberInfo("", getter, null, new object?[0], ArgumentFlags.Optional, typeof(object));
            var metadataMethod = new MethodAccessorMemberInfo("", getter, null, new object?[0], ArgumentFlags.Metadata, typeof(object));
            var paramMethod = new MethodAccessorMemberInfo("", getter, null, new object?[0], ArgumentFlags.ParamArray, typeof(object));
            var emptyParamMethod = new MethodAccessorMemberInfo("", getter, null, new object?[0], ArgumentFlags.EmptyParamArray, typeof(object));


            var list = new List<object?[]>();
            var members = new List<IMemberInfo> { defaultMethod, optionalMethod, metadataMethod, paramMethod, emptyParamMethod };

            //filter member flags
            list.Add(new object[] { typeof(IEnumerable), members.ToArray(), MemberType.Accessor, MemberFlags.All, new[] { defaultMethod } });
            members.Remove(defaultMethod);
            list.Add(new object[] { typeof(IEnumerable<object>), members.ToArray(), MemberType.Accessor, MemberFlags.All, new[] { metadataMethod } });
            members.Remove(metadataMethod);
            list.Add(new object[] { typeof(List<object>), members.ToArray(), MemberType.Accessor, MemberFlags.All, new[] { optionalMethod } });
            members.Remove(optionalMethod);
            list.Add(new object[] { typeof(TestList), members.ToArray(), MemberType.Accessor, MemberFlags.All, new[] { paramMethod } });
            members.Remove(paramMethod);
            list.Add(new object[] { typeof(TestList), members.ToArray(), MemberType.Accessor, MemberFlags.All, new[] { emptyParamMethod } });
            return list;
        }

        private static void TrySelectMembersShouldSelectCorrectMembers(IReadOnlyList<IMemberInfo> members, Type type, MemberType memberTypes, EnumFlags<MemberFlags> flags,
            IReadOnlyList<IMemberInfo> result)
        {
            var component = new MemberSelector();
            var array = component.TryGetMembers(null!, type, memberTypes, flags, members, EmptyMetadataContext.Instance);
            for (var i = 0; i < array.Count; i++)
                array[i].ShouldEqual(result[i]);
            array.Count.ShouldEqual(result.Count);
        }

        private static IMemberInfo[] Filter(IEnumerable<IMemberInfo> members, EnumFlags<MemberType> type, EnumFlags<MemberFlags> flags)
        {
            var memberInfo = members.SingleOrDefault(info => flags.HasFlag(info.MemberFlags) && type.HasFlag(info.MemberType));
            if (memberInfo == null)
                return new IMemberInfo[0];
            return new[] { memberInfo };
        }

        private static TestAccessorMemberInfo GetAccessorInfo(Type declaringType, MemberType type, EnumFlags<MemberFlags> flags) =>
            new()
            {
                DeclaringType = declaringType,
                MemberFlags = flags,
                MemberType = type
            };

        private sealed class TestList : List<object>
        {
        }
    }
}