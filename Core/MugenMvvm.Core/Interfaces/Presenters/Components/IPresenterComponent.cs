using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Presenters.Components
{
    public interface IPresenterComponent : IComponent<IPresenter>
    {
        IReadOnlyList<IPresenterResult>? TryShow<TRequest>([DisallowNull] in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);

        IReadOnlyList<IPresenterResult>? TryClose<TRequest>([DisallowNull] in TRequest request, CancellationToken cancellationToken, IReadOnlyMetadataContext? metadata);
    }
}