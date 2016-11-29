#region Copyright

// ****************************************************************************
// <copyright file="WrapperRegistrationModuleBase.cs">
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

using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Modules
{
    public abstract class WrapperRegistrationModuleBase : IModule
    {
        #region Methods

        protected virtual bool CanLoad(IModuleContext context)
        {
            return true;
        }

        protected abstract void RegisterWrappers(IConfigurableWrapperManager wrapperManager);

        #endregion

        #region Implementation of interfaces

        public int Priority { get; set; } = ApplicationSettings.ModulePriorityWrapperRegistration;

        public bool Load(IModuleContext context)
        {
            if ((context.IocContainer == null) || !CanLoad(context))
                return false;
            IWrapperManager wrapperManager;
            context.IocContainer.TryGet(out wrapperManager);
            var manager = wrapperManager as IConfigurableWrapperManager;
            if (manager == null)
            {
                Tracer.Warn("The IConfigurableWrapperManager is not registered, the '{0}' is ignored", GetType().FullName);
                return false;
            }
            RegisterWrappers(manager);
            return true;
        }

        public void Unload(IModuleContext context)
        {
        }

        #endregion
    }
}