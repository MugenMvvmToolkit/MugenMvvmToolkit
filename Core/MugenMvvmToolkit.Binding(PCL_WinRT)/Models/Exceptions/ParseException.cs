#region Copyright

// ****************************************************************************
// <copyright file="ParseException.cs">
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

namespace MugenMvvmToolkit.Binding.Models.Exceptions
{
    public sealed class ParseException : Exception
    {
        #region Fields

        private readonly int _position;

        #endregion

        #region Constructors

        public ParseException(string message, int position)
            : base($"{message} (at index {position.ToString()})")
        {
            _position = position;
        }

        #endregion

        #region Properties

        public int Position => _position;

        #endregion
    }
}
