using System.Collections.Generic;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using Xunit;

namespace MugenMvvm.UnitTests.Metadata
{
    public class ReadOnlyMetadataContextTest : ReadOnlyMetadataContextTestBase
    {
        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ConstructorShouldInitializeContext(int count)
        {
            var values = new List<KeyValuePair<IMetadataContextKey, object?>>();
            var keyValues = new List<(IMetadataContextKey<int>, int)>();
            for (var i = 0; i < count; i++)
            {
                var contextKey = MetadataContextKey.FromKey<int>(i.ToString());
                var value = contextKey.ToValue(i);
                values.Add(value);
                keyValues.Add((contextKey, i));
            }

            var context = new ReadOnlyMetadataContext(values);
            EnumeratorCountTest(context, values);
            ContainsTest(context, values);
            foreach (var valueTuple in keyValues)
                TryGetTest(context, valueTuple.Item1, valueTuple.Item2);
        }

        [Fact]
        public void TryGetShouldUseCustomGetter()
        {
            var context = new ReadOnlyMetadataContext(new[] {CustomGetterKey.ToValue(DefaultGetterValue)});
            TryGetGetterTest(context);
        }

        [Fact]
        public void TryGetShouldUseDefaultValues()
        {
            var context = new ReadOnlyMetadataContext(new KeyValuePair<IMetadataContextKey, object?>[0]);
            TryGetDefaultTest(context);
        }
    }
}