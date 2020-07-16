using System;
using System.Reflection;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTest.Internal.Internal
{
    public class TestActivatorReflectionDelegateProviderComponent : IActivatorReflectionDelegateProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IReflectionManager? _reflectionManager;

        #endregion

        #region Constructors

        public TestActivatorReflectionDelegateProviderComponent(IReflectionManager? reflectionManager)
        {
            _reflectionManager = reflectionManager;
        }

        #endregion

        #region Properties

        public Func<ConstructorInfo, Func<object?[], object>?>? TryGetActivator { get; set; }

        public Func<ConstructorInfo, Type, Delegate?>? TryGetActivator1 { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        Func<object?[], object>? IActivatorReflectionDelegateProviderComponent.TryGetActivator(IReflectionManager reflectionManager, ConstructorInfo constructor)
        {
            _reflectionManager?.ShouldEqual(reflectionManager);
            return TryGetActivator?.Invoke(constructor);
        }

        Delegate? IActivatorReflectionDelegateProviderComponent.TryGetActivator(IReflectionManager reflectionManager, ConstructorInfo constructor, Type delegateType)
        {
            _reflectionManager?.ShouldEqual(reflectionManager);
            return TryGetActivator1?.Invoke(constructor, delegateType);
        }

        #endregion
    }
}