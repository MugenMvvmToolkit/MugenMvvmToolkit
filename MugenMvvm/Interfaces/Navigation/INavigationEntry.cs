using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationEntry : IMetadataOwner<IMetadataContext>, IHasNavigationInfo, IHasNavigationProvider, IHasTarget<object?>
    {
    }
}