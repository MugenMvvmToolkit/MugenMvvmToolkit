#region Copyright

// ****************************************************************************
// <copyright file="DesignTimeException.cs">
// Copyright (c) 2012-2016 Vyacheslav Volkov
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
    public sealed class DesignTimeException : Exception
    {
        #region Constructors

        public DesignTimeException(Exception exception)
            : base(exception.Flatten(true), exception)
        {
        }

        #endregion
    }
}
