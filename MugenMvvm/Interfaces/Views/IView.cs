using MugenMvvm.Interfaces.Components;
using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views
{
    public interface IView : IComponentOwner<IView>, IMetadataOwner<IMetadataContext>, IWrapper<object>
    {
        IViewMapping Mapping { get; }

        IViewModelBase ViewModel { get; }
    }
}