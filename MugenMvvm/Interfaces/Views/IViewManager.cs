﻿using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Views
{
    public interface IViewManager : IComponentOwner<IViewManager>
    {
        void OnLifecycleChanged<TState>(object view, ViewLifecycleState lifecycleState, in TState state, IReadOnlyMetadataContext? metadata = null);

        ItemOrList<IView, IReadOnlyList<IView>> GetViews<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null);

        ItemOrList<IViewMapping, IReadOnlyList<IViewMapping>> GetMappings<TRequest>([DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata = null);

        Task<IView>? TryInitializeAsync<TRequest>(IViewMapping mapping, [DisallowNull] in TRequest request, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null);

        Task? TryCleanupAsync<TRequest>(IView view, in TRequest request, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null);
    }
}