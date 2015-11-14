#region Copyright

// ****************************************************************************
// <copyright file="WrapperRegistrationModuleBase.cs">
// Copyright (c) 2012-2015 Vyacheslav Volkov
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

using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Modules
{
    public abstract class WrapperRegistrationModuleBase : ModuleBase
    {
        #region Constructors

        protected WrapperRegistrationModuleBase()
            : this(LoadMode.All)
        {
        }

        protected WrapperRegistrationModuleBase(LoadMode supportedModes, int priority = WrapperRegistrationModulePriority)
            : base(false, supportedModes, priority)
        {
        }

        #endregion

        #region Overrides of ModuleBase

        protected sealed override bool LoadInternal()
        {
            IWrapperManager wrapperManager;
            IocContainer.TryGet(out wrapperManager);
            var manager = wrapperManager as IConfigurableWrapperManager;
            if (manager == null)
                Tracer.Warn("The IConfigurableWrapperManager is not registered, the '{0}' is ignored", GetType().FullName);
            else
                RegisterWrappers(manager);
            return true;
        }

        protected sealed override void UnloadInternal()
        {
        }

        #endregion

        #region Methods

        protected abstract void RegisterWrappers(IConfigurableWrapperManager wrapperManager);

        #endregion
    }
}
