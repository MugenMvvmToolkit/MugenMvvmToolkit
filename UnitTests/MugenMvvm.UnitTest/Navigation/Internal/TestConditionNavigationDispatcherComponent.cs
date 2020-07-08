using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.UnitTest.Navigation.Internal
{
    public class TestConditionNavigationDispatcherComponent : IConditionNavigationDispatcherComponent, IHasPriority
    {
        #region Properties

        public Func<INavigationDispatcher, INavigationContext, CancellationToken, Task<bool>?>? CanNavigateAsync { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        Task<bool>? IConditionNavigationDispatcherComponent.CanNavigateAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            return CanNavigateAsync?.Invoke(navigationDispatcher, navigationContext, cancellationToken);
        }

        #endregion
    }
}