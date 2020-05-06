using System.Linq;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTest.Binding.Members.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members.Components
{
    public class MemberManagerComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMembersShouldIgnoreNotSupportedRequest()
        {
            var component = new MemberManagerComponent();
            component.TryGetMembers("", DefaultMetadata).IsNullOrEmpty().ShouldBeTrue();
        }

        [Fact]
        public void TryGetMembersShouldUseSelector()
        {
            var request = new MemberManagerRequest(GetType(), nameof(TryGetMembersShouldIgnoreNotSupportedRequest), MemberType.All, MemberFlags.All);
            var selectorCount = 0;
            var providerCount = 0;
            var members = new[] {new TestMemberAccessorInfo(), new TestMemberAccessorInfo()};

            var manager = new MemberManager();
            var selector = new TestMemberSelectorComponent
            {
                TrySelectMembers = (list, type, arg3, arg4, arg5) =>
                {
                    ++selectorCount;
                    list.SequenceEqual(members).ShouldBeTrue();
                    type.ShouldEqual(request.Type);
                    arg3.ShouldEqual(request.MemberTypes);
                    arg4.ShouldEqual(request.Flags);
                    arg5.ShouldEqual(DefaultMetadata);
                    return members;
                }
            };
            var provider = new TestMemberProviderComponent
            {
                TryGetMembers = (type, s, arg3) =>
                {
                    ++providerCount;
                    type.ShouldEqual(request.Type);
                    arg3.ShouldEqual(DefaultMetadata);
                    return members;
                }
            };
            var component = new MemberManagerComponent();
            manager.AddComponent(selector);
            manager.AddComponent(provider);
            manager.AddComponent(component);

            manager.GetMembers(request, DefaultMetadata).ToArray().SequenceEqual(members).ShouldBeTrue();
            selectorCount.ShouldEqual(1);
            providerCount.ShouldEqual(1);
        }

        #endregion
    }
}