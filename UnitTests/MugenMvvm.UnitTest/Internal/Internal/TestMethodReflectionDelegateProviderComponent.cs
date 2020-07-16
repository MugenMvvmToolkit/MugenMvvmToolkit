using System;
using System.Reflection;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.Internal.Internal
{
    public class TestMethodReflectionDelegateProviderComponent : IMethodReflectionDelegateProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IReflectionManager? _reflectionManager;

        #endregion

        #region Constructors

        public TestMethodReflectionDelegateProviderComponent(IReflectionManager? reflectionManager)
        {
            _reflectionManager = reflectionManager;
        }

        #endregion

        #region Properties

        public Func<MethodInfo, Func<object?, object?[], object?>?>? TryGetMethodInvoker { get; set; }

        public Func<MethodInfo, Type, Delegate?>? TryGetMethodInvoker1 { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        Func<object?, object?[], object?>? IMethodReflectionDelegateProviderComponent.TryGetMethodInvoker(IReflectionManager reflectionManager, MethodInfo method)
        {
            _reflectionManager?.ShouldEqual(reflectionManager);
            return TryGetMethodInvoker?.Invoke(method);
        }

        Delegate? IMethodReflectionDelegateProviderComponent.TryGetMethodInvoker(IReflectionManager reflectionManager, MethodInfo method, Type delegateType)
        {
            _reflectionManager?.ShouldEqual(reflectionManager);
            return TryGetMethodInvoker1?.Invoke(method, delegateType);
        }

        #endregion
    }
}