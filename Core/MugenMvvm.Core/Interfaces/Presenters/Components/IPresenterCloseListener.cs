using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Presenters.Components
{
    public interface IPresenterCloseListener : IComponent<IPresenter>
    {
        void OnClosing(IPresenter presenter, string operationId, IMetadataContext metadata);

        void OnClosed(IPresenter presenter, string operationId, IReadOnlyList<IPresenterResult> results, IMetadataContext metadata);

        void OnCloseError(IPresenter presenter, string operationId, Exception exception, IMetadataContext metadata);
    }
}