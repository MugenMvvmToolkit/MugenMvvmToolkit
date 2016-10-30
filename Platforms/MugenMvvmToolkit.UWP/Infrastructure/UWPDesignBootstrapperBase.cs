#region Copyright

// ****************************************************************************
// <copyright file="UwpDesignBootstrapperBase.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
// </copyright>
// ****************************************************************************
// <author>Vyacheslav Volkov</author>
// <email>vvs0205@outlook.com</email>
// <project>MugenMvvmToolkit</project>
// <web>https://github.com/MugenMvvmToolkit/MugenMvvmToolkit</web>
// <license>
// See license.txt in this solution or http://opensource.org/licenses/MS-PL
// </license>
// ****************************************************************************

#endregion

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