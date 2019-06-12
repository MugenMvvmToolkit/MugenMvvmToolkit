using System;
using System.Reflection;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IReflectionDelegateProvider
    {
        IComponentCollection<IChildReflectionDelegateProvider> Providers { get; }

        bool CanCreateDelegate(Type delegateType, MethodInfo method);

        Delegate? TryCreateDelegate(Type delegateType, object? target, MethodInfo method);

        Func<object?[], object> GetActivator(ConstructorInfo constructor);

        Func<object?, object?[], object?> GetMethodInvoker(MethodInfo method);

        Delegate GetMethodInvoker(Type delegateType, MethodInfo method);

        Func<object?, TType> GetMemberGetter<TType>(MemberInfo member);

        Action<object?, TType> GetMemberSetter<TType>(MemberInfo member);
    }
}