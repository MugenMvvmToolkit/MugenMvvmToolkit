#region Copyright
// ****************************************************************************
// <copyright file="ModuleBase.cs">
// Copyright © Vyacheslav Volkov 2012-2014
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
using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Modules
{
    /// <summary>
    ///     Represents the base class that is used to initialize ioc bindings.
    /// </summary>
    public abstract class ModuleBase : IModule
    {
        #region Fields


        /// <summary>
        ///     Gets the intialization module priority.
        /// </summary>
        public const int InitializationModulePriority = 1;

        /// <summary>
        ///     Gets the binding module priority.
        /// </summary>
        public const int BindingModulePriority = -1;

        private readonly bool _iocContainerCanBeNull;
        private readonly object _locker;
        private readonly int _priority;
        private readonly LoadMode _supportedModes;
        private IModuleContext _context;
        private IIocContainer _iocContainer;
        private LoadMode _mode;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModuleBase" /> class.
        /// </summary>
        protected ModuleBase(bool iocContainerCanBeNull)
            : this(iocContainerCanBeNull, LoadMode.All)
        {
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModuleBase" /> class.
        /// </summary>
        protected ModuleBase(bool iocContainerCanBeNull, LoadMode supportedModes = LoadMode.All, int priority = InitializationModulePriority - 1)
        {
            _iocContainerCanBeNull = iocContainerCanBeNull;
            _priority = priority;
            _supportedModes = supportedModes;
            _locker = new object();
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the current context.
        /// </summary>
        [NotNull]
        protected IModuleContext Context
        {
            get { return _context; }
        }

        /// <summary>
        ///     Gets the load mode.
        /// </summary>
        protected LoadMode Mode
        {
            get { return _mode; }
        }

        /// <summary>
        ///     Gets the current <see cref="IIocContainer" />.
        /// </summary>
        protected IIocContainer IocContainer
        {
            get { return _iocContainer; }
        }

        #endregion

        #region Implementation of IModule

        /// <summary>
        ///     Gets the priority.
        /// </summary>
        public int Priority
        {
            get { return _priority; }
        }

        /// <summary>
        ///     Loads the current module.
        /// </summary>
        public bool Load(IModuleContext context)
        {
            Should.NotBeNull(context, "context");
            if (!_supportedModes.HasFlagEx(context.Mode))
                return false;
            if (!_iocContainerCanBeNull && context.IocContainer == null)
                return false;
            lock (_locker)
            {
                _context = context;
                _iocContainer = context.IocContainer;
                _mode = context.Mode;
                try
                {
                    return LoadInternal();
                }
                finally
                {
                    _context = null;
                    _iocContainer = null;
                }
            }
        }

        /// <summary>
        ///     Unloads the current module.
        /// </summary>
        public void Unload(IModuleContext context)
        {
            Should.NotBeNull(context, "context");
            if (context.IocContainer == null)
                return;
            lock (_locker)
            {
                _context = context;
                _iocContainer = context.IocContainer;
                _mode = context.Mode;
                try
                {
                    UnloadInternal();
                }
                finally
                {
                    _context = null;
                    _iocContainer = null;
                }
            }
        }

        #endregion

        #region Methods

        /// <summary>
        ///     Loads the current module.
        /// </summary>
        protected abstract bool LoadInternal();

        /// <summary>
        ///     Unloads the current module.
        /// </summary>
        protected abstract void UnloadInternal();

        #endregion
    }
}