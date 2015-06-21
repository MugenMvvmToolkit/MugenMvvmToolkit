using MugenMvvmToolkit.Xamarin.Forms.Infrastructure;
using Xamarin.Forms;

namespace $rootnamespace$
{
    public class App : Application
    {
        public App()
        {
            XamarinFormsBootstrapperBase bootstrapper = XamarinFormsBootstrapperBase.Current ??
                                                        new Bootstrapper<MainViewModel>(/*new IIocContainer()*/);
            MainPage = bootstrapper.Start();
        }
    }
}