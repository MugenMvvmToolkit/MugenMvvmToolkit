using System;
using System.Reflection;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface IMemberReflectionDelegateProviderComponent : IComponent<IReflectionManager>
    {
        Delegate? TryGetMemberGetter(IReflectionManager reflectionManager, MemberInfo member, Type delegateType);

        Delegate? TryGetMemberSetter(IReflectionManager reflectionManager, MemberInfo member, Type delegateType);
    }
}