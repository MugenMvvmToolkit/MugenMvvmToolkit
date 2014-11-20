using MugenMvvmToolkit.ViewModels;

namespace $rootnamespace$.ViewModels
{
    public class MainViewModel : CloseableViewModel
    {
        #region Fields

        private string _text = "Hello MugenMvvmToolkit";

        #endregion

        #region Properties

        public string Text
        {
            get { return _text; }
            set
            {
                if (Equals(_text, value))
                    return;
                _text = value;
                OnPropertyChanged();
            }
        }

        #endregion
    }
}