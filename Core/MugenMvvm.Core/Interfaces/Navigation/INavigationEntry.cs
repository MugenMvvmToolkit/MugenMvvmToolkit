using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationEntry : IMetadataOwner<IMetadataContext>
    {
        string NavigationOperationId { get; }

        NavigationType NavigationType { get; }

        INavigationProvider NavigationProvider { get; }
    }
}