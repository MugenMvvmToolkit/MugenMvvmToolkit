using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Views.Components
{
    public interface IViewManagerComponent : IComponent<IViewManager>
    {
        Task<IView>? TryInitializeAsync<TRequest>(IViewManager viewManager, IViewMapping mapping, [DisallowNull] in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);

        Task? TryCleanupAsync<TRequest>(IViewManager viewManager, IView view, in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);
    }
}