using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Extensions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members.Components
{
    public class FakeMemberProviderTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMembersShouldReturnFakeMember1()
        {
            var component = new FakeMemberProvider();
            component.TryGetMembers(typeof(object), $"{FakeMemberProvider.FakeMemberPrefix}test", DefaultMetadata).Item.ShouldBeType<ConstantMemberInfo>();
        }

        [Fact]
        public void TryGetMembersShouldReturnFakeMember2()
        {
            var component = new FakeMemberProvider();
            component.TryGetMembers(typeof(object), $"{FakeMemberProvider.FakeMemberPrefixSymbol}test", DefaultMetadata).Item.ShouldBeType<ConstantMemberInfo>();
        }

        [Fact]
        public void TryGetMembersShouldReturnEmptyResultNoPrefix()
        {
            var component = new FakeMemberProvider();
            component.TryGetMembers(typeof(object), "test", DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
        }

        #endregion
    }
}