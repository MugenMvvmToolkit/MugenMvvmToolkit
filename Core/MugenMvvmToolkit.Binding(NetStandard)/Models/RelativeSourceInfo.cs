﻿#region Copyright

// ****************************************************************************
// <copyright file="RelativeSourceInfo.cs">
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

using JetBrains.Annotations;

namespace MugenMvvmToolkit.Binding.Models
{
    public class RelativeSourceInfo
    {
        #region Fields

        public const string RelativeSourceType = "RelativeSource";
        public const string ElementSourceType = "ElementSource";
        public const string SelfType = "Self";
        public const string ContextSourceType = "ContextSource";

        #endregion

        #region Constructors

        public RelativeSourceInfo([NotNull] string type, [CanBeNull] string elementName, [CanBeNull] string path, uint level)
        {
            Should.NotBeNull(type, type);
            Type = type;
            ElementName = elementName;
            Path = path;
            Level = level;
        }

        #endregion

        #region Properties

        [NotNull]
        public string Type { get; }

        [CanBeNull]
        public string ElementName { get; }

        [CanBeNull]
        public string Path { get; }

        public uint Level { get; }

        #endregion
    }
}