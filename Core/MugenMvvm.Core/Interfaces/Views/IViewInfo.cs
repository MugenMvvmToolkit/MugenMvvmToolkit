using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Views
{
    public interface IViewInfo : IMetadataOwner<IMetadataContext>
    {
        IViewInitializer Initializer { get; }

        object View { get; }
    }
}