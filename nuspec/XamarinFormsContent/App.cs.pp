using MugenMvvmToolkit.Infrastructure;
using Xamarin.Forms;

namespace $rootnamespace$
{
    public class App
    {
        public static Page GetMainPage()
        {
            var bootstrapper = new Bootstrapper<MainViewModel>(new IIocContainer());
            return bootstrapper.Start();
        }
    }
}