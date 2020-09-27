using System.Collections.Generic;
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

        public static void OnLifecycleChanged(this IViewLifecycleDispatcherComponent[] components, IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(lifecycleState, nameof(lifecycleState));
            for (var i = 0; i < components.Length; i++)
                components[i].OnLifecycleChanged(viewManager, view, lifecycleState, state, metadata);
        }

        public static ItemOrList<IView, IReadOnlyList<IView>> TryGetViews(this IViewProviderComponent[] components, IViewManager viewManager, object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(request, nameof(request));
            if (components.Length == 1)
                return components[0].TryGetViews(viewManager, request, metadata);

            var result = ItemOrListEditor.Get<IView>();
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetViews(viewManager, request, metadata));
            return result.ToItemOrList<IReadOnlyList<IView>>();
        }

        public static ItemOrList<IViewMapping, IReadOnlyList<IViewMapping>> TryGetMappings(this IViewMappingProviderComponent[] components, IViewManager viewManager, object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(request, nameof(request));
            if (components.Length == 1)
                return components[0].TryGetMappings(viewManager, request, metadata);

            var result = ItemOrListEditor.Get<IViewMapping>();
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetMappings(viewManager, request, metadata));
            return result.ToItemOrList<IReadOnlyList<IViewMapping>>();
        }

        public static async ValueTask<IView?> TryInitializeAsync(this IViewManagerComponent[] components, IViewManager viewManager, IViewMapping mapping, object request, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(mapping, nameof(mapping));
            Should.NotBeNull(request, nameof(request));
            for (var i = 0; i < components.Length; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var view = await components[i].TryInitializeAsync(viewManager, mapping, request, cancellationToken, metadata).ConfigureAwait(false);
                if (view != null)
                    return view;
            }

            return null;
        }

        public static async Task<bool> TryCleanupAsync(this IViewManagerComponent[] components, IViewManager viewManager, IView view, object? state, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(view, nameof(view));
            var editor = ItemOrListEditor.Get<Task<bool>>();
            for (var i = 0; i < components.Length; i++)
                editor.Add(components[i].TryCleanupAsync(viewManager, view, state, cancellationToken, metadata));
            if (editor.Count == 0)
                return false;
            if (editor.Count == 1)
                return await editor[0].ConfigureAwait(false);
            var result = await Task.WhenAll((List<Task<bool>>) editor.GetRawValue()!).ConfigureAwait(false);
            return result.WhenAny();
        }

        #endregion
    }
}