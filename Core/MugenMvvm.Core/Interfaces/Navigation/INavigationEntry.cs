using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationEntry
    {
        DateTime NavigationDate { get; }

        NavigationType NavigationType { get; }

        INavigationProvider NavigationProvider { get; }

        IViewModelBase ViewModel { get; }
    }
}