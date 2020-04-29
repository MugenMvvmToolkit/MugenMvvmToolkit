using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTest.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members
{
    public class AttachedMemberProviderComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMembersShouldReturnNullResult()
        {
            var component = new AttachedMemberProviderComponent();
            component.TryGetMembers(typeof(object), string.Empty, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            component.GetAttachedMembers(DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
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
                Invalidate = (o, type, arg3) => ++invalidateCount
            };
            var owner = new MemberManager();
            var component = new AttachedMemberProviderComponent();
            owner.AddComponent(component);
            owner.Components.Add(hasCache);

            var member = new TestMemberAccessorInfo {Name = memberName, DeclaringType = requestType};
            component.Register(member, name);
            invalidateCount.ShouldEqual(1);

            var members = component.TryGetMembers(requestType, name ?? memberName, DefaultMetadata);
            members.Count().ShouldEqual(1);
            members.Get(0).ShouldEqual(member);

            members = component.GetAttachedMembers(DefaultMetadata);
            members.Count().ShouldEqual(1);
            members.Get(0).ShouldEqual(member);

            invalidateCount = 0;
            if (clear)
                component.Clear();
            else
                component.Unregister(member);
            component.TryGetMembers(requestType, name ?? memberName, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            component.GetAttachedMembers(DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            invalidateCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMembersShouldReturnAssignableTypes()
        {
            const string memberName = "f";
            var owner = new MemberManager();
            var component = new AttachedMemberProviderComponent();
            owner.AddComponent(component);

            var memberInfo1 = new TestMemberAccessorInfo {Name = memberName, DeclaringType = typeof(List<object>)};
            var memberInfo2 = new TestMemberAccessorInfo {Name = memberName, DeclaringType = typeof(IEnumerable<object>)};
            var memberInfo3 = new TestMemberAccessorInfo {Name = memberName, DeclaringType = typeof(string)};
            component.Register(memberInfo1);
            component.Register(memberInfo2);
            component.Register(memberInfo3);

            var members = component.TryGetMembers(typeof(IEnumerable<object>), memberName, DefaultMetadata).ToArray();
            members.Length.ShouldEqual(2);
            members.ShouldContain(memberInfo1);
            members.ShouldContain(memberInfo2);

            component.TryGetMembers(typeof(MemberManager), memberName, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            component.TryGetMembers(typeof(string), memberName, DefaultMetadata).ToArray().Single().ShouldEqual(memberInfo3);
        }

        [Fact]
        public void TryGetMembersShouldReturnAssignableTypesGeneric()
        {
            const string memberName = "f";
            var owner = new MemberManager();
            var component = new AttachedMemberProviderComponent();
            owner.AddComponent(component);

            var memberInfo1 = new TestMemberAccessorInfo {Name = memberName, DeclaringType = typeof(List<>)};
            var memberInfo2 = new TestMemberAccessorInfo {Name = memberName, DeclaringType = typeof(IEnumerable<>)};
            var memberInfo3 = new TestMemberAccessorInfo {Name = memberName, DeclaringType = typeof(string)};
            var memberInfo4 = new TestMemberAccessorInfo {Name = memberName, DeclaringType = typeof(object)};
            component.Register(memberInfo1);
            component.Register(memberInfo2);
            component.Register(memberInfo3);
            component.Register(memberInfo4);

            var members = component.TryGetMembers(typeof(IEnumerable<>), memberName, DefaultMetadata).ToArray();
            members.Length.ShouldEqual(3);
            members.ShouldContain(memberInfo1);
            members.ShouldContain(memberInfo2);
            members.ShouldContain(memberInfo3);
        }

        #endregion
    }
}