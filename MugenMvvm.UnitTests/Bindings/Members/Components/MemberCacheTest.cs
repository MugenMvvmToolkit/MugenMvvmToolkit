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
    public class MemberCacheTest : UnitTestBase
    {
        private readonly MemberCache _memberCache;

        public MemberCacheTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _memberCache = new MemberCache();
            MemberManager.AddComponent(_memberCache);
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

            MemberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (m, _, _, _, _, _) =>
                {
                    ++invokeCount;
                    m.ShouldEqual(MemberManager);
                    return result;
                }
            });

            MemberManager.TryGetMembers(type1, memberType, memberFlags, request1, Metadata).ShouldEqual(result);
            MemberManager.TryGetMembers(type1, memberType, memberFlags, request1, Metadata).ShouldEqual(result);
            MemberManager.TryGetMembers(type2, memberType, memberFlags, request2, Metadata).ShouldEqual(result);
            MemberManager.TryGetMembers(type2, memberType, memberFlags, request2, Metadata).ShouldEqual(result);
            invokeCount.ShouldEqual(2);

            MemberManager.RemoveComponent(_memberCache);
            invokeCount = 0;
            _memberCache.TryGetMembers(MemberManager, type1, memberType, memberFlags, request1, Metadata).IsEmpty.ShouldBeTrue();
            _memberCache.TryGetMembers(MemberManager, type1, memberType, memberFlags, request1, Metadata).IsEmpty.ShouldBeTrue();
            _memberCache.TryGetMembers(MemberManager, type2, memberType, memberFlags, request2, Metadata).IsEmpty.ShouldBeTrue();
            _memberCache.TryGetMembers(MemberManager, type2, memberType, memberFlags, request2, Metadata).IsEmpty.ShouldBeTrue();
            invokeCount.ShouldEqual(0);
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

            MemberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (m, _, _, _, _, _) =>
                {
                    ++invokeCount;
                    m.ShouldEqual(MemberManager);
                    return result;
                }
            });

            MemberManager.TryGetMembers(type1, memberType, memberFlags, request1, Metadata).ShouldEqual(result);
            MemberManager.TryGetMembers(type1, memberType, memberFlags, request1, Metadata).ShouldEqual(result);
            MemberManager.TryGetMembers(type2, memberType, memberFlags, request2, Metadata).ShouldEqual(result);
            MemberManager.TryGetMembers(type2, memberType, memberFlags, request2, Metadata).ShouldEqual(result);
            invokeCount.ShouldEqual(2);

            MemberManager.TryInvalidateCache(null, Metadata);
            invokeCount = 0;
            MemberManager.TryGetMembers(type1, memberType, memberFlags, request1, Metadata).ShouldEqual(result);
            MemberManager.TryGetMembers(type1, memberType, memberFlags, request1, Metadata).ShouldEqual(result);
            MemberManager.TryGetMembers(type2, memberType, memberFlags, request2, Metadata).ShouldEqual(result);
            MemberManager.TryGetMembers(type2, memberType, memberFlags, request2, Metadata).ShouldEqual(result);
            invokeCount.ShouldEqual(2);

            MemberManager.TryInvalidateCache(type1, Metadata);
            invokeCount = 0;
            MemberManager.TryGetMembers(type1, memberType, memberFlags, request1, Metadata).ShouldEqual(result);
            MemberManager.TryGetMembers(type1, memberType, memberFlags, request1, Metadata).ShouldEqual(result);
            MemberManager.TryGetMembers(type2, memberType, memberFlags, request2, Metadata).ShouldEqual(result);
            MemberManager.TryGetMembers(type2, memberType, memberFlags, request2, Metadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMembersShouldNotCacheUnsupportedType()
        {
            var invokeCount = 0;
            var type = typeof(string);
            var memberType = MemberType.Accessor;
            var memberFlags = MemberFlags.All;
            object request = this;
            var result = new TestAccessorMemberInfo();
            MemberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (mm, t, m, f, r, meta) =>
                {
                    ++invokeCount;
                    mm.ShouldEqual(MemberManager);
                    t.ShouldEqual(type);
                    m.ShouldEqual(memberType);
                    f.ShouldEqual(memberFlags);
                    r.ShouldEqual(request);
                    meta.ShouldEqual(Metadata);
                    return result;
                }
            });

            MemberManager.TryGetMembers(type, memberType, memberFlags, this, Metadata).ShouldEqual(result);
            MemberManager.TryGetMembers(type, memberType, memberFlags, this, Metadata).ShouldEqual(result);
            invokeCount.ShouldEqual(2);

            request = 1;
            MemberManager.TryGetMembers(type, memberType, memberFlags, 1, Metadata).ShouldEqual(result);
            MemberManager.TryGetMembers(type, memberType, memberFlags, 1, Metadata).ShouldEqual(result);
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
            MemberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (mm, t, m, f, r, meta) =>
                {
                    ++invokeCount;
                    mm.ShouldEqual(MemberManager);
                    t.ShouldEqual(type);
                    m.ShouldEqual(memberType);
                    f.ShouldEqual(memberFlags);
                    r.ShouldEqual(request);
                    meta.ShouldEqual(Metadata);
                    return result;
                }
            });

            MemberManager.TryGetMembers(type, memberType, memberFlags, request, Metadata).ShouldEqual(result);
            MemberManager.TryGetMembers(type, memberType, memberFlags, request, Metadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMembersShouldUseCache2()
        {
            var invokeCount = 0;
            var type = typeof(string);
            var memberType = MemberType.Accessor;
            var memberFlags = MemberFlags.All;
            var request = new MemberTypesRequest("test", new[] { typeof(object) });
            var result = new TestAccessorMemberInfo();
            MemberManager.AddComponent(new TestMemberManagerComponent
            {
                TryGetMembers = (mm, t, m, f, r, meta) =>
                {
                    ++invokeCount;
                    mm.ShouldEqual(MemberManager);
                    t.ShouldEqual(type);
                    m.ShouldEqual(memberType);
                    f.ShouldEqual(memberFlags);
                    r.ShouldEqual(request);
                    meta.ShouldEqual(Metadata);
                    return result;
                }
            });

            MemberManager.TryGetMembers(type, memberType, memberFlags, request, Metadata).ShouldEqual(result);
            MemberManager.TryGetMembers(type, memberType, memberFlags, request, Metadata).ShouldEqual(result);
            MemberManager.TryGetMembers(type, memberType, memberFlags, new MemberTypesRequest(request.Name, request.Types.ToArray()), Metadata).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        protected override IMemberManager GetMemberManager() => new MemberManager(ComponentCollectionManager);
    }
}