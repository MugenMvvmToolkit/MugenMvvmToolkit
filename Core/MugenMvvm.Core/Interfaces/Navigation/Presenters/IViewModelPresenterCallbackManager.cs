﻿using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IViewModelPresenterCallbackManager : IHasListeners<IViewModelPresenterCallbackManagerListener>, IAttachableComponent<IViewModelPresenter>,
        IDetachableComponent<IViewModelPresenter>
    {
        IDisposable BeginPresenterOperation(IReadOnlyMetadataContext metadata);

        INavigationCallback<T> AddCallback<T>(IViewModelBase viewModel, NavigationCallbackType callbackType, IChildViewModelPresenterResult presenterResult,
            IReadOnlyMetadataContext metadata);
    }
}