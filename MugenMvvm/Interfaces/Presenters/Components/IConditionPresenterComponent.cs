using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Internal;

namespace MugenMvvm.Interfaces.Presenters.Components
{
    public interface IConditionPresenterComponent : IComponent<IPresenter>
    {
        bool CanShow<TRequest>(IPresenter presenter, IPresenterComponent presenterComponent, ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> results, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata);

        bool CanClose<TRequest>(IPresenter presenter, IPresenterComponent presenterComponent, ItemOrList<IPresenterResult, IReadOnlyList<IPresenterResult>> results, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}