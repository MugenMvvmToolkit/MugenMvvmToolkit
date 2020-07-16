using System;
using System.Reflection;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.Internal.Internal
{
    public class TestReflectionDelegateProviderComponent : IReflectionDelegateProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IReflectionManager? _reflectionManager;

        #endregion

        #region Constructors

        public TestReflectionDelegateProviderComponent(IReflectionManager? reflectionManager)
        {
            _reflectionManager = reflectionManager;
        }

        #endregion

        #region Properties

        public Func<Type, MethodInfo, bool>? CanCreateDelegate { get; set; }

        public Func<Type, object?, MethodInfo, Delegate?>? TryCreateDelegate { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        bool IReflectionDelegateProviderComponent.CanCreateDelegate(IReflectionManager reflectionManager, Type delegateType, MethodInfo method)
        {
            _reflectionManager?.ShouldEqual(reflectionManager);
            return CanCreateDelegate?.Invoke(delegateType, method) ?? false;
        }

        Delegate? IReflectionDelegateProviderComponent.TryCreateDelegate(IReflectionManager reflectionManager, Type delegateType, object? target, MethodInfo method)
        {
            _reflectionManager?.ShouldEqual(reflectionManager);
            return TryCreateDelegate?.Invoke(delegateType, target, method);
        }

        #endregion
    }
}