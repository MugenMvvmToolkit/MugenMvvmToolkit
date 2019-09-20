using MugenMvvm.Enums;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Navigation;

namespace MugenMvvm.Interfaces.Presenters
{
    public interface IPresenterResult : IMetadataOwner<IMetadataContext>
    {
        string NavigationOperationId { get; }

        INavigationProvider NavigationProvider { get; }

        NavigationType NavigationType { get; }
    }
}