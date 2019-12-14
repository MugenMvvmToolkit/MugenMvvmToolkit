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
    public sealed class ReflectionDelegateProvider : ComponentOwnerBase<IReflectionDelegateProvider>, IReflectionDelegateProvider
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public ReflectionDelegateProvider(IComponentCollectionProvider? componentCollectionProvider = null) : base(componentCollectionProvider)
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

        public Func<object?[], object> GetActivator(ConstructorInfo constructor)
        {
            var result = GetComponents<IActivatorReflectionDelegateProviderComponent>().TryGetActivator(constructor);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this);
            return result;
        }

        public Delegate GetActivator(ConstructorInfo constructor, Type delegateType)
        {
            var result = GetComponents<IActivatorReflectionDelegateProviderComponent>().TryGetActivator(constructor, delegateType);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this);
            return result;
        }

        public Func<object?, object?[], object?> GetMethodInvoker(MethodInfo method)
        {
            var result = GetComponents<IMethodReflectionDelegateProviderComponent>().TryGetMethodInvoker(method);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this);
            return result;
        }

        public Delegate GetMethodInvoker(MethodInfo method, Type delegateType)
        {
            var result = GetComponents<IMethodReflectionDelegateProviderComponent>().TryGetMethodInvoker(method, delegateType);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this);
            return result;
        }

        public Delegate GetMemberGetter(MemberInfo member, Type delegateType)
        {
            var result = GetComponents<IMemberReflectionDelegateProviderComponent>().TryGetMemberGetter(member, delegateType);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this);
            return result;
        }

        public Delegate GetMemberSetter(MemberInfo member, Type delegateType)
        {
            var result = GetComponents<IMemberReflectionDelegateProviderComponent>().TryGetMemberSetter(member, delegateType);
            if (result == null)
                ExceptionManager.ThrowObjectNotInitialized(this);
            return result;
        }

        #endregion
    }
}