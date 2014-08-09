#region Copyright
// ****************************************************************************
// <copyright file="EmptyValue.cs">
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

namespace MugenMvvmToolkit.Models
{
    /// <summary>
    ///     Represents the empty list helper.
    /// </summary>
    public static class EmptyValue<T>
    {
        #region Fields

        /// <summary>
        ///     Gets the array instance.
        /// </summary>
        public static readonly T[] ArrayInstance;

        /// <summary>
        ///     Gets the <see cref="IList{T}" /> instance.
        /// </summary>
        public static readonly IList<T> ListInstance;

        #endregion

        #region Constructors

        static EmptyValue()
        {
            ArrayInstance = new T[0];
            ListInstance = ArrayInstance;
        }

        #endregion
    }
}