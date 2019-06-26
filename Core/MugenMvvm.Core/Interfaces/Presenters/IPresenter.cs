using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Presenters
{
    public interface IPresenter : IComponentOwner<IPresenter>
    {
        IPresenterResult Show(IReadOnlyMetadataContext metadata);

        IReadOnlyList<IPresenterResult> TryClose(IReadOnlyMetadataContext metadata);

        IReadOnlyList<IPresenterResult> TryRestore(IReadOnlyMetadataContext metadata);
    }
}