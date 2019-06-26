using System;
using System.Reflection;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IReflectionDelegateProviderComponent : IComponent<IReflectionDelegateProvider>
    {
        bool CanCreateDelegate(Type delegateType, MethodInfo method);

        Delegate? TryCreateDelegate(Type delegateType, object? target, MethodInfo method);

        Func<object?[], object>? TryGetActivator(ConstructorInfo constructor);

        Func<object?, object?[], object?>? TryGetMethodInvoker(MethodInfo method);

        Delegate? TryGetMethodInvoker(Type delegateType, MethodInfo method);

        Func<object?, TType>? TryGetMemberGetter<TType>(MemberInfo member);

        Action<object?, TType>? TryGetMemberSetter<TType>(MemberInfo member);
    }
}