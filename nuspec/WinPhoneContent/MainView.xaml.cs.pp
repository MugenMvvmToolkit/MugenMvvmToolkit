using Microsoft.Phone.Controls;
using System.ComponentModel;
using MugenMvvmToolkit.WinPhone;

namespace $rootnamespace$.Views
{
    public partial class MainView : PhoneApplicationPage
    {
        #region Constructors

        public MainView()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            PlatformExtensions.HandleMainPageOnBackKeyPress(base.OnBackKeyPress, e);
        }

        #endregion
    }
}