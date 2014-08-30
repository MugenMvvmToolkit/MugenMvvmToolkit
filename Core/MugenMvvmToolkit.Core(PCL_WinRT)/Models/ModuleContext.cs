#region Copyright
// ****************************************************************************
// <copyright file="ModuleContext.cs">
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
using System.Collections.Generic;
using System.Reflection;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Models
{
    /// <summary>
    ///     Represents the module context.
    /// </summary>
    public sealed class ModuleContext : IModuleContext
    {
        #region Fields

        private readonly IList<Assembly> _assemblies;
        private readonly IDataContext _context;
        private readonly IIocContainer _iocContainer;
        private readonly LoadMode _mode;
        private readonly PlatformInfo _platform;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ModuleContext" /> class.
        /// </summary>
        public ModuleContext(PlatformInfo platform, LoadMode mode, IIocContainer iocContainer, IDataContext context,
            IList<Assembly> assemblies)
        {
            _platform = platform ?? PlatformInfo.Unknown;
            _mode = mode;
            _iocContainer = iocContainer;
            _context = context ?? new DataContext();
            _assemblies = assemblies ?? Empty.Array<Assembly>();
        }

        #endregion

        #region Implementation of IModuleContext

        /// <summary>
        ///     Gets the <see cref="IIocContainer" />.
        /// </summary>
        public IIocContainer IocContainer
        {
            get { return _iocContainer; }
        }

        /// <summary>
        ///     Gets the <see cref="IDataContext" />.
        /// </summary>
        public IDataContext Context
        {
            get { return _context; }
        }

        /// <summary>
        ///     Gets the application assemblies.
        /// </summary>
        public IList<Assembly> Assemblies
        {
            get { return _assemblies; }
        }

        /// <summary>
        ///     Gets the module load mode.
        /// </summary>
        public LoadMode Mode
        {
            get { return _mode; }
        }

        /// <summary>
        ///     Gets the current platform.
        /// </summary>
        public PlatformInfo Platform
        {
            get { return _platform; }
        }

        #endregion
    }
}