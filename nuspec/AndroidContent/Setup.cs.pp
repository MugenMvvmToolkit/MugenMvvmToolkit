using System;
using $rootnamespace$;
using MugenMvvmToolkit.Android.Attributes;
using MugenMvvmToolkit.Android.Infrastructure;
using MugenMvvmToolkit.Interfaces;

[assembly: Bootstrapper(typeof (Setup))]

namespace $rootnamespace$
{
    public class Setup : AndroidBootstrapperBase
    {
        #region Overrides of AndroidBootstrapperBase

        protected override IIocContainer CreateIocContainer()
        {
            return new IIocContainer();
        }

        protected override Type GetMainViewModelType()
        {
            return typeof (MainViewModel);
        }

        #endregion
    }
}