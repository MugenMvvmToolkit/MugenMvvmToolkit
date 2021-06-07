using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Metadata.Components;
using MugenMvvm.Metadata;
using MugenMvvm.Tests.Metadata;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Metadata
{
    [Collection(SharedContext)]
    public class MetadataContextTest : ReadOnlyMetadataContextTestBase
    {
        protected IMetadataContextKey<int> TestKey = MetadataContextKey.FromKey<int>(nameof(TestKey));

        public MetadataContextTest()
        {
            RegisterDisposeToken(WithGlobalService(ComponentCollectionManager));
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
            context.AddOrUpdate(CustomSetterKey, this, (metadataContext, key, test) => 0, (_, __, ___, ____) => throw new NotSupportedException()).ShouldEqual(int.MaxValue);
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
            context.AddOrUpdate(CustomSetterKey, this, (metadataContext, key, test) => 0, (item, value, currentValue, state) => 0).ShouldEqual(newValue);
            context.TryGet(CustomSetterKey, out var v).ShouldBeTrue();
            v.ShouldEqual(newValue);
            SetterCount.ShouldEqual(1);
            CurrentSetterContext.ShouldEqual(context);
            CurrentSetterOldValue.ShouldEqual(oldValue);
        }

        [Fact]
        public void ConstructorShouldInitializeContext3()
        {
            var values = new List<KeyValuePair<IMetadataContextKey, object?>>();
            var context = new MetadataContext(default(ItemOrIReadOnlyList<KeyValuePair<IMetadataContextKey, object?>>));
            EnumeratorCountTest(context, values);
            ContainsTest(context, values);
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
            context.GetOrAdd(CustomSetterKey, this, (metadataContext, key, test) => 0).ShouldEqual(int.MaxValue);
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

        [Fact]
        public void TryGetShouldUseCustomGetter()
        {
            var context = new MetadataContext(new ItemOrIReadOnlyList<KeyValuePair<IMetadataContextKey, object?>>(CustomGetterKey.ToValue(DefaultGetterValue), true));
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
            var values = new List<KeyValuePair<IMetadataContextKey, object?>>();
            var keyValues = new List<(IMetadataContextKey<int>, int)>();
            for (var i = 0; i < count; i++)
            {
                var contextKey = MetadataContextKey.FromKey<int>(i.ToString());
                var value = contextKey.ToValue(i);
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
            var values = new List<KeyValuePair<IMetadataContextKey, object?>>();
            var keyValues = new List<(IMetadataContextKey<int>, int)>();
            for (var i = 0; i < count; i++)
            {
                var contextKey = MetadataContextKey.FromKey<int>(i.ToString());
                var value = contextKey.ToValue(i);
                values.Add(value);
                keyValues.Add((contextKey, i));
            }

            var context = new MetadataContext((ItemOrIReadOnlyList<KeyValuePair<IMetadataContextKey, object?>>)values);
            EnumeratorCountTest(context, values);
            ContainsTest(context, values);
            foreach (var valueTuple in keyValues)
                TryGetTest(context, valueTuple.Item1, valueTuple.Item2);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ConstructorShouldInitializeContext4(int intValue)
        {
            var contextKey = MetadataContextKey.FromKey<int>(intValue.ToString());
            var value = contextKey.ToValue(intValue);
            var context = new MetadataContext(value);
            EnumeratorCountTest(context, new List<KeyValuePair<IMetadataContextKey, object?>> { value });
            ContainsTest(context, new List<KeyValuePair<IMetadataContextKey, object?>> { value });
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
                context.AddComponent(new TestMetadataContextListener()
                {
                    OnAdded = (c, key, arg3) =>
                    {
                        ++listenerInvokedCount;
                        c.ShouldEqual(context);
                        key.ShouldEqual(TestKey);
                        arg3.ShouldEqual(int.MaxValue);
                    },
                    OnChanged = (_, _, _, _) => throw new NotSupportedException(),
                    OnRemoved = (_, _, _) => throw new NotSupportedException()
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
                context.AddComponent(new TestMetadataContextListener
                {
                    OnAdded = (c, key, arg3) =>
                    {
                        ++listenerInvokedCount;
                        c.ShouldEqual(context);
                        key.ShouldEqual(TestKey);
                        arg3.ShouldEqual(int.MaxValue);
                    },
                    OnChanged = (_, _, _, _) => throw new NotSupportedException(),
                    OnRemoved = (_, _, _) => throw new NotSupportedException()
                });
            }

            context.AddOrUpdate(TestKey, this, (metadataContext, key, test) =>
            {
                ++invokeCount;
                key.ShouldEqual(TestKey);
                test.ShouldEqual(this);
                metadataContext.ShouldEqual(context);
                return int.MaxValue;
            }, (_, __, ___, ____) => throw new NotSupportedException()).ShouldEqual(int.MaxValue);
            context.TryGet(TestKey, out var v).ShouldBeTrue();
            invokeCount.ShouldEqual(1);
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
                context.AddComponent(new TestMetadataContextListener
                {
                    OnChanged = (c, key, oldV, newV) =>
                    {
                        ++listenerInvokedCount;
                        c.ShouldEqual(context);
                        key.ShouldEqual(TestKey);
                        oldV.ShouldEqual(oldValue);
                        newV.ShouldEqual(newValue);
                    },
                    OnAdded = (_, _, _) => throw new NotSupportedException(),
                    OnRemoved = (_, _, _) => throw new NotSupportedException()
                });
            }

            context.AddOrUpdate(TestKey, newValue, this, (item, key, currentValue, state) =>
            {
                ++invokeCount;
                key.ShouldEqual(TestKey);
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
                context.AddComponent(new TestMetadataContextListener
                {
                    OnChanged = (c, key, oldV, newV) =>
                    {
                        ++listenerInvokedCount;
                        c.ShouldEqual(context);
                        key.ShouldEqual(TestKey);
                        oldV.ShouldEqual(oldValue);
                        newV.ShouldEqual(newValue);
                    },
                    OnAdded = (_, _, _) => throw new NotSupportedException(),
                    OnRemoved = (_, _, _) => throw new NotSupportedException()
                });
            }

            context.AddOrUpdate(TestKey, this, (metadataContext, key, test) =>
            {
                metadataContext.ShouldEqual(context);
                key.ShouldEqual(TestKey);
                test.ShouldEqual(this);
                return newValue;
            }, (item, key, currentValue, state) =>
            {
                ++invokeCount;
                key.ShouldEqual(TestKey);
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
        public void GetOrAddShouldAddNewValue1(bool addListener)
        {
            var context = GetMetadataContext();
            var listenerInvokedCount = 0;
            if (addListener)
            {
                context.AddComponent(new TestMetadataContextListener
                {
                    OnAdded = (c, key, arg3) =>
                    {
                        ++listenerInvokedCount;
                        c.ShouldEqual(context);
                        key.ShouldEqual(TestKey);
                        arg3.ShouldEqual(int.MaxValue);
                    },
                    OnChanged = (_, _, _, _) => throw new NotSupportedException(),
                    OnRemoved = (_, _, _) => throw new NotSupportedException()
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
                context.AddComponent(new TestMetadataContextListener
                {
                    OnAdded = (c, key, arg3) =>
                    {
                        ++listenerInvokedCount;
                        c.ShouldEqual(context);
                        key.ShouldEqual(TestKey);
                        arg3.ShouldEqual(int.MaxValue);
                    },
                    OnChanged = (_, _, _, _) => throw new NotSupportedException(),
                    OnRemoved = (_, _, _) => throw new NotSupportedException()
                });
            }

            context.GetOrAdd(TestKey, this, (metadataContext, key, test) =>
            {
                ++invokeCount;
                key.ShouldEqual(TestKey);
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
                context.AddComponent(new TestMetadataContextListener
                {
                    OnAdded = (_, _, _) => throw new NotSupportedException(),
                    OnChanged = (_, _, _, _) => throw new NotSupportedException(),
                    OnRemoved = (_, _, _) => throw new NotSupportedException()
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
                context.AddComponent(new TestMetadataContextListener
                {
                    OnAdded = (_, _, _) => throw new NotSupportedException(),
                    OnChanged = (_, _, _, _) => throw new NotSupportedException(),
                    OnRemoved = (_, _, _) => throw new NotSupportedException()
                });
            }

            context.GetOrAdd(TestKey, this, (metadataContext, key, test) => throw new NotSupportedException()).ShouldEqual(int.MinValue);
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
                context.AddComponent(new TestMetadataContextListener
                {
                    OnAdded = (c, key, arg3) =>
                    {
                        ++listenerInvokedCount;
                        c.ShouldEqual(context);
                        key.ShouldEqual(TestKey);
                        arg3.ShouldEqual(int.MinValue);
                    },
                    OnChanged = (_, _, _, _) => throw new NotSupportedException(),
                    OnRemoved = (_, _, _) => throw new NotSupportedException()
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
                context.RemoveComponents<IMetadataContextListener>();
                context.AddComponent(new TestMetadataContextListener
                {
                    OnChanged = (c, key, oldV, newV) =>
                    {
                        ++listenerInvokedCount;
                        c.ShouldEqual(context);
                        key.ShouldEqual(TestKey);
                        oldV.ShouldEqual(int.MinValue);
                        newV.ShouldEqual(int.MaxValue);
                    },
                    OnAdded = (_, _, _) => throw new NotSupportedException(),
                    OnRemoved = (_, _, _) => throw new NotSupportedException()
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
            var values = new List<KeyValuePair<IMetadataContextKey, object?>>();
            var keyValues = new List<(IMetadataContextKey<int>, int)>();

            var listenerInvokedCount = 0;
            if (addListener)
            {
                context.AddComponent(new TestMetadataContextListener
                {
                    OnAdded = (c, key, arg3) =>
                    {
                        ++listenerInvokedCount;
                        c.ShouldEqual(context);
                        keyValues.Any(tuple => tuple.Item1.Equals(key)).ShouldBeTrue();
                        keyValues.Any(tuple => arg3!.Equals(tuple.Item2)).ShouldBeTrue();
                    },
                    OnChanged = (_, _, _, _) => throw new NotSupportedException(),
                    OnRemoved = (_, _, _) => throw new NotSupportedException()
                });
            }

            for (var i = 0; i < count; i++)
            {
                var contextKey = MetadataContextKey.FromKey<int>(i.ToString());
                var value = contextKey.ToValue(i);
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
                context.RemoveComponents<IMetadataContextListener>();
                context.AddComponent(new TestMetadataContextListener
                {
                    OnChanged = (c, key, oldV, newV) =>
                    {
                        ++listenerInvokedCount;
                        c.ShouldEqual(context);
                        keyValues.Any(tuple => tuple.Item1.Equals(key)).ShouldBeTrue();
                        oldValues.Any(tuple => oldV!.Equals(tuple.Item2)).ShouldBeTrue();
                        keyValues.Any(tuple => newV!.Equals(tuple.Item2)).ShouldBeTrue();
                    },
                    OnAdded = (_, _, _) => throw new NotSupportedException(),
                    OnRemoved = (_, _, _) => throw new NotSupportedException()
                });
            }

            values.Clear();
            keyValues.Clear();
            for (var i = 0; i < count; i++)
            {
                var contextKey = MetadataContextKey.FromKey<int>(i.ToString());
                var value = contextKey.ToValue(-i);
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
            var values = new List<KeyValuePair<IMetadataContextKey, object?>>();
            var keyValues = new List<(IMetadataContextKey<int>, int)>();
            for (var i = 0; i < count; i++)
            {
                var contextKey = MetadataContextKey.FromKey<int>(i.ToString());
                var value = contextKey.ToValue(i);
                values.Add(value);
                keyValues.Add((contextKey, i));
            }

            var context = GetMetadataContext(values);
            var listenerInvokedCount = 0;
            (IMetadataContextKey<int>, int) currentValue = default;
            if (addListener)
            {
                context.AddComponent(new TestMetadataContextListener
                {
                    OnChanged = (_, _, _, _) => throw new NotSupportedException(),
                    OnAdded = (_, _, _) => throw new NotSupportedException(),
                    OnRemoved = (c, key, arg3) =>
                    {
                        ++listenerInvokedCount;
                        c.ShouldEqual(context);
                        currentValue.Item1.ShouldEqual(key);
                        arg3.ShouldEqual(currentValue.Item2);
                    }
                });
            }

            for (var i = 0; i < count; i++)
            {
                currentValue = keyValues[0];
                context.Remove(keyValues[0].Item1, out var old).ShouldBeTrue();
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
            var values = new List<KeyValuePair<IMetadataContextKey, object?>>();
            var keyValues = new List<(IMetadataContextKey<int>, int)>();
            for (var i = 0; i < count; i++)
            {
                var contextKey = MetadataContextKey.FromKey<int>(i.ToString());
                var value = contextKey.ToValue(i);
                values.Add(value);
                keyValues.Add((contextKey, i));
            }

            var context = GetMetadataContext(values);
            var listenerInvokedCount = 0;
            if (addListener)
            {
                context.AddComponent(new TestMetadataContextListener
                {
                    OnChanged = (_, _, _, _) => throw new NotSupportedException(),
                    OnAdded = (_, _, _) => throw new NotSupportedException(),
                    OnRemoved = (c, key, arg3) =>
                    {
                        ++listenerInvokedCount;
                        c.ShouldEqual(context);
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

        protected virtual MetadataContext GetMetadataContext(IReadOnlyCollection<KeyValuePair<IMetadataContextKey, object?>>? values = null) => new(values);
    }
}