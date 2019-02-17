using MugenMvvm.Interfaces.ViewModels;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface IViewModelPresenterResult : IChildViewModelPresenterResult
    {
        IViewModelBase ViewModel { get; }

        INavigationCallback ShowingCallback { get; }

        INavigationCallback CloseCallback { get; }
    }
}