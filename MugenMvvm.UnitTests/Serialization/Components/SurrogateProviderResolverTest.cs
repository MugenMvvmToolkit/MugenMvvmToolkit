using MugenMvvm.Serialization;
using MugenMvvm.Serialization.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Serialization.Components
{
    public class SurrogateProviderResolverTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetSurrogateProviderShouldReturnNullEmpty()
        {
            var component = new SurrogateProviderResolver();
            component.TryGetSurrogateProvider(null!, typeof(string), null).ShouldBeNull();
        }

        [Fact]
        public void TryGetSurrogateProviderShouldAddRemoveResolve()
        {
            var provider = new DelegateSurrogateProvider<string, object>((s, context) => null, (o, context) => null);
            var component = new SurrogateProviderResolver();

            component.Add(provider);
            component.TryGetSurrogateProvider(null!, typeof(string), null).ShouldEqual(provider);

            component.Remove(typeof(string));
            component.TryGetSurrogateProvider(null!, typeof(string), null).ShouldBeNull();

            component.Add(typeof(UnitTestBase), provider);
            component.TryGetSurrogateProvider(null!, typeof(UnitTestBase), null).ShouldEqual(provider);
            component.TryGetSurrogateProvider(null!, typeof(SurrogateProviderResolverTest), null).ShouldEqual(provider);

            component.Remove(typeof(UnitTestBase));
            component.TryGetSurrogateProvider(null!, typeof(UnitTestBase), null).ShouldBeNull();
            component.TryGetSurrogateProvider(null!, typeof(SurrogateProviderResolverTest), null).ShouldBeNull();
        }

        #endregion
    }
}