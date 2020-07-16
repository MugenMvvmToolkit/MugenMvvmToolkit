using System;
using System.Reflection;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface IReflectionDelegateProviderComponent : IComponent<IReflectionManager>
    {
        bool CanCreateDelegate(IReflectionManager reflectionManager, Type delegateType, MethodInfo method);

        Delegate? TryCreateDelegate(IReflectionManager reflectionManager, Type delegateType, object? target, MethodInfo method);
    }
}