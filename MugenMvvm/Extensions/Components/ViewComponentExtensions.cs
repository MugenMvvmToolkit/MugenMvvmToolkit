using System.Runtime.CompilerServices;
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnLifecycleChanged(this ItemOrArray<IViewLifecycleListener> components, IViewManager viewManager, object view, ViewLifecycleState lifecycleState, object? state,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(lifecycleState, nameof(lifecycleState));
            foreach (var c in components)
                c.OnLifecycleChanged(viewManager, view, lifecycleState, state, metadata);
        }

        public static ItemOrIReadOnlyList<IView> TryGetViews(this ItemOrArray<IViewProviderComponent> components, IViewManager viewManager, object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(request, nameof(request));
            if (components.Count == 0)
                return default;
            if (components.Count == 1)
                return components[0].TryGetViews(viewManager, request, metadata);

            var result = new ItemOrListEditor<IView>();
            foreach (var c in components)
                result.AddRange(c.TryGetViews(viewManager, request, metadata));

            return result.ToItemOrList();
        }

        public static ItemOrIReadOnlyList<IViewMapping> TryGetMappings(this ItemOrArray<IViewMappingProviderComponent> components, IViewManager viewManager, object request, IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(request, nameof(request));
            if (components.Count == 0)
                return default;
            if (components.Count == 1)
                return components[0].TryGetMappings(viewManager, request, metadata);

            var result = new ItemOrListEditor<IViewMapping>();
            foreach (var c in components)
                result.AddRange(c.TryGetMappings(viewManager, request, metadata));

            return result.ToItemOrList();
        }

        public static async ValueTask<IView?> TryInitializeAsync(this ItemOrArray<IViewManagerComponent> components, IViewManager viewManager, IViewMapping mapping, object request, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(mapping, nameof(mapping));
            Should.NotBeNull(request, nameof(request));
            foreach (var c in components)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var view = await c.TryInitializeAsync(viewManager, mapping, request, cancellationToken, metadata).ConfigureAwait(false);
                if (view != null)
                    return view;
            }

            return null;
        }

        public static async ValueTask<bool> TryCleanupAsync(this ItemOrArray<IViewManagerComponent> components, IViewManager viewManager, IView view, object? state, CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(view, nameof(view));
            if (components.Count == 0)
                return false;
            if (components.Count == 1)
                return await components[0].TryCleanupAsync(viewManager, view, state, cancellationToken, metadata).ConfigureAwait(false);

            var editor = new ItemOrListEditor<ValueTask<bool>>();
            foreach (var c in components)
                editor.Add(c.TryCleanupAsync(viewManager, view, state, cancellationToken, metadata));

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