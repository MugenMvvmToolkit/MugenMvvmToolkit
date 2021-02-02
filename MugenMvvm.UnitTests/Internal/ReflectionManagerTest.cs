using System;
using System.Reflection;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Internal;
using MugenMvvm.UnitTests.Components;
using MugenMvvm.UnitTests.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Internal
{
    public class ReflectionManagerTest : ComponentOwnerTestBase<ReflectionManager>
    {
        public static readonly MethodInfo TestMethod = typeof(ReflectionManagerTest).GetMethod(nameof(CanCreateDelegateShouldReturnFalseNoComponents))!;
        public static readonly ConstructorInfo TestConstructor = typeof(ReflectionManagerTest).GetConstructor(new Type[0])!;

        [Fact]
        public void CanCreateDelegateShouldReturnFalseNoComponents() => GetComponentOwner(ComponentCollectionManager).CanCreateDelegate(typeof(Action), TestMethod).ShouldBeFalse();

        [Fact]
        public void GetActivatorShouldThrowNoComponents1() => ShouldThrow<InvalidOperationException>(() => GetComponentOwner(ComponentCollectionManager).GetActivator(TestConstructor));

        [Fact]
        public void GetActivatorShouldThrowNoComponents2() => ShouldThrow<InvalidOperationException>(() => GetComponentOwner(ComponentCollectionManager).GetActivator(TestConstructor, typeof(Action)));

        [Fact]
        public void GetMemberGetterShouldThrowNoComponents() => ShouldThrow<InvalidOperationException>(() => GetComponentOwner(ComponentCollectionManager).GetMemberGetter(TestMethod, typeof(Action)));

        [Fact]
        public void GetMemberSetterShouldThrowNoComponents() => ShouldThrow<InvalidOperationException>(() => GetComponentOwner(ComponentCollectionManager).GetMemberSetter(TestMethod, typeof(Action)));

        [Fact]
        public void GetMethodInvokerShouldThrowNoComponents1() => ShouldThrow<InvalidOperationException>(() => GetComponentOwner(ComponentCollectionManager).GetMethodInvoker(TestMethod));

        [Fact]
        public void GetMethodInvokerShouldThrowNoComponents2() =>
            ShouldThrow<InvalidOperationException>(() => GetComponentOwner(ComponentCollectionManager).GetMethodInvoker(TestMethod, typeof(Action)));

        [Fact]
        public void TryCreateDelegateShouldReturnNullNoComponents() => GetComponentOwner(ComponentCollectionManager).TryCreateDelegate(typeof(Action), this, TestMethod).ShouldBeNull();

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void CanCreateDelegateShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var delType = typeof(Action);
            var reflectionManager = GetComponentOwner(ComponentCollectionManager);
            for (var i = 0; i < count; i++)
            {
                var canCreate = count - 1 == i;
                var component = new TestReflectionDelegateProviderComponent(reflectionManager);
                component.Priority = -i;
                component.CanCreateDelegate = (type, info) =>
                {
                    ++invokeCount;
                    type.ShouldEqual(delType);
                    info.ShouldEqual(TestMethod);
                    return canCreate;
                };
                reflectionManager.AddComponent(component);
            }

            reflectionManager.CanCreateDelegate(delType, TestMethod).ShouldEqual(true);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void TryCreateDelegateShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var delType = typeof(Action);
            var reflectionManager = GetComponentOwner(ComponentCollectionManager);
            Action result = () => { };
            for (var i = 0; i < count; i++)
            {
                var canCreate = count - 1 == i;
                var component = new TestReflectionDelegateProviderComponent(reflectionManager);
                component.Priority = -i;
                component.TryCreateDelegate = (type, target, info) =>
                {
                    ++invokeCount;
                    target.ShouldEqual(this);
                    type.ShouldEqual(delType);
                    info.ShouldEqual(TestMethod);
                    if (canCreate)
                        return result;
                    return null;
                };
                reflectionManager.AddComponent(component);
            }

            reflectionManager.TryCreateDelegate(delType, this, TestMethod).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetActivatorShouldBeHandledByComponents1(int count)
        {
            var invokeCount = 0;
            var reflectionManager = GetComponentOwner(ComponentCollectionManager);
            Func<ItemOrArray<object?>, object>? result = objects => objects;
            for (var i = 0; i < count; i++)
            {
                var canCreate = count - 1 == i;
                var component = new TestActivatorReflectionDelegateProviderComponent(reflectionManager);
                component.Priority = -i;
                component.TryGetActivator = info =>
                {
                    ++invokeCount;
                    info.ShouldEqual(TestConstructor);
                    if (canCreate)
                        return result;
                    return null;
                };
                reflectionManager.AddComponent(component);
            }

            reflectionManager.GetActivator(TestConstructor).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetActivatorShouldBeHandledByComponents2(int count)
        {
            var invokeCount = 0;
            var delType = typeof(Action);
            var reflectionManager = GetComponentOwner(ComponentCollectionManager);
            Func<object?[], object>? result = objects => objects;
            for (var i = 0; i < count; i++)
            {
                var canCreate = count - 1 == i;
                var component = new TestActivatorReflectionDelegateProviderComponent(reflectionManager);
                component.Priority = -i;
                component.TryGetActivator1 = (info, type) =>
                {
                    ++invokeCount;
                    info.ShouldEqual(TestConstructor);
                    type.ShouldEqual(delType.GetType());
                    if (canCreate)
                        return result;
                    return null;
                };
                reflectionManager.AddComponent(component);
            }

            reflectionManager.GetActivator(TestConstructor, delType.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMethodInvokerShouldBeHandledByComponents1(int count)
        {
            var invokeCount = 0;
            var reflectionManager = GetComponentOwner(ComponentCollectionManager);
            Func<object?, ItemOrArray<object?>, object?> result = (o, objects) => o;
            for (var i = 0; i < count; i++)
            {
                var canCreate = count - 1 == i;
                var component = new TestMethodReflectionDelegateProviderComponent(reflectionManager);
                component.Priority = -i;
                component.TryGetMethodInvoker = info =>
                {
                    ++invokeCount;
                    info.ShouldEqual(TestMethod);
                    if (canCreate)
                        return result;
                    return null;
                };
                reflectionManager.AddComponent(component);
            }

            reflectionManager.GetMethodInvoker(TestMethod).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMethodInvokerShouldBeHandledByComponents2(int count)
        {
            var invokeCount = 0;
            var reflectionManager = GetComponentOwner(ComponentCollectionManager);
            Func<object?, object?[], object?> result = (o, objects) => o;
            for (var i = 0; i < count; i++)
            {
                var canCreate = count - 1 == i;
                var component = new TestMethodReflectionDelegateProviderComponent(reflectionManager);
                component.Priority = -i;
                component.TryGetMethodInvoker1 = (info, t) =>
                {
                    ++invokeCount;
                    info.ShouldEqual(TestMethod);
                    t.ShouldEqual(result.GetType());
                    if (canCreate)
                        return result;
                    return null;
                };
                reflectionManager.AddComponent(component);
            }

            reflectionManager.GetMethodInvoker(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMemberGetterShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var reflectionManager = GetComponentOwner(ComponentCollectionManager);
            Func<object?, object?[], object?> result = (o, objects) => o;
            for (var i = 0; i < count; i++)
            {
                var canCreate = count - 1 == i;
                var component = new TestMemberReflectionDelegateProviderComponent(reflectionManager);
                component.Priority = -i;
                component.TryGetMemberGetter = (info, t) =>
                {
                    ++invokeCount;
                    info.ShouldEqual(TestMethod);
                    t.ShouldEqual(result.GetType());
                    if (canCreate)
                        return result;
                    return null;
                };
                reflectionManager.AddComponent(component);
            }

            reflectionManager.GetMemberGetter(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(10)]
        public void GetMemberSetterShouldBeHandledByComponents(int count)
        {
            var invokeCount = 0;
            var reflectionManager = GetComponentOwner(ComponentCollectionManager);
            Func<object?, object?[], object?> result = (o, objects) => o;
            for (var i = 0; i < count; i++)
            {
                var canCreate = count - 1 == i;
                var component = new TestMemberReflectionDelegateProviderComponent(reflectionManager);
                component.Priority = -i;
                component.TryGetMemberSetter = (info, t) =>
                {
                    ++invokeCount;
                    info.ShouldEqual(TestMethod);
                    t.ShouldEqual(result.GetType());
                    if (canCreate)
                        return result;
                    return null;
                };
                reflectionManager.AddComponent(component);
            }

            reflectionManager.GetMemberSetter(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(count);
        }

        protected override ReflectionManager GetComponentOwner(IComponentCollectionManager? componentCollectionManager = null) => new(componentCollectionManager);
    }
}