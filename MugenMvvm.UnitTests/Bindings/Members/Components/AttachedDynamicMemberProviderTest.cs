using System;
using System.Collections.Generic;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Tests.Bindings.Members;
using MugenMvvm.Tests.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Members.Components
{
    public class AttachedDynamicMemberProviderTest : UnitTestBase
    {
        private readonly AttachedDynamicMemberProvider _provider;

        public AttachedDynamicMemberProviderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _provider = new AttachedDynamicMemberProvider();
            MemberManager.AddComponent(_provider);
        }

        [Fact]
        public void TryGetMembersShouldReturnNullResult() =>
            _provider.TryGetMembers(MemberManager, typeof(object), string.Empty, MemberType.All, DefaultMetadata).IsEmpty.ShouldBeTrue();

        protected override IMemberManager GetMemberManager() => new MemberManager(ComponentCollectionManager);

        [Theory]
        [InlineData(1, true)]
        [InlineData(1, false)]
        [InlineData(10, true)]
        [InlineData(10, false)]
        public void TryGetMembersShouldRegisterUnregisterMembers(int count, bool clear)
        {
            var requestType = typeof(string);
            var name = "test";
            var memberType = MemberType.Method.AsFlags();

            var invalidateCount = 0;
            var hasCache = new TestHasCacheComponent<IMemberManager>
            {
                Invalidate = (s, o, arg3) => ++invalidateCount
            };

            MemberManager.Components.TryAdd(hasCache);
            var list = new List<IMemberInfo>();
            var delegates = new List<Func<Type, string, EnumFlags<MemberType>, IReadOnlyMetadataContext?, IMemberInfo?>>();
            for (var i = 0; i < count; i++)
            {
                Func<Type, string, EnumFlags<MemberType>, IReadOnlyMetadataContext?, IMemberInfo?> func = (type, s, t, arg3) =>
                {
                    memberType.ShouldEqual(t);
                    type.ShouldEqual(requestType);
                    name.ShouldEqual(s);
                    arg3.ShouldEqual(DefaultMetadata);
                    var info = new TestAccessorMemberInfo();
                    list.Add(info);
                    return info;
                };
                delegates.Add(func);
                _provider.Register(func);
            }

            invalidateCount.ShouldEqual(count);
            _provider.TryGetMembers(MemberManager, requestType, name, memberType, DefaultMetadata).AsList().ShouldEqual(list);
            list.Count.ShouldEqual(count);

            invalidateCount = 0;
            if (clear)
                _provider.Clear();
            else
            {
                foreach (var @delegate in delegates)
                    _provider.Unregister(@delegate);
            }

            _provider.TryGetMembers(MemberManager, typeof(object), string.Empty, memberType, DefaultMetadata).IsEmpty.ShouldBeTrue();
            invalidateCount.ShouldEqual(clear ? 1 : count);
        }
    }
}