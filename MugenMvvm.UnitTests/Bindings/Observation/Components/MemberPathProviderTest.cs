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
        private readonly ObservationManager _observationManager;

        public MemberPathProviderTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _observationManager = new ObservationManager(ComponentCollectionManager);
            _observationManager.AddComponent(new MemberPathProvider());
        }

        [Fact]
        public void TryGetMemberPathShouldReturnEmptyPath() => _observationManager.TryGetMemberPath("", DefaultMetadata).ShouldEqual(MemberPath.Empty);

        [Fact]
        public void TryGetMemberPathShouldReturnEmptyUnsupportedRequest() => _observationManager.TryGetMemberPath(this, DefaultMetadata).ShouldBeNull();

        [Fact]
        public void TryGetMemberPathShouldReturnMultiPath()
        {
            const string member = "Test.Test[T]";
            var path = _observationManager.TryGetMemberPath(member, DefaultMetadata)!;
            path.Path.ShouldEqual(member);
            path.Members.AsList().ShouldEqual(new[] {"Test", "Test", "[T]"});
        }

        [Fact]
        public void TryGetMemberPathShouldReturnSinglePath()
        {
            const string member = "Test";
            var path = _observationManager.TryGetMemberPath(member, DefaultMetadata)!;
            path.Path.ShouldEqual(member);
            path.Members.Item.ShouldEqual(member);
        }
    }
}