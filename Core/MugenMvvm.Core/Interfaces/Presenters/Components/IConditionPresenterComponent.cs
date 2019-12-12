using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Presenters;

namespace MugenMvvm.Interfaces.Presenters.Components
{
    public interface IConditionPresenterComponent : IComponent<IPresenter>
    {
        bool CanShow(IPresenterComponent presenterComponent, IReadOnlyMetadataContext metadata);

        bool CanClose(IPresenterComponent presenterComponent, IReadOnlyList<PresenterResult> results, IReadOnlyMetadataContext metadata);

        bool CanRestore(IPresenterComponent presenterComponent, IReadOnlyList<PresenterResult> results, IReadOnlyMetadataContext metadata);
    }
}