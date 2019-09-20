using System;
using System.Collections.Generic;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Presenters.Components
{
    public interface IPresenterRestoreListener : IComponent<IPresenter>
    {
        void OnRestoring(IPresenter presenter, string operationId, IMetadataContext metadata);

        void OnRestored(IPresenter presenter, string operationId, IReadOnlyList<IPresenterResult> results, IMetadataContext metadata);

        void OnRestoreError(IPresenter presenter, string operationId, Exception exception, IMetadataContext metadata);
    }
}