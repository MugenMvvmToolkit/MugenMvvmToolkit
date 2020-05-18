using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Presenters;

namespace MugenMvvm.Interfaces.Presenters.Components
{
    public interface IPresenterComponent : IComponent<IPresenter>
    {
        PresenterResult TryShow<TRequest>([DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata, CancellationToken cancellationToken);

        IReadOnlyList<PresenterResult>? TryClose<TRequest>([DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata, CancellationToken cancellationToken);

        IReadOnlyList<PresenterResult>? TryRestore<TRequest>([DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata, CancellationToken cancellationToken);
    }
}