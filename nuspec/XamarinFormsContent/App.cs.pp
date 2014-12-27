using MugenMvvmToolkit.Infrastructure;
using Xamarin.Forms;

namespace $rootnamespace$
{
    public class App : Application
    {
        public App()
        {
            var bootstrapper = new Bootstrapper<MainViewModel>(new IIocContainer());
            MainPage = bootstrapper.Start();
        }
    }
}