#region Copyright

// ****************************************************************************
// <copyright file="TypeNameAliasAttribute.cs">
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

namespace MugenMvvmToolkit.Android.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class TypeNameAliasAttribute : Attribute
    {
        #region Fields

        private readonly string _alias;

        #endregion

        #region Constructors

        public TypeNameAliasAttribute([NotNull] string @alias)
        {
            Should.NotBeNull(@alias, "alias");
            _alias = alias;
        }

        #endregion

        #region Properties

        public string Alias
        {
            get { return _alias; }
        }

        #endregion
    }
}