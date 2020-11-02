using System.Linq;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Members;
using MugenMvvm.Bindings.Members.Components;
using MugenMvvm.Extensions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Members.Components
{
    public class ExtensionMethodMemberProviderTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMembersShouldReturnExtensionMethodsAddRemove()
        {
            var memberManager = new MemberManager();
            var provider = new ExtensionMethodMemberProvider();
            memberManager.AddComponent(provider);
            provider.Add(typeof(ExtensionMethodMemberProviderComponentExtTest));

            provider.TryGetMembers(null!, typeof(string), nameof(Enumerable.FirstOrDefault), MemberType.Accessor, DefaultMetadata).IsEmpty.ShouldBeTrue();

            var method = typeof(ExtensionMethodMemberProviderComponentExtTest).GetMethod(nameof(ExtensionMethodMemberProviderComponentExtTest.Method), new[] {typeof(string)});
            var itemOrList = provider.TryGetMembers(null!, typeof(string), nameof(ExtensionMethodMemberProviderComponentExtTest.Method), MemberType.Method, DefaultMetadata);
            itemOrList.Item.UnderlyingMember.ShouldEqual(method);

            method = typeof(ExtensionMethodMemberProviderComponentExtTest).GetMethod(nameof(ExtensionMethodMemberProviderComponentExtTest.Method), new[] {typeof(int)});
            itemOrList = provider.TryGetMembers(null!, typeof(int), nameof(ExtensionMethodMemberProviderComponentExtTest.Method), MemberType.Method, DefaultMetadata);
            itemOrList.Item.UnderlyingMember.ShouldEqual(method);

            provider.Remove(typeof(ExtensionMethodMemberProviderComponentExtTest));
            itemOrList = provider.TryGetMembers(null!, typeof(int), nameof(ExtensionMethodMemberProviderComponentExtTest.Method), MemberType.Method, DefaultMetadata);
            itemOrList.IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void TryGetMembersShouldReturnExtensionMethodsGeneric()
        {
            var memberManager = new MemberManager();
            var provider = new ExtensionMethodMemberProvider();
            memberManager.AddComponent(provider);
            provider.Add(typeof(ExtensionMethodMemberProviderComponentExtTest));

            provider.TryGetMembers(null!, typeof(string), nameof(Enumerable.FirstOrDefault), MemberType.Accessor, DefaultMetadata).IsEmpty.ShouldBeTrue();

            var members = provider.TryGetMembers(null!, typeof(string), nameof(Enumerable.FirstOrDefault), MemberType.Method, DefaultMetadata);
            members.Count.ShouldEqual(2);

            var methodInfos = typeof(Enumerable)
                .GetMethods()
                .Where(info => info.Name == nameof(Enumerable.FirstOrDefault))
                .Select(info => info.MakeGenericMethod(typeof(char)))
                .ToList();
            methodInfos.ShouldEqual(members.AsList().Select(info => info.UnderlyingMember));
        }

        #endregion
    }

    public static class ExtensionMethodMemberProviderComponentExtTest
    {
        #region Methods

        public static void Method(this string value)
        {
        }

        public static void Method(this int value)
        {
        }

        #endregion
    }
}