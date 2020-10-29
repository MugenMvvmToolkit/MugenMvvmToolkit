using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using MugenMvvm.Bindings.Enums;
using MugenMvvm.Bindings.Interfaces.Core;
using MugenMvvm.Bindings.Interfaces.Core.Components;
using MugenMvvm.Bindings.Interfaces.Core.Components.Binding;
using MugenMvvm.Bindings.Interfaces.Observation;
using MugenMvvm.Bindings.Observation;
using MugenMvvm.Extensions;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Bindings.Extensions.Components
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

        public static void OnBeginEvent(this IBindingEventHandlerComponent[] components, IBindingManager bindingManager, object? sender, object? message, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(bindingManager, nameof(bindingManager));
            for (var i = 0; i < components.Length; i++)
                components[i].OnBeginEvent(bindingManager, sender, message, metadata);
        }

        public static void OnEndEvent(this IBindingEventHandlerComponent[] components, IBindingManager bindingManager, object? sender, object? message, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(bindingManager, nameof(bindingManager));
            for (var i = 0; i < components.Length; i++)
                components[i].OnEndEvent(bindingManager, sender, message, metadata);
        }

        public static void OnEventError(this IBindingEventHandlerComponent[] components, IBindingManager bindingManager, Exception exception, object? sender, object? message, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(bindingManager, nameof(bindingManager));
            Should.NotBeNull(exception, nameof(exception));
            for (var i = 0; i < components.Length; i++)
                components[i].OnEventError(bindingManager, exception, sender, message, metadata);
        }

        public static ItemOrList<IBindingBuilder, IReadOnlyList<IBindingBuilder>> TryParseBindingExpression(this IBindingExpressionParserComponent[] components,
            IBindingManager bindingManager, object expression, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(bindingManager, nameof(bindingManager));
            Should.NotBeNull(expression, nameof(expression));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryParseBindingExpression(bindingManager, expression, metadata);
                if (result.Item != null || result.List != null)
                    return result;
            }

            return default;
        }

        public static void Initialize(this IBindingExpressionInitializerComponent[] components, IBindingManager bindingManager, IBindingExpressionInitializerContext context)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(context, nameof(context));
            for (var i = 0; i < components.Length; i++)
                components[i].Initialize(bindingManager, context);
        }

        public static ItemOrList<IBinding, IReadOnlyList<IBinding>> TryGetBindings(this IBindingHolderComponent[] components, IBindingManager bindingManager, object target, string? path,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(bindingManager, nameof(bindingManager));
            Should.NotBeNull(target, nameof(target));
            if (components.Length == 1)
                return components[0].TryGetBindings(bindingManager, target, path, metadata);

            var result = ItemOrListEditor.Get<IBinding>();
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetBindings(bindingManager, target, path, metadata));
            return result.ToItemOrList<IReadOnlyList<IBinding>>();
        }

        public static bool TryRegister(this IBindingHolderComponent[] components, IBindingManager bindingManager, object? target, IBinding binding, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(bindingManager, nameof(bindingManager));
            Should.NotBeNull(binding, nameof(binding));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryRegister(bindingManager, target, binding, metadata))
                    return true;
            }

            return false;
        }

        public static bool TryUnregister(this IBindingHolderComponent[] components, IBindingManager bindingManager, object? target, IBinding binding, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(bindingManager, nameof(bindingManager));
            Should.NotBeNull(binding, nameof(binding));
            for (var i = 0; i < components.Length; i++)
            {
                if (components[i].TryUnregister(bindingManager, target, binding, metadata))
                    return true;
            }

            return false;
        }

        public static void OnLifecycleChanged(this IBindingLifecycleDispatcherComponent[] components, IBindingManager bindingManager, IBinding binding,
            BindingLifecycleState lifecycleState, object? state, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(bindingManager, nameof(bindingManager));
            Should.NotBeNull(binding, nameof(binding));
            Should.NotBeNull(lifecycleState, nameof(lifecycleState));
            for (var i = 0; i < components.Length; i++)
                components[i].OnLifecycleChanged(bindingManager, binding, lifecycleState, state, metadata);
        }

        public static ItemOrList<IComponent<IBinding>?, IComponent<IBinding>?[]> TryGetBindingComponents(object?[] bindingComponents, IComparer<IComponent<IBinding>?> comparer,
            IBinding binding, object target, object? source, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(bindingComponents, nameof(bindingComponents));
            Should.NotBeNull(comparer, nameof(comparer));
            Should.NotBeNull(binding, nameof(binding));
            Should.NotBeNull(target, nameof(target));
            if (bindingComponents!.Length == 1)
                return ItemOrList.FromItem<IComponent<IBinding>?, IComponent<IBinding>?[]>(TryGetBindingComponent(bindingComponents[0], binding, target, source, metadata));
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

                return ItemOrList.FromList(components);
            }

            return default;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IComponent<IBinding>? TryGetBindingComponent(object? item, IBinding binding, object target, object? source, IReadOnlyMetadataContext? metadata) =>
            item as IComponent<IBinding> ?? (item as IBindingComponentProvider)?.TryGetComponent(binding, target, source, metadata);

        #endregion
    }
}