using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Members;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Components;
using MugenMvvm.Extensions;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Members.Components
{
    public class FakeMemberProviderTest : UnitTestBase
    {
        private readonly FakeMemberProvider _provider;

        public FakeMemberProviderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _provider = new FakeMemberProvider();
            MemberManager.AddComponent(_provider);
        }

        [Fact]
        public void TryGetMembersShouldReturnEmptyResultNoPrefix() =>
            _provider.TryGetMembers(MemberManager, typeof(object), "test", MemberType.Accessor, DefaultMetadata).IsEmpty.ShouldBeTrue();

        [Fact]
        public void TryGetMembersShouldReturnFakeMember1()
        {
            _provider.TryGetMembers(MemberManager, typeof(object), $"{FakeMemberProvider.FakeMemberPrefix}test", MemberType.Accessor, DefaultMetadata).Item
                     .ShouldBeType<ConstantMemberInfo>();
            _provider.TryGetMembers(MemberManager, typeof(object), $"{FakeMemberProvider.FakeMemberPrefix}test", MemberType.Method, DefaultMetadata).IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void TryGetMembersShouldReturnFakeMember2() =>
            _provider.TryGetMembers(null!, typeof(object), $"{FakeMemberProvider.FakeMemberPrefixSymbol}test", MemberType.Accessor, DefaultMetadata).Item
                     .ShouldBeType<ConstantMemberInfo>();

        protected override IMemberManager GetMemberManager() => new MemberManager(ComponentCollectionManager);
    }
}