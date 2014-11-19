using System.Windows;
using MugenMvvmToolkit.Infrastructure;

namespace $rootnamespace$
{
    public partial class App : Application
    {
        public App()
        {
			//NOTE Remove tag StartupUri from App.xaml
            new Bootstrapper<MainViewModel>(this, new IIocContainer());
        }
    }
}