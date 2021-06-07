using System;
using System.Reflection;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Internal
{
    public class TestActivatorReflectionDelegateProviderComponent : IActivatorReflectionDelegateProviderComponent, IHasPriority
    {
        public Func<IReflectionManager, ConstructorInfo, Func<ItemOrArray<object?>, object>?>? TryGetActivator { get; set; }

        public Func<IReflectionManager, ConstructorInfo, Type, Delegate?>? TryGetActivator1 { get; set; }

        public int Priority { get; set; }

        Func<ItemOrArray<object?>, object>? IActivatorReflectionDelegateProviderComponent.TryGetActivator(IReflectionManager reflectionManager, ConstructorInfo constructor) =>
            TryGetActivator?.Invoke(reflectionManager, constructor);

        Delegate? IActivatorReflectionDelegateProviderComponent.TryGetActivator(IReflectionManager reflectionManager, ConstructorInfo constructor, Type delegateType) =>
            TryGetActivator1?.Invoke(reflectionManager, constructor, delegateType);
    }
}