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
        public ViewManager(IComponentCollectionManager? componentCollectionManager = null)
            : base(componentCollectionManager)
        {
        }

        #endregion

        #region Implementation of interfaces

        public void OnLifecycleChanged<TState>(object view, ViewLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata = null)
        {
            GetComponents<IViewLifecycleDispatcherComponent>().OnLifecycleChanged(this, view, lifecycleState, state, metadata);
        }

        public ItemOrList<IView, IReadOnlyList<IView>> GetViews<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IViewProviderComponent>(metadata).TryGetViews(this, request, metadata);
        }

        public ItemOrList<IViewMapping, IReadOnlyList<IViewMapping>> GetMappings<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IViewMappingProviderComponent>(metadata).TryGetMappings(this, request, metadata);
        }

        public Task<IView>? TryInitializeAsync<TRequest>(IViewMapping mapping, [DisallowNull] in TRequest request, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IViewManagerComponent>(metadata).TryInitializeAsync(this, mapping, request, cancellationToken, metadata);
        }

        public Task? TryCleanupAsync<TRequest>(IView view, [DisallowNull] in TRequest request, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IViewManagerComponent>(metadata).TryCleanupAsync(this, view, request, cancellationToken, metadata);
        }

        #endregion
    }
}