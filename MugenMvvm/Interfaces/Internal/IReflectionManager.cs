using System;
using System.Reflection;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Internal
{
    public interface IReflectionManager : IComponentOwner<IReflectionManager>
    {
        bool CanCreateDelegate(Type delegateType, MethodInfo method);

        Delegate? TryCreateDelegate(Type delegateType, object? target, MethodInfo method);

        Func<object?[], object>? TryGetActivator(ConstructorInfo constructor);

        Delegate? TryGetActivator(ConstructorInfo constructor, Type delegateType);

        Func<object?, object?[], object?>? TryGetMethodInvoker(MethodInfo method);

        Delegate? TryGetMethodInvoker(MethodInfo method, Type delegateType);

        Delegate? TryGetMemberGetter(MemberInfo member, Type delegateType);

        Delegate? TryGetMemberSetter(MemberInfo member, Type delegateType);
    }
}