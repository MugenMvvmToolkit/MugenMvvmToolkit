using MugenMvvm.Internal;
using MugenMvvm.Serialization;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Serialization
{
    public class MementoResultTest : UnitTestBase
    {
        #region Methods

        [Fact]
        public void IsRestoredShouldBeFalseDefault()
        {
            MementoResult result = default;
            result.IsRestored.ShouldBeFalse();
            result.Target.ShouldBeNull();
            result.Metadata.ShouldEqual(Default.Metadata);
        }

        [Fact]
        public void ConstructorShouldInitializeValues1()
        {
            var target = new object();
            var mementoResult = new MementoResult(target, DefaultMetadata);
            mementoResult.IsRestored.ShouldBeTrue();
            mementoResult.Metadata.ShouldEqual(DefaultMetadata);
            mementoResult.Target.ShouldEqual(target);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void ConstructorShouldInitializeValues2(bool isRestored)
        {
            var mementoResult = new MementoResult(isRestored, DefaultMetadata);
            mementoResult.IsRestored.ShouldEqual(isRestored);
            mementoResult.Metadata.ShouldEqual(DefaultMetadata);
            mementoResult.Target.ShouldBeNull();
        }

        #endregion
    }
}