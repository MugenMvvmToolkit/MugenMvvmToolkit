using System;
using System.Collections.Generic;
using System.Linq;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Internal;
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
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());

            manager.Contains(item, TestPath).ShouldBeFalse();
            manager.TryGet(item, TestPath, out AttachedValueProviderTestBase? value).ShouldBeFalse();
            value.ShouldBeNull();

            manager.Set(item, TestPath, this, out var old);
            old.ShouldBeNull();
            manager.TryGet(item, TestPath, out value).ShouldBeTrue();
            manager.Contains(item, TestPath).ShouldBeTrue();
            value.ShouldEqual(this);

            manager.Set(item, TestPath, new object(), out old);
            old.ShouldEqual(this);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public virtual void TryGetValuesShouldReturnAllValues(int count)
        {
            var item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            var hashSet = new HashSet<KeyValuePair<string, object?>>();
            var values = new List<KeyValuePair<string, object?>>();
            for (var i = 0; i < count; i++)
            {
                var pair = new KeyValuePair<string, object?>(TestPath + i, i);
                manager.Set(item, pair.Key, pair.Value, out _);
                values.Add(pair);
            }

            manager.GetValues(item, this, (o, pair, arg3) =>
            {
                o.ShouldEqual(item);
                arg3.ShouldEqual(this);
                hashSet.Remove(pair);
                return false;
            }).AsList().ShouldBeEmpty();
            hashSet.Count.ShouldEqual(0);
            manager.GetValues(item, this, (o, pair, arg3) => true).AsList().SequenceEqual(values).ShouldBeTrue();
        }

        [Fact]
        public virtual void AddOrUpdateShouldAddNewValue1()
        {
            var item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            manager.AddOrUpdate(item, TestPath, this, this, (_, __, ___, ____) => throw new NotSupportedException()).ShouldEqual(this);
            manager.TryGet(item, TestPath, out object? v).ShouldBeTrue();
            v.ShouldEqual(this);
        }

        [Fact]
        public virtual void AddOrUpdateShouldAddNewValue2()
        {
            var invokeCount = 0;
            var item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            manager.AddOrUpdate(item, TestPath, this, (i, state) =>
            {
                ++invokeCount;
                i.ShouldEqual(item);
                state.ShouldEqual(this);
                return this;
            }, (_, __, ___, ____) => throw new NotSupportedException()).ShouldEqual(this);
            manager.TryGet(item, TestPath, out object? v).ShouldBeTrue();
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
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            manager.Set(item, TestPath, oldValue, out _);
            manager.AddOrUpdate(item, TestPath, newValue, this, (o, value, currentValue, state) =>
            {
                ++invokeCount;
                o.ShouldEqual(item);
                value.ShouldEqual(newValue);
                currentValue.ShouldEqual(oldValue);
                state.ShouldEqual(this);
                return newValue;
            }).ShouldEqual(newValue);
            manager.TryGet(item, TestPath, out object? v).ShouldBeTrue();
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
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            manager.Set(item, TestPath, oldValue, out _);
            manager.AddOrUpdate(item, TestPath, this, (it, state) =>
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
            manager.TryGet(item, TestPath, out object? v).ShouldBeTrue();
            v.ShouldEqual(newValue);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public virtual void GetOrAddShouldAddNewValue1()
        {
            var item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            manager.GetOrAdd(item, TestPath, this).ShouldEqual(this);
            manager.TryGet(item, TestPath, out object? v).ShouldBeTrue();
            v.ShouldEqual(this);
        }

        [Fact]
        public virtual void GetOrAddShouldAddNewValue2()
        {
            int invokeCount = 0;
            var item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            manager.GetOrAdd(item, TestPath, manager, (o, providerComponent) =>
            {
                ++invokeCount;
                o.ShouldEqual(item);
                providerComponent.ShouldEqual(manager);
                return this;
            }).ShouldEqual(this);
            manager.TryGet(item, TestPath, out object? v).ShouldBeTrue();
            v.ShouldEqual(this);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public virtual void GetOrAddShouldGetOldValue1()
        {
            var oldValue = new object();
            var newValue = new object();
            var item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            manager.Set(item, TestPath, oldValue, out _);
            manager.GetOrAdd(item, TestPath, newValue).ShouldEqual(oldValue);
            manager.TryGet(item, TestPath, out object? v).ShouldBeTrue();
            v.ShouldEqual(oldValue);
        }

        [Fact]
        public virtual void GetOrAddShouldGetOldValue2()
        {
            var oldValue = new object();
            var item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            manager.Set(item, TestPath, oldValue, out _);
            manager.GetOrAdd<object, object, object>(item, TestPath, this, (_, __) => throw new NotSupportedException()).ShouldEqual(oldValue);
            manager.TryGet(item, TestPath, out object? v).ShouldBeTrue();
            v.ShouldEqual(oldValue);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public virtual void ClearShouldClearByPath(int count)
        {
            var item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            for (var i = 0; i < count; i++)
            {
                var pair = new KeyValuePair<string, object>(TestPath + i, i + 1);
                manager.Set(item, pair.Key, pair.Value, out _);
            }

            for (var i = 0; i < count; i++)
            {
                manager.TryGet(item, TestPath + i, out int v).ShouldBeTrue();
                v.ShouldEqual(i + 1);

                manager.Clear(item, TestPath + i, out var old).ShouldBeTrue();
                old.ShouldEqual(i + 1);

                manager.TryGet(item, TestPath + i, out v).ShouldBeFalse();
                v.ShouldNotEqual(i + 1);
            }
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public virtual void ClearShouldClearAll(int count)
        {
            var item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());
            for (var i = 0; i < count; i++)
            {
                var pair = new KeyValuePair<string, object>(TestPath + i, i + 1);
                manager.Set(item, pair.Key, pair.Value, out _);
            }

            manager.Clear(item);
            for (var i = 0; i < count; i++)
            {
                manager.TryGet(item, TestPath + i, out int v).ShouldBeFalse();
                v.ShouldNotEqual(i + 1);
            }
        }

#if !DEBUG
        [Fact]
        public virtual void ClearShouldNotKeepRef()
        {
            var item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());

            var value = new object();
            var weakReference = new WeakReference(value);
            manager.Set(item, TestPath, value, out _);

            manager.Clear(item, TestPath, out _);
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            weakReference.IsAlive.ShouldBeFalse();
        }

        [Fact]
        public virtual void ShouldBeEphemeron1()
        {
            object? item = GetSupportedItem();
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());

            var value = new object();
            var weakReference = new WeakReference(value);
            manager.Set(item, TestPath, value, out _);

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
            var manager = new AttachedValueManager();
            manager.AddComponent(GetComponent());

            var value = item;
            var weakReference = new WeakReference(value);
            manager.Set(item, TestPath, value, out _);

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