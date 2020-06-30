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
            new AttachedValueManager().GetValues(this, this, (test, pair, arg3) => true).AsList().ShouldBeEmpty();
        }

        [Fact]
        public void TryGetShouldReturnEmptyNoComponents()
        {
            new AttachedValueManager().TryGet(this, TestPath, out int value).ShouldBeFalse();
            value.ShouldEqual(0);
        }

        [Fact]
        public void ContainsShouldReturnEmptyNoComponents()
        {
            new AttachedValueManager().Contains(this, TestPath).ShouldBeFalse();
        }

        [Fact]
        public void AddOrUpdateThrowNoComponents1()
        {
            ShouldThrow<InvalidOperationException>(() => new AttachedValueManager().AddOrUpdate(this, TestPath, this, this, (item, value, currentValue, state) => currentValue));
        }

        [Fact]
        public void AddOrUpdateThrowNoComponents2()
        {
            ShouldThrow<InvalidOperationException>(() => new AttachedValueManager().AddOrUpdate(this, TestPath, this, (test, providerTest) => test, (item, value, currentValue, state) => currentValue));
        }

        [Fact]
        public void GetOrAddShouldThrowNoComponents1()
        {
            ShouldThrow<InvalidOperationException>(() => new AttachedValueManager().GetOrAdd(this, TestPath, this));
        }

        [Fact]
        public void GetOrAddShouldThrowNoComponents2()
        {
            ShouldThrow<InvalidOperationException>(() => new AttachedValueManager().GetOrAdd(this, TestPath, test => this));
        }

        [Fact]
        public void SetShouldThrowNoComponents()
        {
            ShouldThrow<InvalidOperationException>(() => new AttachedValueManager().Set(this, TestPath, this, out _));
        }

        [Fact]
        public void ClearThrowNoComponents1()
        {
            ShouldThrow<InvalidOperationException>(() => new AttachedValueManager().Clear(this, TestPath, out _));
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
            var attachedValueProvider = new AttachedValueManager();
            for (var i = 0; i < count; i++)
            {
                var isSupported = count - 1 == i;
                var component = new TestAttachedValueProviderComponent
                {
                    IsSupported = (o, context) => isSupported,
                    TryGetValues = (o, type, arg3, arg4, arg5) =>
                    {
                        ++methodExecuted;
                        o.ShouldEqual(this);
                        type.ShouldEqual(typeof(AttachedValueManagerTest));
                        arg3.ShouldEqual(attachedValueProvider);
                        arg4.ShouldEqual(typeof(AttachedValueManager));
                        if (hasFilter)
                            arg5!.Invoke(o!, default, arg3!).ShouldBeTrue();
                        else
                            arg5.ShouldBeNull();
                        return result;
                    }
                };
                attachedValueProvider.AddComponent(component);
            }

            Func<AttachedValueManagerTest, KeyValuePair<string, object?>, AttachedValueManager, bool> func = (test, pair, arg3) => ++filterExecuted != -1;
            attachedValueProvider.GetValues(this, attachedValueProvider, hasFilter ? func : null).ShouldEqual(result);
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
            var attachedValueProvider = new AttachedValueManager();
            for (var i = 0; i < count; i++)
            {
                var isSupported = count - 1 == i;
                var component = new TestAttachedValueProviderComponent
                {
                    IsSupported = (o, context) => isSupported,
                    TryGet = (o, s, arg3) =>
                    {
                        ++methodExecuted;
                        s.ShouldEqual(TestPath);
                        o.ShouldEqual(this);
                        arg3.ShouldEqual(typeof(int));
                        return result;
                    }
                };
                attachedValueProvider.AddComponent(component);
            }

            attachedValueProvider.TryGet(this, TestPath, out int r).ShouldBeTrue();
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
            var attachedValueProvider = new AttachedValueManager();
            for (var i = 0; i < count; i++)
            {
                var isSupported = count - 1 == i;
                var component = new TestAttachedValueProviderComponent
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
                attachedValueProvider.AddComponent(component);
            }

            attachedValueProvider.Contains(this, TestPath).ShouldBeTrue();
            result = false;
            attachedValueProvider.Contains(this, TestPath).ShouldBeFalse();
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
            var attachedValueProvider = new AttachedValueManager();
            for (var i = 0; i < count; i++)
            {
                var isSupported = count - 1 == i;
                var component = new TestAttachedValueProviderComponent
                {
                    IsSupported = (o, context) => isSupported,
                    AddOrUpdate = (o, type, arg3, arg4, arg5, arg6, arg7, arg8) =>
                    {
                        ++methodExecuted;
                        o.ShouldEqual(this);
                        type.ShouldEqual(typeof(AttachedValueManagerTest));
                        arg3.ShouldEqual(TestPath);
                        arg4.ShouldEqual(valueToSet);
                        arg5.ShouldEqual(valueToSet.GetType());
                        arg6.ShouldEqual(attachedValueProvider);
                        arg7.ShouldEqual(typeof(AttachedValueManager));
                        return arg8(o, arg4, arg4, arg6);
                    }
                };
                attachedValueProvider.AddComponent(component);
            }

            valueToSet = this;
            attachedValueProvider.AddOrUpdate(this, TestPath, this, attachedValueProvider, (item, value, currentValue, state) =>
            {
                ++delExecuted;
                return this;
            }).ShouldEqual(this);
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
            var attachedValueProvider = new AttachedValueManager();
            for (var i = 0; i < count; i++)
            {
                var isSupported = count - 1 == i;
                var component = new TestAttachedValueProviderComponent
                {
                    IsSupported = (o, context) => isSupported,
                    AddOrUpdate1 = (o, type, arg3, arg4, arg5, arg6, arg7, arg8) =>
                    {
                        ++methodExecuted;
                        o.ShouldEqual(this);
                        type.ShouldEqual(typeof(AttachedValueManagerTest));
                        arg3.ShouldEqual(TestPath);
                        arg4.ShouldEqual(attachedValueProvider);
                        arg5.ShouldEqual(typeof(AttachedValueManager));
                        arg7.ShouldEqual(typeof(AttachedValueManagerTest));
                        return arg8(o, arg6, arg6(null, null), arg4);
                    }
                };
                attachedValueProvider.AddComponent(component);
            }

            valueToSet = this;
            attachedValueProvider.AddOrUpdate(this, TestPath, attachedValueProvider, (test, provider) => this, (item, value, currentValue, state) =>
            {
                ++delExecuted;
                return this;
            }).ShouldEqual(this);
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
            var attachedValueProvider = new AttachedValueManager();
            for (var i = 0; i < count; i++)
            {
                var isSupported = count - 1 == i;
                var component = new TestAttachedValueProviderComponent
                {
                    IsSupported = (o, context) => isSupported,
                    GetOrAdd = (o, s, v, t) =>
                    {
                        o.ShouldEqual(this);
                        s.ShouldEqual(TestPath);
                        v.ShouldEqual(valueToSet);
                        t.ShouldEqual(valueToSet.GetType());
                        ++methodExecuted;
                        return v;
                    }
                };
                attachedValueProvider.AddComponent(component);
            }

            attachedValueProvider.GetOrAdd(this, TestPath, this).ShouldEqual(valueToSet);
            methodExecuted.ShouldEqual(1);

            valueToSet = 1;
            attachedValueProvider.GetOrAdd(this, TestPath, 1).ShouldEqual(valueToSet);
            methodExecuted.ShouldEqual(2);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetOrAddShouldBeHandledBySupportedComponent2(int count)
        {
            object valueToSet = this;
            var methodExecuted = 0;
            var attachedValueProvider = new AttachedValueManager();
            for (var i = 0; i < count; i++)
            {
                var isSupported = count - 1 == i;
                var component = new TestAttachedValueProviderComponent
                {
                    IsSupported = (o, context) => isSupported,
                    GetOrAdd1 = (o, type, arg3, arg4, arg5, arg6, arg7) =>
                    {
                        o.ShouldEqual(this);
                        type.ShouldEqual(typeof(AttachedValueManagerTest));
                        arg3.ShouldEqual(TestPath);
                        arg4.ShouldEqual(attachedValueProvider);
                        arg5.ShouldEqual(typeof(AttachedValueManager));
                        arg6.ShouldEqual(valueToSet.GetType());
                        ++methodExecuted;
                        return arg7(null, null);
                    }
                };
                attachedValueProvider.AddComponent(component);
            }

            attachedValueProvider.GetOrAdd(this, TestPath, attachedValueProvider, (test, provider) => this).ShouldEqual(valueToSet);
            methodExecuted.ShouldEqual(1);

            valueToSet = 1;
            attachedValueProvider.GetOrAdd(this, TestPath, attachedValueProvider, (test, provider) => 1).ShouldEqual(valueToSet);
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
            var attachedValueProvider = new AttachedValueManager();
            for (var i = 0; i < count; i++)
            {
                var isSupported = count - 1 == i;
                var component = new TestAttachedValueProviderComponent
                {
                    IsSupported = (o, context) => isSupported,
                    Set = (object item, string path, object value, Type type, out object? oldValue) =>
                    {
                        item.ShouldEqual(this);
                        path.ShouldEqual(TestPath);
                        value.ShouldEqual(valueToSet);
                        type.ShouldEqual(valueToSet.GetType());
                        oldValue = oldV;
                        ++methodExecuted;
                    }
                };
                attachedValueProvider.AddComponent(component);
            }

            attachedValueProvider.Set(this, TestPath, this, out var old);
            old.ShouldEqual(oldV);
            methodExecuted.ShouldEqual(1);

            valueToSet = 1;
            attachedValueProvider.Set(this, TestPath, 1, out old);
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
            var attachedValueProvider = new AttachedValueManager();
            for (var i = 0; i < count; i++)
            {
                var isSupported = count - 1 == i;
                var component = new TestAttachedValueProviderComponent
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
                attachedValueProvider.AddComponent(component);
            }

            attachedValueProvider.Clear(this, TestPath, out var old).ShouldBeTrue();
            old.ShouldEqual(oldV);
            result = false;
            attachedValueProvider.Clear(this, TestPath, out old).ShouldBeFalse();
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
            var attachedValueProvider = new AttachedValueManager();
            for (var i = 0; i < count; i++)
            {
                var isSupported = count - 1 == i;
                var component = new TestAttachedValueProviderComponent
                {
                    IsSupported = (o, context) => isSupported,
                    Clear = item =>
                    {
                        ++methodExecuted;
                        item.ShouldEqual(this);
                        return result;
                    }
                };
                attachedValueProvider.AddComponent(component);
            }

            attachedValueProvider.Clear(this).ShouldBeTrue();
            result = false;
            attachedValueProvider.Clear(this).ShouldBeFalse();
            methodExecuted.ShouldEqual(2);
        }

        protected override AttachedValueManager GetComponentOwner(IComponentCollectionProvider? collectionProvider = null)
        {
            return new AttachedValueManager(collectionProvider);
        }

        #endregion
    }
}