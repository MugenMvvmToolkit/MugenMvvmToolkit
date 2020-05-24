using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Views
{
    public interface IView : IComponentOwner<IView>, IMetadataOwner<IMetadataContext>, IHasTarget<object>
    {
        IViewModelViewMapping Mapping { get; }
    }
}