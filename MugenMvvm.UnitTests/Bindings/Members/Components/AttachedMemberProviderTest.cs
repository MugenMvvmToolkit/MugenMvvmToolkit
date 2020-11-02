using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Components;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Bindings.Members.Internal;
using MugenMvvm.UnitTests.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Members.Components
{
    public class AttachedMemberProviderTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMembersShouldReturnNullResult()
        {
            var component = new AttachedMemberProvider();
            component.TryGetMembers(null!, typeof(object), string.Empty, MemberType.All, DefaultMetadata).IsEmpty.ShouldBeTrue();
            component.GetAttachedMembers(DefaultMetadata).IsEmpty.ShouldBeTrue();
        }

        [Theory]
        [InlineData(null, true)]
        [InlineData(null, false)]
        [InlineData("test", true)]
        [InlineData("test", false)]
        public void TryGetMembersShouldRegisterUnregisterMembers(string name, bool clear)
        {
            const string memberName = "f";
            var requestType = typeof(string);
            var invalidateCount = 0;
            var hasCache = new TestHasCache
            {
                Invalidate = (o, arg3) => ++invalidateCount
            };
            var owner = new MemberManager();
            var component = new AttachedMemberProvider();
            owner.AddComponent(component);
            owner.Components.Add(hasCache);

            var member = new TestAccessorMemberInfo {Name = memberName, DeclaringType = requestType, MemberType = MemberType.Accessor};
            component.Register(member, name);
            invalidateCount.ShouldEqual(1);

            var members = component.TryGetMembers(null!, requestType, name ?? memberName, MemberType.Accessor, DefaultMetadata);
            members.Count.ShouldEqual(1);
            members.Item.ShouldEqual(member);

            component.TryGetMembers(null!, requestType, name ?? memberName, MemberType.Method, DefaultMetadata).IsEmpty.ShouldBeTrue();

            members = component.GetAttachedMembers(DefaultMetadata);
            members.Count.ShouldEqual(1);
            members.Item.ShouldEqual(member);

            invalidateCount = 0;
            if (clear)
                component.Clear();
            else
                component.Unregister(member);
            component.TryGetMembers(null!, requestType, name ?? memberName, MemberType.Accessor, DefaultMetadata).IsEmpty.ShouldBeTrue();
            component.GetAttachedMembers(DefaultMetadata).IsEmpty.ShouldBeTrue();
            invalidateCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMembersShouldReturnAssignableTypes1()
        {
            const string memberName = "f";
            var owner = new MemberManager();
            var component = new AttachedMemberProvider();
            owner.AddComponent(component);

            var memberInfo1 = new TestAccessorMemberInfo {Name = memberName, DeclaringType = typeof(List<object>), MemberType = MemberType.Method};
            var memberInfo2 = new TestAccessorMemberInfo {Name = memberName, DeclaringType = typeof(IEnumerable<object>), MemberType = MemberType.Accessor};
            var memberInfo3 = new TestAccessorMemberInfo {Name = memberName, DeclaringType = typeof(string), MemberType = MemberType.Event};
            component.Register(memberInfo1);
            component.Register(memberInfo2);
            component.Register(memberInfo3);

            var members = component.TryGetMembers(null!, typeof(List<object>), memberName, MemberType.All, DefaultMetadata).AsList();
            members.Count.ShouldEqual(2);
            members.ShouldContain(memberInfo1);
            members.ShouldContain(memberInfo2);

            component.TryGetMembers(null!, typeof(MemberManager), memberName, MemberType.All, DefaultMetadata).IsEmpty.ShouldBeTrue();
            component.TryGetMembers(null!, typeof(string), memberName, MemberType.All, DefaultMetadata).AsList().Single().ShouldEqual(memberInfo3);
        }

        [Fact]
        public void TryGetMembersShouldReturnAssignableTypes2()
        {
            const string memberName = "f";
            var owner = new MemberManager();
            var component = new AttachedMemberProvider();
            owner.AddComponent(component);

            var memberInfo1 = new TestAccessorMemberInfo {Name = memberName, DeclaringType = typeof(object), MemberType = MemberType.Method};
            var memberInfo2 = new TestAccessorMemberInfo {Name = memberName, DeclaringType = typeof(IEnumerable<char>), MemberType = MemberType.Accessor};
            var memberInfo3 = new TestAccessorMemberInfo {Name = memberName, DeclaringType = typeof(string), MemberType = MemberType.Event};
            component.Register(memberInfo1);
            component.Register(memberInfo2);
            component.Register(memberInfo3);

            var members = component.TryGetMembers(null!, typeof(string), memberName, MemberType.All, DefaultMetadata).AsList();
            members.Count.ShouldEqual(3);
            members.ShouldContain(memberInfo1);
            members.ShouldContain(memberInfo2);
            members.ShouldContain(memberInfo3);

            component.TryGetMembers(null!, typeof(MemberManager), memberName, MemberType.All, DefaultMetadata).AsList().Single().ShouldEqual(memberInfo1);
            ;
            component.TryGetMembers(null!, typeof(IList<string>), memberName, MemberType.All, DefaultMetadata).AsList().Single().ShouldEqual(memberInfo1);
        }

        [Fact]
        public void TryGetMembersShouldReturnAssignableTypesGeneric()
        {
            const string memberName = "f";
            var owner = new MemberManager();
            var component = new AttachedMemberProvider();
            owner.AddComponent(component);

            var memberInfo1 = new TestAccessorMemberInfo {Name = memberName, DeclaringType = typeof(List<>), MemberType = MemberType.Accessor};
            var memberInfo2 = new TestAccessorMemberInfo {Name = memberName, DeclaringType = typeof(IEnumerable<>), MemberType = MemberType.Accessor};
            var memberInfo3 = new TestAccessorMemberInfo {Name = memberName, DeclaringType = typeof(string), MemberType = MemberType.Accessor};
            var memberInfo4 = new TestAccessorMemberInfo {Name = memberName, DeclaringType = typeof(object), MemberType = MemberType.Accessor};
            component.Register(memberInfo1);
            component.Register(memberInfo2);
            component.Register(memberInfo3);
            component.Register(memberInfo4);

            var members = component.TryGetMembers(null!, typeof(List<>), memberName, MemberType.Accessor, DefaultMetadata).AsList();
            members.Count.ShouldEqual(3);
            members.ShouldContain(memberInfo1);
            members.ShouldContain(memberInfo2);
            members.ShouldContain(memberInfo4);
        }

        #endregion
    }
}