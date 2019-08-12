using System;
using System.Reflection;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Internal.Components
{
    public interface IMemberReflectionDelegateProviderComponent : IComponent<IReflectionDelegateProvider>
    {
        Func<object?, TType>? TryGetMemberGetter<TType>(MemberInfo member);

        Action<object?, TType>? TryGetMemberSetter<TType>(MemberInfo member);
    }
}