using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Presenters
{
    public interface IPresenter : IComponentOwner<IPresenter>, IComponent<IMugenApplication>
    {
        IPresenterResult Show<TRequest>([DisallowNull]in TRequest request, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null);

        IReadOnlyList<IPresenterResult> TryClose<TRequest>([DisallowNull]in TRequest request, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null);

        IReadOnlyList<IPresenterResult> TryRestore<TRequest>([DisallowNull]in TRequest request, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null);
        //todo merge with Show?
    }
}