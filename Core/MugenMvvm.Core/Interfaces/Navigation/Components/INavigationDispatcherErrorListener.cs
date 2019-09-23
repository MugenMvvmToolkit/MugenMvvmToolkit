﻿using System;
using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Navigation.Components
{
    public interface INavigationDispatcherErrorListener : IComponent<INavigationDispatcher>
    {
        void OnNavigationFailed(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, Exception exception);

        void OnNavigationCanceled(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext);
    }
}