using System;
using System.Reflection;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Internal
{
    public class TestMethodReflectionDelegateProviderComponent : IMethodReflectionDelegateProviderComponent, IHasPriority
    {
        public Func<IReflectionManager, MethodInfo, Func<object?, ItemOrArray<object?>, object?>?>? TryGetMethodInvoker { get; set; }

        public Func<IReflectionManager, MethodInfo, Type, Delegate?>? TryGetMethodInvoker1 { get; set; }

        public int Priority { get; set; }

        Func<object?, ItemOrArray<object?>, object?>? IMethodReflectionDelegateProviderComponent.TryGetMethodInvoker(IReflectionManager reflectionManager, MethodInfo method) =>
            TryGetMethodInvoker?.Invoke(reflectionManager, method);

        Delegate? IMethodReflectionDelegateProviderComponent.TryGetMethodInvoker(IReflectionManager reflectionManager, MethodInfo method, Type delegateType) =>
            TryGetMethodInvoker1?.Invoke(reflectionManager, method, delegateType);
    }
}