using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationContext : IHasMetadata<IMetadataContext>//todo navigation id?
    {
        NavigationMode NavigationMode { get; }

        NavigationType NavigationType { get; }

        INavigationProvider NavigationProvider { get; }

        IViewModel? ViewModelFrom { get; }

        IViewModel? ViewModelTo { get; }
    }
}