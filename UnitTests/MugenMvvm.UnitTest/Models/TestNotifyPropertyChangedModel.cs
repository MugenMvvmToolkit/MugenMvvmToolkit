using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MugenMvvm.UnitTest.Models
{
    public class TestNotifyPropertyChangedModel : INotifyPropertyChanged
    {
        #region Fields

        private string? _property;

        #endregion

        #region Properties

        public string? Property
        {
            get => _property;
            set
            {
                if (_property == value)
                    return;
                _property = value;
                OnPropertyChanged();
            }
        }

        #endregion

        #region Events

        public event PropertyChangedEventHandler? PropertyChanged;

        #endregion

        #region Methods

        public void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}