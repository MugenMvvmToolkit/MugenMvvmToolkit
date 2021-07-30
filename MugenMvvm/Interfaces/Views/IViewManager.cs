using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Collections;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Views;

namespace MugenMvvm.Interfaces.Views
{
    public interface IViewManager : IComponentOwner<IViewManager>
    {
        bool IsInState(object view, ViewLifecycleState state, IReadOnlyMetadataContext? metadata = null);

        void OnLifecycleChanged(ViewInfo view, ViewLifecycleState lifecycleState, object? state = null, IReadOnlyMetadataContext? metadata = null);

        ItemOrIReadOnlyList<IView> GetViews(object request, IReadOnlyMetadataContext? metadata = null);

        ItemOrIReadOnlyList<IViewMapping> GetMappings(object request, IReadOnlyMetadataContext? metadata = null);

        ValueTask<IView?> TryInitializeAsync(IViewMapping mapping, object request, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null);

        Task<bool> TryCleanupAsync(IView view, object? state = null, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null);
    }
}