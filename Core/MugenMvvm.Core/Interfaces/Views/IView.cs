using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;

namespace MugenMvvm.Interfaces.Views
{
    public interface IView : IComponentOwner<IView>, IMetadataOwner<IMetadataContext>
    {
        IViewModelViewMapping Mapping { get; }

        object View { get; }
    }
}