using System.Collections.Generic;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Members.Components;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members.Components
{
    public class CacheMemberManagerDecoratorTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMembersShouldUseCache()
        {
            var invokeCount = 0;
            var request = new MemberManagerRequest(typeof(string), "test", MemberType.Accessor, MemberFlags.All);
            var result = new TestMemberAccessorInfo();
            var providerComponent = new TestMemberManagerComponent
            {
                TryGetMembers = (o, type, arg3) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(request);
                    arg3.ShouldEqual(DefaultMetadata);
                    return result;
                }
            };
            var cacheComponent = new CacheMemberManagerDecorator();
            ((IComponentCollectionDecorator<IMemberManagerComponent>) cacheComponent).Decorate(new List<IMemberManagerComponent> {cacheComponent, providerComponent}, DefaultMetadata);

            cacheComponent.TryGetMembers(request, DefaultMetadata).ShouldEqual(result);
            cacheComponent.TryGetMembers(request, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void InvalidateShouldClearCache()
        {
            var invokeCount = 0;
            var request1 = new MemberManagerRequest(typeof(string), "test", MemberType.Accessor, MemberFlags.All);
            var request2 = new MemberManagerRequest(typeof(object), "test", MemberType.Accessor, MemberFlags.All);
            var result = new TestMemberAccessorInfo();
            var providerComponent = new TestMemberManagerComponent
            {
                TryGetMembers = (o, type, arg3) =>
                {
                    ++invokeCount;
                    return result;
                }
            };
            var cacheComponent = new CacheMemberManagerDecorator();
            ((IComponentCollectionDecorator<IMemberManagerComponent>) cacheComponent).Decorate(new List<IMemberManagerComponent> {cacheComponent, providerComponent}, DefaultMetadata);

            cacheComponent.TryGetMembers(request1, DefaultMetadata).ShouldEqual(result);
            cacheComponent.TryGetMembers(request1, DefaultMetadata).ShouldEqual(result);
            cacheComponent.TryGetMembers(request2, DefaultMetadata).ShouldEqual(result);
            cacheComponent.TryGetMembers(request2, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(2);

            cacheComponent.Invalidate<object?>(null, DefaultMetadata);
            invokeCount = 0;
            cacheComponent.TryGetMembers(request1, DefaultMetadata).ShouldEqual(result);
            cacheComponent.TryGetMembers(request1, DefaultMetadata).ShouldEqual(result);
            cacheComponent.TryGetMembers(request2, DefaultMetadata).ShouldEqual(result);
            cacheComponent.TryGetMembers(request2, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(2);

            cacheComponent.Invalidate<object?>(request1.Type, DefaultMetadata);
            invokeCount = 0;
            cacheComponent.TryGetMembers(request1, DefaultMetadata).ShouldEqual(result);
            cacheComponent.TryGetMembers(request1, DefaultMetadata).ShouldEqual(result);
            cacheComponent.TryGetMembers(request2, DefaultMetadata).ShouldEqual(result);
            cacheComponent.TryGetMembers(request2, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void AttachDetachShouldClearCache()
        {
            var invokeCount = 0;
            var request1 = new MemberManagerRequest(typeof(string), "test", MemberType.Accessor, MemberFlags.All);
            var request2 = new MemberManagerRequest(typeof(object), "test", MemberType.Accessor, MemberFlags.All);
            var result = new TestMemberAccessorInfo();
            var providerComponent = new TestMemberManagerComponent
            {
                TryGetMembers = (o, type, arg3) =>
                {
                    ++invokeCount;
                    return result;
                }
            };
            var cacheComponent = new CacheMemberManagerDecorator();
            var memberManager = new MemberManager();
            memberManager.AddComponent(providerComponent);
            memberManager.AddComponent(cacheComponent);

            memberManager.GetMembers(request1, DefaultMetadata).ShouldEqual(result);
            memberManager.GetMembers(request1, DefaultMetadata).ShouldEqual(result);
            memberManager.GetMembers(request2, DefaultMetadata).ShouldEqual(result);
            memberManager.GetMembers(request2, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(2);

            memberManager.RemoveComponent(cacheComponent);
            invokeCount = 0;
            cacheComponent.TryGetMembers(request1, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            cacheComponent.TryGetMembers(request1, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            cacheComponent.TryGetMembers(request2, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            cacheComponent.TryGetMembers(request2, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
            invokeCount.ShouldEqual(0);
        }

        #endregion
    }
}