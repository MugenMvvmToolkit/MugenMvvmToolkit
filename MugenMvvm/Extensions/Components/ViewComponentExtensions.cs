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

        public static void OnLifecycleChanged<TState>(this IViewLifecycleDispatcherComponent[] components, object view, ViewLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(lifecycleState, nameof(lifecycleState));
            for (int i = 0; i < components.Length; i++)
                components[i].OnLifecycleChanged(view, lifecycleState, state, metadata);
        }

        public static ItemOrList<IView, IReadOnlyList<IView>> TryGetViews<TRequest>(this IViewProviderComponent[] components, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            if (components.Length == 1)
                return components[0].TryGetViews(request, metadata);

            ItemOrList<IView, List<IView>> result = default;
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetViews(request, metadata));
            return result.Cast<IReadOnlyList<IView>>();
        }

        public static ItemOrList<IViewModelViewMapping, IReadOnlyList<IViewModelViewMapping>> TryGetMappings<TRequest>(this IViewModelViewMappingProviderComponent[] components, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            if (components.Length == 1)
                return components[0].TryGetMappings(request, metadata);

            ItemOrList<IViewModelViewMapping, List<IViewModelViewMapping>> result = default;
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetMappings(request, metadata));
            return result.Cast<IReadOnlyList<IViewModelViewMapping>>();
        }

        public static Task<IView>? TryInitializeAsync<TRequest>(this IViewManagerComponent[] components, IViewModelViewMapping mapping,
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

        public static Task? TryCleanupAsync<TRequest>(this IViewManagerComponent[] components, IView view, TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(view, nameof(view));
            if (components.Length == 0)
                return components[0].TryCleanupAsync(view, request, cancellationToken, metadata);

            ItemOrList<Task, List<Task>> result = default;
            for (var i = 0; i < components.Length; i++)
                result.Add(components[i].TryCleanupAsync(view, request, cancellationToken, metadata));
            return result.WhenAll();
        }

        #endregion
    }
}