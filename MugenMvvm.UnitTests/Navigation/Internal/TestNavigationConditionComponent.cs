using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Internal;
using Should;

namespace MugenMvvm.UnitTests.Navigation.Internal
{
    public class TestNavigationConditionComponent : INavigationConditionComponent, IHasPriority
    {
        #region Fields

        private readonly INavigationDispatcher? _navigationDispatcher;

        #endregion

        #region Constructors

        public TestNavigationConditionComponent(INavigationDispatcher? navigationDispatcher = null)
        {
            _navigationDispatcher = navigationDispatcher;
        }

        #endregion

        #region Properties

        public Func<INavigationContext, CancellationToken, ValueTask<bool>?>? CanNavigateAsync { get; set; }

        public int Priority { get; set; }

        #endregion

        #region Implementation of interfaces

        ValueTask<bool> INavigationConditionComponent.CanNavigateAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            _navigationDispatcher?.ShouldEqual(navigationDispatcher);
            return CanNavigateAsync?.Invoke(navigationContext, cancellationToken) ?? new ValueTask<bool>(true);
        }

        #endregion
    }
}