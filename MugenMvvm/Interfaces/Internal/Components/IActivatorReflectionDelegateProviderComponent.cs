using System;
using System.Reflection;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface IActivatorReflectionDelegateProviderComponent : IComponent<IReflectionManager>
    {
        Func<object?[], object>? TryGetActivator(IReflectionManager reflectionManager, ConstructorInfo constructor);

        Delegate? TryGetActivator(IReflectionManager reflectionManager, ConstructorInfo constructor, Type delegateType);
    }
}