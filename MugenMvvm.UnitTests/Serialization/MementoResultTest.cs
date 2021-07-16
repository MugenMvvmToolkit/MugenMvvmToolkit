using MugenMvvm.Metadata;
using MugenMvvm.Serialization;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Serialization
{
    public class MementoResultTest : UnitTestBase
    {
        [Fact]
        public void ConstructorShouldInitializeValues1()
        {
            var target = new object();
            var mementoResult = new MementoResult(target, Metadata);
            mementoResult.IsRestored.ShouldBeTrue();
            mementoResult.Metadata.ShouldEqual(Metadata);
            mementoResult.Target.ShouldEqual(target);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConstructorShouldInitializeValues2(bool isRestored)
        {
            var mementoResult = new MementoResult(isRestored, Metadata);
            mementoResult.IsRestored.ShouldEqual(isRestored);
            mementoResult.Metadata.ShouldEqual(Metadata);
            mementoResult.Target.ShouldBeNull();
        }

        [Fact]
        public void IsRestoredShouldBeFalseDefault()
        {
            MementoResult result = default;
            result.IsRestored.ShouldBeFalse();
            result.Target.ShouldBeNull();
            result.Metadata.ShouldEqual(EmptyMetadataContext.Instance);
        }
    }
}