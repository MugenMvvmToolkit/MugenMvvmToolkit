using System;
using System.Reflection;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Models;
using Should;

namespace MugenMvvm.UnitTests.Internal.Internal
{
    public class TestMemberReflectionDelegateProviderComponent : IMemberReflectionDelegateProviderComponent, IHasPriority
    {
        #region Fields

        private readonly IReflectionManager? _reflectionManager;

        #endregion

        #region Constructors

        public TestMemberReflectionDelegateProviderComponent(IReflectionManager? reflectionManager)
        {
            _reflectionManager = reflectionManager;
        }

        #endregion

        #region Properties

        public Func<MemberInfo, Type, Delegate?>? TryGetMemberGetter { get; set; }

        public Func<MemberInfo, Type, Delegate?>? TryGetMemberSetter { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        Delegate? IMemberReflectionDelegateProviderComponent.TryGetMemberGetter(IReflectionManager reflectionManager, MemberInfo member, Type delegateType)
        {
            _reflectionManager?.ShouldEqual(reflectionManager);
            return TryGetMemberGetter?.Invoke(member, delegateType);
        }

        Delegate? IMemberReflectionDelegateProviderComponent.TryGetMemberSetter(IReflectionManager reflectionManager, MemberInfo member, Type delegateType)
        {
            _reflectionManager?.ShouldEqual(reflectionManager);
            return TryGetMemberSetter?.Invoke(member, delegateType);
        }

        #endregion
    }
}