using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Components;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Bindings.Members.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Members.Components
{
    public class NameRequestMemberManagerDecoratorTest : UnitTestBase
    {
        private readonly MemberManager _memberManager;
        private readonly NameRequestMemberManagerDecorator _provider;

        public NameRequestMemberManagerDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _memberManager = new MemberManager(ComponentCollectionManager);
            _provider = new NameRequestMemberManagerDecorator();
            _memberManager.AddComponent(_provider);
        }

        [Fact]
        public void TryGetMembersShouldIgnoreNotSupportedRequest()
        {
            _provider.TryGetMembers(_memberManager, typeof(object), MemberType.All, MemberFlags.All, "", DefaultMetadata).IsEmpty.ShouldBeTrue();
            _provider.TryGetMembers(_memberManager, typeof(object), MemberType.All, MemberFlags.All, this, DefaultMetadata).IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void TryGetMembersShouldUseSelector()
        {
            var type = GetType();
            var memberType = MemberType.All;
            var memberFlags = MemberFlags.Instance.AsFlags();
            var request = "";
            var selectorCount = 0;
            var providerCount = 0;
            var members = new[] {new TestAccessorMemberInfo(), new TestAccessorMemberInfo()};

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
                    s.ShouldEqual(request);
                    arg3.ShouldEqual(DefaultMetadata);
                    return members;
                }
            });

            _memberManager.TryGetMembers(type, memberType, memberFlags, request, DefaultMetadata).AsList().ShouldEqual(members);
            selectorCount.ShouldEqual(1);
            providerCount.ShouldEqual(1);
        }
    }
}