using System;
using System.Reflection;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Internal.Internal
{
    public class TestReflectionDelegateProviderComponent : IReflectionDelegateProviderComponent, IHasPriority
    {
        #region Properties

        public Func<Type, MethodInfo, bool>? CanCreateDelegate { get; set; }

        public Func<Type, object?, MethodInfo, Delegate?>? TryCreateDelegate { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IReflectionDelegateProviderComponent.CanCreateDelegate(Type delegateType, MethodInfo method)
        {
            return CanCreateDelegate?.Invoke(delegateType, method) ?? false;
        }

        Delegate? IReflectionDelegateProviderComponent.TryCreateDelegate(Type delegateType, object? target, MethodInfo method)
        {
            return TryCreateDelegate?.Invoke(delegateType, target, method);
        }

        #endregion
    }
}