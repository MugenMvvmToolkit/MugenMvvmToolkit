#region Copyright

// ****************************************************************************
// <copyright file="WpfDesignBootstrapperBase.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
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

        protected sealed override IList<Assembly> GetAssemblies()
        {
            var assemblies = new HashSet<Assembly> { GetType().GetAssembly(), typeof(WpfDesignBootstrapperBase).GetAssembly() };
            var application = Application.Current;
            if (application != null)
                assemblies.Add(application.GetType().GetAssembly());
            BootstrapperBase.TryLoadAssembly(WpfBootstrapperBase.BindingAssemblyName, assemblies);
            assemblies.AddRange(GetAssembliesInternal());
            return assemblies.ToArrayEx();
        }

        protected virtual IList<Assembly> GetAssembliesInternal()
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