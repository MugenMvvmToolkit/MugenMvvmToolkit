using MugenMvvm.Interfaces.Metadata;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views.Infrastructure
{
    public interface IViewManagerResult : IHasMetadata<IReadOnlyMetadataContext>
    {
        IViewModelBase ViewModel { get; }

        IViewInfo ViewInfo { get; }
    }
}