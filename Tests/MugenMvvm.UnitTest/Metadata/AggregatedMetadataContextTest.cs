using System.Collections.Generic;
using MugenMvvm.Metadata;
using Xunit;

namespace MugenMvvm.UnitTest.Metadata
{
    public class AggregatedMetadataContextTest : ReadOnlyMetadataContextTestBase
    {
        #region Methods

        [Fact]
        public void TryGetShouldUseCustomGetter()
        {
            var context = new AggregatedMetadataContext(new SingleValueMetadataContext(MetadataContextValue.Create(CustomGetterKey, DefaultGetterValue)));
            TryGetGetterTest(context);
        }

        [Fact]
        public void TryGetShouldUseDefaultValues()
        {
            var context = new AggregatedMetadataContext();
            TryGetDefaultTest(context);
        }

        [Fact]
        public void ConstructorShouldMergeContexts1()
        {
            var contextKey1 = MetadataContextKey.FromKey<object>("1");
            var value1 = MetadataContextValue.Create(contextKey1, 1);
            var context = new AggregatedMetadataContext(new SingleValueMetadataContext(value1));
            EnumeratorCountTest(context, new List<MetadataContextValue> { value1 });
            ContainsTest(context, new List<MetadataContextValue> { value1 });
            TryGetTest(context, contextKey1!, value1.Value);
        }

        [Fact]
        public void ConstructorShouldMergeContexts2()
        {
            var contextKey1 = MetadataContextKey.FromKey<object>("1");
            var value1 = MetadataContextValue.Create(contextKey1, 1);

            var contextKey2 = MetadataContextKey.FromKey<int>("2");
            var value2 = MetadataContextValue.Create(contextKey2, 2);

            var context = new AggregatedMetadataContext(new SingleValueMetadataContext(value1), new SingleValueMetadataContext(value2));
            EnumeratorCountTest(context, new List<MetadataContextValue> { value1, value2 });
            ContainsTest(context, new List<MetadataContextValue> { value1, value2 });
            TryGetTest(context, contextKey1!, value1.Value);
            TryGetTest(context, contextKey2!, (int)value2.Value!);
        }

        [Fact]
        public void ConstructorShouldMergeContexts3()
        {
            var contextKey1 = MetadataContextKey.FromKey<object>("1");
            var value1 = MetadataContextValue.Create(contextKey1, 1);

            var contextKey2 = MetadataContextKey.FromKey<int>("2");
            var value2 = MetadataContextValue.Create(contextKey2, 2);

            var contextKey3 = MetadataContextKey.FromKey<string>("3");
            var value3 = MetadataContextValue.Create(contextKey3, "");

            var context = new AggregatedMetadataContext(new SingleValueMetadataContext(value1), new SingleValueMetadataContext(value2), new SingleValueMetadataContext(value3));
            EnumeratorCountTest(context, new List<MetadataContextValue> { value1, value2, value3 });
            ContainsTest(context, new List<MetadataContextValue> { value1, value2, value3 });
            TryGetTest(context, contextKey1!, value1.Value);
            TryGetTest(context, contextKey2!, (int)value2.Value!);
            TryGetTest(context, contextKey3!, (string)value3.Value!);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(1, false)]
        [InlineData(10, true)]
        [InlineData(10, false)]
        public void AggregateShouldMergeItems(int count, bool toEnd)
        {
            var context = new AggregatedMetadataContext();
            var values = new List<MetadataContextValue>();
            for (var i = 0; i < count; i++)
            {
                var contextKey = MetadataContextKey.FromKey<int>(i.ToString());
                var value = MetadataContextValue.Create(contextKey, i);
                context.Aggregate(new SingleValueMetadataContext(value), toEnd);
                if (toEnd)
                    values.Add(value);
                else
                    values.Insert(0, value);

                EnumeratorCountTest(context, values);
                ContainsTest(context, values);
                TryGetTest(context, contextKey, i);
            }
        }

        #endregion
    }
}