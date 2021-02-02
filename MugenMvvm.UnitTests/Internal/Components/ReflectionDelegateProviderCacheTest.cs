using System;
using System.Reflection;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using MugenMvvm.UnitTests.Internal.Internal;
using Should;
using Xunit;
using Xunit.Abstractions;

namespace MugenMvvm.UnitTests.Internal.Components
{
    public class ReflectionDelegateProviderCacheTest : UnitTestBase
    {
        public static readonly MethodInfo TestMethod = typeof(ReflectionManagerTest).GetMethod(nameof(GetHashCode))!;
        public static readonly ConstructorInfo TestConstructor = typeof(ReflectionManagerTest).GetConstructor(new Type[0])!;
        private readonly ReflectionManager _reflectionManager;

        public ReflectionDelegateProviderCacheTest(ITestOutputHelper? outputHelper = null) : base(outputHelper)
        {
            _reflectionManager = new ReflectionManager(ComponentCollectionManager);
            _reflectionManager.AddComponent(new ReflectionDelegateProviderCache());
        }

        [Fact]
        public void TryGetActivatorShouldUseCache1()
        {
            var invokeCount = 0;
            Func<ItemOrArray<object?>, object> result = objects => objects;
            _reflectionManager.AddComponent(new TestActivatorReflectionDelegateProviderComponent(_reflectionManager)
            {
                TryGetActivator = info =>
                {
                    ++invokeCount;
                    info.ShouldEqual(TestConstructor);
                    return result;
                }
            });

            _reflectionManager.TryGetActivator(TestConstructor).ShouldEqual(result);
            _reflectionManager.TryGetActivator(TestConstructor).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            _reflectionManager.TryInvalidateCache(null, DefaultMetadata);
            invokeCount = 0;
            _reflectionManager.TryGetActivator(TestConstructor).ShouldEqual(result);
            _reflectionManager.TryGetActivator(TestConstructor).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetActivatorShouldUseCache2()
        {
            var invokeCount = 0;
            Func<object?[], object> result = objects => objects;
            _reflectionManager.AddComponent(new TestActivatorReflectionDelegateProviderComponent(_reflectionManager)
            {
                TryGetActivator1 = (info, t) =>
                {
                    ++invokeCount;
                    t.ShouldEqual(result.GetType());
                    info.ShouldEqual(TestConstructor);
                    return result;
                }
            });

            _reflectionManager.TryGetActivator(TestConstructor, result.GetType()).ShouldEqual(result);
            _reflectionManager.TryGetActivator(TestConstructor, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            _reflectionManager.TryInvalidateCache(null, DefaultMetadata);
            invokeCount = 0;
            _reflectionManager.TryGetActivator(TestConstructor, result.GetType()).ShouldEqual(result);
            _reflectionManager.TryGetActivator(TestConstructor, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMemberGetterShouldUseCache()
        {
            var invokeCount = 0;
            Func<object?[], object> result = objects => objects;
            _reflectionManager.AddComponent(new TestMemberReflectionDelegateProviderComponent(_reflectionManager)
            {
                TryGetMemberGetter = (info, t) =>
                {
                    ++invokeCount;
                    t.ShouldEqual(result.GetType());
                    info.ShouldEqual(TestMethod);
                    return result;
                }
            });

            _reflectionManager.TryGetMemberGetter(TestMethod, result.GetType()).ShouldEqual(result);
            _reflectionManager.TryGetMemberGetter(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            _reflectionManager.TryInvalidateCache(null, DefaultMetadata);
            invokeCount = 0;
            _reflectionManager.TryGetMemberGetter(TestMethod, result.GetType()).ShouldEqual(result);
            _reflectionManager.TryGetMemberGetter(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMemberSetterShouldUseCache()
        {
            var invokeCount = 0;
            Func<object?[], object> result = objects => objects;
            _reflectionManager.AddComponent(new TestMemberReflectionDelegateProviderComponent(_reflectionManager)
            {
                TryGetMemberSetter = (info, t) =>
                {
                    ++invokeCount;
                    t.ShouldEqual(result.GetType());
                    info.ShouldEqual(TestMethod);
                    return result;
                }
            });

            _reflectionManager.TryGetMemberSetter(TestMethod, result.GetType()).ShouldEqual(result);
            _reflectionManager.TryGetMemberSetter(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            _reflectionManager.TryInvalidateCache(null, DefaultMetadata);
            invokeCount = 0;
            _reflectionManager.TryGetMemberSetter(TestMethod, result.GetType()).ShouldEqual(result);
            _reflectionManager.TryGetMemberSetter(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMethodInvokerShouldUseCache1()
        {
            var invokeCount = 0;
            Func<object?, ItemOrArray<object?>, object?> result = (o, objects) => o;
            _reflectionManager.AddComponent(new TestMethodReflectionDelegateProviderComponent(_reflectionManager)
            {
                TryGetMethodInvoker = info =>
                {
                    ++invokeCount;
                    info.ShouldEqual(TestMethod);
                    return result;
                }
            });

            _reflectionManager.TryGetMethodInvoker(TestMethod).ShouldEqual(result);
            _reflectionManager.TryGetMethodInvoker(TestMethod).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            _reflectionManager.TryInvalidateCache(null, DefaultMetadata);
            invokeCount = 0;
            _reflectionManager.TryGetMethodInvoker(TestMethod).ShouldEqual(result);
            _reflectionManager.TryGetMethodInvoker(TestMethod).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMethodInvokerShouldUseCache2()
        {
            var invokeCount = 0;
            Func<object?, object?[], object?> result = (o, objects) => o;
            _reflectionManager.AddComponent(new TestMethodReflectionDelegateProviderComponent(_reflectionManager)
            {
                TryGetMethodInvoker1 = (info, t) =>
                {
                    ++invokeCount;
                    t.ShouldEqual(result.GetType());
                    info.ShouldEqual(TestMethod);
                    return result;
                }
            });

            _reflectionManager.TryGetMethodInvoker(TestMethod, result.GetType()).ShouldEqual(result);
            _reflectionManager.TryGetMethodInvoker(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            _reflectionManager.TryInvalidateCache(null, DefaultMetadata);
            invokeCount = 0;
            _reflectionManager.TryGetMethodInvoker(TestMethod, result.GetType()).ShouldEqual(result);
            _reflectionManager.TryGetMethodInvoker(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }
    }
}