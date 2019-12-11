using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Views
{
    public interface IView : IMetadataOwner<IMetadataContext>
    {
        IViewModelViewMapping Mapping { get; }

        object View { get; }
    }
}