using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Collections;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using MugenMvvm.UnitTest.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members
{
    public class MemberProviderDecoratorTest : ComponentOwnerTestBase<IMemberManager>
    {
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMembersShouldBeHandledByComponents(int count)
        {
            var type = typeof(string);
            var memberType = MemberType.Accessor;
            var memberFlags = MemberFlags.All;
            var request = "test";
            var memberManager = new MemberManager();
            var member = new TestMemberAccessorInfo();
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestMemberManagerComponent
                {
                    Priority = -i,
                    TryGetMembers = (t, m, f, r, tt, meta) =>
                    {
                        ++invokeCount;
                        t.ShouldEqual(type);
                        m.ShouldEqual(memberType);
                        f.ShouldEqual(memberFlags);
                        r.ShouldEqual(request);
                        tt.ShouldEqual(request.GetType());
                        meta.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return member;
                        return default;
                    }
                };
                memberManager.AddComponent(component);
            }

            var result = memberManager.GetMembers(type, memberType, memberFlags, request, DefaultMetadata);
            result.Count().ShouldEqual(1);
            result.Item.ShouldEqual(member);
            invokeCount.ShouldEqual(count);
        }

        protected override IMemberManager GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new MemberManager(collectionProvider);
        }

        #endregion
    }
}