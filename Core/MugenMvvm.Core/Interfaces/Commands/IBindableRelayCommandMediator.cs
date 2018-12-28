using System.ComponentModel;
using MugenMvvm.Interfaces.Models;

namespace MugenMvvm.Interfaces.Commands
{
    public interface IBindableRelayCommandMediator : INotifyPropertyChanged, IHasDisplayName, ISuspendNotifications
    {
        bool IsCanExecuteNullParameter { get; }

        bool IsCanExecuteLastParameter { get; }

        void InvalidateProperties();
    }
}