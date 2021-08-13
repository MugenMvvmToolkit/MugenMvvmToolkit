﻿using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions.Components
{
    public static class InternalComponentExtensions
    {
        public static bool IsInState<TOwner, T>(this ItemOrArray<ILifecycleTrackerComponent<TOwner, T>> components, TOwner owner, object target, T state,
            IReadOnlyMetadataContext? metadata) where TOwner : class, IComponentOwner where T : class, IEnum
        {
            Should.NotBeNull(owner, nameof(owner));
            Should.NotBeNull(target, nameof(target));
            Should.NotBeNull(state, nameof(state));
            foreach (var c in components)
            {
                if (c.IsInState(owner, target, state, metadata))
                    return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static AttachedValueStorage TryGetAttachedValues(this ItemOrArray<IAttachedValueStorageProviderComponent> components, IAttachedValueManager attachedValueManager,
            object item, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(attachedValueManager, nameof(attachedValueManager));
            Should.NotBeNull(item, nameof(item));
            foreach (var c in components)
            {
                var storage = c.TryGetAttachedValues(attachedValueManager, item, metadata);
                if (!storage.IsEmpty)
                    return storage;
            }

            return default;
        }

        public static Func<ItemOrArray<object?>, object>? TryGetActivator(this ItemOrArray<IActivatorReflectionDelegateProviderComponent> components,
            IReflectionManager reflectionManager, ConstructorInfo constructor)
        {
            Should.NotBeNull(reflectionManager, nameof(reflectionManager));
            Should.NotBeNull(constructor, nameof(constructor));
            foreach (var c in components)
            {
                var activator = c.TryGetActivator(reflectionManager, constructor);
                if (activator != null)
                    return activator;
            }

            return null;
        }

        public static Delegate? TryGetActivator(this ItemOrArray<IActivatorReflectionDelegateProviderComponent> components, IReflectionManager reflectionManager,
            ConstructorInfo constructor, Type delegateType)
        {
            Should.NotBeNull(reflectionManager, nameof(reflectionManager));
            Should.NotBeNull(constructor, nameof(constructor));
            Should.NotBeNull(delegateType, nameof(delegateType));
            foreach (var c in components)
            {
                var activator = c.TryGetActivator(reflectionManager, constructor, delegateType);
                if (activator != null)
                    return activator;
            }

            return null;
        }

        public static Delegate? TryGetMemberGetter(this ItemOrArray<IMemberReflectionDelegateProviderComponent> components, IReflectionManager reflectionManager, MemberInfo member,
            Type delegateType)
        {
            Should.NotBeNull(reflectionManager, nameof(reflectionManager));
            Should.NotBeNull(member, nameof(member));
            Should.NotBeNull(delegateType, nameof(delegateType));
            foreach (var c in components)
            {
                var getter = c.TryGetMemberGetter(reflectionManager, member, delegateType);
                if (getter != null)
                    return getter;
            }

            return null;
        }

        public static Delegate? TryGetMemberSetter(this ItemOrArray<IMemberReflectionDelegateProviderComponent> components, IReflectionManager reflectionManager, MemberInfo member,
            Type delegateType)
        {
            Should.NotBeNull(reflectionManager, nameof(reflectionManager));
            Should.NotBeNull(member, nameof(member));
            Should.NotBeNull(delegateType, nameof(delegateType));
            foreach (var c in components)
            {
                var setter = c.TryGetMemberSetter(reflectionManager, member, delegateType);
                if (setter != null)
                    return setter;
            }

            return null;
        }

        public static Func<object?, ItemOrArray<object?>, object?>? TryGetMethodInvoker(this ItemOrArray<IMethodReflectionDelegateProviderComponent> components,
            IReflectionManager reflectionManager, MethodInfo method)
        {
            Should.NotBeNull(reflectionManager, nameof(reflectionManager));
            Should.NotBeNull(method, nameof(method));
            foreach (var c in components)
            {
                var invoker = c.TryGetMethodInvoker(reflectionManager, method);
                if (invoker != null)
                    return invoker;
            }

            return null;
        }

        public static Delegate? TryGetMethodInvoker(this ItemOrArray<IMethodReflectionDelegateProviderComponent> components, IReflectionManager reflectionManager,
            MethodInfo method, Type delegateType)
        {
            Should.NotBeNull(reflectionManager, nameof(reflectionManager));
            Should.NotBeNull(method, nameof(method));
            Should.NotBeNull(delegateType, nameof(delegateType));
            foreach (var c in components)
            {
                var invoker = c.TryGetMethodInvoker(reflectionManager, method, delegateType);
                if (invoker != null)
                    return invoker;
            }

            return null;
        }

        public static bool CanCreateDelegate(this ItemOrArray<IReflectionDelegateProviderComponent> components, IReflectionManager reflectionManager, Type delegateType,
            MethodInfo method)
        {
            Should.NotBeNull(reflectionManager, nameof(reflectionManager));
            Should.NotBeNull(delegateType, nameof(delegateType));
            Should.NotBeNull(method, nameof(method));
            foreach (var c in components)
            {
                if (c.CanCreateDelegate(reflectionManager, delegateType, method))
                    return true;
            }

            return false;
        }

        public static Delegate? TryCreateDelegate(this ItemOrArray<IReflectionDelegateProviderComponent> components, IReflectionManager reflectionManager, Type delegateType,
            object? target, MethodInfo method)
        {
            Should.NotBeNull(reflectionManager, nameof(reflectionManager));
            Should.NotBeNull(delegateType, nameof(delegateType));
            Should.NotBeNull(method, nameof(method));
            foreach (var c in components)
            {
                var value = c.TryCreateDelegate(reflectionManager, delegateType, target, method);
                if (value != null)
                    return value;
            }

            return null;
        }

        public static ILogger? TryGetLogger(this ItemOrArray<ILoggerProviderComponent> components, ILogger logger, object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(logger, nameof(logger));
            Should.NotBeNull(request, nameof(request));
            foreach (var c in components)
            {
                var result = c.TryGetLogger(logger, request, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static bool CanLog(this ItemOrArray<ILoggerComponent> components, ILogger logger, LogLevel level, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(logger, nameof(logger));
            Should.NotBeNull(level, nameof(level));
            foreach (var c in components)
            {
                if (c.CanLog(logger, level, metadata))
                    return true;
            }

            return false;
        }

        public static void Log(this ItemOrArray<ILoggerComponent> components, ILogger logger, LogLevel level, object message, Exception? exception,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(logger, nameof(logger));
            Should.NotBeNull(level, nameof(level));
            Should.NotBeNull(message, nameof(message));
            foreach (var c in components)
                c.Log(logger, level, message, exception, metadata);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IWeakReference? TryGetWeakReference(this ItemOrArray<IWeakReferenceProviderComponent> components, IWeakReferenceManager weakReferenceManager, object item,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(weakReferenceManager, nameof(weakReferenceManager));
            Should.NotBeNull(item, nameof(item));
            foreach (var c in components)
            {
                var weakReference = c.TryGetWeakReference(weakReferenceManager, item, metadata);
                if (weakReference != null)
                    return weakReference;
            }

            return null;
        }

        public static void OnChanged<T>(this ItemOrArray<ILockerChangedListener<T>> listeners, T owner, ILocker locker, IReadOnlyMetadataContext? metadata) where T : class
        {
            Should.NotBeNull(owner, nameof(owner));
            Should.NotBeNull(locker, nameof(locker));
            foreach (var listener in listeners)
                listener.OnChanged(owner, locker, metadata);
        }

        public static void Dispose<T>(object? components, T owner, IReadOnlyMetadataContext? metadata) where T : class
        {
            if (components is object[] c)
            {
                foreach (var t in c)
                    (t as IDisposableComponent<T>)?.Dispose(owner, metadata);
            }
            else
                (components as IDisposableComponent<T>)?.Dispose(owner, metadata);
        }
        
        public static void Dispose<T>(this ItemOrArray<IDisposableComponent<T>> components, T owner, IReadOnlyMetadataContext? metadata) where T : class
        {
            foreach (var c in components)
                c.Dispose(owner, metadata);
        }
        
        public static void Invalidate<T>(this ItemOrArray<IHasCacheComponent<T>> components, T owner, object? state, IReadOnlyMetadataContext? metadata) where T : class
        {
            Should.NotBeNull(owner, nameof(owner));
            foreach (var c in components)
                c.Invalidate(owner, state, metadata);
        }

        public static bool IsSuspended<T>(this ItemOrArray<ISuspendableComponent<T>> components, T owner, IReadOnlyMetadataContext? metadata)
            where T : class
        {
            foreach (var c in components)
            {
                if (c.IsSuspended(owner, metadata))
                    return true;
            }

            return false;
        }

        public static ActionToken TrySuspend<T>(this ItemOrArray<ISuspendableComponent<T>> components, T owner, IReadOnlyMetadataContext? metadata)
            where T : class
        {
            if (components.Count == 0)
                return default;
            if (components.Count == 1)
                return components[0].TrySuspend(owner, metadata);

            var tokens = new ActionToken[components.Count];
            for (var i = 0; i < tokens.Length; i++)
                tokens[i] = components[i].TrySuspend(owner, metadata);
            return ActionToken.FromTokens(tokens);
        }
    }
}