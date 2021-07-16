using System.Collections.Generic;
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
    public class NameRequestMemberManagerDecoratorTest : UnitTestBase
    {
        private readonly NameRequestMemberManagerDecorator _provider;

        public NameRequestMemberManagerDecoratorTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _provider = new NameRequestMemberManagerDecorator();
            MemberManager.AddComponent(_provider);
        }

        [Fact]
        public void TryGetMembersShouldIgnoreNotSupportedRequest()
        {
            _provider.TryGetMembers(MemberManager, typeof(object), MemberType.All, MemberFlags.All, "", Metadata).IsEmpty.ShouldBeTrue();
            _provider.TryGetMembers(MemberManager, typeof(object), MemberType.All, MemberFlags.All, this, Metadata).IsEmpty.ShouldBeTrue();
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
            var members = new[] { new TestAccessorMemberInfo(), new TestAccessorMemberInfo() };

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
                    meta.ShouldEqual(Metadata);
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
                    s.ShouldEqual(request);
                    arg3.ShouldEqual(Metadata);
                    return members;
                }
            });

            MemberManager.TryGetMembers(type, memberType, memberFlags, request, Metadata).ShouldEqual(members);
            selectorCount.ShouldEqual(1);
            providerCount.ShouldEqual(1);
        }

        protected override IMemberManager GetMemberManager() => new MemberManager(ComponentCollectionManager);
    }
}