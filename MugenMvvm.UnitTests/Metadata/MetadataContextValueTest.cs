using System.Collections.Generic;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using Xunit;

namespace MugenMvvm.UnitTests.Metadata
{
    public class MetadataContextValueTest : ReadOnlyMetadataContextTestBase
    {
        [Fact]
        public void CreateShouldCreateMetadataFromKeyValuePair()
        {
            var contextKey = MetadataContextKey.FromKey<int>("test");
            var value = new KeyValuePair<IMetadataContextKey, object?>(contextKey, 1);
            var context = contextKey.ToContext(1);
            EnumeratorCountTest(context, new List<KeyValuePair<IMetadataContextKey, object?>> {value});
            ContainsTest(context, new List<KeyValuePair<IMetadataContextKey, object?>> {value});
            TryGetTest(context, contextKey, 1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ToContextShouldCreateSingleValueContext(int intValue)
        {
            var contextKey = MetadataContextKey.FromKey<int>(intValue.ToString());
            var value = new KeyValuePair<IMetadataContextKey, object?>(contextKey, intValue);
            var context = contextKey.ToContext(intValue);
            EnumeratorCountTest(context, new List<KeyValuePair<IMetadataContextKey, object?>> {value});
            ContainsTest(context, new List<KeyValuePair<IMetadataContextKey, object?>> {value});
            TryGetTest(context, contextKey, intValue);
        }
    }
}