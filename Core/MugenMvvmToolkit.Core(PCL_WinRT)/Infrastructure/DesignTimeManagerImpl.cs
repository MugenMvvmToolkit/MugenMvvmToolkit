#region Copyright

// ****************************************************************************
// <copyright file="DesignTimeManagerImpl.cs">
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
using MugenMvvmToolkit.Interfaces.Models;
using MugenMvvmToolkit.Interfaces.ViewModels;
using MugenMvvmToolkit.Models;

namespace MugenMvvmToolkit.Infrastructure
{
    internal class DesignTimeManagerImpl : IDesignTimeManager
    {
        #region Fields

        private readonly PlatformInfo _platform;
        public static readonly DesignTimeManagerImpl Instance;

        #endregion

        #region Constructors

        static DesignTimeManagerImpl()
        {
            Instance = new DesignTimeManagerImpl(PlatformInfo.Unknown);
        }

        public DesignTimeManagerImpl(PlatformInfo platform)
        {
            _platform = platform ?? PlatformInfo.Unknown;
        }

        #endregion

        #region Implementation of IDesignTimeManager

        public bool IsDesignMode
        {
            get { return false; }
        }

        public int Priority
        {
            get { return int.MinValue; }
        }

        public PlatformInfo Platform
        {
            get
            {
                if (_platform == null && MvvmApplication.Current != null)
                    return MvvmApplication.Current.Platform;
                return _platform;
            }
        }

        public IIocContainer IocContainer
        {
            get { return null; }
        }

        public IDataContext Context
        {
            get { return DataContext.Empty; }
        }

        public void Initialize()
        {
        }

        public void InitializeViewModel(IViewModel viewModel)
        {
        }

        public void Dispose()
        {
        }

        #endregion
    }
}