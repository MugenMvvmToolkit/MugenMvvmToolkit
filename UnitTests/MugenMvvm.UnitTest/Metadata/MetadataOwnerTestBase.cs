using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Metadata
{
    public abstract class MetadataOwnerTestBase : UnitTestBase
    {
        #region Methods

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public virtual void HasMetadataShouldReturnCorrectValue(bool emptyValue)
        {
            var context = emptyValue ? DefaultMetadata : new SingleValueMetadataContext(MetadataContextKey.FromKey<object?, object>("test").ToValue(""));
            var owner = GetMetadataOwner(context);
            owner.HasMetadata.ShouldEqual(!emptyValue);
        }

        [Fact]
        public virtual void MetadataShouldReturnInputValue()
        {
            var context = new MetadataContext();
            var owner = GetMetadataOwner(context);
            owner.Metadata.ShouldEqual(context);
        }

        protected abstract IMetadataOwner<IMetadataContext> GetMetadataOwner(IReadOnlyMetadataContext? metadata);

        #endregion
    }
}