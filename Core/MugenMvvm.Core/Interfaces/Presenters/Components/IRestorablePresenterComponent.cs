using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Presenters.Components
{
    public interface IRestorablePresenterComponent : IComponent<IPresenter>
    {
        IReadOnlyList<IPresenterResult> TryRestore(IMetadataContext metadata);
    }
}