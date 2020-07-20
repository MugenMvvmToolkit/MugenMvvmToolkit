using System.Collections.Generic;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Metadata.Components
{
    public class MetadataContextProviderComponentTest : ReadOnlyMetadataContextTestBase
    {
        #region Methods

        [Fact]
        public void TryGetReadOnlyMetadataContextShouldReturnMetadataContextDefault()
        {
            var component = new MugenMvvm.Metadata.Components.MetadataContextProviderComponent();
            var context = component.TryGetReadOnlyMetadataContext(null!, this, default)!;
            context.ShouldEqual(Default.Metadata);
            EnumeratorCountTest(context, new List<KeyValuePair<IMetadataContextKey, object?>>());
            ContainsTest(context, new List<KeyValuePair<IMetadataContextKey, object?>>());
        }

        [Fact]
        public void TryGetReadOnlyMetadataContextShouldReturnMetadataContextSingleValue()
        {
            const int intValue = 1;
            var contextKey = MetadataContextKey.FromKey<int, int>(intValue.ToString());
            var value = contextKey.ToValue(intValue);
            var component = new MugenMvvm.Metadata.Components.MetadataContextProviderComponent();
            var context = component.TryGetReadOnlyMetadataContext(null!, this, value)!;
            EnumeratorCountTest(context, new List<KeyValuePair<IMetadataContextKey, object?>> { value });
            ContainsTest(context, new List<KeyValuePair<IMetadataContextKey, object?>> { value });
            TryGetTest(context, contextKey, intValue);
        }

        [Fact]
        public void TryGetReadOnlyMetadataContextShouldReturnMetadataContextList()
        {
            var values = new List<KeyValuePair<IMetadataContextKey, object?>>();
            var keyValues = new List<(IMetadataContextKey<int, int>, int)>();
            for (var i = 0; i < 10; i++)
            {
                var contextKey = MetadataContextKey.FromKey<int, int>(i.ToString());
                var value = contextKey.ToValue(i);
                values.Add(value);
                keyValues.Add((contextKey, i));
            }

            var component = new MugenMvvm.Metadata.Components.MetadataContextProviderComponent();
            var context = component.TryGetReadOnlyMetadataContext(null!, this, values)!;
            EnumeratorCountTest(context, values);
            ContainsTest(context, values);
            foreach (var valueTuple in keyValues)
                TryGetTest(context, valueTuple.Item1, valueTuple.Item2);
        }

        [Fact]
        public void TryGetMetadataContextShouldReturnMetadataContext()
        {
            var values = new List<KeyValuePair<IMetadataContextKey, object?>>();
            var keyValues = new List<(IMetadataContextKey<int, int>, int)>();
            for (var i = 0; i < 10; i++)
            {
                var contextKey = MetadataContextKey.FromKey<int, int>(i.ToString());
                var value = contextKey.ToValue(i);
                values.Add(value);
                keyValues.Add((contextKey, i));
            }

            var component = new MugenMvvm.Metadata.Components.MetadataContextProviderComponent();
            var context = component.TryGetMetadataContext(null!, this, values)!;
            EnumeratorCountTest(context, values);
            ContainsTest(context, values);
            foreach (var valueTuple in keyValues)
                TryGetTest(context, valueTuple.Item1, valueTuple.Item2);
        }

        #endregion
    }
}