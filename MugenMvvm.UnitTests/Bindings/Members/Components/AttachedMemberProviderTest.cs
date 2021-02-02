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
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Members.Components
{
    public class AttachedMemberProviderTest : UnitTestBase
    {
        private readonly MemberManager _memberManager;
        private readonly AttachedMemberProvider _provider;

        public AttachedMemberProviderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _memberManager = new MemberManager(ComponentCollectionManager);
            _provider = new AttachedMemberProvider();
            _memberManager.AddComponent(_provider);
        }

        [Fact]
        public void TryGetMembersShouldReturnAssignableTypes1()
        {
            const string memberName = "f";

            var memberInfo1 = new TestAccessorMemberInfo {Name = memberName, DeclaringType = typeof(List<object>), MemberType = MemberType.Method};
            var memberInfo2 = new TestAccessorMemberInfo {Name = memberName, DeclaringType = typeof(IEnumerable<object>), MemberType = MemberType.Accessor};
            var memberInfo3 = new TestAccessorMemberInfo {Name = memberName, DeclaringType = typeof(string), MemberType = MemberType.Event};
            _provider.Register(memberInfo1);
            _provider.Register(memberInfo2);
            _provider.Register(memberInfo3);

            var members = _provider.TryGetMembers(null!, typeof(List<object>), memberName, MemberType.All, DefaultMetadata).AsList();
            members.Count.ShouldEqual(2);
            members.ShouldContain(memberInfo1);
            members.ShouldContain(memberInfo2);

            _provider.TryGetMembers(_memberManager, typeof(MemberManager), memberName, MemberType.All, DefaultMetadata).IsEmpty.ShouldBeTrue();
            _provider.TryGetMembers(_memberManager, typeof(string), memberName, MemberType.All, DefaultMetadata).AsList().Single().ShouldEqual(memberInfo3);
        }

        [Fact]
        public void TryGetMembersShouldReturnAssignableTypes2()
        {
            const string memberName = "f";

            var memberInfo1 = new TestAccessorMemberInfo {Name = memberName, DeclaringType = typeof(object), MemberType = MemberType.Method};
            var memberInfo2 = new TestAccessorMemberInfo {Name = memberName, DeclaringType = typeof(IEnumerable<char>), MemberType = MemberType.Accessor};
            var memberInfo3 = new TestAccessorMemberInfo {Name = memberName, DeclaringType = typeof(string), MemberType = MemberType.Event};
            _provider.Register(memberInfo1);
            _provider.Register(memberInfo2);
            _provider.Register(memberInfo3);

            var members = _provider.TryGetMembers(_memberManager, typeof(string), memberName, MemberType.All, DefaultMetadata).AsList();
            members.Count.ShouldEqual(3);
            members.ShouldContain(memberInfo1);
            members.ShouldContain(memberInfo2);
            members.ShouldContain(memberInfo3);

            _provider.TryGetMembers(_memberManager, typeof(MemberManager), memberName, MemberType.All, DefaultMetadata).AsList().Single().ShouldEqual(memberInfo1);
            _provider.TryGetMembers(_memberManager, typeof(IList<string>), memberName, MemberType.All, DefaultMetadata).AsList().Single().ShouldEqual(memberInfo1);
        }

        [Fact]
        public void TryGetMembersShouldReturnAssignableTypesGeneric()
        {
            const string memberName = "f";

            var memberInfo1 = new TestAccessorMemberInfo {Name = memberName, DeclaringType = typeof(List<>), MemberType = MemberType.Accessor};
            var memberInfo2 = new TestAccessorMemberInfo {Name = memberName, DeclaringType = typeof(IEnumerable<>), MemberType = MemberType.Accessor};
            var memberInfo3 = new TestAccessorMemberInfo {Name = memberName, DeclaringType = typeof(string), MemberType = MemberType.Accessor};
            var memberInfo4 = new TestAccessorMemberInfo {Name = memberName, DeclaringType = typeof(object), MemberType = MemberType.Accessor};
            _provider.Register(memberInfo1);
            _provider.Register(memberInfo2);
            _provider.Register(memberInfo3);
            _provider.Register(memberInfo4);

            var members = _provider.TryGetMembers(_memberManager, typeof(List<>), memberName, MemberType.Accessor, DefaultMetadata).AsList();
            members.Count.ShouldEqual(3);
            members.ShouldContain(memberInfo1);
            members.ShouldContain(memberInfo2);
            members.ShouldContain(memberInfo4);
        }

        [Fact]
        public void TryGetMembersShouldReturnNullResult()
        {
            _provider.TryGetMembers(_memberManager, typeof(object), string.Empty, MemberType.All, DefaultMetadata).IsEmpty.ShouldBeTrue();
            _provider.GetAttachedMembers().IsEmpty.ShouldBeTrue();
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

            _memberManager.Components.TryAdd(hasCache);

            var member = new TestAccessorMemberInfo {Name = memberName, DeclaringType = requestType, MemberType = MemberType.Accessor};
            _provider.Register(member, name);
            invalidateCount.ShouldEqual(1);

            var members = _provider.TryGetMembers(_memberManager, requestType, name ?? memberName, MemberType.Accessor, DefaultMetadata);
            members.Count.ShouldEqual(1);
            members.Item.ShouldEqual(member);

            _provider.TryGetMembers(_memberManager, requestType, name ?? memberName, MemberType.Method, DefaultMetadata).IsEmpty.ShouldBeTrue();

            members = _provider.GetAttachedMembers();
            members.Count.ShouldEqual(1);
            members.Item.ShouldEqual(member);

            invalidateCount = 0;
            if (clear)
                _provider.Clear();
            else
                _provider.Unregister(member);
            _provider.TryGetMembers(_memberManager, requestType, name ?? memberName, MemberType.Accessor, DefaultMetadata).IsEmpty.ShouldBeTrue();
            _provider.GetAttachedMembers().IsEmpty.ShouldBeTrue();
            invalidateCount.ShouldEqual(1);
        }
    }
}