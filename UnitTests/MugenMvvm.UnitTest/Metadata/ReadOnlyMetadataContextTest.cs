using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using Xunit;

namespace MugenMvvm.UnitTest.Metadata
{
    public class ReadOnlyMetadataContextTest : ReadOnlyMetadataContextTestBase
    {
        #region Methods

        [Fact]
        public void TryGetShouldUseCustomGetter()
        {
            var context = new ReadOnlyMetadataContext(new[] { MetadataContextValue.Create(CustomGetterKey, DefaultGetterValue) });
            TryGetGetterTest(context);
        }

        [Fact]
        public void TryGetShouldUseDefaultValues()
        {
            var context = new ReadOnlyMetadataContext(new MetadataContextValue[0]);
            TryGetDefaultTest(context);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ConstructorShouldInitializeContext(int count)
        {
            var values = new List<MetadataContextValue>();
            var keyValues = new List<(IMetadataContextKey<int, int>, int)>();
            for (var i = 0; i < count; i++)
            {
                var contextKey = MetadataContextKey.FromKey<int, int>(i.ToString());
                var value = MetadataContextValue.Create(contextKey, i);
                values.Add(value);
                keyValues.Add((contextKey, i));
            }

            var context = new ReadOnlyMetadataContext(values);
            EnumeratorCountTest(context, values);
            ContainsTest(context, values);
            foreach (var valueTuple in keyValues)
                TryGetTest(context, valueTuple.Item1, valueTuple.Item2);
        }

        #endregion
    }
}