using System.Collections.Generic;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using Xunit;

namespace MugenMvvm.UnitTests.Metadata
{
    public class SingleValueMetadataContextTest : ReadOnlyMetadataContextTestBase
    {
        [Fact]
        public void TryGetShouldUseCustomGetter()
        {
            var context = new SingleValueMetadataContext(CustomGetterKey.ToValue(DefaultGetterValue));
            TryGetGetterTest(context);
        }

        [Fact]
        public void TryGetShouldUseDefaultValues()
        {
            var context = new SingleValueMetadataContext(MetadataContextKey.FromKey<int>("t").ToValue(1));
            TryGetDefaultTest(context);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ConstructorShouldInitializeContext(int intValue)
        {
            var contextKey = MetadataContextKey.FromKey<int>(intValue.ToString());
            var value = contextKey.ToValue(intValue);
            var context = new SingleValueMetadataContext(value);
            EnumeratorCountTest(context, new List<KeyValuePair<IMetadataContextKey, object?>> {value});
            ContainsTest(context, new List<KeyValuePair<IMetadataContextKey, object?>> {value});
            TryGetTest(context, contextKey, intValue);
        }
    }
}