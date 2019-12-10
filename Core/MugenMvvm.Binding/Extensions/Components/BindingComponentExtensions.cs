using System;
using MugenMvvm.Binding.Interfaces.Core;
using MugenMvvm.Binding.Interfaces.Core.Components.Binding;
using MugenMvvm.Binding.Interfaces.Observers;
using MugenMvvm.Binding.Observers;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Binding.Extensions.Components
{
    public static class BindingComponentExtensions
    {
        #region Methods

        public static bool HasComponent<TComponent>(object components) where TComponent : class, IComponent<IBinding>
        {
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is TComponent)
                        return true;
                }

                return false;
            }

            return components is TComponent;
        }

        public static bool OnComponentAdding(object components, IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is IComponentCollectionChangingListener listener && !listener.OnAdding(collection, component, metadata))
                        return false;
                }
            }
            else if (components is IComponentCollectionChangingListener listener && !listener.OnAdding(collection, component, metadata))
                return false;

            return true;
        }

        public static void OnComponentAdded(object components, IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    var comp = c[i];
                    if (!ReferenceEquals(comp, component))
                        (comp as IComponentCollectionChangedListener)?.OnAdded(collection, component, metadata);
                }
            }
            else if (!ReferenceEquals(components, component))
                (components as IComponentCollectionChangedListener)?.OnAdded(collection, component, metadata);
        }

        public static bool OnComponentRemoving(object components, IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is IComponentCollectionChangingListener listener && !ReferenceEquals(listener, component) && !listener.OnRemoving(collection, component, metadata))
                        return false;
                }
            }
            else if (components is IComponentCollectionChangingListener listener && !ReferenceEquals(listener, component) && !listener.OnRemoving(collection, component, metadata))
                return false;

            return true;
        }

        public static void OnComponentRemoved(object components, IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata)
        {
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IComponentCollectionChangedListener)?.OnRemoved(collection, component, metadata);
            }
            else
                (components as IComponentCollectionChangedListener)?.OnRemoved(collection, component, metadata);
        }

        public static void OnTargetPathMembersChanged(object components, IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata)
        {
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingTargetObserverListener)?.OnTargetPathMembersChanged(binding, observer, metadata);
            }
            else
                (components as IBindingTargetObserverListener)?.OnTargetPathMembersChanged(binding, observer, metadata);
        }

        public static void OnTargetLastMemberChanged(object components, IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata)
        {
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingTargetObserverListener)?.OnTargetLastMemberChanged(binding, observer, metadata);
            }
            else
                (components as IBindingTargetObserverListener)?.OnTargetLastMemberChanged(binding, observer, metadata);
        }

        public static void OnTargetError(object components, IBinding binding, IMemberPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata)
        {
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingTargetObserverListener)?.OnTargetError(binding, observer, exception, metadata);
            }
            else
                (components as IBindingTargetObserverListener)?.OnTargetError(binding, observer, exception, metadata);
        }

        public static void OnTargetUpdateFailed(object components, IBinding binding, Exception error, IReadOnlyMetadataContext metadata)
        {
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingTargetListener)?.OnTargetUpdateFailed(binding, error, metadata);
            }
            else
                (components as IBindingTargetListener)?.OnTargetUpdateFailed(binding, error, metadata);
        }

        public static void OnTargetUpdateCanceled(object components, IBinding binding, IReadOnlyMetadataContext metadata)
        {
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingTargetListener)?.OnTargetUpdateCanceled(binding, metadata);
            }
            else
                (components as IBindingTargetListener)?.OnTargetUpdateCanceled(binding, metadata);
        }

        public static void OnTargetUpdated(object components, IBinding binding, object? newValue, IReadOnlyMetadataContext metadata)
        {
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingTargetListener)?.OnTargetUpdated(binding, newValue, metadata);
            }
            else
                (components as IBindingTargetListener)?.OnTargetUpdated(binding, newValue, metadata);
        }

        public static void OnSourcePathMembersChanged(object components, IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata)
        {
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingSourceObserverListener)?.OnSourcePathMembersChanged(binding, observer, metadata);
            }
            else
                (components as IBindingSourceObserverListener)?.OnSourcePathMembersChanged(binding, observer, metadata);
        }

        public static void OnSourceLastMemberChanged(object components, IBinding binding, IMemberPathObserver observer, IReadOnlyMetadataContext metadata)
        {
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingSourceObserverListener)?.OnSourceLastMemberChanged(binding, observer, metadata);
            }
            else
                (components as IBindingSourceObserverListener)?.OnSourceLastMemberChanged(binding, observer, metadata);
        }

        public static void OnSourceError(object components, IBinding binding, IMemberPathObserver observer, Exception exception, IReadOnlyMetadataContext metadata)
        {
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingSourceObserverListener)?.OnSourceError(binding, observer, exception, metadata);
            }
            else
                (components as IBindingSourceObserverListener)?.OnSourceError(binding, observer, exception, metadata);
        }

        public static void OnSourceUpdateFailed(object components, IBinding binding, Exception error, IReadOnlyMetadataContext metadata)
        {
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingSourceListener)?.OnSourceUpdateFailed(binding, error, metadata);
            }
            else
                (components as IBindingSourceListener)?.OnSourceUpdateFailed(binding, error, metadata);
        }

        public static void OnSourceUpdateCanceled(object components, IBinding binding, IReadOnlyMetadataContext metadata)
        {
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingSourceListener)?.OnSourceUpdateCanceled(binding, metadata);
            }
            else
                (components as IBindingSourceListener)?.OnSourceUpdateCanceled(binding, metadata);
        }

        public static void OnSourceUpdated(object components, IBinding binding, object? newValue, IReadOnlyMetadataContext metadata)
        {
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                    (c[i] as IBindingSourceListener)?.OnSourceUpdated(binding, newValue, metadata);
            }
            else
                (components as IBindingSourceListener)?.OnSourceUpdated(binding, newValue, metadata);
        }

        public static object? InterceptTargetValue(object components, IBinding binding, MemberPathLastMember targetMember, object? value, IReadOnlyMetadataContext metadata)
        {
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

        public static object? InterceptSourceValue(object components, IBinding binding, MemberPathLastMember sourceMember, object? value, IReadOnlyMetadataContext metadata)
        {
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

        public static bool TryGetTargetValue(object components, IBinding binding, MemberPathLastMember sourceMember, IReadOnlyMetadataContext metadata, out object? value)
        {
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ITargetValueGetterBindingComponent setter && setter.TryGetTargetValue(binding, sourceMember, metadata, out value))
                        return true;
                }
            }
            else if (components is ITargetValueGetterBindingComponent setter && setter.TryGetTargetValue(binding, sourceMember, metadata, out value))
                return true;

            value = null;
            return false;
        }

        public static bool TrySetTargetValue(object components, IBinding binding, MemberPathLastMember targetMember, object? newValue, IReadOnlyMetadataContext metadata)
        {
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

        public static bool TryGetSourceValue(object components, IBinding binding, MemberPathLastMember targetMember, IReadOnlyMetadataContext metadata, out object? value)
        {
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ISourceValueGetterBindingComponent setter && setter.TryGetSourceValue(binding, targetMember, metadata, out value))
                        return true;
                }
            }
            else if (components is ISourceValueGetterBindingComponent setter && setter.TryGetSourceValue(binding, targetMember, metadata, out value))
                return true;

            value = null;
            return false;
        }

        public static bool TrySetSourceValue(object components, IBinding binding, MemberPathLastMember sourceMember, object? newValue, IReadOnlyMetadataContext metadata)
        {
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

        public static bool TryGetTargetLastMember(object components, IBinding binding, IReadOnlyMetadataContext metadata, out MemberPathLastMember targetMember)
        {
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ITargetLastMemberProviderBindingComponent provider && provider.TryGetTargetLastMember(binding, metadata, out targetMember))
                        return true;
                }
            }
            else if (components is ITargetLastMemberProviderBindingComponent provider && provider.TryGetTargetLastMember(binding, metadata, out targetMember))
                return true;

            targetMember = default;
            return false;
        }

        public static bool TryGetSourceLastMember(object components, IBinding binding, IReadOnlyMetadataContext metadata, out MemberPathLastMember sourceMember)
        {
            if (components is object[] c)
            {
                for (var i = 0; i < c.Length; i++)
                {
                    if (c[i] is ISourceLastMemberProviderBindingComponent provider && provider.TryGetSourceLastMember(binding, metadata, out sourceMember))
                        return true;
                }
            }
            else if (components is ISourceLastMemberProviderBindingComponent provider && provider.TryGetSourceLastMember(binding, metadata, out sourceMember))
                return true;

            sourceMember = default;
            return false;
        }

        public static void AddListener(object sourceRaw, IMemberPathObserverListener listener)
        {
            if (sourceRaw is IMemberPathObserver source)
                source.AddListener(listener);
            else if (sourceRaw is object[] sources)
            {
                for (var i = 0; i < sources.Length; i++)
                    (sources[i] as IMemberPathObserver)?.AddListener(listener);
            }
        }

        public static void RemoveListener(object sourceRaw, IMemberPathObserverListener listener)
        {
            if (sourceRaw is IMemberPathObserver source)
                source.RemoveListener(listener);
            else if (sourceRaw is object[] sources)
            {
                for (var i = 0; i < sources.Length; i++)
                    (sources[i] as IMemberPathObserver)?.RemoveListener(listener);
            }
        }

        #endregion
    }
}