using System.Collections.Generic;
using System.Reflection;
using Windows.UI.Xaml;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.WinRT.Infrastructure
{
    public abstract class WinRTDesignBootstrapperBase : DesignBootstrapperBase
    {
        #region Constructors

        static WinRTDesignBootstrapperBase()
        {
            WinRTBootstrapperBase.SetDefaultPlatformValues();
        }

        #endregion

        #region Methods

        protected sealed override IList<Assembly> GetAssemblies()
        {
            var assemblies = new HashSet<Assembly> { GetType().GetAssembly(), typeof(WinRTBootstrapperBase).GetAssembly() };
            var application = Application.Current;
            if (application != null)
                assemblies.Add(application.GetType().GetAssembly());
            BootstrapperBase.TryLoadAssembly(WinRTBootstrapperBase.BindingAssemblyName, assemblies);
            assemblies.AddRange(GetAssembliesInternal());
            return assemblies.ToArrayEx();
        }

        protected abstract IList<Assembly> GetAssembliesInternal();

        protected override PlatformInfo GetPlatformInfo()
        {
            return PlatformExtensions.GetPlatformInfo();
        }

        #endregion
    }
}