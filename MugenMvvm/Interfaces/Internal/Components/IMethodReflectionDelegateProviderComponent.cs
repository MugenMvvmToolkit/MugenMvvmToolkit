using System;
using System.Reflection;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface IMethodReflectionDelegateProviderComponent : IComponent<IReflectionManager>
    {
        Func<object?, object?[], object?>? TryGetMethodInvoker(MethodInfo method);

        Delegate? TryGetMethodInvoker(MethodInfo method, Type delegateType);
    }
}