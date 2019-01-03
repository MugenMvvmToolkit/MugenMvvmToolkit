using MugenMvvm.Interfaces.ViewModels;
using MugenMvvm.Models;

namespace MugenMvvm.Interfaces.Navigation
{
    public interface INavigationEntry
    {
        NavigationType Type { get; }

        IViewModel ViewModel { get; }

        object? Provider { get; }
    }
}