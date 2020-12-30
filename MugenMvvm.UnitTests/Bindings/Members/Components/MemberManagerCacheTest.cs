using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Components;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Bindings.Members.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Members.Components
{
    public class MemberManagerCacheTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMembersShouldNotCacheUnsupportedType()
        {
            var invokeCount = 0;
            var type = typeof(string);
            var memberType = MemberType.Accessor;
            var memberFlags = MemberFlags.All;
            object request = this;
            var result = new TestAccessorMemberInfo();
            var memberManager = new MemberManager();
            memberManager.AddComponent(new TestMemberManagerComponent(memberManager)
            {
                TryGetMembers = (t, m, f, r, meta) =>
                {
                    ++invokeCount;
                    t.ShouldEqual(type);
                    m.ShouldEqual(memberType);
                    f.ShouldEqual(memberFlags);
                    r.ShouldEqual(request);
                    meta.ShouldEqual(DefaultMetadata);
                    return result;
                }
            });
            memberManager.AddComponent(new MemberManagerCache());

            memberManager.TryGetMembers(type, memberType, memberFlags, this, DefaultMetadata).ShouldEqual(result);
            memberManager.TryGetMembers(type, memberType, memberFlags, this, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(2);

            request = 1;
            memberManager.TryGetMembers(type, memberType, memberFlags, 1, DefaultMetadata).ShouldEqual(result);
            memberManager.TryGetMembers(type, memberType, memberFlags, 1, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(4);
        }

        [Fact]
        public void TryGetMembersShouldUseCache1()
        {
            var invokeCount = 0;
            var type = typeof(string);
            var memberType = MemberType.Accessor;
            var memberFlags = MemberFlags.All;
            var request = "test";
            var result = new TestAccessorMemberInfo();
            var memberManager = new MemberManager();
            memberManager.AddComponent(new TestMemberManagerComponent(memberManager)
            {
                TryGetMembers = (t, m, f, r, meta) =>
                {
                    ++invokeCount;
                    t.ShouldEqual(type);
                    m.ShouldEqual(memberType);
                    f.ShouldEqual(memberFlags);
                    r.ShouldEqual(request);
                    meta.ShouldEqual(DefaultMetadata);
                    return result;
                }
            });
            memberManager.AddComponent(new MemberManagerCache());

            memberManager.TryGetMembers(type, memberType, memberFlags, request, DefaultMetadata).ShouldEqual(result);
            memberManager.TryGetMembers(type, memberType, memberFlags, request, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMembersShouldUseCache2()
        {
            var invokeCount = 0;
            var type = typeof(string);
            var memberType = MemberType.Accessor;
            var memberFlags = MemberFlags.All;
            var request = new MemberTypesRequest("test", new[] {typeof(object)});
            var result = new TestAccessorMemberInfo();
            var memberManager = new MemberManager();
            memberManager.AddComponent(new TestMemberManagerComponent(memberManager)
            {
                TryGetMembers = (t, m, f, r, meta) =>
                {
                    ++invokeCount;
                    t.ShouldEqual(type);
                    m.ShouldEqual(memberType);
                    f.ShouldEqual(memberFlags);
                    r.ShouldEqual(request);
                    meta.ShouldEqual(DefaultMetadata);
                    return result;
                }
            });
            memberManager.AddComponent(new MemberManagerCache());

            memberManager.TryGetMembers(type, memberType, memberFlags, request, DefaultMetadata).ShouldEqual(result);
            memberManager.TryGetMembers(type, memberType, memberFlags, request, DefaultMetadata).ShouldEqual(result);
            memberManager.TryGetMembers(type, memberType, memberFlags, new MemberTypesRequest(request.Name, request.Types.ToArray()), DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void InvalidateShouldClearCache()
        {
            var invokeCount = 0;
            var type1 = typeof(string);
            var type2 = typeof(object);
            var memberType = MemberType.Accessor;
            var memberFlags = MemberFlags.All;
            var request1 = "test1";
            var request2 = "test2";
            var result = new TestAccessorMemberInfo();
            var memberManager = new MemberManager();
            memberManager.AddComponent(new TestMemberManagerComponent(memberManager)
            {
                TryGetMembers = (t, m, f, r, meta) =>
                {
                    ++invokeCount;
                    return result;
                }
            });
            memberManager.AddComponent(new MemberManagerCache());

            memberManager.TryGetMembers(type1, memberType, memberFlags, request1, DefaultMetadata).ShouldEqual(result);
            memberManager.TryGetMembers(type1, memberType, memberFlags, request1, DefaultMetadata).ShouldEqual(result);
            memberManager.TryGetMembers(type2, memberType, memberFlags, request2, DefaultMetadata).ShouldEqual(result);
            memberManager.TryGetMembers(type2, memberType, memberFlags, request2, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(2);

            memberManager.TryInvalidateCache(null, DefaultMetadata);
            invokeCount = 0;
            memberManager.TryGetMembers(type1, memberType, memberFlags, request1, DefaultMetadata).ShouldEqual(result);
            memberManager.TryGetMembers(type1, memberType, memberFlags, request1, DefaultMetadata).ShouldEqual(result);
            memberManager.TryGetMembers(type2, memberType, memberFlags, request2, DefaultMetadata).ShouldEqual(result);
            memberManager.TryGetMembers(type2, memberType, memberFlags, request2, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(2);

            memberManager.TryInvalidateCache(type1, DefaultMetadata);
            invokeCount = 0;
            memberManager.TryGetMembers(type1, memberType, memberFlags, request1, DefaultMetadata).ShouldEqual(result);
            memberManager.TryGetMembers(type1, memberType, memberFlags, request1, DefaultMetadata).ShouldEqual(result);
            memberManager.TryGetMembers(type2, memberType, memberFlags, request2, DefaultMetadata).ShouldEqual(result);
            memberManager.TryGetMembers(type2, memberType, memberFlags, request2, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void AttachDetachShouldClearCache()
        {
            var invokeCount = 0;
            var type1 = typeof(string);
            var type2 = typeof(object);
            var memberType = MemberType.Accessor;
            var memberFlags = MemberFlags.All;
            var request1 = "test1";
            var request2 = "test2";
            var result = new TestAccessorMemberInfo();
            var cacheComponent = new MemberManagerCache();
            var memberManager = new MemberManager();
            memberManager.AddComponent(new TestMemberManagerComponent(memberManager)
            {
                TryGetMembers = (t, m, f, r, meta) =>
                {
                    ++invokeCount;
                    return result;
                }
            });
            memberManager.AddComponent(cacheComponent);

            memberManager.TryGetMembers(type1, memberType, memberFlags, request1, DefaultMetadata).ShouldEqual(result);
            memberManager.TryGetMembers(type1, memberType, memberFlags, request1, DefaultMetadata).ShouldEqual(result);
            memberManager.TryGetMembers(type2, memberType, memberFlags, request2, DefaultMetadata).ShouldEqual(result);
            memberManager.TryGetMembers(type2, memberType, memberFlags, request2, DefaultMetadata).ShouldEqual(result);
            invokeCount.ShouldEqual(2);

            memberManager.RemoveComponent(cacheComponent);
            invokeCount = 0;
            cacheComponent.TryGetMembers(memberManager, type1, memberType, memberFlags, request1, DefaultMetadata).IsEmpty.ShouldBeTrue();
            cacheComponent.TryGetMembers(memberManager, type1, memberType, memberFlags, request1, DefaultMetadata).IsEmpty.ShouldBeTrue();
            cacheComponent.TryGetMembers(memberManager, type2, memberType, memberFlags, request2, DefaultMetadata).IsEmpty.ShouldBeTrue();
            cacheComponent.TryGetMembers(memberManager, type2, memberType, memberFlags, request2, DefaultMetadata).IsEmpty.ShouldBeTrue();
            invokeCount.ShouldEqual(0);
        }

        #endregion
    }
}