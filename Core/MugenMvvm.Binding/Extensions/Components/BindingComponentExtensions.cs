using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Binding.Enums;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components;
using MugenMvvm.Binding.Interfaces.Core.Components.Binding;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Binding.Extensions.Components
{
    public static class BindingComponentExtensions
    {
        #region Methods

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
                    if (c[i] is ITargetValueInterceptorBindingComponent interceptor)
                        value = interceptor.InterceptTargetValue(binding, targetMember, value, metadata);
                }
            }
            else if (components is ITargetValueInterceptorBindingComponent interceptor)
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
                    if (c[i] is ISourceValueInterceptorBindingComponent interceptor)
                        value = interceptor.InterceptSourceValue(binding, sourceMember, value, metadata);
                }
            }
            else if (components is ISourceValueInterceptorBindingComponent interceptor)
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
                    if (c[i] is ITargetValueSetterBindingComponent setter && setter.TrySetTargetValue(binding, targetMember, newValue, metadata))
                        return true;
                }
            }
            else if (components is ITargetValueSetterBindingComponent setter && setter.TrySetTargetValue(binding, targetMember, newValue, metadata))
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
                    if (c[i] is ISourceValueSetterBindingComponent setter && setter.TrySetSourceValue(binding, sourceMember, newValue, metadata))
                        return true;
                }
            }
            else if (components is ISourceValueSetterBindingComponent setter && setter.TrySetSourceValue(binding, sourceMember, newValue, metadata))
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

        public static void OnBeginEvent(this IBindingEventHandlerComponent[] components, object sender, object? message, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(sender, nameof(sender));
            for (var i = 0; i < components.Length; i++)
                components[i].OnBeginEvent(sender, message, metadata);
        }

        public static void OnEndEvent(this IBindingEventHandlerComponent[] components, object sender, object? message, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(sender, nameof(sender));
            for (var i = 0; i < components.Length; i++)
                components[i].OnEndEvent(sender, message, metadata);
        }

        public static void OnEventError(this IBindingEventHandlerComponent[] components, Exception exception, object sender, object? message, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(exception, nameof(exception));
            Should.NotBeNull(sender, nameof(sender));
            for (var i = 0; i < components.Length; i++)
                components[i].OnEventError(exception, sender, message, metadata);
        }

        public static ItemOrList<IBindingExpression, IReadOnlyList<IBindingExpression>> TryBuildBindingExpression<TExpression>(this IBindingExpressionBuilderComponent[] components,
            in TExpression expression, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryBuildBindingExpression(expression, metadata);
                if (result.Item != null || result.List != null)
                    return result;
            }

            return default;
        }

        public static void Initialize(this IBindingExpressionInitializerComponent[] components, IBindingExpressionInitializerContext context)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(context, nameof(context));
            for (var i = 0; i < components.Length; i++)
                components[i].Initialize(context);
        }

        public static ItemOrList<IBinding, IReadOnlyList<IBinding>> TryGetBindings(this IBindingHolderComponent[] components, object target, string? path, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(target, nameof(target));
            if (components.Length == 0)
                return default;
            if (components.Length == 1)
                return components[0].TryGetBindings(target, path, metadata);

            ItemOrList<IBinding, List<IBinding>> result = default;
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetBindings(target, path, metadata));
            return result.Cast<IReadOnlyList<IBinding>>();
        }

        public static bool TryRegister(this IBindingHolderComponent[] components, object? target, IBinding binding, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(binding, nameof(binding));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryRegister(target, binding, metadata))
                    return true;
            }

            return false;
        }

        public static bool TryUnregister(this IBindingHolderComponent[] components, object? target, IBinding binding, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(binding, nameof(binding));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryUnregister(target, binding, metadata))
                    return true;
            }

            return false;
        }

        public static IReadOnlyMetadataContext? OnLifecycleChanged<TState>(this IBindingStateDispatcherComponent[] components, IBinding binding,
            BindingLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(binding, nameof(binding));
            Should.NotBeNull(lifecycleState, nameof(lifecycleState));
            if (components.Length == 0)
                return null;
            if (components.Length == 1)
                return components[0].OnLifecycleChanged(binding, lifecycleState, state, metadata);

            IReadOnlyMetadataContext? result = null;
            for (var i = 0; i < components.Length; i++)
                components[i].OnLifecycleChanged(binding, lifecycleState, state, metadata).Aggregate(ref result);
            return result;
        }

        public static ItemOrList<IComponent<IBinding>?, IComponent<IBinding>?[]> TryGetBindingComponents(object?[] bindingComponents, IComparer<IComponent<IBinding>?> comparer,
            IBinding binding, object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(bindingComponents, nameof(bindingComponents));
            Should.NotBeNull(comparer, nameof(comparer));
            Should.NotBeNull(binding, nameof(binding));
            Should.NotBeNull(target, nameof(target));
            if (bindingComponents!.Length == 1)
                return new ItemOrList<IComponent<IBinding>?, IComponent<IBinding>?[]>(TryGetBindingComponent(bindingComponents[0], binding, target, source, metadata));
            if (bindingComponents.Length != 0)
            {
                var components = new IComponent<IBinding>?[bindingComponents.Length];
                var size = 0;
                for (var i = 0; i < components.Length; i++)
                {
                    var component = TryGetBindingComponent(bindingComponents[i], binding, target, source, metadata);
                    if (component != null)
                        MugenExtensions.AddOrdered(components!, component, size++, comparer);
                }

                return components;
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IComponent<IBinding>? TryGetBindingComponent(object? item, IBinding binding, object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            return (item as IComponent<IBinding>) ?? (item as IBindingComponentProvider)?.GetComponent(binding, target, source, metadata);
        }

        #endregion
    }
}