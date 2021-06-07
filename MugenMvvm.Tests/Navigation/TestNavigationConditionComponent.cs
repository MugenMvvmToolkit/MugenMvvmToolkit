using System;
using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Interfaces.Models;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;

namespace MugenMvvm.Tests.Navigation
{
    public class TestNavigationConditionComponent : INavigationConditionComponent, IHasPriority
    {
        public Func<INavigationDispatcher, INavigationContext, CancellationToken, ValueTask<bool>?>? CanNavigateAsync { get; set; }

        public int Priority { get; set; }

        ValueTask<bool> INavigationConditionComponent.CanNavigateAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext,
            CancellationToken cancellationToken) =>
            CanNavigateAsync?.Invoke(navigationDispatcher, navigationContext, cancellationToken) ?? new ValueTask<bool>(true);
    }
}