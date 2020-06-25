using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using MugenMvvm.Interfaces.App;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Presenters
{
    public interface IPresenter : IComponentOwner<IPresenter>, IComponent<IMugenApplication>
    {
        ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryShow<TRequest>([DisallowNull] in TRequest request, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null);

        ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> TryClose<TRequest>([DisallowNull] in TRequest request, CancellationToken cancellationToken = default, IReadOnlyMetadataContext? metadata = null);
    }
}