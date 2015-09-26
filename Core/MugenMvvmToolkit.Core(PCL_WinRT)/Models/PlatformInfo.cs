#region Copyright

// ****************************************************************************
// <copyright file="PlatformInfo.cs">
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

using System;
using JetBrains.Annotations;

namespace MugenMvvmToolkit.Models
{
    public class PlatformInfo
    {
        #region Fields

        public static readonly PlatformInfo Unknown;

        public static readonly PlatformInfo UnitTest;

        private readonly PlatformType _platform;
        private readonly Version _version;

        #endregion

        #region Constructors

        static PlatformInfo()
        {
            Unknown = new PlatformInfo(PlatformType.Unknown, new Version(0, 0));
            UnitTest = new PlatformInfo(PlatformType.UnitTest, new Version(0, 0));
        }

        public PlatformInfo(PlatformType platform, Version version)
        {
            Should.NotBeNull(platform, "platform");
            Should.NotBeNull(version, "version");
            _platform = platform;
            _version = version;
        }

        #endregion

        #region Properties

        [NotNull]
        public PlatformType Platform
        {
            get { return _platform; }
        }

        [NotNull]
        public Version Version
        {
            get { return _version; }
        }

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            return Platform + " - " + Version;
        }

        #endregion
    }
}
