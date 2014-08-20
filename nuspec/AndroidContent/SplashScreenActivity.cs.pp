using Android.App;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Views.Activities;

namespace $rootnamespace$.Views
{
    [Activity(Label = "$rootnamespace$", MainLauncher = true, Icon = "@drawable/icon", NoHistory = true)]
    public class SplashScreenActivity : SplashScreenActivityBase
    {
        #region Overrides of SplashScreenActivity

        protected override AndroidBootstrapperBase CreateBootstrapper()
        {
            return new Setup();
        }

        #endregion
    }
}