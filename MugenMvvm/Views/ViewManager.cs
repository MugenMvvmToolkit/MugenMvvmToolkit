using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Collections;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Internal.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Views;
using MugenMvvm.Interfaces.Views.Components;
using MugenMvvm.Internal;

namespace MugenMvvm.Views
{
    public sealed class ViewManager : ComponentOwnerBase<IViewManager>, IViewManager
    {
        #region Constructors

        [Preserve(Conditional = true)]
        public ViewManager(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
        }

        #endregion

        #region Implementation of interfaces

        public bool IsInState(object view, ViewLifecycleState state, IReadOnlyMetadataContext? metadata = null)
            => GetComponents<ILifecycleTrackerComponent<ViewLifecycleState>>(metadata).IsInState(this, view, state, metadata);

        public void OnLifecycleChanged(object view, ViewLifecycleState lifecycleState, object? state = null, IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<IViewLifecycleListener>(metadata).OnLifecycleChanged(this, view, lifecycleState, state, metadata);

        public ItemOrIReadOnlyList<IView> GetViews(object request, IReadOnlyMetadataContext? metadata = null) => GetComponents<IViewProviderComponent>(metadata).TryGetViews(this, request, metadata);

        public ItemOrIReadOnlyList<IViewMapping> GetMappings(object request, IReadOnlyMetadataContext? metadata = null) => GetComponents<IViewMappingProviderComponent>(metadata).TryGetMappings(this, request, metadata);

        public ValueTask<IView?> TryInitializeAsync(IViewMapping mapping, object request, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null) => GetComponents<IViewManagerComponent>(metadata).TryInitializeAsync(this, mapping, request, cancellationToken, metadata);

        public Task<bool> TryCleanupAsync(IView view, object? state = null, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null) =>
            GetComponents<IViewManagerComponent>(metadata).TryCleanupAsync(this, view, state, cancellationToken, metadata);

        #endregion
    }
}