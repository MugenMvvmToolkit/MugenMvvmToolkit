using System;
using System.Reflection;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IReflectionDelegateProvider
    {
        IComponentCollection<IChildReflectionDelegateProvider> Providers { get; }

        Func<object?[], object> GetActivatorDelegate(ConstructorInfo constructor);

        Func<object?, object?[], object?> GetMethodDelegate(MethodInfo method);

        Delegate GetMethodDelegate(Type delegateType, MethodInfo method);

        Func<object?, TType> GetMemberGetter<TType>(MemberInfo member);

        Action<object?, TType> GetMemberSetter<TType>(MemberInfo member);
    }
}