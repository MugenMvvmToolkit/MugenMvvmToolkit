using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Attributes;
using MugenMvvm.Components;
using MugenMvvm.Enums;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Components;
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
        public ViewManager(IComponentCollectionProvider? componentCollectionProvider = null)
            : base(componentCollectionProvider)
        {
        }

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged<TState>(object view, ViewLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata = null)
        {
            GetComponents<IViewLifecycleDispatcherComponent>().OnLifecycleChanged(view, lifecycleState, state, metadata);
        }

        public ItemOrList<IView, IReadOnlyList<IView>> GetViews<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IViewProviderComponent>(metadata).TryGetViews(request, metadata);
        }

        public ItemOrList<IViewModelViewMapping, IReadOnlyList<IViewModelViewMapping>> GetMappings<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IViewModelViewMappingProviderComponent>(metadata).TryGetMappings(request, metadata);
        }

        public Task<IView>? TryInitializeAsync<TRequest>(IViewModelViewMapping mapping, [DisallowNull] in TRequest request, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IViewManagerComponent>(metadata).TryInitializeAsync(mapping, request, cancellationToken, metadata);
        }

        public Task? TryCleanupAsync<TRequest>(IView view, [DisallowNull] in TRequest request, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IViewManagerComponent>(metadata).TryCleanupAsync(view, request, cancellationToken, metadata);
        }

        #endregion
    }
}