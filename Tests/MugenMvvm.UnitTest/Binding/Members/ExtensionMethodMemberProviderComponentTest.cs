using System.Linq;
using MugenMvvm.Binding.Members;
using MugenMvvm.Binding.Members.Components;
using MugenMvvm.Extensions;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Members
{
    public class ExtensionMethodMemberProviderComponentTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMembersShouldReturnExtensionMethodsAddRemove()
        {
            var memberManager = new MemberManager();
            var provider = new ExtensionMethodMemberProviderComponent();
            memberManager.AddComponent(provider);
            provider.Add(typeof(ExtensionMethodMemberProviderComponentExtTest));

            var method = typeof(ExtensionMethodMemberProviderComponentExtTest).GetMethod(nameof(ExtensionMethodMemberProviderComponentExtTest.Method), new[] {typeof(string)});
            var itemOrList = provider.TryGetMembers(typeof(string), nameof(ExtensionMethodMemberProviderComponentExtTest.Method), DefaultMetadata);
            itemOrList.Item.UnderlyingMember.ShouldEqual(method);

            method = typeof(ExtensionMethodMemberProviderComponentExtTest).GetMethod(nameof(ExtensionMethodMemberProviderComponentExtTest.Method), new[] {typeof(int)});
            itemOrList = provider.TryGetMembers(typeof(int), nameof(ExtensionMethodMemberProviderComponentExtTest.Method), DefaultMetadata);
            itemOrList.Item.UnderlyingMember.ShouldEqual(method);

            provider.Remove(typeof(ExtensionMethodMemberProviderComponentExtTest));
            itemOrList = provider.TryGetMembers(typeof(int), nameof(ExtensionMethodMemberProviderComponentExtTest.Method), DefaultMetadata);
            itemOrList.IsNullOrEmpty().ShouldBeTrue();
        }

        [Fact]
        public void TryGetMembersShouldReturnExtensionMethodsGeneric()
        {
            var memberManager = new MemberManager();
            var provider = new ExtensionMethodMemberProviderComponent();
            memberManager.AddComponent(provider);
            provider.Add(typeof(ExtensionMethodMemberProviderComponentExtTest));

            var members = provider.TryGetMembers(typeof(string), nameof(Enumerable.FirstOrDefault), DefaultMetadata);
            members.Count().ShouldEqual(2);

            var methodInfos = typeof(Enumerable)
                .GetMethods()
                .Where(info => info.Name == nameof(Enumerable.FirstOrDefault))
                .Select(info => info.MakeGenericMethod(typeof(char)))
                .ToList();
            methodInfos.SequenceEqual(members.ToArray().Select(info => info.UnderlyingMember)).ShouldBeTrue();
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