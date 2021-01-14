using System;
using System.Reflection;
using MugenMvvm.Collections;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Internal.Internal
{
    public class TestActivatorReflectionDelegateProviderComponent : IActivatorReflectionDelegateProviderComponent, IHasPriority
    {
        private readonly IReflectionManager? _reflectionManager;

        public TestActivatorReflectionDelegateProviderComponent(IReflectionManager? reflectionManager)
        {
            _reflectionManager = reflectionManager;
        }

        public Func<ConstructorInfo, Func<ItemOrArray<object?>, object>?>? TryGetActivator { get; set; }

        public Func<ConstructorInfo, Type, Delegate?>? TryGetActivator1 { get; set; }

        public int Priority { get; set; }

        Func<ItemOrArray<object?>, object>? IActivatorReflectionDelegateProviderComponent.TryGetActivator(IReflectionManager reflectionManager, ConstructorInfo constructor)
        {
            _reflectionManager?.ShouldEqual(reflectionManager);
            return TryGetActivator?.Invoke(constructor);
        }

        Delegate? IActivatorReflectionDelegateProviderComponent.TryGetActivator(IReflectionManager reflectionManager, ConstructorInfo constructor, Type delegateType)
        {
            _reflectionManager?.ShouldEqual(reflectionManager);
            return TryGetActivator1?.Invoke(constructor, delegateType);
        }
    }
}