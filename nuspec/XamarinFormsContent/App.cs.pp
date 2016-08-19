using MugenMvvmToolkit;
using MugenMvvmToolkit.Xamarin.Forms.Infrastructure;
using Xamarin.Forms;

namespace $rootnamespace$
{
    public class App : Application
    {
        public App(XamarinFormsBootstrapperBase.IPlatformService platformService)
        {
            XamarinFormsBootstrapperBase bootstrapper = XamarinFormsBootstrapperBase.Current ??
                                                        new Bootstrapper<Core.App>(platformService, new AutofacContainer());
            bootstrapper.Start();
        }
    }
}