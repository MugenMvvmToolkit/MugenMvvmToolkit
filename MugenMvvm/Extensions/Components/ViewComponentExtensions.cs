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

        public static void OnLifecycleChanged<TState>(this IViewLifecycleDispatcherComponent[] components, IViewManager viewManager, object view, ViewLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(lifecycleState, nameof(lifecycleState));
            for (int i = 0; i < components.Length; i++)
                components[i].OnLifecycleChanged(viewManager, view, lifecycleState, state, metadata);
        }

        public static ItemOrList<IView, IReadOnlyList<IView>> TryGetViews<TRequest>(this IViewProviderComponent[] components, IViewManager viewManager, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(viewManager, nameof(viewManager));
            if (components.Length == 1)
                return components[0].TryGetViews(viewManager, request, metadata);

            ItemOrListEditor<IView, List<IView>> result = ItemOrListEditor.Get<IView>();
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetViews(viewManager, request, metadata));
            return result.ToItemOrList<IReadOnlyList<IView>>();
        }

        public static ItemOrList<IViewMapping, IReadOnlyList<IViewMapping>> TryGetMappings<TRequest>(this IViewMappingProviderComponent[] components, IViewManager viewManager, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(viewManager, nameof(viewManager));
            if (components.Length == 1)
                return components[0].TryGetMappings(viewManager, request, metadata);

            ItemOrListEditor<IViewMapping, List<IViewMapping>> result = ItemOrListEditor.Get<IViewMapping>();
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetMappings(viewManager, request, metadata));
            return result.ToItemOrList<IReadOnlyList<IViewMapping>>();
        }

        public static Task<IView>? TryInitializeAsync<TRequest>(this IViewManagerComponent[] components, IViewManager viewManager, IViewMapping mapping,
            [DisallowNull] in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(mapping, nameof(mapping));
            for (var i = 0; i < components.Length; i++)
            {
                var result = components[i].TryInitializeAsync(viewManager, mapping, request, cancellationToken, metadata);
                if (result != null)
                    return result;
            }

            return null;
        }

        public static Task? TryCleanupAsync<TRequest>(this IViewManagerComponent[] components, IViewManager viewManager, IView view, TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(view, nameof(view));
            if (components.Length == 0)
                return components[0].TryCleanupAsync(viewManager, view, request, cancellationToken, metadata);

            ItemOrListEditor<Task, List<Task>> result = ItemOrListEditor.Get<Task>();
            for (var i = 0; i < components.Length; i++)
                result.Add(components[i].TryCleanupAsync(viewManager, view, request, cancellationToken, metadata));
            return result.ToItemOrList().WhenAll();
        }

        #endregion
    }
}