using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Presenters;

namespace MugenMvvm.Interfaces.Presenters.Components
{
    public interface IConditionPresenterComponent : IComponent<IPresenter>
    {
        bool CanShow<TRequest>(IPresenterComponent presenter, [DisallowNull]in TRequest request, IReadOnlyMetadataContext? metadata);

        bool CanClose<TRequest>(IPresenterComponent presenter, IReadOnlyList<PresenterResult> results, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata);

        bool CanRestore<TRequest>(IPresenterComponent presenter, IReadOnlyList<PresenterResult> results, [DisallowNull] in TRequest request, IReadOnlyMetadataContext? metadata);
    }
}