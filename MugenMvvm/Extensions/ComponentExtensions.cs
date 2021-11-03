using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Models.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions
{
    public static partial class MugenExtensions
    {
        public static IComponentCollection GetComponentCollection(this IComponentCollectionManager componentCollectionManager, object owner,
            IReadOnlyMetadataContext? metadata = null)
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
        public static T DefaultIfNull<T>(this T? component) where T : class, IMugenService => component ?? MugenService.Instance<T>();

        public static T DefaultIfNull<T>(this T? component, object? source) where T : class, IMugenService =>
            component ?? (source as IHasService<T>)?.GetService(false) ?? (T?) (source as IServiceProvider)?.GetService(typeof(T)) ?? MugenService.Instance<T>();

        public static object? GetService(this IServiceProvider serviceProvider, Type serviceType, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(serviceProvider, nameof(serviceProvider));
            if (serviceProvider is IMugenServiceProvider mugenServiceProvider)
                return mugenServiceProvider.GetService(serviceType, metadata);
            return serviceProvider.GetService(serviceType);
        }

        public static void TryInvalidateCache<T>(this IComponentOwner<T> owner, object? state = null, IReadOnlyMetadataContext? metadata = null)
            where T : class
        {
            Should.NotBeNull(owner, nameof(owner));
            if (owner is IHasCache hasCache)
                hasCache.Invalidate(state, metadata);
            else
                owner.GetComponents<IHasCacheComponent<T>>(metadata).Invalidate((T) owner, state, metadata);
        }

        public static bool IsSuspended<T>(this IComponentOwner<T> owner, IReadOnlyMetadataContext? metadata = null)
            where T : class
        {
            Should.NotBeNull(owner, nameof(owner));
            if (owner is ISuspendable suspendable)
                return suspendable.IsSuspended;
            return owner.GetComponents<ISuspendableComponent<T>>(metadata).IsSuspended((T) owner, metadata);
        }

        public static ActionToken TrySuspend<T>(this IComponentOwner<T> owner, IReadOnlyMetadataContext? metadata = null)
            where T : class
        {
            Should.NotBeNull(owner, nameof(owner));
            if (owner is ISuspendable suspendable)
                return suspendable.Suspend(metadata);
            return owner.GetComponents<ISuspendableComponent<T>>(metadata).TrySuspend((T) owner, metadata);
        }

        public static ActionToken AddComponent<T>(this IComponentOwner<T> owner, IComponent<T> component, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            var t = owner.TryAddComponent(component, metadata);
            if (t.IsEmpty)
                ExceptionManager.ThrowCannotAddComponent(owner.Components, component);
            return t;
        }

        public static ActionToken TryAddComponent<T>(this IComponentOwner<T> owner, IComponent<T> component, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            Should.NotBeNull(owner, nameof(owner));
            Should.NotBeNull(component, nameof(component));
            if (owner.Components.TryAdd(component, metadata))
                return ActionToken.FromDelegate((o, comp) => ((IComponentOwner) o!).Components.Remove(comp!), owner, component);
            return default;
        }

        public static bool RemoveComponent<T>(this IComponentOwner<T> owner, IComponent<T> component, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            Should.NotBeNull(owner, nameof(owner));
            if (owner.HasComponents)
                return owner.Components.Remove(component, metadata);
            return false;
        }

        public static void RemoveComponents<T>(this IComponentOwner owner, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            Should.NotBeNull(owner, nameof(owner));
            if (owner.HasComponents)
            {
                foreach (var t in owner.Components.Get<T>())
                    owner.Components.Remove(t, metadata);
            }
        }

        public static void ClearComponents(this IComponentOwner owner, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(owner, nameof(owner));
            if (owner.HasComponents)
                owner.Components.Clear(metadata);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ItemOrArray<T> GetComponents<T>(this IComponentOwner owner, IReadOnlyMetadataContext? metadata = null) where T : class
        {
            Should.NotBeNull(owner, nameof(owner));
            if (owner.HasComponents)
                return owner.Components.Get<T>(metadata);
            return default;
        }

        public static TComponent GetOrAddComponent<TComponent, TState>(this IComponentOwner owner, TState state, Func<TState, IReadOnlyMetadataContext?, TComponent> getComponent,
            IReadOnlyMetadataContext? metadata = null) where TComponent : class, IComponent
        {
            Should.NotBeNull(owner, nameof(owner));
            Should.NotBeNull(getComponent, nameof(getComponent));
            var component = owner.GetComponentOptional<TComponent>(metadata);
            if (component != null)
                return component;

            component = (TComponent?) owner.Components.TryAdd((state, getComponent), (c, s, m) =>
            {
                var components = c.Get<TComponent>(m);
                if (components.Count == 0)
                    return s.getComponent(s.state, m);
                return components[0];
            }, metadata);
            if (component == null)
                ExceptionManager.ThrowCannotAddComponent(owner.Components, typeof(TComponent));
            return component;
        }

        public static TComponent GetOrAddComponent<TComponent>(this IComponentOwner owner, IReadOnlyMetadataContext? metadata = null) where TComponent : class, IComponent, new()
        {
            Should.NotBeNull(owner, nameof(owner));
            var component = owner.GetComponentOptional<TComponent>(metadata);
            if (component != null)
                return component;

            component = (TComponent?) owner.Components.TryAdd<object?>(null, (c, _, m) =>
            {
                var components = c.Get<TComponent>(m);
                if (components.Count == 0)
                    return new TComponent();
                return components[0];
            }, metadata);
            if (component == null)
                ExceptionManager.ThrowCannotAddComponent(owner.Components, typeof(TComponent));
            return component;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TComponent GetComponent<TComponent>(this IComponentOwner owner, IReadOnlyMetadataContext? metadata = null) where TComponent : class, IComponent
        {
            var component = owner.GetComponentOptional<TComponent>();
            if (component == null)
                ExceptionManager.ThrowCannotGetComponent(owner, typeof(TComponent));
            return component;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TComponent? GetComponentOptional<TComponent>(this IComponentOwner owner, IReadOnlyMetadataContext? metadata = null) where TComponent : class, IComponent =>
            owner.GetComponents<TComponent>(metadata).FirstOrDefault();

        public static void Add(this IComponentCollection collection, object component, IReadOnlyMetadataContext? metadata = null)
        {
            Should.NotBeNull(collection, nameof(collection));
            if (!collection.TryAdd(component, metadata))
                ExceptionManager.ThrowCannotAddComponent(collection, component);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetComponentPriority(object component)
        {
            if (component is IHasPriority p)
                return p.Priority;
            return 0;
        }

        public static T InvokeSequentially<TComponent, TState, T>(this ItemOrArray<TComponent> components, TState state,
            IReadOnlyMetadataContext? metadata, Func<TComponent, TState, IReadOnlyMetadataContext?, T> invoke)
            where TComponent : class, IComponent
            where T : class?
        {
            Should.NotBeNull(invoke, nameof(invoke));
            foreach (var c in components)
            {
                var result = invoke(c, state, metadata);
                if (result != null)
                    return result;
            }

            return default!;
        }

        public static T InvokeSequentially<TComponent, TState, T>(this ItemOrArray<TComponent> components, TState state,
            IReadOnlyMetadataContext? metadata, Func<TComponent, TState, IReadOnlyMetadataContext?, T> invoke, Func<T, bool> isValid)
            where TComponent : class, IComponent
        {
            Should.NotBeNull(invoke, nameof(invoke));
            foreach (var c in components)
            {
                var result = invoke(c, state, metadata);
                if (isValid(result))
                    return result;
            }

            return default!;
        }

        public static bool InvokeSequentially<TComponent, TState>(this ItemOrArray<TComponent> components, TState state, IReadOnlyMetadataContext? metadata,
            Func<TComponent, TState, IReadOnlyMetadataContext?, bool> invoke, bool invert = false)
            where TComponent : class, IComponent
        {
            Should.NotBeNull(invoke, nameof(invoke));
            foreach (var c in components)
            {
                var b = invoke(c, state, metadata);
                if (invert)
                {
                    if (!b)
                        return false;
                }
                else if (b)
                    return true;
            }

            return invert;
        }

        public static void InvokeAll<TComponent, TState>(this ItemOrArray<TComponent> components, TState state, IReadOnlyMetadataContext? metadata,
            Action<TComponent, TState, IReadOnlyMetadataContext?> invoke)
            where TComponent : class, IComponent
        {
            Should.NotBeNull(invoke, nameof(invoke));
            foreach (var c in components)
                invoke(c, state, metadata);
        }

        public static bool InvokeAll<TComponent, TState>(this ItemOrArray<TComponent> components, TState state, IReadOnlyMetadataContext? metadata,
            Func<TComponent, TState, IReadOnlyMetadataContext?, bool> invoke)
            where TComponent : class, IComponent
        {
            Should.NotBeNull(invoke, nameof(invoke));
            var result = false;
            foreach (var c in components)
            {
                if (invoke(c, state, metadata))
                    result = true;
            }

            return result;
        }

        public static ItemOrIReadOnlyList<T> InvokeAll<TComponent, TState, T>(this ItemOrArray<TComponent> components, TState state, IReadOnlyMetadataContext? metadata,
            Func<TComponent, TState, IReadOnlyMetadataContext?, ItemOrIReadOnlyList<T>> invoke)
            where TComponent : class, IComponent
        {
            Should.NotBeNull(invoke, nameof(invoke));
            if (components.Count == 0)
                return default;
            if (components.Count == 1)
                return invoke(components[0], state, metadata);

            var editor = new ItemOrListEditor<T>();
            foreach (var c in components)
                editor.AddRange(invoke(c, state, metadata));

            return editor.ToItemOrList();
        }

        public static ItemOrIReadOnlyList<T> InvokeAllDisposable<TComponent, TState, T>(this ItemOrArray<TComponent> components, TState state, IReadOnlyMetadataContext? metadata,
            Func<TComponent, TState, IReadOnlyMetadataContext?, ItemOrIReadOnlyList<T>> invoke)
            where T : IDisposable
            where TComponent : class, IComponent
        {
            Should.NotBeNull(invoke, nameof(invoke));
            if (components.Count == 0)
                return default;
            if (components.Count == 1)
                return invoke(components[0], state, metadata);

            var editor = new ItemOrListEditor<T>();
            try
            {
                foreach (var c in components)
                    editor.AddRange(invoke(c, state, metadata));
            }
            catch
            {
                foreach (var item in editor)
                    item.Dispose();
                throw;
            }

            return editor.ToItemOrList();
        }

        public static EnumFlags<T> InvokeAll<TComponent, TState, T>(this ItemOrArray<TComponent> components, TState state, IReadOnlyMetadataContext? metadata,
            Func<TComponent, TState, IReadOnlyMetadataContext?, EnumFlags<T>> invoke)
            where TComponent : class, IComponent
            where T : class, IFlagsEnum
        {
            Should.NotBeNull(invoke, nameof(invoke));
            EnumFlags<T> result = default;
            foreach (var c in components)
                result |= invoke(c, state, metadata);
            return result;
        }

        public static ValueTask<T> InvokeSequentiallyAsync<TComponent, TState, T>(this ItemOrArray<TComponent> components, TState state,
            CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata, Func<TComponent, TState, CancellationToken, IReadOnlyMetadataContext?, ValueTask<T>> invoke)
            where TComponent : class, IComponent
            where T : class? =>
            components.InvokeSequentiallyAsync(state, cancellationToken, metadata, invoke, arg => arg != null);

        public static async ValueTask<T> InvokeSequentiallyAsync<TComponent, TState, T>(this ItemOrArray<TComponent> components, TState state,
            CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata, Func<TComponent, TState, CancellationToken, IReadOnlyMetadataContext?, ValueTask<T>> invoke,
            Func<T, bool> isValid)
            where TComponent : class, IComponent
        {
            Should.NotBeNull(invoke, nameof(invoke));
            foreach (var c in components)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var result = await invoke(c, state, cancellationToken, metadata).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                if (isValid(result))
                    return result;
            }

            return default!;
        }

        public static async ValueTask<bool> InvokeSequentiallyAsync<TComponent, TState>(this ItemOrArray<TComponent> components, TState state,
            CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata, Func<TComponent, TState, CancellationToken, IReadOnlyMetadataContext?, ValueTask<bool>> invoke,
            bool invert = false)
            where TComponent : class, IComponent
        {
            Should.NotBeNull(invoke, nameof(invoke));
            foreach (var c in components)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var b = await invoke(c, state, cancellationToken, metadata).ConfigureAwait(false);
                if (invert)
                {
                    if (!b)
                        return false;
                }
                else if (b)
                    return true;
            }

            return invert;
        }

        public static async Task InvokeSequentiallyAsync<TComponent, TState>(this ItemOrArray<TComponent> components, TState state,
            CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata, Func<TComponent, TState, CancellationToken, IReadOnlyMetadataContext?, Task> invoke)
            where TComponent : class, IComponent
        {
            Should.NotBeNull(invoke, nameof(invoke));
            foreach (var c in components)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await invoke(c, state, cancellationToken, metadata).ConfigureAwait(false);
            }
        }

        public static async ValueTask<bool> InvokeAllAsync<TComponent, TState>(this ItemOrArray<TComponent> components, TState state,
            CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata, Func<TComponent, TState, CancellationToken, IReadOnlyMetadataContext?, ValueTask<bool>> invoke)
            where TComponent : class, IComponent
        {
            Should.NotBeNull(invoke, nameof(invoke));
            var result = false;
            foreach (var c in components)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (await invoke(c, state, cancellationToken, metadata).ConfigureAwait(false))
                    result = true;
                cancellationToken.ThrowIfCancellationRequested();
            }

            return result;
        }

        public static async ValueTask<ItemOrIReadOnlyList<T>> InvokeAllAsync<TComponent, TState, T>(this ItemOrArray<TComponent> components, TState state,
            CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata,
            Func<TComponent, TState, CancellationToken, IReadOnlyMetadataContext?, ValueTask<ItemOrIReadOnlyList<T>>> invoke, bool disposeOnException = false)
            where TComponent : class, IComponent
        {
            Should.NotBeNull(invoke, nameof(invoke));
            if (components.Count == 0)
                return default;
            if (components.Count == 1)
                return await invoke(components[0], state, cancellationToken, metadata).ConfigureAwait(false);

            var editor = new ItemOrListEditor<T>();
            foreach (var c in components)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var result = await invoke(c, state, cancellationToken, metadata).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
                editor.AddRange(result);
            }

            return editor.ToItemOrList();
        }

        public static async ValueTask<ItemOrIReadOnlyList<T>> InvokeAllDisposableAsync<TComponent, TState, T>(this ItemOrArray<TComponent> components, TState state,
            CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata,
            Func<TComponent, TState, CancellationToken, IReadOnlyMetadataContext?, ValueTask<ItemOrIReadOnlyList<T>>> invoke, bool disposeOnException = false)
            where T : IDisposable
            where TComponent : class, IComponent
        {
            Should.NotBeNull(invoke, nameof(invoke));
            if (components.Count == 0)
                return default;
            if (components.Count == 1)
                return await invoke(components[0], state, cancellationToken, metadata).ConfigureAwait(false);

            var editor = new ItemOrListEditor<T>();
            try
            {
                foreach (var c in components)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var result = await invoke(c, state, cancellationToken, metadata).ConfigureAwait(false);
                    cancellationToken.ThrowIfCancellationRequested();
                    editor.AddRange(result);
                }
            }
            catch
            {
                foreach (var item in editor)
                    item.Dispose();
                throw;
            }

            return editor.ToItemOrList();
        }

        public static async ValueTask<EnumFlags<T>> InvokeAllAsync<TComponent, TState, T>(this ItemOrArray<TComponent> components, TState state,
            CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata,
            Func<TComponent, TState, CancellationToken, IReadOnlyMetadataContext?, ValueTask<EnumFlags<T>>> invoke)
            where TComponent : class, IComponent
            where T : class, IFlagsEnum
        {
            Should.NotBeNull(invoke, nameof(invoke));
            EnumFlags<T> result = default;
            foreach (var c in components)
            {
                cancellationToken.ThrowIfCancellationRequested();
                result |= await invoke(c, state, cancellationToken, metadata).ConfigureAwait(false);
                cancellationToken.ThrowIfCancellationRequested();
            }

            return result;
        }

        public static Task InvokeAllAsync<TComponent, TState>(this ItemOrArray<TComponent> components, TState state, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata, Func<TComponent, TState, CancellationToken, IReadOnlyMetadataContext?, Task> invoke)
            where TComponent : class, IComponent
        {
            Should.NotBeNull(invoke, nameof(invoke));

            Task Invoke(TComponent component)
            {
                try
                {
                    return invoke(component, state, cancellationToken, metadata);
                }
                catch (Exception e)
                {
                    return e.ToTask();
                }
            }

            if (components.Count == 0)
                return Task.CompletedTask;
            if (components.Count == 1)
                return Invoke(components[0]);

            var tasks = new ItemOrListEditor<Task>();
            foreach (var c in components)
            {
                var result = Invoke(c);
                if (!result.IsCompletedSuccessfully())
                    tasks.Add(result);
            }

            if (tasks.Count == 0)
                return Task.CompletedTask;
            return tasks.WhenAll();
        }
    }
}