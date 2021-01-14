using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using Should;

namespace MugenMvvm.UnitTests.Navigation.Internal
{
    public class TestNavigationConditionComponent : INavigationConditionComponent, IHasPriority
    {
        private readonly INavigationDispatcher? _navigationDispatcher;

        public TestNavigationConditionComponent(INavigationDispatcher? navigationDispatcher = null)
        {
            _navigationDispatcher = navigationDispatcher;
        }

        public Func<INavigationContext, CancellationToken, ValueTask<bool>?>? CanNavigateAsync { get; set; }

        public int Priority { get; set; }

        ValueTask<bool> INavigationConditionComponent.CanNavigateAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext,
            CancellationToken cancellationToken)
        {
            _navigationDispatcher?.ShouldEqual(navigationDispatcher);
            return CanNavigateAsync?.Invoke(navigationContext, cancellationToken) ?? new ValueTask<bool>(true);
        }
    }
}