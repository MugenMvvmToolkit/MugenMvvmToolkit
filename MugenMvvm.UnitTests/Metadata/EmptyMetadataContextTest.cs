using MugenMvvm.Extensions;
using MugenMvvm.Metadata;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Metadata
{
    public class EmptyMetadataContextTest : UnitTestBase
    {
        [Fact]
        public void InstanceShouldBeValid()
        {
            EmptyMetadataContext.Instance.Count.ShouldEqual(0);
            EmptyMetadataContext.Instance.Contains(MetadataContextKey.FromKey<object>("test")).ShouldBeFalse();
            EmptyMetadataContext.Instance.GetValues().IsEmpty.ShouldBeTrue();
            EmptyMetadataContext.Instance.TryGet(MetadataContextKey.FromKey<object>("test"), out _).ShouldBeFalse();
        }
    }
}