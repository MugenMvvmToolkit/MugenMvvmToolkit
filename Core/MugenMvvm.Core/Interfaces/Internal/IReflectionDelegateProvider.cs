using System;
using System.Reflection;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IReflectionDelegateProvider : IComponentOwner<IReflectionDelegateProvider>, IComponent<IMugenApplication>
    {
        bool CanCreateDelegate(Type delegateType, MethodInfo method);

        Delegate? TryCreateDelegate(Type delegateType, object? target, MethodInfo method);

        Func<object?[], object> GetActivator(ConstructorInfo constructor);

        Delegate GetActivator(ConstructorInfo constructor, Type delegateType);

        Func<object?, object?[], object?> GetMethodInvoker(MethodInfo method);

        Delegate GetMethodInvoker(MethodInfo method, Type delegateType);

        Delegate GetMemberGetter(MemberInfo member, Type delegateType);

        Delegate GetMemberSetter(MemberInfo member, Type delegateType);
    }
}