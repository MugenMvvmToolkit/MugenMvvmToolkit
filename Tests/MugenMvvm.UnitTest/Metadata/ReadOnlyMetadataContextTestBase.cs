using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using Should;

namespace MugenMvvm.UnitTest.Metadata
{
    public abstract class ReadOnlyMetadataContextTestBase : UnitTestBase
    {
        #region Fields

        protected static readonly IMetadataContextKey<string?> CustomGetterKey = MetadataContextKey
            .Create<string?>(nameof(CustomGetterKey))
            .Getter(Getter)
            .Build();

        protected static readonly IMetadataContextKey<int> CustomSetterKey = MetadataContextKey
            .Create<int>(nameof(CustomSetterKey))
            .Setter(Setter)
            .Build();

        protected const string DefaultGetterValue = "Testf";

        #endregion

        #region Properties

        protected static IReadOnlyMetadataContext? CurrentGetterContext { get; set; }
        protected static object? CurrentGetterValue { get; set; }
        protected static int GetterCount { get; set; }
        protected static string? GetterValue { get; set; }

        protected static IReadOnlyMetadataContext? CurrentSetterContext { get; set; }
        protected static int CurrentSetterValue { get; set; }
        protected static int SetterCount { get; set; }
        protected static object? CurrentSetterOldValue { get; set; }
        protected static object? SetterValue { get; set; }

        #endregion

        #region Methods

        private static string? Getter(IReadOnlyMetadataContext arg1, IMetadataContextKey<string?> arg2, object? arg3)
        {
            ++GetterCount;
            arg2.ShouldEqual(CustomGetterKey);
            CurrentGetterContext = arg1;
            CurrentGetterValue = arg3;
            return GetterValue;
        }

        private static object? Setter(IReadOnlyMetadataContext arg1, IMetadataContextKey<int> arg2, object? arg3, int arg4)
        {
            ++SetterCount;
            arg2.ShouldEqual(CustomSetterKey);
            CurrentSetterOldValue = arg3;
            CurrentSetterContext = arg1;
            CurrentSetterValue = arg4;
            return SetterValue;
        }

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

        public void TryGetGetterTest(IReadOnlyMetadataContext metadataContext)
        {
            const string getterValueToSet = "Test";
            GetterCount = 0;
            GetterValue = getterValueToSet;

            metadataContext.TryGet(CustomGetterKey, out var value).ShouldBeTrue();
            GetterCount.ShouldEqual(1);
            CurrentGetterContext.ShouldEqual(metadataContext);
            CurrentGetterValue.ShouldEqual(DefaultGetterValue);
            value.ShouldEqual(getterValueToSet);
        }

        public void TryGetDefaultTest(IReadOnlyMetadataContext metadataContext)
        {
            const string defaultValue = "Test1";
            const string defaultValueGet = "t";
            var contextKey = MetadataContextKey.Create<string>("Test").DefaultValue(defaultValue).Build();
            metadataContext.TryGet(contextKey!, out var value).ShouldBeFalse();
            value.ShouldEqual(defaultValue);

            metadataContext.TryGet(contextKey!, out value, defaultValueGet).ShouldBeFalse();
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