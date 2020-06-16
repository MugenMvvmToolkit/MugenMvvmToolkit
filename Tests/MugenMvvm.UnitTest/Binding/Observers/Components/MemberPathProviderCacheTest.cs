using MugenMvvm.Binding.Observers;
using MugenMvvm.Binding.Observers.Components;
using MugenMvvm.Binding.Observers.MemberPaths;
using MugenMvvm.Extensions;
using MugenMvvm.UnitTest.Binding.Observers.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Binding.Observers.Components
{
    public class MemberPathProviderCacheTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void TryGetMemberPathShouldCacheStringRequest()
        {
            var invokeCount = 0;
            var path = new SingleMemberPath("test");
            var provider = new ObserverProvider();
            var decorator = new MemberPathProviderCache();
            var testPathProvider = new TestMemberPathProviderComponent
            {
                TryGetMemberPath = (o, type, arg3) =>
                {
                    ++invokeCount;
                    o.ShouldEqual(path.Path);
                    type.ShouldEqual(typeof(string));
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
            decorator.TryGetMemberPath(path.Path, DefaultMetadata).ShouldBeNull();
        }

        #endregion
    }
}