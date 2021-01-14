using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Collections;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Bindings.Extensions.Components
{
    public static class BindingComponentExtensions
    {
        public static void OnTargetPathMembersChanged(object? components, IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(binding, nameof(binding));
            Should.NotBeNull(observer, nameof(observer));
            Should.NotBeNull(metadata, nameof(metadata));
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingTargetObserverListener)?.OnTargetPathMembersChanged(binding, observer, metadata);
            }
            else
                (components as IBindingTargetObserverListener)?.OnTargetPathMembersChanged(binding, observer, metadata);
        }

        public static void OnTargetLastMemberChanged(object? components, IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(binding, nameof(binding));
            Should.NotBeNull(observer, nameof(observer));
            Should.NotBeNull(metadata, nameof(metadata));
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingTargetObserverListener)?.OnTargetLastMemberChanged(binding, observer, metadata);
            }
            else
                (components as IBindingTargetObserverListener)?.OnTargetLastMemberChanged(binding, observer, metadata);
        }

        public static void OnTargetError(object? components, IBinding binding, IMemberPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(binding, nameof(binding));
            Should.NotBeNull(observer, nameof(observer));
            Should.NotBeNull(exception, nameof(exception));
            Should.NotBeNull(metadata, nameof(metadata));
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingTargetObserverListener)?.OnTargetError(binding, observer, exception, metadata);
            }
            else
                (components as IBindingTargetObserverListener)?.OnTargetError(binding, observer, exception, metadata);
        }

        public static void OnTargetUpdateFailed(object? components, IBinding binding, Exception exception, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(binding, nameof(binding));
            Should.NotBeNull(exception, nameof(exception));
            Should.NotBeNull(metadata, nameof(metadata));
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingTargetListener)?.OnTargetUpdateFailed(binding, exception, metadata);
            }
            else
                (components as IBindingTargetListener)?.OnTargetUpdateFailed(binding, exception, metadata);
        }

        public static void OnTargetUpdateCanceled(object? components, IBinding binding, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(binding, nameof(binding));
            Should.NotBeNull(metadata, nameof(metadata));
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingTargetListener)?.OnTargetUpdateCanceled(binding, metadata);
            }
            else
                (components as IBindingTargetListener)?.OnTargetUpdateCanceled(binding, metadata);
        }

        public static void OnTargetUpdated(object? components, IBinding binding, object? newValue, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(binding, nameof(binding));
            Should.NotBeNull(metadata, nameof(metadata));
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingTargetListener)?.OnTargetUpdated(binding, newValue, metadata);
            }
            else
                (components as IBindingTargetListener)?.OnTargetUpdated(binding, newValue, metadata);
        }

        public static void OnSourcePathMembersChanged(object? components, IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(binding, nameof(binding));
            Should.NotBeNull(observer, nameof(observer));
            Should.NotBeNull(metadata, nameof(metadata));
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingSourceObserverListener)?.OnSourcePathMembersChanged(binding, observer, metadata);
            }
            else
                (components as IBindingSourceObserverListener)?.OnSourcePathMembersChanged(binding, observer, metadata);
        }

        public static void OnSourceLastMemberChanged(object? components, IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(binding, nameof(binding));
            Should.NotBeNull(observer, nameof(observer));
            Should.NotBeNull(metadata, nameof(metadata));
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingSourceObserverListener)?.OnSourceLastMemberChanged(binding, observer, metadata);
            }
            else
                (components as IBindingSourceObserverListener)?.OnSourceLastMemberChanged(binding, observer, metadata);
        }

        public static void OnSourceError(object? components, IBinding binding, IMemberPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(binding, nameof(binding));
            Should.NotBeNull(observer, nameof(observer));
            Should.NotBeNull(exception, nameof(exception));
            Should.NotBeNull(metadata, nameof(metadata));
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingSourceObserverListener)?.OnSourceError(binding, observer, exception, metadata);
            }
            else
                (components as IBindingSourceObserverListener)?.OnSourceError(binding, observer, exception, metadata);
        }

        public static void OnSourceUpdateFailed(object? components, IBinding binding, Exception exception, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(binding, nameof(binding));
            Should.NotBeNull(exception, nameof(exception));
            Should.NotBeNull(metadata, nameof(metadata));
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingSourceListener)?.OnSourceUpdateFailed(binding, exception, metadata);
            }
            else
                (components as IBindingSourceListener)?.OnSourceUpdateFailed(binding, exception, metadata);
        }

        public static void OnSourceUpdateCanceled(object? components, IBinding binding, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(binding, nameof(binding));
            Should.NotBeNull(metadata, nameof(metadata));
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingSourceListener)?.OnSourceUpdateCanceled(binding, metadata);
            }
            else
                (components as IBindingSourceListener)?.OnSourceUpdateCanceled(binding, metadata);
        }

        public static void OnSourceUpdated(object? components, IBinding binding, object? newValue, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(binding, nameof(binding));
            Should.NotBeNull(metadata, nameof(metadata));
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingSourceListener)?.OnSourceUpdated(binding, newValue, metadata);
            }
            else
                (components as IBindingSourceListener)?.OnSourceUpdated(binding, newValue, metadata);
        }

        public static object? InterceptTargetValue(object? components, IBinding binding, MemberPathLastMember targetMember, object? value, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(binding, nameof(binding));
            Should.NotBeNull(metadata, nameof(metadata));
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ITargetValueInterceptorComponent interceptor)
                        value = interceptor.InterceptTargetValue(binding, targetMember, value, metadata);
                }
            }
            else if (components is ITargetValueInterceptorComponent interceptor)
                value = interceptor.InterceptTargetValue(binding, targetMember, value, metadata);

            return value;
        }

        public static object? InterceptSourceValue(object? components, IBinding binding, MemberPathLastMember sourceMember, object? value, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(binding, nameof(binding));
            Should.NotBeNull(metadata, nameof(metadata));
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ISourceValueInterceptorComponent interceptor)
                        value = interceptor.InterceptSourceValue(binding, sourceMember, value, metadata);
                }
            }
            else if (components is ISourceValueInterceptorComponent interceptor)
                value = interceptor.InterceptSourceValue(binding, sourceMember, value, metadata);

            return value;
        }

        public static bool TrySetTargetValue(object? components, IBinding binding, MemberPathLastMember targetMember, object? newValue, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(binding, nameof(binding));
            Should.NotBeNull(metadata, nameof(metadata));
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ITargetValueSetterComponent setter && setter.TrySetTargetValue(binding, targetMember, newValue, metadata))
                        return true;
                }
            }
            else if (components is ITargetValueSetterComponent setter && setter.TrySetTargetValue(binding, targetMember, newValue, metadata))
                return true;

            return false;
        }

        public static bool TrySetSourceValue(object? components, IBinding binding, MemberPathLastMember sourceMember, object? newValue, IReadOnlyMetadataContext metadata)
        {
            Should.NotBeNull(binding, nameof(binding));
            Should.NotBeNull(metadata, nameof(metadata));
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ISourceValueSetterComponent setter && setter.TrySetSourceValue(binding, sourceMember, newValue, metadata))
                        return true;
                }
            }
            else if (components is ISourceValueSetterComponent setter && setter.TrySetSourceValue(binding, sourceMember, newValue, metadata))
                return true;

            return false;
        }

        public static void AddListener(object? sourceRaw, IMemberPathObserverListener listener)
        {
            Should.NotBeNull(listener, nameof(listener));
            if (sourceRaw is IMemberPathObserver source)
                source.AddListener(listener);
            else if (sourceRaw is object[] sources)
            {
                for (var i = 0; i < sources.Length; i++)
                    (sources[i] as IMemberPathObserver)?.AddListener(listener);
            }
        }

        public static void RemoveListener(object? sourceRaw, IMemberPathObserverListener listener)
        {
            Should.NotBeNull(listener, nameof(listener));
            if (sourceRaw is IMemberPathObserver source)
                source.RemoveListener(listener);
            else if (sourceRaw is object[] sources)
            {
                for (var i = 0; i < sources.Length; i++)
                    (sources[i] as IMemberPathObserver)?.RemoveListener(listener);
            }
        }

        public static void OnBeginEvent(this ItemOrArray<IBindingEventHandlerComponent> components, IBindingManager bindingManager, object? sender, object? message,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(bindingManager, nameof(bindingManager));
            foreach (var c in components)
                c.OnBeginEvent(bindingManager, sender, message, metadata);
        }

        public static void OnEndEvent(this ItemOrArray<IBindingEventHandlerComponent> components, IBindingManager bindingManager, object? sender, object? message,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(bindingManager, nameof(bindingManager));
            foreach (var c in components)
                c.OnEndEvent(bindingManager, sender, message, metadata);
        }

        public static void OnEventError(this ItemOrArray<IBindingEventHandlerComponent> components, IBindingManager bindingManager, Exception exception, object? sender,
            object? message,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(bindingManager, nameof(bindingManager));
            Should.NotBeNull(exception, nameof(exception));
            foreach (var c in components)
                c.OnEventError(bindingManager, exception, sender, message, metadata);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrIReadOnlyList<IBindingBuilder> TryParseBindingExpression(this ItemOrArray<IBindingExpressionParserComponent> components,
            IBindingManager bindingManager, object expression, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(bindingManager, nameof(bindingManager));
            Should.NotBeNull(expression, nameof(expression));
            foreach (var c in components)
            {
                var result = c.TryParseBindingExpression(bindingManager, expression, metadata);
                if (!result.IsEmpty)
                    return result;
            }

            return new ItemOrIReadOnlyList<IBindingBuilder>(expression as IReadOnlyList<IBindingBuilder>);
        }

        public static void Initialize(this ItemOrArray<IBindingExpressionInitializerComponent> components, IBindingManager bindingManager,
            IBindingExpressionInitializerContext context)
        {
            Should.NotBeNull(context, nameof(context));
            foreach (var c in components)
                c.Initialize(bindingManager, context);
        }

        public static ItemOrIReadOnlyList<IBinding> TryGetBindings(this ItemOrArray<IBindingHolderComponent> components, IBindingManager bindingManager, object target,
            string? path, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(bindingManager, nameof(bindingManager));
            Should.NotBeNull(target, nameof(target));
            if (components.Count == 0)
                return default;
            if (components.Count == 1)
                return components[0].TryGetBindings(bindingManager, target, path, metadata);

            var result = new ItemOrListEditor<IBinding>();
            foreach (var c in components)
                result.AddRange(c.TryGetBindings(bindingManager, target, path, metadata));

            return result.ToItemOrList();
        }

        public static bool TryRegister(this ItemOrArray<IBindingHolderComponent> components, IBindingManager bindingManager, object? target, IBinding binding,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(bindingManager, nameof(bindingManager));
            Should.NotBeNull(binding, nameof(binding));
            foreach (var c in components)
            {
                if (c.TryRegister(bindingManager, target, binding, metadata))
                    return true;
            }

            return false;
        }

        public static bool TryUnregister(this ItemOrArray<IBindingHolderComponent> components, IBindingManager bindingManager, object? target, IBinding binding,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(bindingManager, nameof(bindingManager));
            Should.NotBeNull(binding, nameof(binding));
            foreach (var c in components)
            {
                if (c.TryUnregister(bindingManager, target, binding, metadata))
                    return true;
            }

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnLifecycleChanged(this ItemOrArray<IBindingLifecycleListener> components, IBindingManager bindingManager, IBinding binding,
            BindingLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(bindingManager, nameof(bindingManager));
            Should.NotBeNull(binding, nameof(binding));
            Should.NotBeNull(lifecycleState, nameof(lifecycleState));
            foreach (var c in components)
                c.OnLifecycleChanged(bindingManager, binding, lifecycleState, state, metadata);
        }

        public static ItemOrArray<object?> TryGetBindingComponents(object?[] bindingComponents, IComparer<object?> comparer,
            IBinding binding, object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(bindingComponents, nameof(bindingComponents));
            Should.NotBeNull(comparer, nameof(comparer));
            Should.NotBeNull(binding, nameof(binding));
            Should.NotBeNull(target, nameof(target));
            var components = new object?[bindingComponents.Length];
            var size = 0;
            for (var i = 0; i < components.Length; i++)
            {
                var component = TryGetBindingComponent(bindingComponents[i], binding, target, source, metadata);
                if (component != null)
                    MugenExtensions.AddOrdered(components!, component, size++, comparer);
            }

            return components;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object? TryGetBindingComponent(object? item, IBinding binding, object target, object? source, IReadOnlyMetadataContext? metadata) =>
            (item as IBindingComponentProvider)?.TryGetComponent(binding, target, source, metadata) ?? item;
    }
}