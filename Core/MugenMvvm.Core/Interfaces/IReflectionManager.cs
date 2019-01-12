using System;
using System.Reflection;

namespace MugenMvvm.Interfaces
{
    public interface IReflectionManager
    {
        Func<object[], object> GetActivatorDelegate(ConstructorInfo constructor);

        Func<object?, object?[], object?> GetMethodDelegate(MethodInfo method);

        Delegate GetMethodDelegate(Type delegateType, MethodInfo method);

        Func<object?, TType> GetMemberGetter<TType>(MemberInfo member);

        Action<object?, TType> GetMemberSetter<TType>(MemberInfo member);
    }
}