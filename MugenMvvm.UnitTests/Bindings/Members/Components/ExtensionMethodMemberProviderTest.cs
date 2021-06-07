using System.Linq;
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
    public class ExtensionMethodMemberProviderTest : UnitTestBase
    {
        private readonly ExtensionMethodMemberProvider _provider;

        public ExtensionMethodMemberProviderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _provider = new ExtensionMethodMemberProvider();
            _provider.Add(typeof(ExtensionMethodMemberProviderComponentExtTest));
            MemberManager.AddComponent(_provider);
        }

        [Fact]
        public void TryGetMembersShouldReturnExtensionMethodsAddRemove()
        {
            _provider.TryGetMembers(MemberManager, typeof(string), nameof(Enumerable.FirstOrDefault), MemberType.Accessor, DefaultMetadata).IsEmpty.ShouldBeTrue();

            var method = typeof(ExtensionMethodMemberProviderComponentExtTest).GetMethod(nameof(ExtensionMethodMemberProviderComponentExtTest.Method), new[] { typeof(string) });
            var itemOrList = _provider.TryGetMembers(MemberManager, typeof(string), nameof(ExtensionMethodMemberProviderComponentExtTest.Method), MemberType.Method,
                DefaultMetadata);
            itemOrList.Item!.UnderlyingMember.ShouldEqual(method);

            method = typeof(ExtensionMethodMemberProviderComponentExtTest).GetMethod(nameof(ExtensionMethodMemberProviderComponentExtTest.Method), new[] { typeof(int) });
            itemOrList = _provider.TryGetMembers(MemberManager, typeof(int), nameof(ExtensionMethodMemberProviderComponentExtTest.Method), MemberType.Method, DefaultMetadata);
            itemOrList.Item!.UnderlyingMember.ShouldEqual(method);

            _provider.Remove(typeof(ExtensionMethodMemberProviderComponentExtTest));
            itemOrList = _provider.TryGetMembers(MemberManager, typeof(int), nameof(ExtensionMethodMemberProviderComponentExtTest.Method), MemberType.Method, DefaultMetadata);
            itemOrList.IsEmpty.ShouldBeTrue();
        }

        [Fact]
        public void TryGetMembersShouldReturnExtensionMethodsGeneric()
        {
            _provider.TryGetMembers(MemberManager, typeof(string), nameof(Enumerable.FirstOrDefault), MemberType.Accessor, DefaultMetadata).IsEmpty.ShouldBeTrue();

            var members = _provider.TryGetMembers(MemberManager, typeof(string), nameof(Enumerable.FirstOrDefault), MemberType.Method, DefaultMetadata);
            members.Count.ShouldEqual(2);

            var methodInfos = typeof(Enumerable)
                              .GetMethods()
                              .Where(info => info.Name == nameof(Enumerable.FirstOrDefault))
                              .Select(info => info.MakeGenericMethod(typeof(char)))
                              .ToList();
            methodInfos.ShouldEqual(members.AsList().Select(info => info.UnderlyingMember));
        }

        protected override IMemberManager GetMemberManager() => new MemberManager(ComponentCollectionManager);
    }

    public static class ExtensionMethodMemberProviderComponentExtTest
    {
        public static void Method(this string value)
        {
        }

        public static void Method(this int value)
        {
        }
    }
}