using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Extensions.Components
{
    public static class ViewComponentExtensions
    {
        #region Methods

        public static void OnLifecycleChanged<TState>(this IViewLifecycleDispatcherComponent[] components, IView view, ViewLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(lifecycleState, nameof(lifecycleState));
            for (int i = 0; i < components.Length; i++)
                components[i].OnLifecycleChanged(view, lifecycleState, state, metadata);
        }

        public static IReadOnlyList<IView>? TryGetViews<TRequest>(this IViewProviderComponent[] components, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            if (components.Length == 1)
                return components[0].TryGetViews(request, metadata);

            LazyList<IView> result = default;
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetViews(request, metadata));
            return result.List;
        }

        public static IReadOnlyList<IViewModelViewMapping>? TryGetMappings<TRequest>(this IViewModelViewMappingProviderComponent[] components, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            if (components.Length == 1)
                return components[0].TryGetMappings(request, metadata);

            LazyList<IViewModelViewMapping> result = default;
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetMappings(request, metadata));
            return result.List;
        }

        public static Task<IView>? TryInitializeAsync<TRequest>(this IViewInitializerComponent[] components, IViewModelViewMapping mapping,
            [DisallowNull] in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(mapping, nameof(mapping));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryInitializeAsync(mapping, request, cancellationToken, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static Task? TryCleanupAsync<TRequest>(this IViewInitializerComponent[] components, IView view, TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(view, nameof(view));
            if (components.Length == 0)
                return components[0].TryCleanupAsync(view, request, cancellationToken, metadata);

            LazyList<Task> result = default;
            for (var i = 0; i < components.Length; i++)
            {
                var task = components[i].TryCleanupAsync(view, request, cancellationToken, metadata);
                if (task != null)
                    result.Add(task);
            }

            if (result.List == null)
                return null;
            if (result.Count == 1)
                return result.List[0];
            return Task.WhenAll(result.List);
        }

        #endregion
    }
}