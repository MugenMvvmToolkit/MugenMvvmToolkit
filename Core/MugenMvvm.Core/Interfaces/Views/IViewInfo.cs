using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Views
{
    public interface IViewInfo : IMetadataOwner<IMetadataContext>
    {
        IViewInitializer Initializer { get; }

        object View { get; }
    }
}