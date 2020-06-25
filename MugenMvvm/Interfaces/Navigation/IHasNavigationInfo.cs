using MugenMvvm.Enums;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface IHasNavigationInfo
    {
        string NavigationId { get; }

        NavigationType NavigationType { get; }
    }
}