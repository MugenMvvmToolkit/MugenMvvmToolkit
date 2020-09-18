using System.Collections.Generic;
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
        void OnLifecycleChanged(object view, ViewLifecycleState lifecycleState, object? state = null, IReadOnlyMetadataContext? metadata = null);

        ItemOrList<IView, IReadOnlyList<IView>> GetViews(object request, IReadOnlyMetadataContext? metadata = null);

        ItemOrList<IViewMapping, IReadOnlyList<IViewMapping>> GetMappings(object request, IReadOnlyMetadataContext? metadata = null);

        ValueTask<IView?> TryInitializeAsync(IViewMapping mapping, object request, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null);

        Task<bool> TryCleanupAsync(IView view, object? state = null, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null);
    }
}