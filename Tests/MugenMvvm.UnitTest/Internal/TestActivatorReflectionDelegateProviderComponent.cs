using System;
using System.Reflection;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Internal
{
    public class TestActivatorReflectionDelegateProviderComponent : IActivatorReflectionDelegateProviderComponent, IHasPriority
    {
        #region Properties

        public Func<ConstructorInfo, Func<object?[], object>?>? TryGetActivator { get; set; }

        public Func<ConstructorInfo, Type, Delegate?>? TryGetActivator1 { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        Func<object?[], object>? IActivatorReflectionDelegateProviderComponent.TryGetActivator(ConstructorInfo constructor)
        {
            return TryGetActivator?.Invoke(constructor);
        }

        Delegate? IActivatorReflectionDelegateProviderComponent.TryGetActivator(ConstructorInfo constructor, Type delegateType)
        {
            return TryGetActivator1?.Invoke(constructor, delegateType);
        }

        #endregion
    }
}