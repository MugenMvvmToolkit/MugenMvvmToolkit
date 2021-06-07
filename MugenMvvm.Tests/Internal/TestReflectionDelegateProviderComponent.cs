using System;
using System.Reflection;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Internal
{
    public class TestReflectionDelegateProviderComponent : IReflectionDelegateProviderComponent, IHasPriority
    {
        public Func<IReflectionManager, Type, MethodInfo, bool>? CanCreateDelegate { get; set; }

        public Func<IReflectionManager, Type, object?, MethodInfo, Delegate?>? TryCreateDelegate { get; set; }

        public int Priority { get; set; }

        bool IReflectionDelegateProviderComponent.CanCreateDelegate(IReflectionManager reflectionManager, Type delegateType, MethodInfo method) =>
            CanCreateDelegate?.Invoke(reflectionManager, delegateType, method) ?? false;

        Delegate? IReflectionDelegateProviderComponent.TryCreateDelegate(IReflectionManager reflectionManager, Type delegateType, object? target, MethodInfo method) =>
            TryCreateDelegate?.Invoke(reflectionManager, delegateType, target, method);
    }
}