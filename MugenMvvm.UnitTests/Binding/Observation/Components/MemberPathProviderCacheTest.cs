using MugenMvvm.Binding.Extensions;
using MugenMvvm.Binding.Observation;
using MugenMvvm.Binding.Observation.Components;
using MugenMvvm.Binding.Observation.Paths;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTests.Binding.Observation.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Binding.Observation.Components
{
    public class MemberPathProviderCacheTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMemberPathShouldCacheStringRequest()
        {
            var invokeCount = 0;
            var path = new SingleMemberPath("test");
            var provider = new ObservationManager();
            var decorator = new MemberPathProviderCache();
            var testPathProvider = new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (o, arg3) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(path.Path);
                    arg3.ShouldEqual(DefaultMetadata);
                    return path;
                }
            };
            provider.AddComponent(testPathProvider);
            provider.AddComponent(decorator);

            provider.GetMemberPath(path.Path, DefaultMetadata).ShouldEqual(path);
            provider.GetMemberPath(path.Path, DefaultMetadata).ShouldEqual(path);
            invokeCount.ShouldEqual(1);

            decorator.Invalidate(this, DefaultMetadata);
            invokeCount = 0;
            provider.GetMemberPath(path.Path, DefaultMetadata).ShouldEqual(path);
            provider.GetMemberPath(path.Path, DefaultMetadata).ShouldEqual(path);
            invokeCount.ShouldEqual(1);

            provider.RemoveComponent(decorator);
            decorator.TryGetMemberPath(provider, path.Path, DefaultMetadata).ShouldBeNull();
        }

        #endregion
    }
}