using MugenMvvmToolkit.Interfaces.Navigation;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;
using MugenMvvmToolkit.Models.EventArg;

namespace MugenMvvmToolkit.Infrastructure.Mediators
{
    public sealed class MultiViewModelNavigationMediator//todo check on iOS
    {
        #region Fields

        private INavigationDispatcher _navigationDispatcher;

        #endregion

        #region Constructors

        public MultiViewModelNavigationMediator(IMultiViewModel multiViewModel)
        {
            multiViewModel.SelectedItemChanged += OnSelectedItemChanged;
            multiViewModel.ViewModelRemoved += OnViewModelRemoved;
        }

        #endregion

        #region Properties

        private INavigationDispatcher NavigationDispatcher
        {
            get
            {
                if (_navigationDispatcher == null)
                    _navigationDispatcher = ServiceProvider.Get<INavigationDispatcher>();
                return _navigationDispatcher;
            }
        }

        #endregion

        #region Methods

        private void OnViewModelRemoved(IMultiViewModel sender, ValueEventArgs<IViewModel> args)
        {
            NavigationDispatcher.OnNavigated(new NavigationContext(NavigationType.Tab, NavigationMode.Remove, args.Value, null, this));
        }

        private void OnSelectedItemChanged(IMultiViewModel sender, SelectedItemChangedEventArgs<IViewModel> args)
        {
            NavigationDispatcher.OnNavigated(new NavigationContext(NavigationType.Tab, NavigationMode.Refresh, args.OldValue, args.NewValue, sender));
        }

        #endregion
    }
}