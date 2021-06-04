using System.Threading;
using System.Threading.Tasks;
using MugenMvvm.Components;
using MugenMvvm.Constants;
using MugenMvvm.Extensions;
using MugenMvvm.Extensions.Components;
using MugenMvvm.Interfaces.Navigation;
using MugenMvvm.Interfaces.Navigation.Components;
using MugenMvvm.Metadata;

namespace MugenMvvm.Navigation.Components
{
    public sealed class ForceCloseNavigationHandler : ComponentDecoratorBase<INavigationDispatcher, INavigationConditionComponent>, INavigationConditionComponent
    {
        public ForceCloseNavigationHandler(int priority = NavigationComponentPriority.ForceCloseHandler) : base(priority)
        {
        }

        public ValueTask<bool> CanNavigateAsync(INavigationDispatcher navigationDispatcher, INavigationContext navigationContext, CancellationToken cancellationToken)
        {
            if (navigationContext.GetOrDefault(NavigationMetadata.ForceClose))
                return new ValueTask<bool>(true);
            return Components.CanNavigateAsync(navigationDispatcher, navigationContext, cancellationToken);
        }
    }
}