using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Presenters;

namespace MugenMvvm.Interfaces.Presenters
{
    public interface IPresenter : IComponentOwner<IPresenter>, IComponent<IMugenApplication>
    {
        PresenterResult Show<TRequest>([DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata = null, CancellationToken cancellationToken = default);

        IReadOnlyList<PresenterResult> TryClose<TRequest>([DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata = null, CancellationToken cancellationToken = default);

        IReadOnlyList<PresenterResult> TryRestore<TRequest>([DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata = null, CancellationToken cancellationToken = default);
    }
}