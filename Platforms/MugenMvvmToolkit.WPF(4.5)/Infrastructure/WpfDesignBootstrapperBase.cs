using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.WPF.Infrastructure
{
    public abstract class WpfDesignBootstrapperBase : DesignBootstrapperBase
    {
        #region Constructors

        static WpfDesignBootstrapperBase()
        {
            WpfBootstrapperBase.SetDefaultPlatformValues();
        }

        #endregion

        #region Methods

        protected override IList<Assembly> GetAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies().Where(assembly => !assembly.IsDynamic).ToList();
        }

        protected override PlatformInfo GetPlatformInfo()
        {
            return PlatformExtensions.GetPlatformInfo();
        }

        #endregion
    }
}