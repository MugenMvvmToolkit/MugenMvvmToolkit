using System;
using System.Collections.Generic;
using System.Reflection;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Internal.Components;
using Should;
using Xunit;

namespace MugenMvvm.UnitTest.Internal
{
    public class CacheReflectionDelegateProviderDecoratorTest : UnitTestBase
    {
        #region Fields

        public static readonly MethodInfo TestMethod = typeof(ReflectionDelegateProviderTest).GetMethod(nameof(GetHashCode));
        public static readonly ConstructorInfo TestConstructor = typeof(ReflectionDelegateProviderTest).GetConstructor(new Type[0]);

        #endregion

        #region Methods

        [Fact]
        public void TryGetActivatorShouldUseCache1()
        {
            var invokeCount = 0;
            Func<object?[], object> result = objects => objects;
            var providerComponent = new TestActivatorReflectionDelegateProviderComponent();
            providerComponent.TryGetActivator = info =>
            {
                ++invokeCount;
                info.ShouldEqual(TestConstructor);
                return result;
            };
            var cacheComponent = new CacheReflectionDelegateProviderDecorator();

            ((IDecoratorComponentCollectionComponent<IActivatorReflectionDelegateProviderComponent>)cacheComponent)
                .Decorate(new List<IActivatorReflectionDelegateProviderComponent> { cacheComponent, providerComponent }, DefaultMetadata);

            cacheComponent.TryGetActivator(TestConstructor).ShouldEqual(result);
            cacheComponent.TryGetActivator(TestConstructor).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            cacheComponent.Invalidate<object?>(null, DefaultMetadata);
            invokeCount = 0;
            cacheComponent.TryGetActivator(TestConstructor).ShouldEqual(result);
            cacheComponent.TryGetActivator(TestConstructor).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetActivatorShouldUseCache2()
        {
            var invokeCount = 0;
            Func<object?[], object> result = objects => objects;
            var providerComponent = new TestActivatorReflectionDelegateProviderComponent();
            providerComponent.TryGetActivator1 = (info, t) =>
            {
                ++invokeCount;
                t.ShouldEqual(result.GetType());
                info.ShouldEqual(TestConstructor);
                return result;
            };
            var cacheComponent = new CacheReflectionDelegateProviderDecorator();

            ((IDecoratorComponentCollectionComponent<IActivatorReflectionDelegateProviderComponent>)cacheComponent)
                .Decorate(new List<IActivatorReflectionDelegateProviderComponent> { cacheComponent, providerComponent }, DefaultMetadata);

            cacheComponent.TryGetActivator(TestConstructor, result.GetType()).ShouldEqual(result);
            cacheComponent.TryGetActivator(TestConstructor, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            cacheComponent.Invalidate<object?>(null, DefaultMetadata);
            invokeCount = 0;
            cacheComponent.TryGetActivator(TestConstructor, result.GetType()).ShouldEqual(result);
            cacheComponent.TryGetActivator(TestConstructor, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMemberGetterShouldUseCache()
        {
            var invokeCount = 0;
            Func<object?[], object> result = objects => objects;
            var providerComponent = new TestMemberReflectionDelegateProviderComponent();
            providerComponent.TryGetMemberGetter = (info, t) =>
            {
                ++invokeCount;
                t.ShouldEqual(result.GetType());
                info.ShouldEqual(TestMethod);
                return result;
            };
            var cacheComponent = new CacheReflectionDelegateProviderDecorator();

            ((IDecoratorComponentCollectionComponent<IMemberReflectionDelegateProviderComponent>)cacheComponent)
                .Decorate(new List<IMemberReflectionDelegateProviderComponent> { cacheComponent, providerComponent }, DefaultMetadata);

            cacheComponent.TryGetMemberGetter(TestMethod, result.GetType()).ShouldEqual(result);
            cacheComponent.TryGetMemberGetter(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            cacheComponent.Invalidate<object?>(null, DefaultMetadata);
            invokeCount = 0;
            cacheComponent.TryGetMemberGetter(TestMethod, result.GetType()).ShouldEqual(result);
            cacheComponent.TryGetMemberGetter(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMemberSetterShouldUseCache()
        {
            var invokeCount = 0;
            Func<object?[], object> result = objects => objects;
            var providerComponent = new TestMemberReflectionDelegateProviderComponent();
            providerComponent.TryGetMemberSetter = (info, t) =>
            {
                ++invokeCount;
                t.ShouldEqual(result.GetType());
                info.ShouldEqual(TestMethod);
                return result;
            };
            var cacheComponent = new CacheReflectionDelegateProviderDecorator();

            ((IDecoratorComponentCollectionComponent<IMemberReflectionDelegateProviderComponent>)cacheComponent)
                .Decorate(new List<IMemberReflectionDelegateProviderComponent> { cacheComponent, providerComponent }, DefaultMetadata);

            cacheComponent.TryGetMemberSetter(TestMethod, result.GetType()).ShouldEqual(result);
            cacheComponent.TryGetMemberSetter(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            cacheComponent.Invalidate<object?>(null, DefaultMetadata);
            invokeCount = 0;
            cacheComponent.TryGetMemberSetter(TestMethod, result.GetType()).ShouldEqual(result);
            cacheComponent.TryGetMemberSetter(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMethodInvokerShouldUseCache1()
        {
            var invokeCount = 0;
            Func<object?, object?[], object?> result = (o, objects) => o;
            var providerComponent = new TestMethodReflectionDelegateProviderComponent();
            providerComponent.TryGetMethodInvoker = info =>
            {
                ++invokeCount;
                info.ShouldEqual(TestMethod);
                return result;
            };
            var cacheComponent = new CacheReflectionDelegateProviderDecorator();

            ((IDecoratorComponentCollectionComponent<IMethodReflectionDelegateProviderComponent>)cacheComponent)
                .Decorate(new List<IMethodReflectionDelegateProviderComponent> { cacheComponent, providerComponent }, DefaultMetadata);

            cacheComponent.TryGetMethodInvoker(TestMethod).ShouldEqual(result);
            cacheComponent.TryGetMethodInvoker(TestMethod).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            cacheComponent.Invalidate<object?>(null, DefaultMetadata);
            invokeCount = 0;
            cacheComponent.TryGetMethodInvoker(TestMethod).ShouldEqual(result);
            cacheComponent.TryGetMethodInvoker(TestMethod).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMethodInvokerShouldUseCache2()
        {
            var invokeCount = 0;
            Func<object?, object?[], object?> result = (o, objects) => o;
            var providerComponent = new TestMethodReflectionDelegateProviderComponent();
            providerComponent.TryGetMethodInvoker1 = (info, t) =>
            {
                ++invokeCount;
                t.ShouldEqual(result.GetType());
                info.ShouldEqual(TestMethod);
                return result;
            };
            var cacheComponent = new CacheReflectionDelegateProviderDecorator();

            ((IDecoratorComponentCollectionComponent<IMethodReflectionDelegateProviderComponent>)cacheComponent)
                .Decorate(new List<IMethodReflectionDelegateProviderComponent> { cacheComponent, providerComponent }, DefaultMetadata);

            cacheComponent.TryGetMethodInvoker(TestMethod, result.GetType()).ShouldEqual(result);
            cacheComponent.TryGetMethodInvoker(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            cacheComponent.Invalidate<object?>(null, DefaultMetadata);
            invokeCount = 0;
            cacheComponent.TryGetMethodInvoker(TestMethod, result.GetType()).ShouldEqual(result);
            cacheComponent.TryGetMethodInvoker(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        #endregion
    }
}