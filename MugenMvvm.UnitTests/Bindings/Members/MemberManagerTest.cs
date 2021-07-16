using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Tests.Bindings.Members;
using MugenMvvm.UnitTests.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Members
{
    public class MemberManagerTest : ComponentOwnerTestBase<IMemberManager>
    {
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMembersShouldBeHandledByComponents(int count)
        {
            var type = typeof(string);
            var memberType = MemberType.Accessor;
            var memberFlags = MemberFlags.All;
            var request = "test";
            var member = new TestAccessorMemberInfo();
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestMemberManagerComponent
                {
                    Priority = -i,
                    TryGetMembers = (mm, t, m, f, r, meta) =>
                    {
                        ++invokeCount;
                        mm.ShouldEqual(MemberManager);
                        t.ShouldEqual(type);
                        m.ShouldEqual(memberType);
                        f.ShouldEqual(memberFlags);
                        r.ShouldEqual(request);
                        meta.ShouldEqual(Metadata);
                        if (isLast)
                            return member;
                        return default;
                    }
                };
                MemberManager.AddComponent(component);
            }

            var result = MemberManager.TryGetMembers(type, memberType, memberFlags, request, Metadata);
            result.Count.ShouldEqual(1);
            result.Item.ShouldEqual(member);
            invokeCount.ShouldEqual(count);
        }

        protected override IMemberManager GetMemberManager() => GetComponentOwner(ComponentCollectionManager);

        protected override IMemberManager GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new MemberManager(componentCollectionManager);
    }
}