using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Extensions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members
{
    public class FakeMemberProviderComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMembersShouldReturnFakeMember1()
        {
            var component = new FakeMemberProviderComponent();
            component.TryGetMembers(typeof(object), $"{FakeMemberProviderComponent.FakeMemberPrefix}test", DefaultMetadata).Item.ShouldBeType<ConstantMemberInfo>();
        }

        [Fact]
        public void TryGetMembersShouldReturnFakeMember2()
        {
            var component = new FakeMemberProviderComponent();
            component.TryGetMembers(typeof(object), $"{FakeMemberProviderComponent.FakeMemberPrefixSymbol}test", DefaultMetadata).Item.ShouldBeType<ConstantMemberInfo>();
        }

        [Fact]
        public void TryGetMembersShouldReturnEmptyResultNoPrefix()
        {
            var component = new FakeMemberProviderComponent();
            component.TryGetMembers(typeof(object), "test", DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
        }

        #endregion
    }
}