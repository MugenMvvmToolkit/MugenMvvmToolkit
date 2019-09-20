using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Presenters.Components
{
    public interface ICloseablePresenterComponent : IComponent<IPresenter>
    {
        IReadOnlyList<IPresenterResult> TryClose(IMetadataContext metadata);
    }
}