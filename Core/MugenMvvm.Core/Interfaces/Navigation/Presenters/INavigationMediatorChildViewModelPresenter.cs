using MugenMvvm.Interfaces.Components;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface INavigationMediatorChildViewModelPresenter : IRestorableChildViewModelPresenter
    {
        IComponentCollection<INavigationMediatorViewModelPresenterManager> Managers { get; }
    }
}