using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Components;
using MugenMvvm.Extensions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Members.Components
{
    public class FakeMemberProviderTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMembersShouldReturnFakeMember1()
        {
            var component = new FakeMemberProvider();
            component.TryGetMembers(null!, typeof(object), $"{FakeMemberProvider.FakeMemberPrefix}test", MemberType.Accessor, DefaultMetadata).Item.ShouldBeType<ConstantMemberInfo>();
            component.TryGetMembers(null!, typeof(object), $"{FakeMemberProvider.FakeMemberPrefix}test", MemberType.Method, DefaultMetadata).IsEmpty.ShouldBeTrue();
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
            component.TryGetMembers(null!, typeof(object), "test", MemberType.Accessor, DefaultMetadata).IsEmpty.ShouldBeTrue();
        }

        #endregion
    }
}