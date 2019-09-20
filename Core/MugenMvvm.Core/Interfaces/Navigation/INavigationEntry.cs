using System;
using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationEntry : IMetadataOwner<IReadOnlyMetadataContext>
    {
        string NavigationOperationId { get; }

        DateTime NavigationDate { get; }

        NavigationType NavigationType { get; }

        INavigationProvider NavigationProvider { get; }
    }
}