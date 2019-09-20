using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Presenters.Components
{
    public interface IPresenterComponent : IComponent<IPresenter>
    {
        IPresenterResult? TryShow(IMetadataContext metadata);
    }
}