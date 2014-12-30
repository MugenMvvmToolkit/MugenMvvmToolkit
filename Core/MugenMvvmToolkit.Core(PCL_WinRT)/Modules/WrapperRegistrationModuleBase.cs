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

using MugenMvvmToolkit.Infrastructure;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Modules
{
    /// <summary>
    ///     Represents the module that allows to register wrappers using <see cref="WrapperManager" /> class.
    /// </summary>
    public abstract class WrapperRegistrationModuleBase : ModuleBase
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="WrapperRegistrationModuleBase" /> class.
        /// </summary>
        protected WrapperRegistrationModuleBase()
            : this(LoadMode.All)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="WrapperRegistrationModuleBase" /> class.
        /// </summary>
        protected WrapperRegistrationModuleBase(LoadMode supportedModes, int priority = int.MinValue + 10)
            : base(false, supportedModes, priority)
        {
        }

        #endregion

        #region Overrides of ModuleBase

        /// <summary>
        ///     Loads the current module.
        /// </summary>
        protected override sealed bool LoadInternal()
        {
            IWrapperManager wrapperManager;
            IocContainer.TryGet(out wrapperManager);
            var manager = wrapperManager as WrapperManager;
            if (manager == null)
                Tracer.Warn("The WrapperManager is not registered, the '{0}' is ignored", GetType().FullName);
            else
                RegisterWrappers(manager);
            return true;
        }

        /// <summary>
        ///     Unloads the current module.
        /// </summary>
        protected override sealed void UnloadInternal()
        {
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Registers the wrappers using <see cref="WrapperManager" /> class.
        /// </summary>
        protected abstract void RegisterWrappers(WrapperManager wrapperManager);

        #endregion
    }
}