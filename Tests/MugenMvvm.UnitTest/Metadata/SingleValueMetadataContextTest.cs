using System.Collections.Generic;
using MugenMvvm.Metadata;
using Xunit;

namespace MugenMvvm.UnitTest.Metadata
{
    public class SingleValueMetadataContextTest : ReadOnlyMetadataContextTestBase
    {
        #region Methods

        [Fact]
        public void TryGetShouldUseDefaultValues()
        {
            var context = new SingleValueMetadataContext(MetadataContextValue.Create(MetadataContextKey.FromKey<int>("t"), 1));
            TryGetDefaultTest(context);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ConstructorShouldInitializeContext(int intValue)
        {
            var contextKey = MetadataContextKey.FromKey<int>(intValue.ToString());
            var value = MetadataContextValue.Create(contextKey, intValue);
            var context = new SingleValueMetadataContext(value);
            EnumeratorCountTest(context, new List<MetadataContextValue> {value});
            ContainsTest(context, new List<MetadataContextValue> {value});
            TryGetTest(context, contextKey, intValue);
        }

        #endregion
    }
}