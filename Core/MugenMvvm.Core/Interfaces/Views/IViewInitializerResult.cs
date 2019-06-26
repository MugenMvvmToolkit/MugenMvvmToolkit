using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views
{
    public interface IViewInitializerResult : IMetadataOwner<IReadOnlyMetadataContext>
    {
        IViewModelBase ViewModel { get; }

        IViewInfo ViewInfo { get; }
    }
}