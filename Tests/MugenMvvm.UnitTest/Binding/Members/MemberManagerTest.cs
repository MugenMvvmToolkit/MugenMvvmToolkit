using MugenMvvm.Binding.Interfaces.Members;
using MugenMvvm.Binding.Members;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTest.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members
{
    public class MemberManagerTest : ComponentOwnerTestBase<IMemberManager>
    {
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMembersShouldBeHandledByComponents(int count)
        {
            var request = "t";
            var memberManager = new MemberManager();
            var member = new TestMemberAccessorInfo();
            var invokeCount = 0;
            for (var i = 0; i < count; i++)
            {
                var isLast = i == count - 1;
                var component = new TestMemberManagerComponent
                {
                    Priority = -i,
                    TryGetMembers = (o, type, arg3) =>
                    {
                        ++invokeCount;
                        o.ShouldEqual(request);
                        type.ShouldEqual(request.GetType());
                        arg3.ShouldEqual(DefaultMetadata);
                        if (isLast)
                            return member;
                        return default;
                    }
                };
                memberManager.AddComponent(component);
            }

            var result = memberManager.GetMembers(request, DefaultMetadata);
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