using System;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;

namespace MugenMvvm.Internal
{
    public sealed class ReflectionManager : ComponentOwnerBase<IReflectionManager>, IReflectionManager
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public ReflectionManager(IComponentCollectionManager? componentCollectionManager = null) : base(componentCollectionManager)
        {
        }

        #endregion

        #region Implementation of interfaces

        public bool CanCreateDelegate(Type delegateType, MethodInfo method)
        {
            return GetComponents<IReflectionDelegateProviderComponent>().CanCreateDelegate(delegateType, method);
        }

        public Delegate? TryCreateDelegate(Type delegateType, object? target, MethodInfo method)
        {
            return GetComponents<IReflectionDelegateProviderComponent>().TryCreateDelegate(delegateType, target, method);
        }

        public Func<object?[], object>? TryGetActivator(ConstructorInfo constructor)
        {
            return GetComponents<IActivatorReflectionDelegateProviderComponent>().TryGetActivator(constructor);
        }

        public Delegate? TryGetActivator(ConstructorInfo constructor, Type delegateType)
        {
            return GetComponents<IActivatorReflectionDelegateProviderComponent>().TryGetActivator(constructor, delegateType);
        }

        public Func<object?, object?[], object?>? TryGetMethodInvoker(MethodInfo method)
        {
            return GetComponents<IMethodReflectionDelegateProviderComponent>().TryGetMethodInvoker(method);
        }

        public Delegate? TryGetMethodInvoker(MethodInfo method, Type delegateType)
        {
            return GetComponents<IMethodReflectionDelegateProviderComponent>().TryGetMethodInvoker(method, delegateType);
        }

        public Delegate? TryGetMemberGetter(MemberInfo member, Type delegateType)
        {
            return GetComponents<IMemberReflectionDelegateProviderComponent>().TryGetMemberGetter(member, delegateType);
        }

        public Delegate? TryGetMemberSetter(MemberInfo member, Type delegateType)
        {
            return GetComponents<IMemberReflectionDelegateProviderComponent>().TryGetMemberSetter(member, delegateType);
        }

        #endregion
    }
}