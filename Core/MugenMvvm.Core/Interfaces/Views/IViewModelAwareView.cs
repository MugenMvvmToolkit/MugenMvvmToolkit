using MugenMvvm.Attributes;
using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Views
{
    public interface IViewModelAwareView<TViewModel> : IView where TViewModel : class, IViewModelBase
    {
        [Preserve(Conditional = true)]
        TViewModel? ViewModel { get; set; }
    }
}