using System.Collections.Generic;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using MugenMvvm.Metadata.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Metadata
{
    public class MetadataContextProviderComponentTest : ReadOnlyMetadataContextTestBase
    {
        #region Methods

        [Fact]
        public void TryGetReadOnlyMetadataContextShouldReturnMetadataContextDefault()
        {
            var component = new MetadataContextProviderComponent();
            var context = component.TryGetReadOnlyMetadataContext(this, default);
            context.ShouldEqual(Default.Metadata);
            EnumeratorCountTest(context, new List<MetadataContextValue>());
            ContainsTest(context, new List<MetadataContextValue>());
        }

        [Fact]
        public void TryGetReadOnlyMetadataContextShouldReturnMetadataContextSingleValue()
        {
            const int intValue = 1;
            var contextKey = MetadataContextKey.FromKey<int>(intValue.ToString());
            var value = MetadataContextValue.Create(contextKey, intValue);
            var component = new MetadataContextProviderComponent();
            var context = component.TryGetReadOnlyMetadataContext(this, value);
            EnumeratorCountTest(context, new List<MetadataContextValue> {value});
            ContainsTest(context, new List<MetadataContextValue> {value});
            TryGetTest(context, contextKey, intValue);
        }

        [Fact]
        public void TryGetReadOnlyMetadataContextShouldReturnMetadataContextList()
        {
            var values = new List<MetadataContextValue>();
            var keyValues = new List<(IMetadataContextKey<int>, int)>();
            for (var i = 0; i < 10; i++)
            {
                var contextKey = MetadataContextKey.FromKey<int>(i.ToString());
                var value = MetadataContextValue.Create(contextKey, i);
                values.Add(value);
                keyValues.Add((contextKey, i));
            }

            var component = new MetadataContextProviderComponent();
            var context = component.TryGetReadOnlyMetadataContext(this, values);
            EnumeratorCountTest(context, values);
            ContainsTest(context, values);
            foreach (var valueTuple in keyValues)
                TryGetTest(context, valueTuple.Item1, valueTuple.Item2);
        }

        [Fact]
        public void TryGetMetadataContextShouldReturnMetadataContext()
        {
            var values = new List<MetadataContextValue>();
            var keyValues = new List<(IMetadataContextKey<int>, int)>();
            for (var i = 0; i < 10; i++)
            {
                var contextKey = MetadataContextKey.FromKey<int>(i.ToString());
                var value = MetadataContextValue.Create(contextKey, i);
                values.Add(value);
                keyValues.Add((contextKey, i));
            }

            var component = new MetadataContextProviderComponent();
            var context = component.TryGetMetadataContext(this, values);
            EnumeratorCountTest(context, values);
            ContainsTest(context, values);
            foreach (var valueTuple in keyValues)
                TryGetTest(context, valueTuple.Item1, valueTuple.Item2);
        }

        #endregion
    }
}