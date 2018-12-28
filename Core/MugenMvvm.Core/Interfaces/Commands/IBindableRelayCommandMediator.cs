using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Commands
{
    public interface IBindableRelayCommandMediator : INotifyPropertyChangedEx, IHasDisplayName
    {
        bool IsCanExecuteNullParameter { get; }

        bool IsCanExecuteLastParameter { get; }
    }
}