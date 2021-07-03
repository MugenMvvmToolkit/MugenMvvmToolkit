using System.Collections;
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
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void OnLifecycleChanged(this ItemOrArray<IViewLifecycleListener> components, IViewManager viewManager, object view, ViewLifecycleState lifecycleState,
            object? state,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(view, nameof(view));
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(lifecycleState, nameof(lifecycleState));
            foreach (var c in components)
                c.OnLifecycleChanged(viewManager, view, lifecycleState, state, metadata);
        }

        public static ItemOrIReadOnlyList<IView> TryGetViews(this ItemOrArray<IViewProviderComponent> components, IViewManager viewManager, object request,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(request, nameof(request));
            if (components.Count == 0)
                return default;
            if (components.Count == 1)
                return components[0].TryGetViews(viewManager, request, metadata);

            var result = new ItemOrListEditor<IView>(2);
            foreach (var c in components)
                result.AddRange(c.TryGetViews(viewManager, request, metadata));

            return result.ToItemOrList();
        }

        public static ItemOrIReadOnlyList<IViewMapping> TryGetMappings(this ItemOrArray<IViewMappingProviderComponent> components, IViewManager viewManager, object request,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(request, nameof(request));
            if (components.Count == 0)
                return default;
            if (components.Count == 1)
                return components[0].TryGetMappings(viewManager, request, metadata);

            var result = new ItemOrListEditor<IViewMapping>(2);
            foreach (var c in components)
                result.AddRange(c.TryGetMappings(viewManager, request, metadata));

            return result.ToItemOrList();
        }

        public static async ValueTask<IView?> TryInitializeAsync(this ItemOrArray<IViewManagerComponent> components, IViewManager viewManager, IViewMapping mapping, object request,
            CancellationToken cancellationToken,
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

        public static ValueTask<bool> TryCleanupAsync(this ItemOrArray<IViewManagerComponent> components, IViewManager viewManager, IView view, object? state,
            CancellationToken cancellationToken,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(view, nameof(view));
            return components.InvokeAllAsync((viewManager, view, state), cancellationToken, metadata,
                (component, s, c, m) => component.TryCleanupAsync(s.viewManager, s.view, s.state, c, m));
        }

        public static bool TryGetItemsSource(this ItemOrArray<IViewCollectionManagerComponent> components, IViewManager viewManager, object view,
            IReadOnlyMetadataContext? metadata, out IEnumerable? itemsSource)
        {
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(view, nameof(view));
            foreach (var component in components)
            {
                if (component.TryGetItemsSource(viewManager, view, metadata, out itemsSource))
                    return true;
            }

            itemsSource = null;
            return false;
        }

        public static bool TryGetItemsSourceRaw(this ItemOrArray<IViewCollectionManagerComponent> components, IViewManager viewManager, object view,
            IReadOnlyMetadataContext? metadata, out IEnumerable? itemsSource)
        {
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(view, nameof(view));
            foreach (var component in components)
            {
                if (component.TryGetItemsSourceRaw(viewManager, view, metadata, out itemsSource))
                    return true;
            }

            itemsSource = null;
            return false;
        }

        public static bool TrySetItemsSource(this ItemOrArray<IViewCollectionManagerComponent> components, IViewManager viewManager, object view, IEnumerable? value,
            IReadOnlyMetadataContext? metadata)
        {
            Should.NotBeNull(viewManager, nameof(viewManager));
            Should.NotBeNull(view, nameof(view));
            foreach (var component in components)
            {
                if (component.TrySetItemsSource(viewManager, view, value, metadata))
                    return true;
            }

            return false;
        }
    }
}