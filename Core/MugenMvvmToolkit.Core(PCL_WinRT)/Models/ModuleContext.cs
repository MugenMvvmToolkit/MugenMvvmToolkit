#region Copyright

// ****************************************************************************
// <copyright file="ModuleContext.cs">
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

using System.Collections.Generic;
using System.Reflection;
using MugenMvvmToolkit.Interfaces;
using MugenMvvmToolkit.Interfaces.Models;

namespace MugenMvvmToolkit.Models
{
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

        public ModuleContext(PlatformInfo platform, LoadMode mode, IIocContainer iocContainer, IDataContext context,
            IList<Assembly> assemblies)
        {
            _platform = platform ?? PlatformInfo.Unknown;
            _mode = mode;
            _iocContainer = iocContainer;
            _context = context.ToNonReadOnly();
            _assemblies = assemblies ?? Empty.Array<Assembly>();
        }

        #endregion

        #region Implementation of IModuleContext

        public IIocContainer IocContainer
        {
            get { return _iocContainer; }
        }

        public IDataContext Context
        {
            get { return _context; }
        }

        public IList<Assembly> Assemblies
        {
            get { return _assemblies; }
        }

        public LoadMode Mode
        {
            get { return _mode; }
        }

        public PlatformInfo Platform
        {
            get { return _platform; }
        }

        #endregion
    }
}
