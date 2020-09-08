using System.Threading;
using System.Threading.Tasks;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface IHasNavigationCondition
    {
        Task<bool>? CanNavigateFromAsync(object? toTarget, INavigationContext navigationContext, CancellationToken cancellationToken);

        Task<bool>? CanNavigateToAsync(object? fromTarget, INavigationContext navigationContext, CancellationToken cancellationToken);
    }
}