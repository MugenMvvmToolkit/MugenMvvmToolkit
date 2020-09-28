using System.Threading;
using System.Threading.Tasks;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface IHasNavigationConditionCallback
    {
        Task<bool> CanNavigateFromAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, object? toTarget, CancellationToken cancellationToken);

        Task<bool> CanNavigateToAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, object? fromTarget, CancellationToken cancellationToken);
    }
}