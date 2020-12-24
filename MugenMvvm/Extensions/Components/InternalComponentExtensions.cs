using System;
using System.Reflection;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions.Components
{
    public static class InternalComponentExtensions
    {
        #region Methods

        public static bool IsInState<T>(this ILifecycleTrackerComponent<T>[] components, object owner, object target, T state, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(owner, nameof(owner));
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(target, nameof(target));
            if (state == null)
                Should.NotBeNull(state, nameof(state));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].IsInState(owner, target, state, metadata))
                    return true;
            }

            return false;
        }

        public static AttachedValueStorage TryGetAttachedValues(this IAttachedValueStorageProviderComponent[] components, IAttachedValueManager attachedValueManager, object item, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(attachedValueManager, nameof(attachedValueManager));
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(item, nameof(item));
            for (var i = 0; i < components.Length; i++)
            {
                var storage = components[i].TryGetAttachedValues(attachedValueManager, item, metadata);
                if (!storage.IsEmpty)
                    return storage;
            }

            return default;
        }

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

        public static ILogger? TryGetLogger(this ILoggerProviderComponent[] components, ILogger logger, object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(logger, nameof(logger));
            Should.NotBeNull(request, nameof(request));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryGetLogger(logger, request, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static bool CanLog(this ILoggerComponent[] components, ILogger logger, LogLevel level, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(logger, nameof(logger));
            Should.NotBeNull(level, nameof(level));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].CanLog(logger, level, metadata))
                    return true;
            }

            return false;
        }

        public static void Log(this ILoggerComponent[] components, ILogger logger, LogLevel level, object message, Exception? exception, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(logger, nameof(logger));
            Should.NotBeNull(level, nameof(level));
            Should.NotBeNull(message, nameof(message));
            for (var i = 0; i < components.Length; i++)
                components[i].Log(logger, level, message, exception, metadata);
        }

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