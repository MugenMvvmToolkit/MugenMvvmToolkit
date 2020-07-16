using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Extensions.Components
{
    public static class InternalComponentExtensions
    {
        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IAttachedValueProviderComponent? TryGetProvider(this IAttachedValueProviderComponent[] components, IAttachedValueManager attachedValueManager, object item, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(attachedValueManager, nameof(attachedValueManager));
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(item, nameof(item));
            for (var i = 0; i < components.Length; i++)
            {
                var provider = components[i];
                if (provider.IsSupported(attachedValueManager, item, metadata))
                    return provider;
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<object?[], object>? TryGetActivator(this IActivatorReflectionDelegateProviderComponent[] components, IReflectionManager reflectionManager, ConstructorInfo constructor)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(reflectionManager, nameof(reflectionManager));
            Should.NotBeNull(constructor, nameof(constructor));
            for (var i = 0; i < components.Length; i++)
            {
                var activator = components[i].TryGetActivator(reflectionManager, constructor);
                if (activator != null)
                    return activator;
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Delegate? TryGetActivator(this IActivatorReflectionDelegateProviderComponent[] components, IReflectionManager reflectionManager, ConstructorInfo constructor, Type delegateType)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(reflectionManager, nameof(reflectionManager));
            Should.NotBeNull(constructor, nameof(constructor));
            Should.NotBeNull(delegateType, nameof(delegateType));
            for (var i = 0; i < components.Length; i++)
            {
                var activator = components[i].TryGetActivator(reflectionManager, constructor, delegateType);
                if (activator != null)
                    return activator;
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Delegate? TryGetMemberGetter(this IMemberReflectionDelegateProviderComponent[] components, IReflectionManager reflectionManager, MemberInfo member, Type delegateType)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(reflectionManager, nameof(reflectionManager));
            Should.NotBeNull(member, nameof(member));
            Should.NotBeNull(delegateType, nameof(delegateType));
            for (var i = 0; i < components.Length; i++)
            {
                var getter = components[i].TryGetMemberGetter(reflectionManager, member, delegateType);
                if (getter != null)
                    return getter;
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Delegate? TryGetMemberSetter(this IMemberReflectionDelegateProviderComponent[] components, IReflectionManager reflectionManager, MemberInfo member, Type delegateType)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(reflectionManager, nameof(reflectionManager));
            Should.NotBeNull(member, nameof(member));
            Should.NotBeNull(delegateType, nameof(delegateType));
            for (var i = 0; i < components.Length; i++)
            {
                var setter = components[i].TryGetMemberSetter(reflectionManager, member, delegateType);
                if (setter != null)
                    return setter;
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Func<object?, object?[], object?>? TryGetMethodInvoker(this IMethodReflectionDelegateProviderComponent[] components, IReflectionManager reflectionManager, MethodInfo method)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(reflectionManager, nameof(reflectionManager));
            Should.NotBeNull(method, nameof(method));
            for (var i = 0; i < components.Length; i++)
            {
                var invoker = components[i].TryGetMethodInvoker(reflectionManager, method);
                if (invoker != null)
                    return invoker;
            }

            return null;
        }

        public static Delegate? TryGetMethodInvoker(this IMethodReflectionDelegateProviderComponent[] components, IReflectionManager reflectionManager, MethodInfo method, Type delegateType)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(reflectionManager, nameof(reflectionManager));
            Should.NotBeNull(method, nameof(method));
            Should.NotBeNull(delegateType, nameof(delegateType));
            for (var i = 0; i < components.Length; i++)
            {
                var invoker = components[i].TryGetMethodInvoker(reflectionManager, method, delegateType);
                if (invoker != null)
                    return invoker;
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CanCreateDelegate(this IReflectionDelegateProviderComponent[] components, IReflectionManager reflectionManager, Type delegateType, MethodInfo method)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(reflectionManager, nameof(reflectionManager));
            Should.NotBeNull(delegateType, nameof(delegateType));
            Should.NotBeNull(method, nameof(method));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].CanCreateDelegate(reflectionManager, delegateType, method))
                    return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Delegate? TryCreateDelegate(this IReflectionDelegateProviderComponent[] components, IReflectionManager reflectionManager, Type delegateType, object? target, MethodInfo method)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(reflectionManager, nameof(reflectionManager));
            Should.NotBeNull(delegateType, nameof(delegateType));
            Should.NotBeNull(method, nameof(method));
            for (var i = 0; i < components.Length; i++)
            {
                var value = components[i].TryCreateDelegate(reflectionManager, delegateType, target, method);
                if (value != null)
                    return value;
            }

            return null;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool CanTrace(this ITracerComponent[] components, ITracer tracer, TraceLevel level, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(tracer, nameof(tracer));
            Should.NotBeNull(level, nameof(level));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].CanTrace(tracer, level, metadata))
                    return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Trace(this ITracerComponent[] components, ITracer tracer, TraceLevel level, string message, Exception? exception, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(tracer, nameof(tracer));
            Should.NotBeNull(level, nameof(level));
            Should.NotBeNull(message, nameof(message));
            for (var i = 0; i < components.Length; i++)
                components[i].Trace(tracer, level, message, exception, metadata);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IWeakReference? TryGetWeakReference(this IWeakReferenceProviderComponent[] components, IWeakReferenceManager weakReferenceManager, object item, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(weakReferenceManager, nameof(weakReferenceManager));
            Should.NotBeNull(item, nameof(item));
            for (var i = 0; i < components.Length; i++)
            {
                var weakReference = components[i].TryGetWeakReference(weakReferenceManager, item, metadata);
                if (weakReference != null)
                    return weakReference;
            }

            return null;
        }

        #endregion
    }
}