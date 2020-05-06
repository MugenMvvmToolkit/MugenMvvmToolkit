using System;
using System.Reflection;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Internal.Internal
{
    public class TestMethodReflectionDelegateProviderComponent : IMethodReflectionDelegateProviderComponent, IHasPriority
    {
        #region Properties

        public Func<MethodInfo, Func<object?, object?[], object?>?>? TryGetMethodInvoker { get; set; }

        public Func<MethodInfo, Type, Delegate?>? TryGetMethodInvoker1 { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        Func<object?, object?[], object?>? IMethodReflectionDelegateProviderComponent.TryGetMethodInvoker(MethodInfo method)
        {
            return TryGetMethodInvoker?.Invoke(method);
        }

        Delegate? IMethodReflectionDelegateProviderComponent.TryGetMethodInvoker(MethodInfo method, Type delegateType)
        {
            return TryGetMethodInvoker1?.Invoke(method, delegateType);
        }

        #endregion
    }
}