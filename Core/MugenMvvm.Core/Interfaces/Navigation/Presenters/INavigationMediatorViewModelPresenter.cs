using MugenMvvm.Interfaces.Collections;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface INavigationMediatorViewModelPresenter : IRestorableChildViewModelPresenter
    {
        IComponentCollection<INavigationMediatorViewModelPresenterManager> Managers { get; }
    }
}