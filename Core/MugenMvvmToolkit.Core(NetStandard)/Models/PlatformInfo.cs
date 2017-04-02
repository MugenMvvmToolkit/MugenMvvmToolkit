#region Copyright

// ****************************************************************************
// <copyright file="PlatformInfo.cs">
// Copyright (c) 2012-2017 Vyacheslav Volkov
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
        private readonly string _rawVersion;
        private readonly Func<PlatformIdiom> _getCurrentIdiom;
        private readonly Version _version;

        #endregion

        #region Constructors

        static PlatformInfo()
        {
            Unknown = new PlatformInfo(PlatformType.Unknown, "0.0", PlatformIdiom.Unknown);
            UnitTest = new PlatformInfo(PlatformType.UnitTest, "0.0", PlatformIdiom.Unknown);
        }

        public PlatformInfo(PlatformType platform, string rawVersion, PlatformIdiom idiom)
            : this(platform, rawVersion, () => idiom)
        {

        }

        public PlatformInfo(PlatformType platform, string rawVersion, Func<PlatformIdiom> getCurrentIdiom)
        {
            Should.NotBeNull(platform, nameof(platform));
            Should.NotBeNull(rawVersion, nameof(rawVersion));
            Should.NotBeNull(getCurrentIdiom, nameof(getCurrentIdiom));
            _platform = platform;
            _rawVersion = rawVersion;
            _getCurrentIdiom = getCurrentIdiom;
            Version.TryParse(rawVersion, out _version);
        }

        #endregion

        #region Properties

        [NotNull]
        public PlatformType Platform => _platform;

        [NotNull]
        public PlatformIdiom Idiom => _getCurrentIdiom();

        [NotNull]
        public string RawVersion => _rawVersion;

        [NotNull]
        public Version Version => _version;

        #endregion

        #region Overrides of Object

        public override string ToString()
        {
            return Platform + " - " + RawVersion;
        }

        #endregion
    }
}
