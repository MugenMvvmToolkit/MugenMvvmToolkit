using MugenMvvmToolkit;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Xamarin.Forms;
using MugenMvvmToolkit.Xamarin.Forms.Infrastructure;

namespace $rootnamespace$
{
    public class App : MvvmXamarinApplicationBase
    {
        #region Constructors

        public App(XamarinFormsBootstrapperBase.IPlatformService platformService)
            : base(platformService)
        {
        }

        #endregion

        #region Methods

        protected override XamarinFormsBootstrapperBase CreateBootstrapper(XamarinFormsBootstrapperBase.IPlatformService platformService, IDataContext context)
        {
            return new Bootstrapper<Core.App>(platformService, new MugenContainer());
        }

        #endregion			
    }
}