using System;
using System.Reflection;
using MugenMvvm.Extensions;
using MugenMvvm.Internal;
using MugenMvvm.Internal.Components;
using MugenMvvm.UnitTests.Internal.Internal;
using Should;
using Xunit;

namespace MugenMvvm.UnitTests.Internal.Components
{
    public class ReflectionDelegateProviderCacheTest : UnitTestBase
    {
        #region Fields

        public static readonly MethodInfo TestMethod = typeof(ReflectionManagerTest).GetMethod(nameof(GetHashCode))!;
        public static readonly ConstructorInfo TestConstructor = typeof(ReflectionManagerTest).GetConstructor(new Type[0])!;

        #endregion

        #region Methods

        [Fact]
        public void TryGetActivatorShouldUseCache1()
        {
            var invokeCount = 0;
            Func<object?[], object> result = objects => objects;
            var manager = new ReflectionManager();
            manager.AddComponent(new TestActivatorReflectionDelegateProviderComponent(manager)
            {
                TryGetActivator = info =>
                {
                    ++invokeCount;
                    info.ShouldEqual(TestConstructor);
                    return result;
                }
            });
            manager.AddComponent(new ReflectionDelegateProviderCache());

            manager.TryGetActivator(TestConstructor).ShouldEqual(result);
            manager.TryGetActivator(TestConstructor).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            manager.TryInvalidateCache(null, DefaultMetadata);
            invokeCount = 0;
            manager.TryGetActivator(TestConstructor).ShouldEqual(result);
            manager.TryGetActivator(TestConstructor).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetActivatorShouldUseCache2()
        {
            var invokeCount = 0;
            Func<object?[], object> result = objects => objects;
            var manager = new ReflectionManager();
            manager.AddComponent(new TestActivatorReflectionDelegateProviderComponent(manager)
            {
                TryGetActivator1 = (info, t) =>
                {
                    ++invokeCount;
                    t.ShouldEqual(result.GetType());
                    info.ShouldEqual(TestConstructor);
                    return result;
                }
            });
            manager.AddComponent(new ReflectionDelegateProviderCache());

            manager.TryGetActivator(TestConstructor, result.GetType()).ShouldEqual(result);
            manager.TryGetActivator(TestConstructor, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            manager.TryInvalidateCache(null, DefaultMetadata);
            invokeCount = 0;
            manager.TryGetActivator(TestConstructor, result.GetType()).ShouldEqual(result);
            manager.TryGetActivator(TestConstructor, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMemberGetterShouldUseCache()
        {
            var invokeCount = 0;
            Func<object?[], object> result = objects => objects;
            var manager = new ReflectionManager();
            manager.AddComponent(new TestMemberReflectionDelegateProviderComponent(manager)
            {
                TryGetMemberGetter = (info, t) =>
                {
                    ++invokeCount;
                    t.ShouldEqual(result.GetType());
                    info.ShouldEqual(TestMethod);
                    return result;
                }
            });
            manager.AddComponent(new ReflectionDelegateProviderCache());

            manager.TryGetMemberGetter(TestMethod, result.GetType()).ShouldEqual(result);
            manager.TryGetMemberGetter(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            manager.TryInvalidateCache(null, DefaultMetadata);
            invokeCount = 0;
            manager.TryGetMemberGetter(TestMethod, result.GetType()).ShouldEqual(result);
            manager.TryGetMemberGetter(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMemberSetterShouldUseCache()
        {
            var invokeCount = 0;
            Func<object?[], object> result = objects => objects;
            var manager = new ReflectionManager();
            manager.AddComponent(new TestMemberReflectionDelegateProviderComponent(manager)
            {
                TryGetMemberSetter = (info, t) =>
                {
                    ++invokeCount;
                    t.ShouldEqual(result.GetType());
                    info.ShouldEqual(TestMethod);
                    return result;
                }
            });
            manager.AddComponent(new ReflectionDelegateProviderCache());

            manager.TryGetMemberSetter(TestMethod, result.GetType()).ShouldEqual(result);
            manager.TryGetMemberSetter(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            manager.TryInvalidateCache(null, DefaultMetadata);
            invokeCount = 0;
            manager.TryGetMemberSetter(TestMethod, result.GetType()).ShouldEqual(result);
            manager.TryGetMemberSetter(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMethodInvokerShouldUseCache1()
        {
            var invokeCount = 0;
            Func<object?, object?[], object?> result = (o, objects) => o;
            var manager = new ReflectionManager();
            manager.AddComponent(new TestMethodReflectionDelegateProviderComponent(manager)
            {
                TryGetMethodInvoker = info =>
                {
                    ++invokeCount;
                    info.ShouldEqual(TestMethod);
                    return result;
                }
            });
            manager.AddComponent(new ReflectionDelegateProviderCache());

            manager.TryGetMethodInvoker(TestMethod).ShouldEqual(result);
            manager.TryGetMethodInvoker(TestMethod).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            manager.TryInvalidateCache(null, DefaultMetadata);
            invokeCount = 0;
            manager.TryGetMethodInvoker(TestMethod).ShouldEqual(result);
            manager.TryGetMethodInvoker(TestMethod).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        [Fact]
        public void TryGetMethodInvokerShouldUseCache2()
        {
            var invokeCount = 0;
            Func<object?, object?[], object?> result = (o, objects) => o;
            var manager = new ReflectionManager();
            manager.AddComponent(new TestMethodReflectionDelegateProviderComponent(manager)
            {
                TryGetMethodInvoker1 = (info, t) =>
                {
                    ++invokeCount;
                    t.ShouldEqual(result.GetType());
                    info.ShouldEqual(TestMethod);
                    return result;
                }
            });
            manager.AddComponent(new ReflectionDelegateProviderCache());

            manager.TryGetMethodInvoker(TestMethod, result.GetType()).ShouldEqual(result);
            manager.TryGetMethodInvoker(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);

            manager.TryInvalidateCache(null, DefaultMetadata);
            invokeCount = 0;
            manager.TryGetMethodInvoker(TestMethod, result.GetType()).ShouldEqual(result);
            manager.TryGetMethodInvoker(TestMethod, result.GetType()).ShouldEqual(result);
            invokeCount.ShouldEqual(1);
        }

        #endregion
    }
}