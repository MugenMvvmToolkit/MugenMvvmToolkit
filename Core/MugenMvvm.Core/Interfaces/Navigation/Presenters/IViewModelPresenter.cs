﻿using System.Collections.Generic;
using MugenMvvm.Interfaces.Collections;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    //todo update listener add mediator
    //todo return empty metadata
    //todo change setter, move all to handler container
    public interface IViewModelPresenter : IHasListeners<IViewModelPresenterListener>
    {
        IViewModelPresenterCallbackManager CallbackManager { get; }

        IComponentCollection<IChildViewModelPresenter> Presenters { get; }

        IViewModelPresenterResult Show(IReadOnlyMetadataContext metadata);

        IReadOnlyList<IClosingViewModelPresenterResult> TryClose(IReadOnlyMetadataContext metadata);

        IRestorationViewModelPresenterResult TryRestore(IReadOnlyMetadataContext metadata);
    }
}