using System.Windows;
using MugenMvvmToolkit.Infrastructure;
using $rootnamespace$.ViewModels;

namespace $rootnamespace$
{
    public partial class App : Application
    {
        public App()
        {
            // ReSharper disable once ObjectCreationAsStatement
            new Bootstrapper<MainViewModel>(this, new IIocContainer());
        }
    }
}