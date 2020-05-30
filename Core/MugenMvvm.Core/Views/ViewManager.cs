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

        public void OnLifecycleChanged<TState>(IView view, ViewLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata = null)
        {
            GetComponents<IViewLifecycleDispatcherComponent>().OnLifecycleChanged(view, lifecycleState, state, metadata);
        }

        public IReadOnlyList<IView> GetViews<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IViewProviderComponent>(metadata).TryGetViews(request, metadata) ?? Default.Array<IView>();
        }

        public IReadOnlyList<IViewModelViewMapping> GetMappings<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IViewModelViewMappingProviderComponent>(metadata).TryGetMappings(request, metadata) ?? Default.Array<IViewModelViewMapping>();
        }

        public Task<IView> InitializeAsync<TRequest>(IViewModelViewMapping mapping, [DisallowNull] in TRequest request, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            var task = GetComponents<IViewInitializerComponent>(metadata).TryInitializeAsync(mapping, request, cancellationToken, metadata);
            if (task == null)
                ExceptionManager.ThrowObjectNotInitialized(this);
            return task;
        }

        public Task CleanupAsync<TRequest>(IView view, [DisallowNull] in TRequest request, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IViewInitializerComponent>(metadata).TryCleanupAsync(view, request, cancellationToken, metadata) ?? Task.CompletedTask;
        }

        #endregion
    }
}