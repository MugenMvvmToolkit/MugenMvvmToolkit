using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Internal;
using MugenMvvm.Metadata;
using MugenMvvm.UnitTest.Metadata.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Metadata
{
    public class MetadataContextTest : ReadOnlyMetadataContextTestBase
    {
        #region Fields

        protected IMetadataContextKey<int, int> TestKey = MetadataContextKey.FromKey<int, int>(nameof(TestKey));

        #endregion

        #region Methods

        [Fact]
        public void TryGetShouldUseCustomGetter()
        {
            var context = new MetadataContext(MetadataContextValue.Create(CustomGetterKey, DefaultGetterValue));
            TryGetGetterTest(context);
        }

        [Fact]
        public void TryGetShouldUseDefaultValues()
        {
            var context = new MetadataContext();
            TryGetDefaultTest(context);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ConstructorShouldInitializeContext1(int count)
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

            var context = GetMetadataContext(values);
            EnumeratorCountTest(context, values);
            ContainsTest(context, values);
            foreach (var valueTuple in keyValues)
                TryGetTest(context, valueTuple.Item1, valueTuple.Item2);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ConstructorShouldInitializeContext2(int count)
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

            var context = new MetadataContext((ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>>)values);
            EnumeratorCountTest(context, values);
            ContainsTest(context, values);
            foreach (var valueTuple in keyValues)
                TryGetTest(context, valueTuple.Item1, valueTuple.Item2);
        }

        [Fact]
        public void ConstructorShouldInitializeContext3()
        {
            var values = new List<MetadataContextValue>();
            var context = new MetadataContext(default(ItemOrList<MetadataContextValue, IReadOnlyCollection<MetadataContextValue>>));
            EnumeratorCountTest(context, values);
            ContainsTest(context, values);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ConstructorShouldInitializeContext4(int intValue)
        {
            var contextKey = MetadataContextKey.FromKey<int, int>(intValue.ToString());
            var value = MetadataContextValue.Create(contextKey, intValue);
            var context = new MetadataContext(value);
            EnumeratorCountTest(context, new List<MetadataContextValue> { value });
            ContainsTest(context, new List<MetadataContextValue> { value });
            TryGetTest(context, contextKey, intValue);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddOrUpdateShouldAddNewValue1(bool addListener)
        {
            var context = GetMetadataContext();
            var listenerInvokedCount = 0;
            if (addListener)
            {
                context.AddComponent(new TestMetadataContextListener(context)
                {
                    OnAdded = (key, arg3) =>
                    {
                        ++listenerInvokedCount;
                        key.ShouldEqual(TestKey);
                        arg3.ShouldEqual(int.MaxValue);
                    },
                    OnChanged = (key, arg3, arg4) => throw new NotSupportedException(),
                    OnRemoved = (key, arg3) => throw new NotSupportedException()
                });
            }

            context.AddOrUpdate(TestKey, int.MaxValue, this, (_, __, ___, ____) => throw new NotSupportedException()).ShouldEqual(int.MaxValue);
            context.TryGet(TestKey, out var v).ShouldBeTrue();
            v.ShouldEqual(int.MaxValue);
            if (addListener)
                listenerInvokedCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddOrUpdateShouldAddNewValue2(bool addListener)
        {
            var invokeCount = 0;
            var context = GetMetadataContext();
            var listenerInvokedCount = 0;
            if (addListener)
            {
                context.AddComponent(new TestMetadataContextListener(context)
                {
                    OnAdded = (key, arg3) =>
                    {
                        ++listenerInvokedCount;
                        key.ShouldEqual(TestKey);
                        arg3.ShouldEqual(int.MaxValue);
                    },
                    OnChanged = (key, arg3, arg4) => throw new NotSupportedException(),
                    OnRemoved = (key, arg3) => throw new NotSupportedException()
                });
            }

            context.AddOrUpdate(TestKey, this, (metadataContext, test) =>
            {
                ++invokeCount;
                test.ShouldEqual(this);
                metadataContext.ShouldEqual(context);
                return int.MaxValue;
            }, (_, __, ___, ____) => throw new NotSupportedException()).ShouldEqual(int.MaxValue);
            context.TryGet(TestKey, out var v).ShouldBeTrue();
            v.ShouldEqual(int.MaxValue);
            if (addListener)
                listenerInvokedCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddOrUpdateShouldUpdateOldValue1(bool addListener)
        {
            var invokeCount = 0;
            var oldValue = 100;
            var newValue = 1000;
            var context = GetMetadataContext();
            context.Set(TestKey, oldValue);
            var listenerInvokedCount = 0;
            if (addListener)
            {
                context.AddComponent(new TestMetadataContextListener(context)
                {
                    OnChanged = (key, oldV, newV) =>
                    {
                        ++listenerInvokedCount;
                        key.ShouldEqual(TestKey);
                        oldV.ShouldEqual(oldValue);
                        newV.ShouldEqual(newValue);
                    },
                    OnAdded = (key, arg3) => throw new NotSupportedException(),
                    OnRemoved = (key, arg3) => throw new NotSupportedException()
                });
            }

            context.AddOrUpdate(TestKey, newValue, this, (item, value, currentValue, state) =>
            {
                ++invokeCount;
                value.ShouldEqual(newValue);
                currentValue.ShouldEqual(oldValue);
                state.ShouldEqual(this);
                return newValue;
            }).ShouldEqual(newValue);
            context.TryGet(TestKey, out var v).ShouldBeTrue();
            v.ShouldEqual(newValue);
            invokeCount.ShouldEqual(1);
            if (addListener)
                listenerInvokedCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void AddOrUpdateShouldUpdateOldValue2(bool addListener)
        {
            var invokeCount = 0;
            var oldValue = 100;
            var newValue = 1000;
            var context = GetMetadataContext();
            context.Set(TestKey, oldValue);
            var listenerInvokedCount = 0;
            if (addListener)
            {
                context.AddComponent(new TestMetadataContextListener(context)
                {
                    OnChanged = (key, oldV, newV) =>
                    {
                        ++listenerInvokedCount;
                        key.ShouldEqual(TestKey);
                        oldV.ShouldEqual(oldValue);
                        newV.ShouldEqual(newValue);
                    },
                    OnAdded = (key, arg3) => throw new NotSupportedException(),
                    OnRemoved = (key, arg3) => throw new NotSupportedException()
                });
            }

            context.AddOrUpdate(TestKey, this, (metadataContext, test) =>
            {
                metadataContext.ShouldEqual(context);
                test.ShouldEqual(this);
                return newValue;
            }, (item, value, currentValue, state) =>
            {
                ++invokeCount;
                value(item, state).ShouldEqual(newValue);
                currentValue.ShouldEqual(oldValue);
                state.ShouldEqual(this);
                return value(item, state);
            }).ShouldEqual(newValue);
            context.TryGet(TestKey, out var v).ShouldBeTrue();
            v.ShouldEqual(newValue);
            invokeCount.ShouldEqual(1);
            if (addListener)
                listenerInvokedCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetOrAddShouldAddNewValue1(bool addListener)
        {
            var context = GetMetadataContext();
            var listenerInvokedCount = 0;
            if (addListener)
            {
                context.AddComponent(new TestMetadataContextListener(context)
                {
                    OnAdded = (key, arg3) =>
                    {
                        ++listenerInvokedCount;
                        key.ShouldEqual(TestKey);
                        arg3.ShouldEqual(int.MaxValue);
                    },
                    OnChanged = (key, arg3, arg4) => throw new NotSupportedException(),
                    OnRemoved = (key, arg3) => throw new NotSupportedException()
                });
            }

            context.GetOrAdd(TestKey, int.MaxValue).ShouldEqual(int.MaxValue);
            context.TryGet(TestKey, out var v).ShouldBeTrue();
            v.ShouldEqual(int.MaxValue);
            if (addListener)
                listenerInvokedCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetOrAddShouldAddNewValue2(bool addListener)
        {
            var invokeCount = 0;
            var context = GetMetadataContext();
            var listenerInvokedCount = 0;
            if (addListener)
            {
                context.AddComponent(new TestMetadataContextListener(context)
                {
                    OnAdded = (key, arg3) =>
                    {
                        ++listenerInvokedCount;
                        key.ShouldEqual(TestKey);
                        arg3.ShouldEqual(int.MaxValue);
                    },
                    OnChanged = (key, arg3, arg4) => throw new NotSupportedException(),
                    OnRemoved = (key, arg3) => throw new NotSupportedException()
                });
            }

            context.GetOrAdd(TestKey, this, (metadataContext, test) =>
            {
                ++invokeCount;
                metadataContext.ShouldEqual(context);
                test.ShouldEqual(this);
                return int.MaxValue;
            }).ShouldEqual(int.MaxValue);
            context.TryGet(TestKey, out var v).ShouldBeTrue();
            v.ShouldEqual(int.MaxValue);
            invokeCount.ShouldEqual(1);
            if (addListener)
                listenerInvokedCount.ShouldEqual(1);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetOrAddShouldGetOldValue1(bool addListener)
        {
            var context = GetMetadataContext();
            context.Set(TestKey, int.MinValue);
            if (addListener)
            {
                context.AddComponent(new TestMetadataContextListener(context)
                {
                    OnAdded = (key, arg3) => throw new NotSupportedException(),
                    OnChanged = (key, arg3, arg4) => throw new NotSupportedException(),
                    OnRemoved = (key, arg3) => throw new NotSupportedException()
                });
            }

            context.GetOrAdd(TestKey, int.MaxValue).ShouldEqual(int.MinValue);
            context.TryGet(TestKey, out var v).ShouldBeTrue();
            v.ShouldEqual(int.MinValue);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void GetOrAddShouldGetOldValue2(bool addListener)
        {
            var context = GetMetadataContext();
            context.Set(TestKey, int.MinValue);
            if (addListener)
            {
                context.AddComponent(new TestMetadataContextListener(context)
                {
                    OnAdded = (key, arg3) => throw new NotSupportedException(),
                    OnChanged = (key, arg3, arg4) => throw new NotSupportedException(),
                    OnRemoved = (key, arg3) => throw new NotSupportedException()
                });
            }

            context.GetOrAdd(TestKey, this, (metadataContext, test) => throw new NotSupportedException()).ShouldEqual(int.MinValue);
            context.TryGet(TestKey, out var v).ShouldBeTrue();
            v.ShouldEqual(int.MinValue);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void SetShouldAddUpdateValue(bool addListener)
        {
            var context = GetMetadataContext();
            var listenerInvokedCount = 0;
            if (addListener)
            {
                context.AddComponent(new TestMetadataContextListener(context)
                {
                    OnAdded = (key, arg3) =>
                    {
                        ++listenerInvokedCount;
                        key.ShouldEqual(TestKey);
                        arg3.ShouldEqual(int.MinValue);
                    },
                    OnChanged = (key, arg3, arg4) => throw new NotSupportedException(),
                    OnRemoved = (key, arg3) => throw new NotSupportedException()
                });
            }

            context.Set(TestKey, int.MinValue, out var old);
            old.ShouldBeNull();
            context.TryGet(TestKey, out var v).ShouldBeTrue();
            v.ShouldEqual(int.MinValue);

            if (addListener)
            {
                listenerInvokedCount.ShouldEqual(1);
                listenerInvokedCount = 0;
                context.ClearComponents<IMetadataContextListener>();
                context.AddComponent(new TestMetadataContextListener(context)
                {
                    OnChanged = (key, oldV, newV) =>
                    {
                        ++listenerInvokedCount;
                        key.ShouldEqual(TestKey);
                        oldV.ShouldEqual(int.MinValue);
                        newV.ShouldEqual(int.MaxValue);
                    },
                    OnAdded = (key, arg3) => throw new NotSupportedException(),
                    OnRemoved = (key, arg3) => throw new NotSupportedException()
                });
            }

            context.Set(TestKey, int.MaxValue, out old);
            old.ShouldEqual(int.MinValue);
            context.TryGet(TestKey, out v).ShouldBeTrue();
            v.ShouldEqual(int.MaxValue);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(1, false)]
        [InlineData(10, true)]
        [InlineData(10, false)]
        public void MergeShouldCorrectMergeValues(int count, bool addListener)
        {
            var context = GetMetadataContext();
            var values = new List<MetadataContextValue>();
            var keyValues = new List<(IMetadataContextKey<int, int>, int)>();

            var listenerInvokedCount = 0;
            if (addListener)
            {
                context.AddComponent(new TestMetadataContextListener(context)
                {
                    OnAdded = (key, arg3) =>
                    {
                        ++listenerInvokedCount;
                        keyValues.Any(tuple => tuple.Item1.Equals(key)).ShouldBeTrue();
                        keyValues.Any(tuple => arg3!.Equals(tuple.Item2)).ShouldBeTrue();
                    },
                    OnChanged = (key, arg3, arg4) => throw new NotSupportedException(),
                    OnRemoved = (key, arg3) => throw new NotSupportedException()
                });
            }

            for (var i = 0; i < count; i++)
            {
                var contextKey = MetadataContextKey.FromKey<int, int>(i.ToString());
                var value = MetadataContextValue.Create(contextKey, i);
                values.Add(value);
                keyValues.Add((contextKey, i));
            }

            context.Merge(values);
            EnumeratorCountTest(context, values);
            ContainsTest(context, values);
            foreach (var valueTuple in keyValues)
                TryGetTest(context, valueTuple.Item1, valueTuple.Item2);

            if (addListener)
            {
                var oldValues = keyValues.ToList();
                listenerInvokedCount.ShouldEqual(count);
                listenerInvokedCount = 0;
                context.ClearComponents<IMetadataContextListener>();
                context.AddComponent(new TestMetadataContextListener(context)
                {
                    OnChanged = (key, oldV, newV) =>
                    {
                        ++listenerInvokedCount;
                        keyValues.Any(tuple => tuple.Item1.Equals(key)).ShouldBeTrue();
                        oldValues.Any(tuple => oldV!.Equals(tuple.Item2)).ShouldBeTrue();
                        keyValues.Any(tuple => newV!.Equals(tuple.Item2)).ShouldBeTrue();
                    },
                    OnAdded = (key, arg3) => throw new NotSupportedException(),
                    OnRemoved = (key, arg3) => throw new NotSupportedException()
                });
            }

            values.Clear();
            keyValues.Clear();
            for (var i = 0; i < count; i++)
            {
                var contextKey = MetadataContextKey.FromKey<int, int>(i.ToString());
                var value = MetadataContextValue.Create(contextKey, -i);
                values.Add(value);
                keyValues.Add((contextKey, -i));
            }

            context.Merge(values);
            EnumeratorCountTest(context, values);
            ContainsTest(context, values);
            foreach (var valueTuple in keyValues)
                TryGetTest(context, valueTuple.Item1, valueTuple.Item2);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(1, false)]
        [InlineData(10, true)]
        [InlineData(10, false)]
        public void ClearShouldRemoveByKey(int count, bool addListener)
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

            var context = GetMetadataContext(values);
            var listenerInvokedCount = 0;
            (IMetadataContextKey<int, int>, int) currentValue = default;
            if (addListener)
            {
                context.AddComponent(new TestMetadataContextListener(context)
                {
                    OnChanged = (key, arg3, nV) => throw new NotSupportedException(),
                    OnAdded = (key, arg3) => throw new NotSupportedException(),
                    OnRemoved = (key, arg3) =>
                    {
                        ++listenerInvokedCount;
                        currentValue.Item1.ShouldEqual(key);
                        arg3.ShouldEqual(currentValue.Item2);
                    }
                });
            }

            for (var i = 0; i < count; i++)
            {
                currentValue = keyValues[0];
                context.Clear(keyValues[0].Item1, out var old).ShouldBeTrue();
                old.ShouldEqual(keyValues[0].Item2);

                values.RemoveAt(0);
                keyValues.RemoveAt(0);

                EnumeratorCountTest(context, values);
                ContainsTest(context, values);
                foreach (var valueTuple in keyValues)
                    TryGetTest(context, valueTuple.Item1, valueTuple.Item2);
            }

            if (addListener)
                listenerInvokedCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(1, false)]
        [InlineData(10, true)]
        [InlineData(10, false)]
        public void ClearShouldClearAll(int count, bool addListener)
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

            var context = GetMetadataContext(values);
            var listenerInvokedCount = 0;
            if (addListener)
            {
                context.AddComponent(new TestMetadataContextListener(context)
                {
                    OnChanged = (key, arg3, nV) => throw new NotSupportedException(),
                    OnAdded = (key, arg3) => throw new NotSupportedException(),
                    OnRemoved = (key, arg3) =>
                    {
                        ++listenerInvokedCount;
                        keyValues.Any(tuple => tuple.Item1.Equals(key)).ShouldBeTrue();
                        keyValues.Any(tuple => arg3!.Equals(tuple.Item2)).ShouldBeTrue();
                    }
                });
            }

            context.Clear();
            values.Clear();
            keyValues.Clear();

            EnumeratorCountTest(context, values);
            ContainsTest(context, values);
            foreach (var valueTuple in keyValues)
                TryGetTest(context, valueTuple.Item1, valueTuple.Item2);
            if (addListener)
                listenerInvokedCount.ShouldEqual(count);
        }

        [Fact]
        public void AddOrUpdateShouldUseSetter1()
        {
            var context = GetMetadataContext();
            SetterCount = 0;
            SetterValue = int.MaxValue;
            context.AddOrUpdate(CustomSetterKey, 0, this, (_, __, ___, ____) => throw new NotSupportedException()).ShouldEqual(int.MaxValue);
            context.TryGet(CustomSetterKey, out var v).ShouldBeTrue();
            v.ShouldEqual(int.MaxValue);
            SetterCount.ShouldEqual(1);
            CurrentSetterContext.ShouldEqual(context);
            CurrentSetterOldValue.ShouldEqual(null);
        }

        [Fact]
        public void AddOrUpdateShouldUseSetter2()
        {
            var context = GetMetadataContext();
            SetterCount = 0;
            SetterValue = int.MaxValue;
            context.AddOrUpdate(CustomSetterKey, this, (metadataContext, test) => 0, (_, __, ___, ____) => throw new NotSupportedException()).ShouldEqual(int.MaxValue);
            context.TryGet(CustomSetterKey, out var v).ShouldBeTrue();
            v.ShouldEqual(int.MaxValue);
            SetterCount.ShouldEqual(1);
            CurrentSetterContext.ShouldEqual(context);
            CurrentSetterOldValue.ShouldEqual(null);
        }

        [Fact]
        public void AddOrUpdateShouldUseSetter3()
        {
            var oldValue = 100;
            var newValue = 1000;
            var context = GetMetadataContext();
            SetterValue = oldValue;
            context.Set(CustomSetterKey, 0);
            SetterCount = 0;
            SetterValue = newValue;
            context.AddOrUpdate(CustomSetterKey, 0, this, (item, value, currentValue, state) => 0).ShouldEqual(newValue);
            context.TryGet(CustomSetterKey, out var v).ShouldBeTrue();
            v.ShouldEqual(newValue);
            SetterCount.ShouldEqual(1);
            CurrentSetterContext.ShouldEqual(context);
            CurrentSetterOldValue.ShouldEqual(oldValue);
        }

        [Fact]
        public void AddOrUpdateShouldUseSetter4()
        {
            var oldValue = 100;
            var newValue = 1000;
            var context = GetMetadataContext();
            SetterValue = oldValue;
            context.Set(CustomSetterKey, 0);
            SetterCount = 0;
            SetterValue = newValue;
            context.AddOrUpdate(CustomSetterKey, this, (metadataContext, test) => 0, (item, value, currentValue, state) => 0).ShouldEqual(newValue);
            context.TryGet(CustomSetterKey, out var v).ShouldBeTrue();
            v.ShouldEqual(newValue);
            SetterCount.ShouldEqual(1);
            CurrentSetterContext.ShouldEqual(context);
            CurrentSetterOldValue.ShouldEqual(oldValue);
        }

        [Fact]
        public void GetOrAddShouldUseSetter1()
        {
            var context = GetMetadataContext();
            SetterCount = 0;
            SetterValue = int.MaxValue;
            context.GetOrAdd(CustomSetterKey, 0).ShouldEqual(int.MaxValue);
            context.TryGet(CustomSetterKey, out var v).ShouldBeTrue();
            v.ShouldEqual(int.MaxValue);
            SetterCount.ShouldEqual(1);
            CurrentSetterContext.ShouldEqual(context);
            CurrentSetterOldValue.ShouldEqual(null);
        }

        [Fact]
        public void GetOrAddShouldUseSetter2()
        {
            var context = GetMetadataContext();
            SetterCount = 0;
            SetterValue = int.MaxValue;
            context.GetOrAdd(CustomSetterKey, this, (metadataContext, test) => 0).ShouldEqual(int.MaxValue);
            context.TryGet(CustomSetterKey, out var v).ShouldBeTrue();
            v.ShouldEqual(int.MaxValue);
            SetterCount.ShouldEqual(1);
            CurrentSetterContext.ShouldEqual(context);
            CurrentSetterOldValue.ShouldEqual(null);
        }

        [Fact]
        public void SetShouldUseSetter()
        {
            var context = GetMetadataContext();
            SetterCount = 0;
            SetterValue = int.MinValue;

            context.Set(CustomSetterKey, 0, out var old);
            old.ShouldBeNull();
            context.TryGet(CustomSetterKey, out var v).ShouldBeTrue();
            v.ShouldEqual(int.MinValue);
            SetterCount.ShouldEqual(1);
            CurrentSetterContext.ShouldEqual(context);
            CurrentSetterOldValue.ShouldEqual(null);

            SetterCount = 0;
            SetterValue = int.MaxValue;
            context.Set(CustomSetterKey, int.MaxValue, out old);
            old.ShouldEqual(int.MinValue);
            context.TryGet(CustomSetterKey, out v).ShouldBeTrue();
            v.ShouldEqual(int.MaxValue);
            SetterCount.ShouldEqual(1);
            CurrentSetterContext.ShouldEqual(context);
            CurrentSetterOldValue.ShouldEqual(int.MinValue);
        }

        protected virtual MetadataContext GetMetadataContext(IReadOnlyCollection<MetadataContextValue>? values = null)
        {
            return new MetadataContext(values);
        }

        #endregion
    }
}