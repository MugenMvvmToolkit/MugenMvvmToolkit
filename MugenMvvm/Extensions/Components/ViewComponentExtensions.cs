using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;

namespace MugenMvvm.Extensions.Components
{
    public static class ViewComponentExtensions
    {
        #region Methods

        public static void OnLifecycleChanged(this IViewLifecycleListener[] components, IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(lifecycleState, nameof(lifecycleState));
            for (var i = 0; i < components.Length; i++)
                components[i].OnLifecycleChanged(viewManager, view, lifecycleState, state, metadata);
        }

        public static ItemOrIReadOnlyList<IView> TryGetViews(this IViewProviderComponent[] components, IViewManager viewManager, object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(request, nameof(request));
            if (components.Length == 1)
                return components[0].TryGetViews(viewManager, request, metadata);

            var result = new ItemOrListEditor<IView>();
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetViews(viewManager, request, metadata));
            return result.ToItemOrList();
        }

        public static ItemOrIReadOnlyList<IViewMapping> TryGetMappings(this IViewMappingProviderComponent[] components, IViewManager viewManager, object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(request, nameof(request));
            if (components.Length == 1)
                return components[0].TryGetMappings(viewManager, request, metadata);

            var result = new ItemOrListEditor<IViewMapping>();
            for (var i = 0; i < components.Length; i++)
                result.AddRange(components[i].TryGetMappings(viewManager, request, metadata));
            return result.ToItemOrList();
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

        public static async ValueTask<bool> TryCleanupAsync(this IViewManagerComponent[] components, IViewManager viewManager, IView view, object? state, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(components, nameof(components));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(view, nameof(view));
            if (components.Length == 0)
                return false;
            if (components.Length == 1)
                return await components[0].TryCleanupAsync(viewManager, view, state, cancellationToken, metadata).ConfigureAwait(false);

            var editor = new ItemOrListEditor<ValueTask<bool>>();
            for (var i = 0; i < components.Length; i++)
                editor.Add(components[i].TryCleanupAsync(viewManager, view, state, cancellationToken, metadata));
            bool result = false;
            foreach (var t in editor.ToItemOrList())
            {
                if (await t.ConfigureAwait(false))
                    result = true;
            }

            return result;
        }

        #endregion
    }
}