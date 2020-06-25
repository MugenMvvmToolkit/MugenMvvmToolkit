using System;
using System.Reflection;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface IReflectionDelegateProviderComponent : IComponent<IReflectionDelegateProvider>
    {
        bool CanCreateDelegate(Type delegateType, MethodInfo method);

        Delegate? TryCreateDelegate(Type delegateType, object? target, MethodInfo method);
    }
}