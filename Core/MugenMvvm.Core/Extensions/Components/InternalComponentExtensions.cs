using System;
using System.Reflection;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Extensions.Components
{
    public static class InternalComponentExtensions
    {
        #region Methods

        public static IAttachedValueProvider? TryGetOrAddAttachedValueProvider(this IAttachedValueManagerComponent[] components, object item, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(item, nameof(item));
            for (var i = 0; i < components.Length; i++)
            {
                var provider = components[i].TryGetOrAddAttachedValueProvider(item, metadata);
                if (provider != null)
                    return provider;
            }

            return null;
        }

        public static bool TryGetAttachedValueProvider(this IAttachedValueManagerComponent[] components, object item, IReadOnlyMetadataContext? metadata, out IAttachedValueProvider? provider)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(item, nameof(item));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryGetAttachedValueProvider(item, metadata, out provider))
                    return true;
            }

            provider = null;
            return false;
        }

        public static Func<object?[], object>? TryGetActivator(this IActivatorReflectionDelegateProviderComponent[] components, ConstructorInfo constructor)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(constructor, nameof(constructor));
            for (var i = 0; i < components.Length; i++)
            {
                var activator = components[i].TryGetActivator(constructor);
                if (activator != null)
                    return activator;
            }

            return null;
        }

        public static Delegate? TryGetActivator(this IActivatorReflectionDelegateProviderComponent[] components, ConstructorInfo constructor, Type delegateType)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(constructor, nameof(constructor));
            Should.NotBeNull(delegateType, nameof(delegateType));
            for (var i = 0; i < components.Length; i++)
            {
                var activator = components[i].TryGetActivator(constructor, delegateType);
                if (activator != null)
                    return activator;
            }

            return null;
        }

        public static Delegate? TryGetMemberGetter(this IMemberReflectionDelegateProviderComponent[] components, MemberInfo member, Type delegateType)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(member, nameof(member));
            Should.NotBeNull(delegateType, nameof(delegateType));
            for (var i = 0; i < components.Length; i++)
            {
                var getter = components[i].TryGetMemberGetter(member, delegateType);
                if (getter != null)
                    return getter;
            }

            return null;
        }

        public static Delegate? TryGetMemberSetter(this IMemberReflectionDelegateProviderComponent[] components, MemberInfo member, Type delegateType)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(member, nameof(member));
            Should.NotBeNull(delegateType, nameof(delegateType));
            for (var i = 0; i < components.Length; i++)
            {
                var setter = components[i].TryGetMemberSetter(member, delegateType);
                if (setter != null)
                    return setter;
            }

            return null;
        }

        public static Func<object?, object?[], object?>? TryGetMethodInvoker(this IMethodReflectionDelegateProviderComponent[] components, MethodInfo method)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(method, nameof(method));
            for (var i = 0; i < components.Length; i++)
            {
                var invoker = components[i].TryGetMethodInvoker(method);
                if (invoker != null)
                    return invoker;
            }

            return null;
        }

        public static Delegate? TryGetMethodInvoker(this IMethodReflectionDelegateProviderComponent[] components, MethodInfo method, Type delegateType)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(method, nameof(method));
            Should.NotBeNull(delegateType, nameof(delegateType));
            for (var i = 0; i < components.Length; i++)
            {
                var invoker = components[i].TryGetMethodInvoker(method, delegateType);
                if (invoker != null)
                    return invoker;
            }

            return null;
        }

        public static bool CanCreateDelegate(this IReflectionDelegateProviderComponent[] components, Type delegateType, MethodInfo method)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(delegateType, nameof(delegateType));
            Should.NotBeNull(method, nameof(method));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].CanCreateDelegate(delegateType, method))
                    return true;
            }

            return false;
        }

        public static Delegate? TryCreateDelegate(this IReflectionDelegateProviderComponent[] components, Type delegateType, object? target, MethodInfo method)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(delegateType, nameof(delegateType));
            Should.NotBeNull(method, nameof(method));
            for (var i = 0; i < components.Length; i++)
            {
                var value = components[i].TryCreateDelegate(delegateType, target, method);
                if (value != null)
                    return value;
            }

            return null;
        }

        public static bool CanTrace(this ITracerComponent[] components, TraceLevel level, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(level, nameof(level));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].CanTrace(level, metadata))
                    return true;
            }

            return false;
        }

        public static void Trace(this ITracerComponent[] components, TraceLevel level, string message, Exception? exception, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(level, nameof(level));
            Should.NotBeNull(message, nameof(message));
            for (var i = 0; i < components.Length; i++)
                components[i].Trace(level, message, exception, metadata);
        }


        public static IWeakReference? TryGetWeakReference(this IWeakReferenceProviderComponent[] components, object item, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(item, nameof(item));
            for (var i = 0; i < components.Length; i++)
            {
                var weakReference = components[i].TryGetWeakReference(item, metadata);
                if (weakReference != null)
                    return weakReference;
            }

            return null;
        }

        #endregion
    }
}