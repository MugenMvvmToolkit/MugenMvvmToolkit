﻿using System;
using System.Reflection;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface IMethodReflectionDelegateProviderComponent : IComponent<IReflectionManager>
    {
        Func<object?, ItemOrArray<object?>, object?>? TryGetMethodInvoker(IReflectionManager reflectionManager, MethodInfo method);

        Delegate? TryGetMethodInvoker(IReflectionManager reflectionManager, MethodInfo method, Type delegateType);
    }
}