using System;
using System.Reflection;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface IActivatorReflectionDelegateProviderComponent : IComponent<IReflectionDelegateProvider>
    {
        Func<object?[], object>? TryGetActivator(ConstructorInfo constructor);

        Delegate? TryGetActivator(ConstructorInfo constructor, Type delegateType);
    }
}