using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationEntry : IMetadataOwner<IMetadataContext>, IHasNavigationInfo
    {
        INavigationProvider NavigationProvider { get; }
    }
}