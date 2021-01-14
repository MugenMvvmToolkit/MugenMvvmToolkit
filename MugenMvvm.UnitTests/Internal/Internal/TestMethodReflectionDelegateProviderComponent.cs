using System;
using System.Reflection;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Internal.Internal
{
    public class TestMethodReflectionDelegateProviderComponent : IMethodReflectionDelegateProviderComponent, IHasPriority
    {
        private readonly IReflectionManager? _reflectionManager;

        public TestMethodReflectionDelegateProviderComponent(IReflectionManager? reflectionManager)
        {
            _reflectionManager = reflectionManager;
        }

        public Func<MethodInfo, Func<object?, ItemOrArray<object?>, object?>?>? TryGetMethodInvoker { get; set; }

        public Func<MethodInfo, Type, Delegate?>? TryGetMethodInvoker1 { get; set; }

        public int Priority { get; set; }

        Func<object?, ItemOrArray<object?>, object?>? IMethodReflectionDelegateProviderComponent.TryGetMethodInvoker(IReflectionManager reflectionManager, MethodInfo method)
        {
            _reflectionManager?.ShouldEqual(reflectionManager);
            return TryGetMethodInvoker?.Invoke(method);
        }

        Delegate? IMethodReflectionDelegateProviderComponent.TryGetMethodInvoker(IReflectionManager reflectionManager, MethodInfo method, Type delegateType)
        {
            _reflectionManager?.ShouldEqual(reflectionManager);
            return TryGetMethodInvoker1?.Invoke(method, delegateType);
        }
    }
}