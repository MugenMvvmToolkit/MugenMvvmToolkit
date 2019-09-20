using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Presenters.Components
{
    public interface IConditionPresenterComponent : IComponent<IPresenter>
    {
        bool CanShow(IPresenterComponent presenterComponent, IMetadataContext metadata);

        bool CanClose(ICloseablePresenterComponent presenterComponent, IReadOnlyList<IPresenterResult> results, IMetadataContext metadata);

        bool CanRestore(IRestorablePresenterComponent presenterComponent, IReadOnlyList<IPresenterResult> results, IMetadataContext metadata);
    }
}