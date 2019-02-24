using System.Collections.Generic;

namespace MugenMvvm.Interfaces.Navigation.Presenters
{
    public interface INavigationMediatorViewModelPresenter : IRestorableChildViewModelPresenter
    {
        void AddManager(INavigationMediatorViewModelPresenterManager manager);

        void RemoveManager(INavigationMediatorViewModelPresenterManager manager);

        IReadOnlyList<INavigationMediatorViewModelPresenterManager> GetManagers();
    }
}