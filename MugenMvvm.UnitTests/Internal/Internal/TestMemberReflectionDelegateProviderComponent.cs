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
        private readonly IReflectionManager? _reflectionManager;

        public TestMemberReflectionDelegateProviderComponent(IReflectionManager? reflectionManager)
        {
            _reflectionManager = reflectionManager;
        }

        public Func<MemberInfo, Type, Delegate?>? TryGetMemberGetter { get; set; }

        public Func<MemberInfo, Type, Delegate?>? TryGetMemberSetter { get; set; }

        public int Priority { get; set; }

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
    }
}