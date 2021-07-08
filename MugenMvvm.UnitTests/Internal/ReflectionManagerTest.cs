using System;
using System.Reflection;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Internal;
using MugenMvvm.Tests.Internal;
using MugenMvvm.UnitTests.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Internal
{
    public class ReflectionManagerTest : ComponentOwnerTestBase<ReflectionManager>
    {
        public static readonly MethodInfo TestMethod = typeof(ReflectionManagerTest).GetMethod(nameof(CanCreateDelegateShouldReturnFalseNoComponents))!;
        public static readonly ConstructorInfo TestConstructor = typeof(ReflectionManagerTest).GetConstructor(Type.EmptyTypes)!;

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void CanCreateDelegateShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var delType = typeof(Action);
            for (var i = 0; i < count; i++)
            {
                var canCreate = count - 1 == i;
                ReflectionManager.AddComponent(new TestReflectionDelegateProviderComponent
                {
                    Priority = -i,
                    CanCreateDelegate = (o, type, info) =>
                    {
                        o.ShouldEqual(ReflectionManager);
                        type.ShouldEqual(delType);
                        info.ShouldEqual(TestMethod);
                        ++invokeCount;
                        return canCreate;
                    }
                });
            }

            ReflectionManager.CanCreateDelegate(delType, TestMethod).ShouldEqual(true);
            invokeCount.ShouldEqual(count);
        }

        [Fact]
        public void CanCreateDelegateShouldReturnFalseNoComponents() => ReflectionManager.CanCreateDelegate(typeof(Action), TestMethod).ShouldBeFalse();

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetActivatorShouldBeHandledByComponents1(int count)
        {
            var invokeCount = 0;
            Func<ItemOrArray<object?>, object>? result = objects => objects;
            for (var i = 0; i < count; i++)
            {
                var canCreate = count - 1 == i;
                ReflectionManager.AddComponent(new TestActivatorReflectionDelegateProviderComponent
                {
                    Priority = -i,
                    TryGetActivator = (o, info) =>
                    {
                        o.ShouldEqual(ReflectionManager);
                        info.ShouldEqual(TestConstructor);
                        ++invokeCount;
                        if (canCreate)
                            return result;
                        return null;
                    }
                });
            }

            ReflectionManager.GetActivator(TestConstructor).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetActivatorShouldBeHandledByComponents2(int count)
        {
            var invokeCount = 0;
            var delType = typeof(Action);
            Func<object?[], object>? result = objects => objects;
            for (var i = 0; i < count; i++)
            {
                var canCreate = count - 1 == i;
                ReflectionManager.AddComponent(new TestActivatorReflectionDelegateProviderComponent
                {
                    Priority = -i,
                    TryGetActivator1 = (o, info, type) =>
                    {
                        o.ShouldEqual(ReflectionManager);
                        info.ShouldEqual(TestConstructor);
                        type.ShouldEqual(delType.GetType());
                        ++invokeCount;
                        if (canCreate)
                            return result;
                        return null;
                    }
                });
            }

            ReflectionManager.GetActivator(TestConstructor, delType.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Fact]
        public void GetActivatorShouldThrowNoComponents1() =>
            ShouldThrow<InvalidOperationException>(() => ReflectionManager.GetActivator(TestConstructor));

        [Fact]
        public void GetActivatorShouldThrowNoComponents2() =>
            ShouldThrow<InvalidOperationException>(() => ReflectionManager.GetActivator(TestConstructor, typeof(Action)));

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMemberGetterShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            Func<object?, object?[], object?> result = (o, objects) => o;
            for (var i = 0; i < count; i++)
            {
                var canCreate = count - 1 == i;
                ReflectionManager.AddComponent(new TestMemberReflectionDelegateProviderComponent
                {
                    Priority = -i,
                    TryGetMemberGetter = (o, info, t) =>
                    {
                        o.ShouldEqual(ReflectionManager);
                        info.ShouldEqual(TestMethod);
                        t.ShouldEqual(result.GetType());
                        ++invokeCount;
                        if (canCreate)
                            return result;
                        return null;
                    }
                });
            }

            ReflectionManager.GetMemberGetter(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Fact]
        public void GetMemberGetterShouldThrowNoComponents() =>
            ShouldThrow<InvalidOperationException>(() => ReflectionManager.GetMemberGetter(TestMethod, typeof(Action)));

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMemberSetterShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            Func<object?, object?[], object?> result = (o, objects) => o;
            for (var i = 0; i < count; i++)
            {
                var canCreate = count - 1 == i;
                ReflectionManager.AddComponent(new TestMemberReflectionDelegateProviderComponent
                {
                    Priority = -i,
                    TryGetMemberSetter = (o, info, t) =>
                    {
                        o.ShouldEqual(ReflectionManager);
                        info.ShouldEqual(TestMethod);
                        t.ShouldEqual(result.GetType());
                        ++invokeCount;
                        if (canCreate)
                            return result;
                        return null;
                    }
                });
            }

            ReflectionManager.GetMemberSetter(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Fact]
        public void GetMemberSetterShouldThrowNoComponents() =>
            ShouldThrow<InvalidOperationException>(() => ReflectionManager.GetMemberSetter(TestMethod, typeof(Action)));

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMethodInvokerShouldBeHandledByComponents1(int count)
        {
            var invokeCount = 0;
            Func<object?, ItemOrArray<object?>, object?> result = (o, objects) => o;
            for (var i = 0; i < count; i++)
            {
                var canCreate = count - 1 == i;
                ReflectionManager.AddComponent(new TestMethodReflectionDelegateProviderComponent
                {
                    Priority = -i,
                    TryGetMethodInvoker = (o, info) =>
                    {
                        o.ShouldEqual(ReflectionManager);
                        info.ShouldEqual(TestMethod);
                        ++invokeCount;
                        if (canCreate)
                            return result;
                        return null;
                    }
                });
            }

            ReflectionManager.GetMethodInvoker(TestMethod).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMethodInvokerShouldBeHandledByComponents2(int count)
        {
            var invokeCount = 0;
            Func<object?, object?[], object?> result = (o, objects) => o;
            for (var i = 0; i < count; i++)
            {
                var canCreate = count - 1 == i;
                ReflectionManager.AddComponent(new TestMethodReflectionDelegateProviderComponent
                {
                    Priority = -i,
                    TryGetMethodInvoker1 = (o, info, t) =>
                    {
                        o.ShouldEqual(ReflectionManager);
                        info.ShouldEqual(TestMethod);
                        t.ShouldEqual(result.GetType());
                        ++invokeCount;
                        if (canCreate)
                            return result;
                        return null;
                    }
                });
            }

            ReflectionManager.GetMethodInvoker(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Fact]
        public void GetMethodInvokerShouldThrowNoComponents1() =>
            ShouldThrow<InvalidOperationException>(() => ReflectionManager.GetMethodInvoker(TestMethod));

        [Fact]
        public void GetMethodInvokerShouldThrowNoComponents2() =>
            ShouldThrow<InvalidOperationException>(() => ReflectionManager.GetMethodInvoker(TestMethod, typeof(Action)));

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryCreateDelegateShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var delType = typeof(Action);
            Action result = () => { };
            for (var i = 0; i < count; i++)
            {
                var canCreate = count - 1 == i;
                ReflectionManager.AddComponent(new TestReflectionDelegateProviderComponent
                {
                    Priority = -i,
                    TryCreateDelegate = (o, type, target, info) =>
                    {
                        o.ShouldEqual(ReflectionManager);
                        target.ShouldEqual(this);
                        type.ShouldEqual(delType);
                        info.ShouldEqual(TestMethod);
                        ++invokeCount;
                        if (canCreate)
                            return result;
                        return null;
                    }
                });
            }

            ReflectionManager.TryCreateDelegate(delType, this, TestMethod).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Fact]
        public void TryCreateDelegateShouldReturnNullNoComponents() =>
            ReflectionManager.TryCreateDelegate(typeof(Action), this, TestMethod).ShouldBeNull();

        protected override IReflectionManager GetReflectionManager() => GetComponentOwner(ComponentCollectionManager);

        protected override ReflectionManager GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(componentCollectionManager);
    }
}