using Android.App;
using MugenMvvmToolkit.Android.Infrastructure;
using MugenMvvmToolkit.Android.Views.Activities;

namespace $rootnamespace$.Views
{
    [Activity(Label = "$rootnamespace$", Theme = "@android:style/Theme.NoTitleBar", MainLauncher = true,
        Icon = "@drawable/icon", NoHistory = true)]
    public class SplashScreenActivity : SplashScreenActivityBase
    {
        #region Overrides of SplashScreenActivityBase

        protected override AndroidBootstrapperBase CreateBootstrapper()
        {
            return new Setup();
        }

        #endregion
    }
}