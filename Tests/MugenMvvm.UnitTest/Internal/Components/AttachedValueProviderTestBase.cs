using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Interfaces.Internal.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Internal.Components
{
    public abstract class AttachedValueProviderTestBase : UnitTestBase
    {
        #region Fields

        protected const string TestPath = "Test";

        #endregion

        #region Methods

        [Fact]
        public virtual void ShouldGetSetValues()
        {
            var item = GetSupportedItem();
            var component = GetComponent();

            component.Contains(item, TestPath).ShouldBeFalse();
            component.TryGet(item, TestPath, out AttachedValueProviderTestBase? value).ShouldBeFalse();
            value.ShouldBeNull();

            component.Set(item, TestPath, this);
            component.TryGet(item, TestPath, out value).ShouldBeTrue();
            component.Contains(item, TestPath).ShouldBeTrue();
            value.ShouldEqual(this);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public virtual void TryGetValuesShouldReturnAllValues(int count)
        {
            var item = GetSupportedItem();
            var component = GetComponent();
            var hashSet = new HashSet<KeyValuePair<string, object?>>();
            var values = new List<KeyValuePair<string, object?>>();
            for (var i = 0; i < count; i++)
            {
                var pair = new KeyValuePair<string, object?>(TestPath + i, i);
                component.Set(item, pair.Key, pair.Value);
                values.Add(pair);
            }

            component.TryGetValues(item, this, (o, pair, arg3) =>
            {
                o.ShouldEqual(item);
                arg3.ShouldEqual(this);
                hashSet.Remove(pair);
                return false;
            }).AsList().ShouldBeEmpty();
            hashSet.Count.ShouldEqual(0);
            component.TryGetValues(item, this, (o, pair, arg3) => true).AsList().SequenceEqual(values).ShouldBeTrue();
        }

        [Fact]
        public virtual void AddOrUpdateShouldAddNewValue1()
        {
            var item = GetSupportedItem();
            var component = GetComponent();
            component.AddOrUpdate(item, TestPath, this, this, (_, __, ___, ____) => throw new NotSupportedException()).ShouldEqual(this);
            component.TryGet(item, TestPath, out object? v).ShouldBeTrue();
            v.ShouldEqual(this);
        }

        [Fact]
        public virtual void AddOrUpdateShouldAddNewValue2()
        {
            var invokeCount = 0;
            var item = GetSupportedItem();
            var component = GetComponent();
            component.AddOrUpdate(item, TestPath, this, (i, state) =>
            {
                ++invokeCount;
                i.ShouldEqual(item);
                state.ShouldEqual(this);
                return this;
            }, (_, __, ___, ____) => throw new NotSupportedException()).ShouldEqual(this);
            component.TryGet(item, TestPath, out object? v).ShouldBeTrue();
            v.ShouldEqual(this);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public virtual void AddOrUpdateShouldUpdateOldValue1()
        {
            var invokeCount = 0;
            var oldValue = new object();
            var newValue = new object();
            var item = GetSupportedItem();
            var component = GetComponent();
            component.Set(item, TestPath, oldValue);
            component.AddOrUpdate(item, TestPath, newValue, this, (o, value, currentValue, state) =>
            {
                ++invokeCount;
                o.ShouldEqual(item);
                value.ShouldEqual(newValue);
                currentValue.ShouldEqual(oldValue);
                state.ShouldEqual(this);
                return newValue;
            }).ShouldEqual(newValue);
            component.TryGet(item, TestPath, out object? v).ShouldBeTrue();
            v.ShouldEqual(newValue);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public virtual void AddOrUpdateShouldUpdateOldValue2()
        {
            var invokeCount = 0;
            var oldValue = new object();
            var newValue = new object();
            var item = GetSupportedItem();
            var component = GetComponent();
            component.Set(item, TestPath, oldValue);
            component.AddOrUpdate(item, TestPath, this, (it, state) =>
            {
                it.ShouldEqual(item);
                state.ShouldEqual(this);
                return newValue;
            }, (o, value, currentValue, state) =>
            {
                ++invokeCount;
                o.ShouldEqual(item);
                value(o, state).ShouldEqual(newValue);
                currentValue.ShouldEqual(oldValue);
                state.ShouldEqual(this);
                return value(o, state);
            }).ShouldEqual(newValue);
            component.TryGet(item, TestPath, out object? v).ShouldBeTrue();
            v.ShouldEqual(newValue);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public virtual void GetOrAddShouldAddNewValue1()
        {
            var item = GetSupportedItem();
            var component = GetComponent();
            component.GetOrAdd(item, TestPath, this).ShouldEqual(this);
            component.TryGet(item, TestPath, out object? v).ShouldBeTrue();
            v.ShouldEqual(this);
        }

        [Fact]
        public virtual void GetOrAddShouldAddNewValue2()
        {
            int invokeCount = 0;
            var item = GetSupportedItem();
            var component = GetComponent();
            component.GetOrAdd(item, TestPath, component, (o, providerComponent) =>
            {
                ++invokeCount;
                o.ShouldEqual(item);
                providerComponent.ShouldEqual(component);
                return this;
            }).ShouldEqual(this);
            component.TryGet(item, TestPath, out object? v).ShouldBeTrue();
            v.ShouldEqual(this);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public virtual void GetOrAddShouldGetOldValue1()
        {
            var oldValue = new object();
            var newValue = new object();
            var item = GetSupportedItem();
            var component = GetComponent();
            component.Set(item, TestPath, oldValue);
            component.GetOrAdd(item, TestPath, newValue).ShouldEqual(oldValue);
            component.TryGet(item, TestPath, out object? v).ShouldBeTrue();
            v.ShouldEqual(oldValue);
        }

        [Fact]
        public virtual void GetOrAddShouldGetOldValue2()
        {
            var oldValue = new object();
            var item = GetSupportedItem();
            var component = GetComponent();
            component.Set(item, TestPath, oldValue);
            component.GetOrAdd<object, object, object>(item, TestPath, this, (_, __) => throw new NotSupportedException()).ShouldEqual(oldValue);
            component.TryGet(item, TestPath, out object? v).ShouldBeTrue();
            v.ShouldEqual(oldValue);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public virtual void ClearShouldClearByPath(int count)
        {
            var item = GetSupportedItem();
            var component = GetComponent();
            for (var i = 0; i < count; i++)
            {
                var pair = new KeyValuePair<string, object>(TestPath + i, i + 1);
                component.Set(item, pair.Key, pair.Value);
            }

            for (var i = 0; i < count; i++)
            {
                component.TryGet(item, TestPath + i, out int v).ShouldBeTrue();
                v.ShouldEqual(i + 1);

                component.Clear(item, TestPath + i).ShouldBeTrue();
                component.TryGet(item, TestPath + i, out v).ShouldBeFalse();
                v.ShouldNotEqual(i + 1);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public virtual void ClearShouldClearAll(int count)
        {
            var item = GetSupportedItem();
            var component = GetComponent();
            for (var i = 0; i < count; i++)
            {
                var pair = new KeyValuePair<string, object>(TestPath + i, i + 1);
                component.Set(item, pair.Key, pair.Value);
            }

            component.Clear(item, null);
            for (var i = 0; i < count; i++)
            {
                component.TryGet(item, TestPath + i, out int v).ShouldBeFalse();
                v.ShouldNotEqual(i + 1);
            }
        }

#if !DEBUG
        [Fact]
        public virtual void ClearShouldNotKeepRef()
        {
            var item = GetSupportedItem();
            var component = GetComponent();

            var value = new object();
            var weakReference = new WeakReference(value);
            component.Set(item, TestPath, value);

            component.Clear(item, TestPath);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            weakReference.IsAlive.ShouldBeFalse();
        }

        [Fact]
        public virtual void ShouldBeEphemeron1()
        {
            object? item = GetSupportedItem();
            var component = GetComponent();

            var value = new object();
            var weakReference = new WeakReference(value);
            component.Set(item, TestPath, value);

            item = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            weakReference.IsAlive.ShouldBeFalse();
        }

        [Fact]
        public virtual void ShouldBeEphemeron2()
        {
            object? item = GetSupportedItem();
            var component = GetComponent();

            var value = item;
            var weakReference = new WeakReference(value);
            component.Set(item, TestPath, value);

            item = null;
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            weakReference.IsAlive.ShouldBeFalse();
        }
#endif

        protected abstract object GetSupportedItem();

        protected abstract IAttachedValueProviderComponent GetComponent();

        #endregion
    }
}