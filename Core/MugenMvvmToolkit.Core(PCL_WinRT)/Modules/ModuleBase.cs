#region Copyright

// ****************************************************************************
// <copyright file="ModuleBase.cs">
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

using JetBrains.Annotations;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Modules
{
    public abstract class ModuleBase : IModule
    {
        #region Fields


        public const int InitializationModulePriority = 1;

        public const int BindingModulePriority = -1;

        public const int WrapperRegistrationModulePriority = -1000;

        private readonly bool _iocContainerCanBeNull;
        private readonly object _locker;
        private readonly int _priority;
        private readonly LoadMode _supportedModes;
        private IModuleContext _context;
        private IIocContainer _iocContainer;
        private LoadMode _mode;

        #endregion

        #region Constructors

        protected ModuleBase(bool iocContainerCanBeNull)
            : this(iocContainerCanBeNull, LoadMode.All)
        {
        }

        protected ModuleBase(bool iocContainerCanBeNull, LoadMode supportedModes = LoadMode.All, int priority = InitializationModulePriority - 1)
        {
            _iocContainerCanBeNull = iocContainerCanBeNull;
            _priority = priority;
            _supportedModes = supportedModes;
            _locker = new object();
        }

        #endregion

        #region Properties

        [NotNull]
        protected IModuleContext Context
        {
            get { return _context; }
        }

        protected LoadMode Mode
        {
            get { return _mode; }
        }

        protected IIocContainer IocContainer
        {
            get { return _iocContainer; }
        }

        #endregion

        #region Implementation of IModule

        public int Priority
        {
            get { return _priority; }
        }

        public bool Load(IModuleContext context)
        {
            Should.NotBeNull(context, "context");
            var mode = _supportedModes & context.Mode;
            if (_supportedModes.HasFlagEx(LoadMode.RuntimeDebug) || _supportedModes.HasFlagEx(LoadMode.RuntimeRelease))
            {
                if (mode != context.Mode)
                    return false;
            }
            else
            {
                if (mode == 0)
                    return false;
            }
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
                    _mode = default(LoadMode);
                }
            }
        }

        public void Unload(IModuleContext context)
        {
            Should.NotBeNull(context, "context");
            if (!_iocContainerCanBeNull && context.IocContainer == null)
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
                    _mode = default(LoadMode);
                }
            }
        }

        #endregion

        #region Methods

        protected abstract bool LoadInternal();

        protected abstract void UnloadInternal();

        #endregion
    }
}
