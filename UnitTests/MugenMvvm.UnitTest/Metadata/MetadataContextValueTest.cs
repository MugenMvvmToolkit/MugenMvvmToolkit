using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using Xunit;

namespace MugenMvvm.UnitTest.Metadata
{
    public class MetadataContextValueTest : ReadOnlyMetadataContextTestBase
    {
        #region Methods

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ToContextShouldCreateSingleValueContext(int intValue)
        {
            var contextKey = MetadataContextKey.FromKey<int, int>(intValue.ToString());
            var value = MetadataContextValue.Create(contextKey, intValue);
            var context = value.ToContext();
            EnumeratorCountTest(context, new List<MetadataContextValue> {value});
            ContainsTest(context, new List<MetadataContextValue> {value});
            TryGetTest(context, contextKey, intValue);
        }

        [Fact]
        public void CreateShouldCreateMetadataFromKeyValuePair()
        {
            var contextKey = MetadataContextKey.FromKey<int, int>("test");
            var value = MetadataContextValue.Create(new KeyValuePair<IMetadataContextKey, object?>(contextKey, 1));
            var context = value.ToContext();
            EnumeratorCountTest(context, new List<MetadataContextValue> {value});
            ContainsTest(context, new List<MetadataContextValue> {value});
            TryGetTest(context, contextKey, 1);
        }

        #endregion
    }
}