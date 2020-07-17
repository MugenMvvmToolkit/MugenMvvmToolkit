﻿using System.Collections.Generic;
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

        public void OnLifecycleChanged(object view, ViewLifecycleState lifecycleState, object? state = null, IReadOnlyMetadataContext? metadata = null)
        {
            GetComponents<IViewLifecycleDispatcherComponent>().OnLifecycleChanged(this, view, lifecycleState, state, metadata);
        }

        public ItemOrList<IView, IReadOnlyList<IView>> GetViews(object request, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IViewProviderComponent>(metadata).TryGetViews(this, request, metadata);
        }

        public ItemOrList<IViewMapping, IReadOnlyList<IViewMapping>> GetMappings(object request, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IViewMappingProviderComponent>(metadata).TryGetMappings(this, request, metadata);
        }

        public Task<IView>? TryInitializeAsync(IViewMapping mapping, object request, CancellationToken cancellationToken = default,
            IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IViewManagerComponent>(metadata).TryInitializeAsync(this, mapping, request, cancellationToken, metadata);
        }

        public Task? TryCleanupAsync(IView view, object? state = null, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null)
        {
            return GetComponents<IViewManagerComponent>(metadata).TryCleanupAsync(this, view, state, cancellationToken, metadata);
        }

        #endregion
    }
}