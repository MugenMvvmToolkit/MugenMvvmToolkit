using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Bindings.Observation.Components;
using MugenMvvm.Extensions;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Bindings.Observation.Components
{
    public class MemberPathProviderTest : UnitTestBase
    {
        public MemberPathProviderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            ObservationManager.AddComponent(new MemberPathProvider());
        }

        [Fact]
        public void TryGetMemberPathShouldReturnEmptyPath() => ObservationManager.TryGetMemberPath("", Metadata).ShouldEqual(MemberPath.Empty);

        [Fact]
        public void TryGetMemberPathShouldReturnEmptyUnsupportedRequest() => ObservationManager.TryGetMemberPath(this, Metadata).ShouldBeNull();

        [Fact]
        public void TryGetMemberPathShouldReturnMultiPath()
        {
            const string member = "Test.Test[T]";
            var path = ObservationManager.TryGetMemberPath(member, Metadata)!;
            path.Path.ShouldEqual(member);
            path.Members.ShouldEqual(new[] { "Test", "Test", "[T]" });
        }

        [Fact]
        public void TryGetMemberPathShouldReturnSinglePath()
        {
            const string member = "Test";
            var path = ObservationManager.TryGetMemberPath(member, Metadata)!;
            path.Path.ShouldEqual(member);
            path.Members.Item.ShouldEqual(member);
        }

        protected override IObservationManager GetObservationManager() => new ObservationManager(ComponentCollectionManager);
    }
}