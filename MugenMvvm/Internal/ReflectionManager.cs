using System;
using System.Reflection;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
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

        public bool CanCreateDelegate(Type delegateType, MethodInfo method) => GetComponents<IReflectionDelegateProviderComponent>().CanCreateDelegate(this, delegateType, method);

        public Delegate? TryCreateDelegate(Type delegateType, object? target, MethodInfo method) => GetComponents<IReflectionDelegateProviderComponent>().TryCreateDelegate(this, delegateType, target, method);

        public Func<ItemOrArray<object?>, object>? TryGetActivator(ConstructorInfo constructor) => GetComponents<IActivatorReflectionDelegateProviderComponent>().TryGetActivator(this, constructor);

        public Delegate? TryGetActivator(ConstructorInfo constructor, Type delegateType) => GetComponents<IActivatorReflectionDelegateProviderComponent>().TryGetActivator(this, constructor, delegateType);

        public Func<object?, ItemOrArray<object?>, object?>? TryGetMethodInvoker(MethodInfo method) => GetComponents<IMethodReflectionDelegateProviderComponent>().TryGetMethodInvoker(this, method);

        public Delegate? TryGetMethodInvoker(MethodInfo method, Type delegateType) => GetComponents<IMethodReflectionDelegateProviderComponent>().TryGetMethodInvoker(this, method, delegateType);

        public Delegate? TryGetMemberGetter(MemberInfo member, Type delegateType) => GetComponents<IMemberReflectionDelegateProviderComponent>().TryGetMemberGetter(this, member, delegateType);

        public Delegate? TryGetMemberSetter(MemberInfo member, Type delegateType) => GetComponents<IMemberReflectionDelegateProviderComponent>().TryGetMemberSetter(this, member, delegateType);

        #endregion
    }
}