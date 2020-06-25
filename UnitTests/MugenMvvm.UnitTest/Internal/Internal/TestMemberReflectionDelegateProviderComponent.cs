using System;
using System.Reflection;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.UnitTest.Internal.Internal
{
    public class TestMemberReflectionDelegateProviderComponent : IMemberReflectionDelegateProviderComponent, IHasPriority
    {
        #region Properties

        public Func<MemberInfo, Type, Delegate?>? TryGetMemberGetter { get; set; }

        public Func<MemberInfo, Type, Delegate?>? TryGetMemberSetter { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        Delegate? IMemberReflectionDelegateProviderComponent.TryGetMemberGetter(MemberInfo member, Type delegateType)
        {
            return TryGetMemberGetter?.Invoke(member, delegateType);
        }

        Delegate? IMemberReflectionDelegateProviderComponent.TryGetMemberSetter(MemberInfo member, Type delegateType)
        {
            return TryGetMemberSetter?.Invoke(member, delegateType);
        }

        #endregion
    }
}