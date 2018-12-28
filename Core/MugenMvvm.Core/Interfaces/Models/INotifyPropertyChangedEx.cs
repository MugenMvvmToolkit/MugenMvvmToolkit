using System.ComponentModel;

namespace MugenMvvm.Interfaces.Models
{
    public interface INotifyPropertyChangedEx : INotifyPropertyChanged, ISuspendNotifications
    {
        void InvalidateProperties();
    }
}