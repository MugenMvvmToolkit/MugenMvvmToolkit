using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using Should;

namespace MugenMvvm.UnitTests.Navigation.Internal
{
    public class TestConditionNavigationDispatcherComponent : IConditionNavigationDispatcherComponent, IHasPriority
    {
        #region Fields

        private readonly INavigationDispatcher? _navigationDispatcher;

        #endregion

        #region Constructors

        public TestConditionNavigationDispatcherComponent(INavigationDispatcher? navigationDispatcher = null)
        {
            _navigationDispatcher = navigationDispatcher;
        }

        #endregion

        #region Properties

        public Func<INavigationContext, CancellationToken, Task<bool>?>? CanNavigateAsync { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        Task<bool>? IConditionNavigationDispatcherComponent.CanNavigateAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            _navigationDispatcher?.ShouldEqual(navigationDispatcher);
            return CanNavigateAsync?.Invoke(navigationContext, cancellationToken);
        }

        #endregion
    }
}