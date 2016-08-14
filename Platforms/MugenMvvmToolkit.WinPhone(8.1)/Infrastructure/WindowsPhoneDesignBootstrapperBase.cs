using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.WinPhone.Infrastructure
{
    public abstract class WindowsPhoneDesignBootstrapperBase : DesignBootstrapperBase
    {
        #region Constructors

        static WindowsPhoneDesignBootstrapperBase()
        {
            WindowsPhoneBootstrapperBase.SetDefaultPlatformValues();
        }

        #endregion

        #region Methods

        protected sealed override IList<Assembly> GetAssemblies()
        {
            var assemblies = new HashSet<Assembly> {GetType().GetAssembly(), typeof(WindowsPhoneDesignBootstrapperBase).GetAssembly()};
            var application = Application.Current;
            if (application != null)
                assemblies.Add(application.GetType().GetAssembly());
            BootstrapperBase.TryLoadAssembly(WindowsPhoneBootstrapperBase.BindingAssemblyName, assemblies);
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