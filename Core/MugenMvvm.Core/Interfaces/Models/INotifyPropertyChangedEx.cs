using System.ComponentModel;

namespace MugenMvvm.Interfaces.Models
{
    public interface INotifyPropertyChangedEx : INotifyPropertyChanged, ISuspendable
    {
        void InvalidateProperties();
    }
}