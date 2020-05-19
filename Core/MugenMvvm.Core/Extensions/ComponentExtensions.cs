using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T DefaultIfNull<T>(this T? component) where T : class, IComponent
        {
            return component ?? MugenService.Instance<T>();
        }

        public static void TryInvalidateCache(this IComponentOwner? owner, IReadOnlyMetadataContext? metadata = null)
        {
            owner.TryInvalidateCache<object?>(null, metadata);
        }

        public static void TryInvalidateCache<TState>(this IComponentOwner? owner, in TState state, IReadOnlyMetadataContext? metadata = null)
        {
            if (owner == null)
                return;
            (owner as IHasCache)?.Invalidate(state, metadata);
            owner.GetComponents<IHasCache>(metadata).Invalidate(state, metadata);
        }

        public static bool AddComponent<T>(this IComponentOwner<T> componentOwner, IComponent<T> component, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            Should.NotBeNull(componentOwner, nameof(componentOwner));
            return componentOwner.Components.Add(component, metadata);
        }

        public static bool RemoveComponent<T>(this IComponentOwner<T> componentOwner, IComponent<T> component, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            Should.NotBeNull(componentOwner, nameof(componentOwner));
            if (componentOwner.HasComponents)
                return componentOwner.Components.Remove(component, metadata);
            return false;
        }

        public static void ClearComponents(this IComponentOwner componentOwner, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(componentOwner, nameof(componentOwner));
            if (componentOwner.HasComponents)
                componentOwner.Components.Clear(metadata);
        }

        public static T[] GetComponents<T>(this IComponentOwner componentOwner, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            Should.NotBeNull(componentOwner, nameof(componentOwner));
            if (componentOwner.HasComponents)
                return componentOwner.Components.Get<T>(metadata);
            return Default.EmptyArray<T>();
        }

        public static TComponent GetComponent<TComponent>(this IComponentOwner owner, IReadOnlyMetadataContext? metadata = null) where TComponent : class, IComponent
        {
            return owner.GetComponent<TComponent>(false, metadata)!;
        }

        public static TComponent? GetComponentOptional<TComponent>(this IComponentOwner owner, IReadOnlyMetadataContext? metadata = null) where TComponent : class, IComponent
        {
            return owner.GetComponent<TComponent>(true, metadata);
        }

        public static T[] GetOrDefault<T>(this IComponentCollection? collection, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            if (collection == null)
                return Default.EmptyArray<T>();
            return collection.Get<T>(metadata);
        }

        public static bool LazyInitialize(this IComponentCollectionProvider? componentCollectionProvider, [NotNull] ref IComponentCollection? item, object target, IReadOnlyMetadataContext? metadata = null)
        {
            return item == null && LazyInitialize(ref item, componentCollectionProvider.DefaultIfNull().GetComponentCollection(target, metadata));
        }

        public static int GetPriority(this IComponent component, object? owner = null)
        {
            return GetComponentPriority(component, owner);
        }

        public static int GetComponentPriority(object component, object? owner)
        {
            var provider = MugenService.Optional<IComponentPriorityProvider>();
            if (provider != null)
                return provider.GetPriority(component, owner);
            if (component is IHasPriority p)
                return p.Priority;
            return 0;
        }

        private static TComponent? GetComponent<TComponent>(this IComponentOwner owner, bool optional, IReadOnlyMetadataContext? metadata)
            where TComponent : class, IComponent
        {
            Should.NotBeNull(owner, nameof(owner));
            var components = owner.GetComponents<TComponent>(metadata);
            if (components.Length != 0)
                return components[0];
            if (!optional)
                ExceptionManager.ThrowCannotGetComponent(owner, typeof(TComponent));
            return null;
        }

        #endregion
    }
}