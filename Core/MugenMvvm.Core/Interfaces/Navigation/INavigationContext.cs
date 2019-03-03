using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationContext : IHasMetadata<IMetadataContext>
    {
        NavigationMode NavigationMode { get; }

        NavigationType NavigationType { get; }//todo 2 type
        
        INavigationProvider NavigationProvider { get; }

        IViewModelBase? ViewModelFrom { get; }

        IViewModelBase? ViewModelTo { get; }
    }
}