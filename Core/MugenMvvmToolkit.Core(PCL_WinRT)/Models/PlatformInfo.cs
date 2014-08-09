#region Copyright
// ****************************************************************************
// <copyright file="PlatformInfo.cs">
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
using System;

namespace MugenMvvmToolkit.Models
{
    /// <summary>
    ///     Represents the information about a platform.
    /// </summary>
    public class PlatformInfo
    {
        #region Fields

        /// <summary>
        ///     Gets the unknown platform info.
        /// </summary>
        public static readonly PlatformInfo Unknown = new PlatformInfo(PlatformType.Unknown, new Version(0, 0));

        /// <summary>
        ///     Gets the unit test platform info.
        /// </summary>
        public static readonly PlatformInfo UnitTest = new PlatformInfo(PlatformType.UnitTest, new Version(0, 0));

        private readonly PlatformType _platform;
        private readonly Version _version;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="PlatformInfo" /> class.
        /// </summary>
        public PlatformInfo(PlatformType platform, Version version)
        {
            _platform = platform;
            _version = version;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the current platform.
        /// </summary>
        public PlatformType Platform
        {
            get { return _platform; }
        }

        /// <summary>
        ///     Gets the current platform version.
        /// </summary>
        public Version Version
        {
            get { return _version; }
        }

        #endregion
    }
}