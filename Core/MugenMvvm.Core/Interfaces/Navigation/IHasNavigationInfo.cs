using MugenMvvm.Enums;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface IHasNavigationInfo
    {
        string NavigationOperationId { get; }

        NavigationType NavigationType { get; }
    }
}