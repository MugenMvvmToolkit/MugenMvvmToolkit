using System;
using System.Reflection;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Tests.Internal
{
    public class TestMemberReflectionDelegateProviderComponent : IMemberReflectionDelegateProviderComponent, IHasPriority
    {
        public Func<IReflectionManager, MemberInfo, Type, Delegate?>? TryGetMemberGetter { get; set; }

        public Func<IReflectionManager, MemberInfo, Type, Delegate?>? TryGetMemberSetter { get; set; }

        public int Priority { get; set; }

        Delegate? IMemberReflectionDelegateProviderComponent.TryGetMemberGetter(IReflectionManager reflectionManager, MemberInfo member, Type delegateType) =>
            TryGetMemberGetter?.Invoke(reflectionManager, member, delegateType);

        Delegate? IMemberReflectionDelegateProviderComponent.TryGetMemberSetter(IReflectionManager reflectionManager, MemberInfo member, Type delegateType) =>
            TryGetMemberSetter?.Invoke(reflectionManager, member, delegateType);
    }
}