using MugenMvvm.Bindings.Extensions;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Components;
using MugenMvvm.Extensions;
using MugenMvvm.Tests.Bindings.Observation;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Bindings.Observation.Components
{
    public class MemberPathProviderCacheTest : UnitTestBase
    {
        [Fact]
        public void TryGetMemberPathShouldCacheStringRequest()
        {
            var invokeCount = 0;
            var path = MemberPath.Get("test");
            var decorator = new MemberPathProviderCache();
            ObservationManager.AddComponent(decorator);
            ObservationManager.AddComponent(new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (om, o, arg3) =>
                {
                    ++invokeCount;
                    om.ShouldEqual(ObservationManager);
                    o.ShouldEqual(path.Path);
                    arg3.ShouldEqual(DefaultMetadata);
                    return path;
                }
            });

            ObservationManager.GetMemberPath(path.Path, DefaultMetadata).ShouldEqual(path);
            ObservationManager.GetMemberPath(path.Path, DefaultMetadata).ShouldEqual(path);
            invokeCount.ShouldEqual(1);

            decorator.Invalidate(this, this, DefaultMetadata);
            invokeCount = 0;
            ObservationManager.GetMemberPath(path.Path, DefaultMetadata).ShouldEqual(path);
            ObservationManager.GetMemberPath(path.Path, DefaultMetadata).ShouldEqual(path);
            invokeCount.ShouldEqual(1);

            ObservationManager.RemoveComponent(decorator);
            decorator.TryGetMemberPath(ObservationManager, path.Path, DefaultMetadata).ShouldBeNull();
        }
    }
}