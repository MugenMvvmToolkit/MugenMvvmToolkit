using MugenMvvm.Binding.Enums;
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
            component.TryGetMembers(null!, typeof(object), $"{FakeMemberProvider.FakeMemberPrefix}test", MemberType.Accessor, DefaultMetadata).Item.ShouldBeType<ConstantMemberInfo>();
            component.TryGetMembers(null!, typeof(object), $"{FakeMemberProvider.FakeMemberPrefix}test", MemberType.Method, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
        }

        [Fact]
        public void TryGetMembersShouldReturnFakeMember2()
        {
            var component = new FakeMemberProvider();
            component.TryGetMembers(null!, typeof(object), $"{FakeMemberProvider.FakeMemberPrefixSymbol}test", MemberType.Accessor, DefaultMetadata).Item.ShouldBeType<ConstantMemberInfo>();
        }

        [Fact]
        public void TryGetMembersShouldReturnEmptyResultNoPrefix()
        {
            var component = new FakeMemberProvider();
            component.TryGetMembers(null!, typeof(object), "test", MemberType.Accessor, DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
        }

        #endregion
    }
}