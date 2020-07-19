using System;
using System.Collections.Generic;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.UnitTest.Components;
using MugenMvvm.UnitTest.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Internal
{
    public class AttachedValueManagerTest : ComponentOwnerTestBase<AttachedValueManager>
    {
        #region Fields

        protected const string TestPath = "Test";

        #endregion

        #region Methods

        [Fact]
        public void GetValuesShouldReturnEmptyNoComponents()
        {
            new AttachedValueManager().GetValues(this, (test, pair, arg3) => true, this).AsList().ShouldBeEmpty();
        }

        [Fact]
        public void TryGetShouldReturnEmptyNoComponents()
        {
            new AttachedValueManager().TryGet(this, TestPath, out var value).ShouldBeFalse();
            value.ShouldEqual(null);
        }

        [Fact]
        public void ContainsShouldReturnEmptyNoComponents()
        {
            new AttachedValueManager().Contains(this, TestPath).ShouldBeFalse();
        }

        [Fact]
        public void AddOrUpdateThrowNoComponents1()
        {
            ShouldThrow<InvalidOperationException>(() => new AttachedValueManager().AddOrUpdate(this, TestPath, this, (item, value, currentValue, state) => currentValue));
        }

        [Fact]
        public void AddOrUpdateThrowNoComponents2()
        {
            ShouldThrow<InvalidOperationException>(() => new AttachedValueManager().AddOrUpdate(this, TestPath, (test, providerTest) => test, (item, value, currentValue, state) => currentValue));
        }

        [Fact]
        public void GetOrAddShouldThrowNoComponents1()
        {
            ShouldThrow<InvalidOperationException>(() => new AttachedValueManager().GetOrAdd(this, TestPath, this));
        }

        [Fact]
        public void GetOrAddShouldThrowNoComponents2()
        {
            ShouldThrow<InvalidOperationException>(() => new AttachedValueManager().GetOrAdd(this, TestPath, (test, s) => this));
        }

        [Fact]
        public void SetShouldThrowNoComponents()
        {
            ShouldThrow<InvalidOperationException>(() => new AttachedValueManager().Set(this, TestPath, this, out _));
        }

        [Fact]
        public void ClearThrowNoComponents1()
        {
            ShouldThrow<InvalidOperationException>(() => new AttachedValueManager().Remove(this, TestPath, out _));
        }

        [Fact]
        public void ClearThrowNoComponents2()
        {
            ShouldThrow<InvalidOperationException>(() => new AttachedValueManager().Clear(this));
        }

        [Theory]
        [InlineData(1, false)]
        [InlineData(1, true)]
        [InlineData(10, false)]
        [InlineData(10, true)]
        public void GetValuesShouldBeHandledBySupportedComponent(int count, bool hasFilter)
        {
            var result = new List<KeyValuePair<string, object?>>();
            var filterExecuted = 0;
            var methodExecuted = 0;
            var attachedValueManager = new AttachedValueManager();
            for (var i = 0; i < count; i++)
            {
                var isSupported = count - 1 == i;
                var component = new TestAttachedValueProviderComponent(attachedValueManager)
                {
                    IsSupported = (o, context) => isSupported,
                    TryGetValues = (o, arg3, arg5) =>
                    {
                        ++methodExecuted;
                        o.ShouldEqual(this);
                        arg3.ShouldEqual(attachedValueManager);
                        if (hasFilter)
                            arg5!.Invoke(o!, default, arg3!).ShouldBeTrue();
                        else
                            arg5.ShouldBeNull();
                        return result;
                    }
                };
                attachedValueManager.AddComponent(component);
            }

            Func<object, KeyValuePair<string, object?>, object?, bool> func = (test, pair, arg3) => ++filterExecuted != -1;
            attachedValueManager.GetValues(this, hasFilter ? func : null, attachedValueManager).ShouldEqual(result);
            filterExecuted.ShouldEqual(hasFilter ? 1 : 0);
            methodExecuted.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryGetShouldBeHandledBySupportedComponent(int count)
        {
            var result = int.MaxValue;
            var methodExecuted = 0;
            var attachedValueManager = new AttachedValueManager();
            for (var i = 0; i < count; i++)
            {
                var isSupported = count - 1 == i;
                var component = new TestAttachedValueProviderComponent(attachedValueManager)
                {
                    IsSupported = (o, context) => isSupported,
                    TryGet = (o, s) =>
                    {
                        ++methodExecuted;
                        s.ShouldEqual(TestPath);
                        o.ShouldEqual(this);
                        return result;
                    }
                };
                attachedValueManager.AddComponent(component);
            }

            attachedValueManager.TryGet(this, TestPath, out var r).ShouldBeTrue();
            r.ShouldEqual(result);
            methodExecuted.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ContainsShouldBeHandledBySupportedComponent(int count)
        {
            var result = true;
            var methodExecuted = 0;
            var attachedValueManager = new AttachedValueManager();
            for (var i = 0; i < count; i++)
            {
                var isSupported = count - 1 == i;
                var component = new TestAttachedValueProviderComponent(attachedValueManager)
                {
                    IsSupported = (o, context) => isSupported,
                    Contains = (o, s) =>
                    {
                        ++methodExecuted;
                        s.ShouldEqual(TestPath);
                        o.ShouldEqual(this);
                        return result;
                    }
                };
                attachedValueManager.AddComponent(component);
            }

            attachedValueManager.Contains(this, TestPath).ShouldBeTrue();
            result = false;
            attachedValueManager.Contains(this, TestPath).ShouldBeFalse();
            methodExecuted.ShouldEqual(2);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void AddOrUpdateShouldBeHandledBySupportedComponent1(int count)
        {
            object valueToSet = this;
            var delExecuted = 0;
            var methodExecuted = 0;
            var attachedValueManager = new AttachedValueManager();
            for (var i = 0; i < count; i++)
            {
                var isSupported = count - 1 == i;
                var component = new TestAttachedValueProviderComponent(attachedValueManager)
                {
                    IsSupported = (o, context) => isSupported,
                    AddOrUpdate = (o, arg3, arg4, arg6, arg8) =>
                    {
                        ++methodExecuted;
                        o.ShouldEqual(this);
                        arg3.ShouldEqual(TestPath);
                        arg4.ShouldEqual(valueToSet);
                        arg6.ShouldEqual(attachedValueManager);
                        return arg8(o, arg4, arg4, arg6);
                    }
                };
                attachedValueManager.AddComponent(component);
            }

            valueToSet = this;
            attachedValueManager.AddOrUpdate(this, TestPath, this, (item, value, currentValue, state) =>
            {
                ++delExecuted;
                return this;
            }, attachedValueManager).ShouldEqual(this);
            delExecuted.ShouldEqual(1);
            methodExecuted.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void AddOrUpdateShouldBeHandledBySupportedComponent2(int count)
        {
            object valueToSet = this;
            var delExecuted = 0;
            var methodExecuted = 0;
            var attachedValueManager = new AttachedValueManager();
            for (var i = 0; i < count; i++)
            {
                var isSupported = count - 1 == i;
                var component = new TestAttachedValueProviderComponent(attachedValueManager)
                {
                    IsSupported = (o, context) => isSupported,
                    AddOrUpdate1 = (o, arg3, arg4, arg6, arg8) =>
                    {
                        ++methodExecuted;
                        o.ShouldEqual(this);
                        arg3.ShouldEqual(TestPath);
                        arg4.ShouldEqual(attachedValueManager);
                        return arg8(o, arg6, arg6(null!, null), arg4);
                    }
                };
                attachedValueManager.AddComponent(component);
            }

            valueToSet = this;
            attachedValueManager.AddOrUpdate(this, TestPath, (test, provider) => this, (item, value, currentValue, state) =>
            {
                ++delExecuted;
                return this;
            }, attachedValueManager).ShouldEqual(this);
            delExecuted.ShouldEqual(1);
            methodExecuted.ShouldEqual(1);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetOrAddShouldBeHandledBySupportedComponent1(int count)
        {
            object valueToSet = this;
            var methodExecuted = 0;
            var attachedValueManager = new AttachedValueManager();
            for (var i = 0; i < count; i++)
            {
                var isSupported = count - 1 == i;
                var component = new TestAttachedValueProviderComponent(attachedValueManager)
                {
                    IsSupported = (o, context) => isSupported,
                    GetOrAdd = (o, s, v) =>
                    {
                        o.ShouldEqual(this);
                        s.ShouldEqual(TestPath);
                        v.ShouldEqual(valueToSet);
                        ++methodExecuted;
                        return v;
                    }
                };
                attachedValueManager.AddComponent(component);
            }

            attachedValueManager.GetOrAdd(this, TestPath, this).ShouldEqual(valueToSet);
            methodExecuted.ShouldEqual(1);

            valueToSet = 1;
            attachedValueManager.GetOrAdd(this, TestPath, 1).ShouldEqual(valueToSet);
            methodExecuted.ShouldEqual(2);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetOrAddShouldBeHandledBySupportedComponent2(int count)
        {
            object valueToSet = this;
            var methodExecuted = 0;
            var attachedValueManager = new AttachedValueManager();
            for (var i = 0; i < count; i++)
            {
                var isSupported = count - 1 == i;
                var component = new TestAttachedValueProviderComponent(attachedValueManager)
                {
                    IsSupported = (o, context) => isSupported,
                    GetOrAdd1 = (o, arg3, arg4, arg7) =>
                    {
                        o.ShouldEqual(this);
                        arg3.ShouldEqual(TestPath);
                        arg4.ShouldEqual(attachedValueManager);
                        ++methodExecuted;
                        return arg7(o, null);
                    }
                };
                attachedValueManager.AddComponent(component);
            }

            attachedValueManager.GetOrAdd(this, TestPath, (test, provider) => this, attachedValueManager).ShouldEqual(valueToSet);
            methodExecuted.ShouldEqual(1);

            valueToSet = 1;
            attachedValueManager.GetOrAdd(this, TestPath, (test, provider) => 1, attachedValueManager).ShouldEqual(valueToSet);
            methodExecuted.ShouldEqual(2);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void SetShouldBeHandledBySupportedComponent(int count)
        {
            var oldV = new object();
            object valueToSet = this;
            var methodExecuted = 0;
            var attachedValueManager = new AttachedValueManager();
            for (var i = 0; i < count; i++)
            {
                var isSupported = count - 1 == i;
                var component = new TestAttachedValueProviderComponent(attachedValueManager)
                {
                    IsSupported = (o, context) => isSupported,
                    Set = (object item, string path, object? value, out object? oldValue) =>
                    {
                        item.ShouldEqual(this);
                        path.ShouldEqual(TestPath);
                        value.ShouldEqual(valueToSet);
                        oldValue = oldV;
                        ++methodExecuted;
                    }
                };
                attachedValueManager.AddComponent(component);
            }

            attachedValueManager.Set(this, TestPath, this, out var old);
            old.ShouldEqual(oldV);
            methodExecuted.ShouldEqual(1);

            valueToSet = 1;
            attachedValueManager.Set(this, TestPath, 1, out old);
            old.ShouldEqual(oldV);
            methodExecuted.ShouldEqual(2);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ClearShouldBeHandledBySupportedComponent1(int count)
        {
            var result = true;
            var oldV = new object();
            var methodExecuted = 0;
            var attachedValueManager = new AttachedValueManager();
            for (var i = 0; i < count; i++)
            {
                var isSupported = count - 1 == i;
                var component = new TestAttachedValueProviderComponent(attachedValueManager)
                {
                    IsSupported = (o, context) => isSupported,
                    ClearKey = (object item, string path, out object? value) =>
                    {
                        ++methodExecuted;
                        item.ShouldEqual(this);
                        path.ShouldEqual(TestPath);
                        value = oldV;
                        return result;
                    }
                };
                attachedValueManager.AddComponent(component);
            }

            attachedValueManager.Remove(this, TestPath, out var old).ShouldBeTrue();
            old.ShouldEqual(oldV);
            result = false;
            attachedValueManager.Remove(this, TestPath, out old).ShouldBeFalse();
            old.ShouldEqual(oldV);
            methodExecuted.ShouldEqual(2);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void ClearShouldBeHandledBySupportedComponent2(int count)
        {
            var result = true;
            var methodExecuted = 0;
            var attachedValueManager = new AttachedValueManager();
            for (var i = 0; i < count; i++)
            {
                var isSupported = count - 1 == i;
                var component = new TestAttachedValueProviderComponent(attachedValueManager)
                {
                    IsSupported = (o, context) => isSupported,
                    Clear = item =>
                    {
                        ++methodExecuted;
                        item.ShouldEqual(this);
                        return result;
                    }
                };
                attachedValueManager.AddComponent(component);
            }

            attachedValueManager.Clear(this).ShouldBeTrue();
            result = false;
            attachedValueManager.Clear(this).ShouldBeFalse();
            methodExecuted.ShouldEqual(2);
        }

        protected override AttachedValueManager GetComponentOwner(IComponentCollectionManager? collectionProvider = null)
        {
            return new AttachedValueManager(collectionProvider);
        }

        #endregion
    }
}