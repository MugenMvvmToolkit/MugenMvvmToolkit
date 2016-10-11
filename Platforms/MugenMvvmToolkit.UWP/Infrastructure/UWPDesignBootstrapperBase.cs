using System.Collections.Generic;
using System.Reflection;
using Windows.UI.Xaml;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.UWP.Infrastructure
{
    public abstract class UwpDesignBootstrapperBase : DesignBootstrapperBase
    {
        #region Constructors

        static UwpDesignBootstrapperBase()
        {
            UwpBootstrapperBase.SetDefaultPlatformValues();
        }

        #endregion

        #region Methods

        protected sealed override IList<Assembly> GetAssemblies()
        {
            var assemblies = new HashSet<Assembly> { GetType().GetAssembly(), typeof(UwpBootstrapperBase).GetAssembly() };
            var application = Application.Current;
            if (application != null)
                assemblies.Add(application.GetType().GetAssembly());
            BootstrapperBase.TryLoadAssembly(UwpBootstrapperBase.BindingAssemblyName, assemblies);
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