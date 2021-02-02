using MugenMvvm.Serialization;
using MugenMvvm.Serialization.Components;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Serialization.Components
{
    public class SurrogateProviderResolverTest : UnitTestBase
    {
        private readonly SurrogateProviderResolver _resolver;

        public SurrogateProviderResolverTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _resolver = new SurrogateProviderResolver();
        }

        [Fact]
        public void TryGetSurrogateProviderShouldAddRemoveResolve()
        {
            var provider = new DelegateSurrogateProvider<string, object>((s, context) => null, (o, context) => null);

            _resolver.Add(provider);
            _resolver.TryGetSurrogateProvider(null!, typeof(string), null).ShouldEqual(provider);

            _resolver.Remove(typeof(string));
            _resolver.TryGetSurrogateProvider(null!, typeof(string), null).ShouldBeNull();

            _resolver.Add(typeof(UnitTestBase), provider);
            _resolver.TryGetSurrogateProvider(null!, typeof(UnitTestBase), null).ShouldEqual(provider);
            _resolver.TryGetSurrogateProvider(null!, typeof(SurrogateProviderResolverTest), null).ShouldEqual(provider);

            _resolver.Remove(typeof(UnitTestBase));
            _resolver.TryGetSurrogateProvider(null!, typeof(UnitTestBase), null).ShouldBeNull();
            _resolver.TryGetSurrogateProvider(null!, typeof(SurrogateProviderResolverTest), null).ShouldBeNull();
        }

        [Fact]
        public void TryGetSurrogateProviderShouldReturnNullEmpty() => new SurrogateProviderResolver().TryGetSurrogateProvider(null!, typeof(string), null).ShouldBeNull();
    }
}