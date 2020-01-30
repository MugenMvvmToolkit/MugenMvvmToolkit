using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using Should;

namespace MugenMvvm.UnitTest.Metadata
{
    public abstract class ReadOnlyMetadataContextTestBase : UnitTestBase
    {
        #region Methods

        protected void EnumeratorCountTest(IReadOnlyMetadataContext metadataContext, List<MetadataContextValue> values)
        {
            metadataContext.Count.ShouldEqual(values.Count);
            metadataContext.SequenceEqual(values).ShouldBeTrue();
        }

        public void ContainsTest(IReadOnlyMetadataContext metadataContext, List<MetadataContextValue> values)
        {
            foreach (var metadataContextValue in values)
                metadataContext.Contains(metadataContextValue.ContextKey);
        }

        public void TryGetTest<T>(IReadOnlyMetadataContext context, IMetadataContextKey<T> key, T expectedValue)
        {
            context.TryGet(key, out var value).ShouldBeTrue();
            value.ShouldEqual(expectedValue);
        }

        public void TryGetDefaultTest(IReadOnlyMetadataContext metadataContext)
        {
            const string defaultValue = "Test1";
            const string defaultValueGet = "t";
            var contextKey = MetadataContextKey.Create<string>("Test").DefaultValue(defaultValue).Build();
            metadataContext.TryGet(contextKey, out var value).ShouldBeFalse();
            value.ShouldEqual(defaultValue);

            metadataContext.TryGet(contextKey, out value, defaultValueGet).ShouldBeFalse();
            value.ShouldEqual(defaultValue);

            var invokedCount = 0;
            contextKey = MetadataContextKey.Create<string>("Test").DefaultValue((context, key, arg3) =>
            {
                ++invokedCount;
                context.ShouldEqual(metadataContext);
                key.ShouldEqual(contextKey);
                arg3.ShouldEqual(defaultValueGet);
                return defaultValue;
            }).Build();

            metadataContext.TryGet(contextKey, out value, defaultValueGet).ShouldBeFalse();
            value.ShouldEqual(defaultValue);
            invokedCount.ShouldEqual(1);
        }

        #endregion
    }
}