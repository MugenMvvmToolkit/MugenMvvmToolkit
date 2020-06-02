using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members.Components
{
    public class MemberSelectorTest : UnitTestBase
    {
        #region Methods

        [Theory]
        [MemberData(nameof(GetMemberFlagsData))]
        public void TrySelectMembersWithGetMemberFlagsData(IReadOnlyList<IMemberInfo> members, Type type, MemberType memberTypes, MemberFlags flags, IReadOnlyList<IMemberInfo> result)
        {
            TrySelectMembersShouldSelectCorrectMembers(members, type, memberTypes, flags, result);
        }

        [Theory]
        [MemberData(nameof(GetMemberTypesData))]
        public void TrySelectMembersWithGetMemberTypesData(IReadOnlyList<IMemberInfo> members, Type type, MemberType memberTypes, MemberFlags flags, IReadOnlyList<IMemberInfo> result)
        {
            TrySelectMembersShouldSelectCorrectMembers(members, type, memberTypes, flags, result);
        }

        [Theory]
        [MemberData(nameof(GetTypesData))]
        public void TrySelectMembersWithGetTypesData(IReadOnlyList<IMemberInfo> members, Type type, MemberType memberTypes, MemberFlags flags, IReadOnlyList<IMemberInfo> result)
        {
            TrySelectMembersShouldSelectCorrectMembers(members, type, memberTypes, flags, result);
        }

        [Theory]
        [MemberData(nameof(GetMemberFlagsExData))]
        public void TrySelectMembersWithGetMemberFlagsExData(IReadOnlyList<IMemberInfo> members, Type type, MemberType memberTypes, MemberFlags flags, IReadOnlyList<IMemberInfo> result)
        {
            TrySelectMembersShouldSelectCorrectMembers(members, type, memberTypes, flags, result);
        }

        [Theory]
        [MemberData(nameof(GetMethodsData))]
        public void TrySelectMembersWithGetMethodsData(IReadOnlyList<IMemberInfo> members, Type type, MemberType memberTypes, MemberFlags flags, IReadOnlyList<IMemberInfo> result)
        {
            TrySelectMembersShouldSelectCorrectMembers(members, type, memberTypes, flags, result);
        }

        [Theory]
        [MemberData(nameof(GetArgumentFlagsData))]
        public void TrySelectMembersWithGetArgumentFlagsData(IReadOnlyList<IMemberInfo> members, Type type, MemberType memberTypes, MemberFlags flags, IReadOnlyList<IMemberInfo> result)
        {
            TrySelectMembersShouldSelectCorrectMembers(members, type, memberTypes, flags, result);
        }

        private static void TrySelectMembersShouldSelectCorrectMembers(IReadOnlyList<IMemberInfo> members, Type type, MemberType memberTypes, MemberFlags flags, IReadOnlyList<IMemberInfo> result)
        {
            var component = new MemberSelector();
            var array = component.TryGetMembers(type, memberTypes, flags, members, DefaultMetadata).AsList();
            array.SequenceEqual(result).ShouldBeTrue();
        }

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
            list.Add(new object[] { members.ToArray(), typeof(object), MemberType.Accessor, MemberFlags.InstancePublic, Filter(members, MemberType.Accessor, MemberFlags.InstancePublic) });
            list.Add(new object[] { members.ToArray(), typeof(object), MemberType.Method, MemberFlags.InstancePublic, Filter(members, MemberType.Method, MemberFlags.InstancePublic) });
            list.Add(new object[] { members.ToArray(), typeof(object), MemberType.Event, MemberFlags.InstancePublic, Filter(members, MemberType.Event, MemberFlags.InstancePublic) });
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
            list.Add(new object[] { members.ToArray(), typeof(object), MemberType.Accessor, MemberFlags.InstanceNonPublic, Filter(members, MemberType.Accessor, MemberFlags.InstanceNonPublic) });
            list.Add(new object[] { members.ToArray(), typeof(object), MemberType.Method, MemberFlags.StaticPublic, Filter(members, MemberType.Method, MemberFlags.StaticPublic) });
            list.Add(new object[] { members.ToArray(), typeof(object), MemberType.Event, MemberFlags.StaticNonPublic, Filter(members, MemberType.Event, MemberFlags.StaticNonPublic) });
            return list;
        }

        public static IEnumerable<object?[]> GetTypesData()
        {
            var enumerableMember = GetAccessorInfo(typeof(IEnumerable), MemberType.Accessor, MemberFlags.InstancePublic);
            var enumerableGenericMember = GetAccessorInfo(typeof(IEnumerable<object>), MemberType.Accessor, MemberFlags.InstancePublic);
            var listMember = GetAccessorInfo(typeof(List<object>), MemberType.Accessor, MemberFlags.InstancePublic);
            var list = new List<object?[]>();
            var members = new List<IMemberInfo> { enumerableGenericMember, listMember, enumerableMember };

            //filter member flags
            list.Add(new object[] { members.ToArray(), typeof(IEnumerable), MemberType.Accessor, MemberFlags.InstancePublic, new[] { enumerableMember } });
            list.Add(new object[] { members.ToArray(), typeof(IEnumerable<object>), MemberType.Accessor, MemberFlags.InstancePublic, new[] { enumerableGenericMember } });
            list.Add(new object[] { members.ToArray(), typeof(List<object>), MemberType.Accessor, MemberFlags.InstancePublic, new[] { listMember } });
            list.Add(new object[] { members.ToArray(), typeof(TestList), MemberType.Accessor, MemberFlags.InstancePublic, new[] { listMember } });
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
            list.Add(new object[] { members.ToArray(), typeof(IEnumerable), MemberType.Accessor, MemberFlags.All, new[] { attached } });
            members.Remove(attached);
            list.Add(new object[] { members.ToArray(), typeof(IEnumerable<object>), MemberType.Accessor, MemberFlags.All, new[] { instance } });
            members.Remove(instance);
            list.Add(new object[] { members.ToArray(), typeof(List<object>), MemberType.Accessor, MemberFlags.All, new[] { ext } });
            members.Remove(ext);
            list.Add(new object[] { members.ToArray(), typeof(TestList), MemberType.Accessor, MemberFlags.All, new[] { dynamic } });
            return list;
        }

        public static IEnumerable<object?[]> GetMethodsData()
        {
            var list = new List<object?[]>();
            var parameters1 = new[]
            {
                new TestParameterInfo {ParameterType = typeof(object)},
                new TestParameterInfo {ParameterType = typeof(string)}
            };
            var parameters2 = new[]
            {
                new TestParameterInfo {ParameterType = typeof(object)}
            };
            var m1 = new TestMethodInfo
            {
                GetParameters = () => parameters1,
                DeclaringType = typeof(object),
                AccessModifiers = MemberFlags.InstancePublic,
                MemberType = MemberType.Method
            };
            var m2 = new TestMethodInfo
            {
                GetParameters = () => parameters2,
                DeclaringType = typeof(object),
                AccessModifiers = MemberFlags.InstancePublic,
                MemberType = MemberType.Method
            };

            list.Add(new object[] { new[] { m1, m2 }, typeof(object), MemberType.Method, MemberFlags.InstancePublic, new[] { m1, m2 } });
            return list;
        }

        public static IEnumerable<object?[]> GetArgumentFlagsData()
        {
            var getter = new TestMethodInfo { AccessModifiers = MemberFlags.InstancePublic, DeclaringType = typeof(object), Type = typeof(object) };
            var defaultMethod = new MethodMemberAccessorInfo("", getter, null, new object?[0], 0, typeof(object), null);
            var optionalMethod = new MethodMemberAccessorInfo("", getter, null, new object?[0], ArgumentFlags.Optional, typeof(object), null);
            var metadataMethod = new MethodMemberAccessorInfo("", getter, null, new object?[0], ArgumentFlags.Metadata, typeof(object), null);
            var paramMethod = new MethodMemberAccessorInfo("", getter, null, new object?[0], ArgumentFlags.ParamArray, typeof(object), null);
            var emptyParamMethod = new MethodMemberAccessorInfo("", getter, null, new object?[0], ArgumentFlags.EmptyParamArray, typeof(object), null);


            var list = new List<object?[]>();
            var members = new List<IMemberInfo> { defaultMethod, optionalMethod, metadataMethod, paramMethod, emptyParamMethod };

            //filter member flags
            list.Add(new object[] { members.ToArray(), typeof(IEnumerable), MemberType.Accessor, MemberFlags.All, new[] { defaultMethod } });
            members.Remove(defaultMethod);
            list.Add(new object[] { members.ToArray(), typeof(IEnumerable<object>), MemberType.Accessor, MemberFlags.All, new[] { metadataMethod } });
            members.Remove(metadataMethod);
            list.Add(new object[] { members.ToArray(), typeof(List<object>), MemberType.Accessor, MemberFlags.All, new[] { optionalMethod } });
            members.Remove(optionalMethod);
            list.Add(new object[] { members.ToArray(), typeof(TestList), MemberType.Accessor, MemberFlags.All, new[] { paramMethod } });
            members.Remove(paramMethod);
            list.Add(new object[] { members.ToArray(), typeof(TestList), MemberType.Accessor, MemberFlags.All, new[] { emptyParamMethod } });
            return list;
        }

        private static IMemberInfo[] Filter(IEnumerable<IMemberInfo> members, MemberType type, MemberFlags flags)
        {
            var memberInfo = members.SingleOrDefault(info => flags.HasFlagEx(info.AccessModifiers) && type.HasFlagEx(info.MemberType));
            if (memberInfo == null)
                return new IMemberInfo[0];
            return new[] { memberInfo };
        }

        private static TestMemberAccessorInfo GetAccessorInfo(Type declaringType, MemberType type, MemberFlags flags)
        {
            return new TestMemberAccessorInfo
            {
                DeclaringType = declaringType,
                AccessModifiers = flags,
                MemberType = type
            };
        }

        #endregion

        #region Nested types

        private sealed class TestList : List<object>
        {
        }

        #endregion
    }
}