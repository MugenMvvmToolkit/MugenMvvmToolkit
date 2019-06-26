using System;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Presenters.Components
{
    public interface IPresenterShowListener : IComponent<IPresenter>
    {
        void OnShowing(IPresenter presenter, string operationId, IMetadataContext metadata);

        void OnShown(IPresenter presenter, string operationId, IPresenterResult result, IMetadataContext metadata);

        void OnShowError(IPresenter presenter, string operationId, Exception exception, IMetadataContext metadata);
    }
}