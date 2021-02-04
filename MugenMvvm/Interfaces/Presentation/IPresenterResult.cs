using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;

namespace MugenMvvm.Interfaces.Presentation
{
    public interface IPresenterResult : IMetadataOwner<IMetadataContext>, IHasNavigationInfo, IHasNavigationProvider, IHasTarget<object?>
    {
    }
}