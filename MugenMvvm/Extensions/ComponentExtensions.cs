﻿using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        #region Methods

        public static IComponentCollection GetComponentCollection(this IComponentCollectionManager componentCollectionManager, object owner, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(componentCollectionManager, nameof(componentCollectionManager));
            Should.NotBeNull(owner, nameof(owner));
            var collection = componentCollectionManager.TryGetComponentCollection(owner, metadata);
            if (collection == null)
                ExceptionManager.ThrowRequestNotSupported<IComponentCollectionProviderComponent>(componentCollectionManager, owner, metadata);
            return collection;
        }

        public static IComponentCollection EnsureInitialized(this IComponentCollectionManager? componentCollectionManager, [NotNull] ref IComponentCollection? item, object target,
            IReadOnlyMetadataContext? metadata = null) => EnsureInitialized(ref item, componentCollectionManager.DefaultIfNull().GetComponentCollection(target, metadata));

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static IServiceProvider DefaultIfNull(this IServiceProvider? serviceProvider) => serviceProvider ?? MugenService.Instance<IServiceProvider>();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T DefaultIfNull<T>(this T? component) where T : class, IComponentOwner => component ?? MugenService.Instance<T>();

        public static void TryInvalidateCache(this IComponentOwner? owner, object? state = null, IReadOnlyMetadataContext? metadata = null)
        {
            if (owner == null)
                return;
            (owner as IHasCache)?.Invalidate(state, metadata);
            owner.GetComponents<IHasCache>(metadata).Invalidate(state, metadata);
        }

        public static ActionToken TrySuspend(this IComponentOwner? owner, object? state = null, IReadOnlyMetadataContext? metadata = null)
        {
            if (owner == null)
                return default;
            return owner.GetComponents<ISuspendable>(metadata).Suspend(state, metadata);
        }

        public static ActionToken AddComponent<T>(this IComponentOwner<T> componentOwner, IComponent<T> component, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            var t = componentOwner.TryAddComponent(component, metadata);
            if (t.IsEmpty)
                ExceptionManager.ThrowCannotAddComponent(componentOwner.Components, component);
            return t;
        }

        public static ActionToken TryAddComponent<T>(this IComponentOwner<T> componentOwner, IComponent<T> component, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            Should.NotBeNull(componentOwner, nameof(componentOwner));
            Should.NotBeNull(component, nameof(component));
            if (componentOwner.Components.TryAdd(component, metadata))
                return new ActionToken((owner, comp) => ((IComponentOwner) owner!).Components.Remove(comp!), componentOwner, component);
            return default;
        }

        public static bool RemoveComponent<T>(this IComponentOwner<T> componentOwner, IComponent<T> component, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            Should.NotBeNull(componentOwner, nameof(componentOwner));
            if (componentOwner.HasComponents)
                return componentOwner.Components.Remove(component, metadata);
            return false;
        }

        public static void RemoveComponents<T>(this IComponentOwner componentOwner, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            Should.NotBeNull(componentOwner, nameof(componentOwner));
            if (componentOwner.HasComponents)
            {
                var components = componentOwner.Components.Get<T>();
                for (var i = 0; i < components.Length; i++)
                    componentOwner.Components.Remove(components[i], metadata);
            }
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
            return Default.Array<T>();
        }

        public static TComponent GetOrAddComponent<TComponent>(this IComponentOwner owner, Func<IReadOnlyMetadataContext?, TComponent> getComponent,
            IReadOnlyMetadataContext? metadata = null) where TComponent : class, IComponent =>
            owner.GetOrAddComponent(getComponent, (func, context) => func(context), metadata);

        public static TComponent GetOrAddComponent<TComponent, TState>(this IComponentOwner owner, TState state, Func<TState, IReadOnlyMetadataContext?, TComponent> getComponent,
            IReadOnlyMetadataContext? metadata = null) where TComponent : class, IComponent
        {
            Should.NotBeNull(owner, nameof(owner));
            Should.NotBeNull(getComponent, nameof(getComponent));
            lock (owner)
            {
                var component = owner.GetComponent<TComponent>(true, metadata);
                if (component == null)
                {
                    component = getComponent(state, metadata);
                    owner.Components.Add(component);
                }

                return component;
            }
        }

        public static TComponent GetComponent<TComponent>(this IComponentOwner owner, IReadOnlyMetadataContext? metadata = null) where TComponent : class, IComponent => owner.GetComponent<TComponent>(false, metadata)!;

        public static TComponent? GetComponentOptional<TComponent>(this IComponentOwner owner, IReadOnlyMetadataContext? metadata = null) where TComponent : class, IComponent =>
            owner.GetComponent<TComponent>(true, metadata);

        public static T[] GetOrDefault<T>(this IComponentCollection? collection, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            if (collection == null)
                return Default.Array<T>();
            return collection.Get<T>(metadata);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Add(this IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (!collection.TryAdd(component, metadata))
                ExceptionManager.ThrowCannotAddComponent(collection, component);
        }

        public static int GetPriority(this IComponent component, object? owner = null) => GetComponentPriority(component, owner);

        public static int GetComponentPriority(object component, object? owner)
        {
            var provider = MugenService.Optional<IComponentPriorityProvider>();
            if (provider != null)
                return provider.GetPriority(component, owner);
            if (component is IHasPriority p)
                return p.Priority;
            return 0;
        }

        public static Task InvokeAllAsync<TComponent, TState>(this TComponent[] components, TState state, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata,
            Func<TComponent, TState, CancellationToken, IReadOnlyMetadataContext?, Task> getResult)
            where TComponent : class, IComponent
        {
            Should.NotBeNull(getResult, nameof(getResult));

            Task GetResult(TComponent component)
            {
                try
                {
                    return getResult(component, state, cancellationToken, metadata);
                }
                catch (Exception e)
                {
                    return e.ToTask();
                }
            }

            if (components.Length == 0)
                return Default.CompletedTask;
            if (components.Length == 1)
                return GetResult(components[0]);

            var tasks = new ItemOrListEditor<Task>();
            for (var i = 0; i < components.Length; i++)
            {
                var result = GetResult(components[i]);
                if (!result.IsCompleted || result.IsFaulted || result.IsCanceled)
                    tasks.Add(result);
            }

            if (tasks.Count == 0)
                return Default.CompletedTask;
            return tasks.WhenAll();
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