using System.Collections.Generic;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Metadata;
using Should;

namespace MugenMvvm.UnitTests.Metadata
{
    public abstract class ReadOnlyMetadataContextTestBase : UnitTestBase
    {
        protected const string DefaultGetterValue = "Testf";

        protected static readonly IMetadataContextKey<string?> CustomGetterKey = MetadataContextKey
                                                                                 .Create<string?>(nameof(CustomGetterKey))
                                                                                 .Getter(Getter)
                                                                                 .Build();

        protected static readonly IMetadataContextKey<int> CustomSetterKey = MetadataContextKey
                                                                             .Create<int>(nameof(CustomSetterKey))
                                                                             .Setter(Setter)
                                                                             .Build();

        protected static IReadOnlyMetadataContext? CurrentGetterContext { get; set; }

        protected static object? CurrentGetterValue { get; set; }

        protected static int GetterCount { get; set; }

        protected static string? GetterValue { get; set; }

        protected static IReadOnlyMetadataContext? CurrentSetterContext { get; set; }

        protected static int CurrentSetterValue { get; set; }

        protected static int SetterCount { get; set; }

        protected static object? CurrentSetterOldValue { get; set; }

        protected static object? SetterValue { get; set; }

        public void ContainsTest(IReadOnlyMetadataContext metadataContext, List<KeyValuePair<IMetadataContextKey, object?>> values)
        {
            foreach (var metadataContextValue in values)
                metadataContext.Contains(metadataContextValue.Key);
        }

        public void TryGetTest<T>(IReadOnlyMetadataContext context, IReadOnlyMetadataContextKey<T> key, T expectedValue)
        {
            context.TryGet(key, out var value).ShouldBeTrue();
            value!.ShouldEqual(expectedValue);

            context.TryGet(key, out var v).ShouldBeTrue();
            v!.ShouldEqual(expectedValue);
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
            var contextKey = MetadataContextKey.Create<string?>("Test").DefaultValue(defaultValue).Build();
            metadataContext.TryGet(contextKey!, out var value).ShouldBeFalse();
            value.ShouldEqual(defaultValue);

            metadataContext.TryGet(contextKey!, out value, defaultValueGet).ShouldBeFalse();
            value.ShouldEqual(defaultValue);

            var invokedCount = 0;
            contextKey = MetadataContextKey.Create<string?>("Test").DefaultValue((context, key, arg3) =>
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

        protected void EnumeratorCountTest(IReadOnlyMetadataContext metadataContext, List<KeyValuePair<IMetadataContextKey, object?>> values)
        {
            metadataContext.Count.ShouldEqual(values.Count);
            metadataContext.GetValues().ShouldEqual(values);
        }

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
    }
}