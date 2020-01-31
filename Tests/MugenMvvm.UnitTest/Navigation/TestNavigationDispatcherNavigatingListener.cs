using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.UnitTest.Navigation
{
    public class TestNavigationDispatcherNavigatingListener : INavigationDispatcherNavigatingListener, IHasPriority
    {
        #region Properties

        public Func<INavigationDispatcher, INavigationContext, CancellationToken, Task<bool>?> OnNavigatingAsync { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        Task<bool>? INavigationDispatcherNavigatingListener.OnNavigatingAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            return OnNavigatingAsync?.Invoke(navigationDispatcher, navigationContext, cancellationToken);
        }

        #endregion
    }
}