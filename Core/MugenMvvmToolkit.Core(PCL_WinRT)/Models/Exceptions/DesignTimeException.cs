#region Copyright

// ****************************************************************************
// <copyright file="DesignTimeException.cs">
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

namespace MugenMvvmToolkit.Models.Exceptions
{
    /// <summary>
    ///     Represents the design time exception.
    /// </summary>
    public sealed class DesignTimeException : Exception
    {
        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="DesignTimeException" /> class.
        /// </summary>
        public DesignTimeException(Exception exception)
            : base(exception.Flatten(true), exception)
        {
        }

        #endregion
    }
}