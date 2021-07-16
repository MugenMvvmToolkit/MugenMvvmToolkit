using System;
using System.Reflection;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using MugenMvvm.Tests.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Internal.Components
{
    public class ReflectionDelegateProviderCacheTest : UnitTestBase
    {
        public static readonly MethodInfo TestMethod = typeof(ReflectionManagerTest).GetMethod(nameof(GetHashCode))!;
        public static readonly ConstructorInfo TestConstructor = typeof(ReflectionManagerTest).GetConstructor(Type.EmptyTypes)!;

        public ReflectionDelegateProviderCacheTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
        }

        [Fact]
        public void TryGetActivatorShouldUseCache1()
        {
            var invokeCount = 0;
            Func<ItemOrArray<object?>, object> result = objects => objects;
            ReflectionManager.AddComponent(new TestActivatorReflectionDelegateProviderComponent
            {
                TryGetActivator = (m, info) =>
                {
                    m.ShouldEqual(ReflectionManager);
                    info.ShouldEqual(TestConstructor);
                    ++invokeCount;
                    return result;
                }
            });

            ReflectionManager.TryGetActivator(TestConstructor).ShouldEqual(result);
            ReflectionManager.TryGetActivator(TestConstructor).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            ReflectionManager.TryInvalidateCache(null, Metadata);
            invokeCount = 0;
            ReflectionManager.TryGetActivator(TestConstructor).ShouldEqual(result);
            ReflectionManager.TryGetActivator(TestConstructor).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetActivatorShouldUseCache2()
        {
            var invokeCount = 0;
            Func<object?[], object> result = objects => objects;
            ReflectionManager.AddComponent(new TestActivatorReflectionDelegateProviderComponent
            {
                TryGetActivator1 = (m, info, t) =>
                {
                    t.ShouldEqual(result.GetType());
                    info.ShouldEqual(TestConstructor);
                    m.ShouldEqual(ReflectionManager);
                    ++invokeCount;
                    return result;
                }
            });

            ReflectionManager.TryGetActivator(TestConstructor, result.GetType()).ShouldEqual(result);
            ReflectionManager.TryGetActivator(TestConstructor, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            ReflectionManager.TryInvalidateCache(null, Metadata);
            invokeCount = 0;
            ReflectionManager.TryGetActivator(TestConstructor, result.GetType()).ShouldEqual(result);
            ReflectionManager.TryGetActivator(TestConstructor, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMemberGetterShouldUseCache()
        {
            var invokeCount = 0;
            Func<object?[], object> result = objects => objects;
            ReflectionManager.AddComponent(new TestMemberReflectionDelegateProviderComponent
            {
                TryGetMemberGetter = (m, info, t) =>
                {
                    m.ShouldEqual(ReflectionManager);
                    t.ShouldEqual(result.GetType());
                    info.ShouldEqual(TestMethod);
                    ++invokeCount;
                    return result;
                }
            });

            ReflectionManager.TryGetMemberGetter(TestMethod, result.GetType()).ShouldEqual(result);
            ReflectionManager.TryGetMemberGetter(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            ReflectionManager.TryInvalidateCache(null, Metadata);
            invokeCount = 0;
            ReflectionManager.TryGetMemberGetter(TestMethod, result.GetType()).ShouldEqual(result);
            ReflectionManager.TryGetMemberGetter(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMemberSetterShouldUseCache()
        {
            var invokeCount = 0;
            Func<object?[], object> result = objects => objects;
            ReflectionManager.AddComponent(new TestMemberReflectionDelegateProviderComponent
            {
                TryGetMemberSetter = (m, info, t) =>
                {
                    m.ShouldEqual(ReflectionManager);
                    t.ShouldEqual(result.GetType());
                    info.ShouldEqual(TestMethod);
                    ++invokeCount;
                    return result;
                }
            });

            ReflectionManager.TryGetMemberSetter(TestMethod, result.GetType()).ShouldEqual(result);
            ReflectionManager.TryGetMemberSetter(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            ReflectionManager.TryInvalidateCache(null, Metadata);
            invokeCount = 0;
            ReflectionManager.TryGetMemberSetter(TestMethod, result.GetType()).ShouldEqual(result);
            ReflectionManager.TryGetMemberSetter(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMethodInvokerShouldUseCache1()
        {
            var invokeCount = 0;
            Func<object?, ItemOrArray<object?>, object?> result = (o, objects) => o;
            ReflectionManager.AddComponent(new TestMethodReflectionDelegateProviderComponent
            {
                TryGetMethodInvoker = (m, info) =>
                {
                    m.ShouldEqual(ReflectionManager);
                    info.ShouldEqual(TestMethod);
                    ++invokeCount;
                    return result;
                }
            });

            ReflectionManager.TryGetMethodInvoker(TestMethod).ShouldEqual(result);
            ReflectionManager.TryGetMethodInvoker(TestMethod).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            ReflectionManager.TryInvalidateCache(null, Metadata);
            invokeCount = 0;
            ReflectionManager.TryGetMethodInvoker(TestMethod).ShouldEqual(result);
            ReflectionManager.TryGetMethodInvoker(TestMethod).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMethodInvokerShouldUseCache2()
        {
            var invokeCount = 0;
            Func<object?, object?[], object?> result = (o, objects) => o;
            ReflectionManager.AddComponent(new TestMethodReflectionDelegateProviderComponent
            {
                TryGetMethodInvoker1 = (m, info, t) =>
                {
                    m.ShouldEqual(ReflectionManager);
                    t.ShouldEqual(result.GetType());
                    info.ShouldEqual(TestMethod);
                    ++invokeCount;
                    return result;
                }
            });

            ReflectionManager.TryGetMethodInvoker(TestMethod, result.GetType()).ShouldEqual(result);
            ReflectionManager.TryGetMethodInvoker(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            ReflectionManager.TryInvalidateCache(null, Metadata);
            invokeCount = 0;
            ReflectionManager.TryGetMethodInvoker(TestMethod, result.GetType()).ShouldEqual(result);
            ReflectionManager.TryGetMethodInvoker(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        protected override IReflectionManager GetReflectionManager()
        {
            var reflectionManager = new ReflectionManager(ComponentCollectionManager);
            reflectionManager.AddComponent(new ReflectionDelegateProviderCache());
            return reflectionManager;
        }
    }
}