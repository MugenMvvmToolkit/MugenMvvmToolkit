using System.Threading;
using System.Threading.Tasks;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface IHasNavigationCondition
    {
        ValueTask<bool> CanNavigateFromAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, object? toTarget, CancellationToken cancellationToken);

        ValueTask<bool> CanNavigateToAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, object? fromTarget, CancellationToken cancellationToken);
    }
}