#region Copyright
// ****************************************************************************
// <copyright file="ParseException.cs">
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

namespace MugenMvvmToolkit.Binding.Models.Exceptions
{
    /// <summary>
    ///     Represents the parse exception.
    /// </summary>
    public sealed class ParseException : Exception
    {
        #region Fields

        private readonly int _position;

        #endregion

        #region Constructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ParseException" /> class.
        /// </summary>
        public ParseException(string message, int position)
            : base(string.Format("{0} (at index {1})", message, position))
        {
            _position = position;
        }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the error position.
        /// </summary>
        public int Position
        {
            get { return _position; }
        }

        #endregion
    }
}